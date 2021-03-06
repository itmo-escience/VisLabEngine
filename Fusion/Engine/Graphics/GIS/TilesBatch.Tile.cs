﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.YandexMaps;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using ContainmentType = Fusion.Engine.Graphics.GIS.GlobeMath.ContainmentType;

namespace Fusion.Engine.Graphics.GIS
{
	partial class TilesGisLayer
	{
		int lowestLod	= 9;
		int minLod		= 3;

		int tileDensity		= 10;
		int CurrentLevel	= 8;

		bool updateTiles = false;

		class Node
		{
			public int X, Y, Z;

			public Node		Parent;
			public Node[]	Childs;
		}

		struct TraversalInfo
		{
			public Node CentralNode;
			public int Offset, Length;
		}


		class GlobeTile : IDisposable
		{
			public int X;
			public int Y;
			public int Z;

			public bool Isloaded = false;

			public VertexBuffer VertexBuf;
			public IndexBuffer	IndexBuf;


			public void Dispose()
			{
				VertexBuf.Dispose();
				IndexBuf.Dispose();
			}
		}

		class TileInfo
		{
			public enum TileState
			{
				NotLoaded,
				Loaded,
				Loading
			}

			public bool IsActive = false;
		}


		Dictionary<string, GlobeTile> tilesToRender = new Dictionary<string, GlobeTile>();
		Dictionary<string, GlobeTile> tilesOld		= new Dictionary<string, GlobeTile>();
		Dictionary<string, GlobeTile> tilesPool		= new Dictionary<string, GlobeTile>();


		Dictionary<string, TileInfo> allTilesInfo = new Dictionary<string, TileInfo>();
		HashSet<string> activeTiles = new HashSet<string>();


		void DetermineTiles()
		{
			var ms = CurrentMapSource;

		    frustum = camera.Frustum;

            //var d = Math.Log((camera.CameraDistance - camera.EarthRadius) * 1000.0, 2.0);
            double lod = lowestLod;
			
			if (camera.Viewport.Width != 0) {
				int closestZoom = lowestLod;
				double	closestRadius	= 100;

				for (int zoom = 3; zoom <= ms.MaxZoom; zoom++) {
					var dis = GetLevelScreenSpaceError(zoom, camera.CameraDistance - camera.EarthRadius);

					if (dis < closestRadius && dis >= 0.0f) {
						closestRadius	= dis;
						closestZoom		= zoom;
					}
				}

				lod = closestZoom;
			}


			var maxLod = ms.MaxZoom;

			lowestLod = (int)lod;

			if (lowestLod > maxLod) lowestLod = maxLod;
			CurrentLevel = lowestLod;

			if (CurrentLevel < 3) CurrentLevel = 3;


			// Get camera mercator position 
			var lonLat = camera.GetCameraLonLat();
			lonLat.X = DMathUtil.RadiansToDegrees(lonLat.X);
			lonLat.Y = DMathUtil.RadiansToDegrees(lonLat.Y);


			if (updateTiles) {
				foreach (var tile in tilesToRender) {
					tilesPool.Add(tile.Key, tile.Value);
				}
				updateTiles = false;
			} else {
				foreach (var tile in tilesToRender) {
					tilesOld.Add(tile.Key, tile.Value);
				}
			}

			tilesToRender.Clear();


			var info = new TraversalInfo[2];

			var centralNode = new Node { Z = CurrentLevel - 2 };

			var tileUpper = ms.Projection.WorldToTilePos(lonLat.X, lonLat.Y, centralNode.Z);
			centralNode.X = (int)tileUpper.X;
			centralNode.Y = Math.Max(0, (int)tileUpper.Y);
            //GetTileIndexByMerc(merc, centralNode.Z, out centralNode.X, out centralNode.Y);

            info[0].CentralNode = new Node { X = centralNode.X, Y = centralNode.Y, Z = centralNode.Z };
			info[0].Offset = 4;
			info[0].Length = 7;

			var offNode = new Node { X = info[0].CentralNode.X + info[0].Offset, Y = info[0].CentralNode.Y + info[0].Offset, Z = info[0].CentralNode.Z };
			GetChilds(ref offNode);

			info[1].CentralNode = offNode.Childs[0];
			info[1].Offset = 3;
			info[1].Length = 8;

		    int tilesNum = 1 << info[0].CentralNode.Z;


		    int yNum = 9;//Math.Min(7, Math.Max(centralNode.Y*2, 0) + 2);

		    
		    yNum = Math.Min(yNum, tilesNum / 2);

		    HashSet<Vector2> usedTiles = new HashSet<Vector2>();
			for (int j = -yNum; j <= yNum; j++)
			{
			    var nodeY = info[0].CentralNode.Y + j;
			    //if (nodeY < 0 || nodeY >= tilesNum) continue;

                int xNum = Math.Max(6, tilesNum / 2 - Math.Min(Math.Max(nodeY, 0), Math.Max((tilesNum - nodeY - 1), 0))*2 + 2);
                xNum = Math.Min(xNum, tilesNum / 2);

                for (int i = -xNum; i <= xNum; i++)
				{
				    
                    var nodeX = info[0].CentralNode.X + i;
					
					
				    if (nodeY < 0)
				    {
				        nodeY = -nodeY;
				        nodeX += nodeX + tilesNum / 2;
				    } else if (nodeY >= tilesNum)
				    {
				        nodeY = tilesNum * 2 - nodeY - 1;
				        nodeX += nodeX + tilesNum / 2;
                    }

				    nodeX = nodeX % tilesNum;
				    if (nodeX < 0) nodeX = tilesNum + nodeX;

                    if (nodeY < 0 || nodeY >= tilesNum) continue;
				    if (!usedTiles.Contains(new Vector2(nodeX, nodeY)))
				    {
				        usedTiles.Add(new Vector2(nodeX, nodeY));
				        var currNode = new Node {X = nodeX, Y = nodeY, Z = info[0].CentralNode.Z};

				        QuadTreeTraversalDownTop(info, currNode, 0);
				    }
				}
			}


			foreach (var tile in tilesOld)
			{
				tilesPool.Add(tile.Key, tile.Value);
			}
			tilesOld.Clear();
		}

		
		private double GetLevelScreenSpaceError(int zoom, double distance)
		{
			double eps	= 256.0/(1 << zoom);
			double xx	= camera.Viewport.Height;
			double dd	= distance;
			double eta	= DMathUtil.DegreesToRadians(camera.Parameters.CameraFovDegrees);

			double p = (eps*xx)/(2*dd*Math.Tan(eta));

			var dis = 1.0 - p;
			return dis;
		}

        public int GetTilesToRenderCount()
	    {
	        return tilesToRender.Count;
	    }

		double GetOptimalDistanceForLevel(int zoom)
		{
			double eps = 256.0 / (1 << zoom);
			double xx = camera.Viewport.Height;
			double eta = DMathUtil.DegreesToRadians(camera.Parameters.CameraFovDegrees);

			double dd = (eps * xx) / (2 * Math.Tan(eta));

			return dd;
		}


		void QuadTreeTraversalDownTop(TraversalInfo[] info, Node node, int step)
		{
			int maxLevel = CurrentMapSource.MaxZoom;

			if (node.Z > maxLevel) return;

			if (step >= info.Length) {
				AddTileToRenderList(node.X, node.Y, node.Z);
				return;
			}

			GetChilds(ref node);

			int offX = node.X - info[step].CentralNode.X;
			int offY = node.Y - info[step].CentralNode.Y;

			CurrentMapSource.GetTile(node.X, node.Y, node.Z);

			if (offX >= info[step].Offset && offX < info[step].Offset + info[step].Length &&
				offY >= info[step].Offset && offY < info[step].Offset + info[step].Length) {

				if (CheckTiles(node)) {
					foreach (var child in node.Childs) {
						QuadTreeTraversalDownTop(info, child, step + 1);
					}
				} else {
					AddTileToRenderList(node.X, node.Y, node.Z);
				}

			} else {
				AddTileToRenderList(node.X, node.Y, node.Z);
			}
		}

	    private bool IsTileInFrustum(int x, int y, int z, DBoundingFrustum frustum)
	    {
	        long numTiles = 1 << z;

	        double x0 = ((double) (x + 0) / (double) numTiles);
	        double y0 = ((double) (y + 0) / (double) numTiles);
	        double x1 = ((double) (x + 1) / (double) numTiles);
	        double y1 = ((double) (y + 1) / (double) numTiles);

	        var p1 = GetCartesianCoord(x0, y0);
	        var p2 = GetCartesianCoord(x0, y1);
	        var p3 = GetCartesianCoord(x1, y0);
	        var p4 = GetCartesianCoord(x1, y1);

	        var sphere = DBoundingSphere.FromPoints(new[] {p1, p2, p3, p4});

	        var res = frustum.Contains(sphere) != ContainmentType.Disjoint;

	        //if (Game.Keyboard.IsKeyDown(Keys.R))
	        //{
	        //    DMatrix trans;                
	        //    DMatrix.Translation(ref p1, out trans);
	        //    Gis.Debug.DrawSphere(1000.0f / numTiles, res ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f), trans, 4);
	        //    DMatrix.Translation(ref p2, out trans);
	        //    Gis.Debug.DrawSphere(1000.0f / numTiles, res ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f), trans, 4);
	        //    DMatrix.Translation(ref p3, out trans);
	        //    Gis.Debug.DrawSphere(1000.0f / numTiles, res ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f), trans, 4);
	        //    DMatrix.Translation(ref p4, out trans);
	        //    Gis.Debug.DrawSphere(1000.0f / numTiles, res ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f), trans, 4);
	        //}

	        return res;
	    }


	    bool CheckTiles(Node node)
		{
			if (node.Childs == null) return false;
			//if (node.Childs[0].Z > CurrentMapSource.MaxZoom) return false;

			bool check = true;
			foreach (var child in node.Childs) {
				check = check && CurrentMapSource.GetTile(child.X, child.Y, child.Z).IsLoaded;
			}
			return check;
		}


		private double viewerHeight = 2000;

	    void DetermineTilesDebug()
	    {
	        int zoom = 5;
	        for (int i = 0; i < 1 << zoom; i++)
	        {
	            for (int j = 0; j < 1 << zoom; j++)
	            {
	                AddTileToRenderList(i, j, zoom);
	            }
	        }
	    }

        void DetermineTiles(int startZoomLevel)
		{
            ////////////////////////////////////////////////////

		    frustum = camera.Frustum;

            if (updateTiles) {
				foreach (var tile in tilesToRender) {
					tilesPool.Add(tile.Key, tile.Value);
				}
				updateTiles = false;
			} else {
				foreach (var tile in tilesToRender) {
					tilesOld.Add(tile.Key, tile.Value);
				}
			}

			tilesToRender.Clear();
			/////////////////////////////////////////////////////

			//var cameraPos = DVector3.Transform(new DVector3(0, 0, GeoHelper.EarthRadius + viewerHeight), DQuaternion.RotationYawPitchRoll(DMathUtil.DegreesToRadians(131.881642), -DMathUtil.DegreesToRadians(43.111248), 0));
		    var cameraPos = camera.CameraPosition;
			var cameraNorm = DVector3.Normalize(cameraPos);
            
			// calculate maxlod
			var distToEarth = cameraPos.Length() - GeoHelper.EarthRadius;
			var maxLod = 0;
			for (int i = 1; i < lodDistances.Length; i++) {
				if (distToEarth >= lodDistances[i]) break;
				maxLod = i;
			}

			var borderLod = Math.Min(maxLod, CurrentMapSource.MaxZoom);

			//var debug		= Gis.Debug; 
			//debug.Clear();
			//debug.DrawPoint(cameraPos, 200);



			var tempDistances = new double[lodDistances.Length];

			for (int i = 0; i < lodDistances.Length; i++) {
				if (i < borderLod) {
					tempDistances[i] = lodDistances[i] / (0.85 * (borderLod-i) + 1);
				}
				else tempDistances[i] = lodDistances[i];
			}



			var basis = GeoHelper.CalculateBasisOnSurface(GeoHelper.CartesianToSpherical(cameraPos));
			//if (borderLod > 0) debug.DrawSphere(tempDistances[borderLod - 1], Color.Red, basis * DMatrix.Translation(cameraPos));
			//if (borderLod-1 > 0) debug.DrawSphere(tempDistances[borderLod - 2], Color.Blue, basis * DMatrix.Translation(cameraPos));
			//if (borderLod - 2 > 0) debug.DrawSphere(tempDistances[borderLod - 3], Color.Blue, basis * DMatrix.Translation(cameraPos));
			//debug.DrawSphere(tempDistances[borderLod], Color.Green, basis * DMatrix.Translation(cameraPos), 100);
			//if (borderLod < lodDistances.Length-1)
			//	debug.DrawSphere(tempDistances[borderLod + 1], Color.Yellow, basis * DMatrix.Translation(cameraPos));
			
			//Console.WriteLine("Camera pos: " + cameraPos);

			Stack<Node> nodes = new Stack<Node>();
			long numTiles = 1 << startZoomLevel;


			for(int i = 0; i < numTiles; i++)
				for (int j = 0; j < numTiles; j++)
					nodes.Push(new Node {
						X = i,
						Y = j,
						Z = startZoomLevel
					});
            

            while (nodes.Any()) {
				var node = nodes.Pop();

				var nodePos = GetTileCenterPosition(node.X, node.Y, node.Z);

				//debug.DrawPoint(nodePos, 100);

				var nodeNorm = DVector3.Normalize(nodePos);
			    
				var dist	= (nodePos - cameraPos).Length();

				//Console.WriteLine();
				//Console.WriteLine("Node: "		+ node.X + " " + node.Y + " " + node.Z);
				//Console.WriteLine("Dist: "		+ dist);
				//Console.WriteLine("Node pos: "	+ nodePos);
				//Console.WriteLine(GetLevelScreenSpaceError(node.Z, dist));

				var factor =  1.0f;

				if (node.Z != borderLod && dist < tempDistances[node.Z] * factor) // Break this tile to pieces
				{
					GetChilds(ref node);


					if (CheckTiles(node)) {
						foreach (var child in node.Childs) {
							nodes.Push(child);
						}
					}
					else {
						if (DVector3.Dot(cameraNorm, nodeNorm) > -0.15 /*&& (IsTileInFrustum(node.X, node.Y, node.Z, frustum) || ( node.Y == 0 ) || (node.Y == numTiles - 1))*/)  
							AddTileToRenderList(node.X, node.Y, node.Z);
					}
				}
				else {
					if (DVector3.Dot(cameraNorm, nodeNorm) > -0.15 /*&& (IsTileInFrustum(node.X, node.Y, node.Z, frustum) || (node.Y == 0) || (node.Y == numTiles - 1))*/)
						AddTileToRenderList(node.X, node.Y, node.Z);
				}
			}

			//foreach (var node in nodes) {
			//	AddTileToRenderList(node.X, node.Y, node.Z);
			//}

			/////////////////////////////////////////////////////
			foreach (var tile in tilesOld) {
				tilesPool.Add(tile.Key, tile.Value);
			}
			tilesOld.Clear();
		}

	    DVector3 GetTileCenterPosition(int x, int y, int z)
		{
			long numTiles = 1 << z;

			double x0 = ((double)(x + 0) / (double)numTiles);
			double y0 = ((double)(y + 0) / (double)numTiles);
			double x1 = ((double)(x + 1) / (double)numTiles);
			double y1 = ((double)(y + 1) / (double)numTiles);

			var xHalf = (x0 + x1)/2.0;
			var yHalf = (y0 + y1)/2.0;

			var lonLat	= CurrentMapSource.Projection.TileToWorldPos(xHalf, yHalf);
			var ret		= GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(lonLat), GeoHelper.EarthRadius);
			return ret;
		}


	    DVector3 GetCartesianCoord(double x, double y, double z = 0)
	    {
            var lonLat = CurrentMapSource.Projection.TileToWorldPos(x, y);
            DVector3 result = GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(lonLat), GeoHelper.EarthRadius);
            return result;
	    }

	    private DBoundingFrustum frustum;

		void AddTileToRenderList(int x, int y, int zoom)
		{
			string key = GenerateKey(x, y, zoom);

			if (tilesToRender.ContainsKey(key)) return;

            if (!IsTileInFrustum(x, y, zoom, frustum)) return;

			if (tilesOld.ContainsKey(key))
			{
				tilesToRender.Add(key, tilesOld[key]);
				tilesOld.Remove(key);
				return;
			}

			long numTiles = 1 << zoom;

			double x0 = ((double)(x + 0) / (double)numTiles);
			double y0 = ((double)(y + 0) / (double)numTiles);
			double x1 = ((double)(x + 1) / (double)numTiles);
			double y1 = ((double)(y + 1) / (double)numTiles);

      //      //var frustum = camera.Frustum;
      //      if (/*localFrustum.Contains(GetCoodsInFrustumCoords(x0, y0, zoom)) != ContainmentType.Disjoint
      //          || localFrustum.Contains(GetCoodsInFrustumCoords(x1, y1, zoom)) != ContainmentType.Disjoint
      //          || localFrustum.Contains(GetCoodsInFrustumCoords(x1, y0, zoom)) != ContainmentType.Disjoint
      //          || localFrustum.Contains(GetCoodsInFrustumCoords(x0, y1, zoom)) != ContainmentType.Disjoint
      //          || */localFrustum.Contains((GetTileCenterPosition(x, y, zoom))) != ContainmentType.Disjoint)
		    //{
		        if (tilesPool.Any())
		        {
		            GlobeTile tile;
		            if (tilesPool.ContainsKey(key))
		            {
		                tile = tilesPool[key];
		                tilesPool.Remove(key);
		            }
		            else
		            {
		                var temp = tilesPool.First();
		                tile = temp.Value;
		                tilesPool.Remove(temp.Key);
		            }


		            tile.X = x;
		            tile.Y = y;
		            tile.Z = zoom;

		            //tile.left = x0;
		            //tile.right = x1;
		            //tile.top = y0;
		            //tile.bottom = y1;

		            int[] indexes;
		            Gis.GeoPoint[] vertices;

		            CalculateVertices(out vertices, out indexes, tileDensity, x0, x1, y0, y1);

		            tile.VertexBuf.SetData(vertices, 0, vertices.Length);
		            tile.IndexBuf.SetData(indexes, 0, indexes.Length);

		            tilesToRender.Add(key, tile);

		        }
		        else
		        {

		            var tile = new GlobeTile
		            {
		                X = x,
		                Y = y,
		                Z = zoom,
		                //left	= x0,
		                //right	= x1,
		                //top		= y0,
		                //bottom	= y1
		            };


		            GenerateTileGrid(tileDensity, ref tile.VertexBuf, out tile.IndexBuf, x0, x1, y0, y1);

		            tilesToRender.Add(key, tile);
		        }
		    //}
		}


		void GenerateTileGrid(int density, ref VertexBuffer vb, out IndexBuffer ib, double left, double right, double top, double bottom)
		{
			int[]			indexes;
			Gis.GeoPoint[]	vertices;

			DisposableBase.SafeDispose(ref vb);

			CalculateVertices(out vertices, out indexes, tileDensity, left, right, top, bottom);

			vb = new VertexBuffer(Game.GraphicsDevice, typeof(Gis.GeoPoint), vertices.Length);
			ib = new IndexBuffer(Game.GraphicsDevice, indexes.Length);
			ib.SetData(indexes);
			vb.SetData(vertices, 0, vertices.Length);
		}


		void CalculateVertices(out Gis.GeoPoint[] vertices, out int[] indeces, int density, double left, double right, double top, double bottom)
		{
			int RowsCount		= density + 2;
			int ColumnsCount	= RowsCount;

			//var el = Game.GetService<LayerService>().ElevationLayer;
			var ms = CurrentMapSource;

			var		verts	= new List<Gis.GeoPoint>();
			float	step	= 1.0f / (density + 1);
			double	dStep	= 1.0 / (double)(density + 1);

			for (int row = 0; row < RowsCount; row++) {
				for (int col = 0; col < ColumnsCount; col++) {

					double xx = left * (1.0 - dStep * col) + right * dStep * col;
					double yy = top * (1.0 - dStep * row) + bottom * dStep * row;

					double lon, lat;
					var sc = ms.Projection.TileToWorldPos(xx, yy, 0);

					//float elev = 0.0f;
					//if (zoom > 8) elev = el.GetElevation(sc.X, sc.Y) / 1000.0f;

					lon = sc.X * Math.PI / 180.0;
					lat = sc.Y * Math.PI / 180.0;


					verts.Add(new Gis.GeoPoint {
						Tex0	= new Vector4(step * col, step * row, 0, 0),
						Lon		= lon,
						Lat		= lat
					});
				}

			}

			var tindexes = new List<int>();

			for (int row = 0; row < RowsCount - 1; row++)
			{
				for (int col = 0; col < ColumnsCount - 1; col++)
				{
					tindexes.Add(col + row * ColumnsCount);
					tindexes.Add(col + (row + 1) * ColumnsCount);
					tindexes.Add(col + 1 + row * ColumnsCount);

					tindexes.Add(col + 1 + row * ColumnsCount);
					tindexes.Add(col + (row + 1) * ColumnsCount);
					tindexes.Add(col + 1 + (row + 1) * ColumnsCount);
				}
			}

			vertices = verts.ToArray();
			indeces = tindexes.ToArray();
		}


		void CalculateVerticesLonLat(out Gis.GeoPoint[] vertices, out int[] indeces, int density, double left, double right, double top, double bottom)
		{
			int RowsCount = density + 2;
			int ColumnsCount = RowsCount;

			//var el = Game.GetService<LayerService>().ElevationLayer;
			var ms = CurrentMapSource;

			var verts		= new List<Gis.GeoPoint>();
			float step		= 1.0f / (density + 1);
			double dStep	= 1.0 / (double)(density + 1);

			var leftTop		= ms.Projection.TileToWorldPos(left, top, 0);
			var rightBottom = ms.Projection.TileToWorldPos(right, bottom, 0);

			for (int row = 0; row < RowsCount; row++) {
				for (int col = 0; col < ColumnsCount; col++) {
					double xx = leftTop.X * (1.0 - dStep * col) + rightBottom.X * dStep * col;
					double yy = leftTop.Y * (1.0 - dStep * row) + rightBottom.Y * dStep * row;

					var sc = new DVector2(xx, yy);

					var lon = sc.X * Math.PI / 180.0;
					var lat = sc.Y * Math.PI / 180.0;

					verts.Add(new Gis.GeoPoint {
						Tex0 = new Vector4(step * col, step * row, 0, 0),
						Lon = lon,
						Lat = lat
					});
				}

			}

			var tindexes = new List<int>();

			for (int row = 0; row < RowsCount - 1; row++) {
				for (int col = 0; col < ColumnsCount - 1; col++) {
					tindexes.Add(col + row * ColumnsCount);
					tindexes.Add(col + (row + 1) * ColumnsCount);
					tindexes.Add(col + 1 + row * ColumnsCount);

					tindexes.Add(col + 1 + row * ColumnsCount);
					tindexes.Add(col + (row + 1) * ColumnsCount);
					tindexes.Add(col + 1 + (row + 1) * ColumnsCount);
				}
			}

			vertices = verts.ToArray();
			indeces = tindexes.ToArray();
		}


		void GetChilds(ref Node node)
		{
			long tilesCurrentLevel = 1 << node.Z;
			long tilesNextLevel = 1 << (node.Z + 1);

			int posX = (int)(((double)node.X / tilesCurrentLevel) * tilesNextLevel);
			int posY = (int)(((double)node.Y / tilesCurrentLevel) * tilesNextLevel);

			node.Childs = new[] {
					new Node{ X = posX,		Y = posY,		Z = node.Z+1, Parent = node }, 
					new Node{ X = posX +1,	Y = posY,		Z = node.Z+1, Parent = node }, 
					new Node{ X = posX,		Y = posY + 1,	Z = node.Z+1, Parent = node }, 
					new Node{ X = posX + 1,	Y = posY + 1,	Z = node.Z+1, Parent = node }
				};
		}


		string GenerateKey(int x, int y, int zoom)
		{
			return x + "_" + y + "_" + zoom;
		}
	}
}
