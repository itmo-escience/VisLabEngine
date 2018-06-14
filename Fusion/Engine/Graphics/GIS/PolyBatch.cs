using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using TriangleNet;
using TriangleNet.Geometry;
using BoundingBox = Fusion.Core.Mathematics.BoundingBox;

namespace Fusion.Engine.Graphics.GIS
{
	public class PolyGisLayer : Gis.GisLayer
	{
		protected Ubershader	shader;
		protected StateFactory	factory;
		protected StateFactory	factoryXray;

		[Flags]
		public enum PolyFlags : int
		{
			VERTEX_SHADER	= 1 << 0,
			PIXEL_SHADER	= 1 << 1,
			DRAW_HEAT		= 1 << 2,
			DRAW_TEXTURED	= 1 << 3,
			SHOW_FRAMES		= 1 << 4,
			COMPUTE_SHADER	= 1 << 5,
			BLUR_HORIZONTAL = 1 << 6,
			BLUR_VERTICAL	= 1 << 7,
			DRAW_COLORED	= 1 << 8,
			XRAY			= 1 << 9,
			NO_DEPTH	= 1 << 10,
			CULL_NONE	= 1 << 11,		    

            USE_PALETTE_COLOR = 1 << 12,
		    UV_TRANSPARENCY = 1 << 13,


			USE_NORMAL = 1 << 14,

		    USE_CONST_COLOR = 1 << 15,
		    USE_VERT_COLOR = 1 << 16,
        }

		public int Flags;

		public bool IsDoubleBuffer { get; protected set; }

		public Texture2D Texture;
		public Texture2D Palette;

		public float PaletteValue			{ get { return constData.Data.X; } set { constData.Data.X = value; } }
		public float PaletteTransparency	{ get { return constData.Data.Y; } set { constData.Data.Y = value; } }
        public Vector4 ColorMultiplier { get { return constData.Data; } set { constData.Data = value; } }
		protected struct ConstData {
			public Vector4 Data;
		}
		protected ConstData			constData;
		protected ConstantBuffer	cb;

		//public Vector2	PatternSize;
		//public float	ArrowsScale;

		protected VertexBuffer	firstBuffer;
		protected VertexBuffer	secondBuffer;
		protected VertexBuffer	currentBuffer;
		protected IndexBuffer	indexBuffer;

		protected SamplerState Sampler = SamplerState.LinearClamp;

		public Gis.GeoPoint[] PointsCpu { get; protected set; }
		public int[] IndecesCpu			{ get; protected set; }

		public int IndecesToDrawCount = 0;
		public int FirstIndex = 0;

		protected PolyGisLayer(Game engine) : base(engine) { }


		public PolyGisLayer(Game engine, Gis.GeoPoint[] points, int[] indeces, bool isDynamic = false) : base(engine)
		{
			//Console.WriteLine(points.Length + " _ " + indeces.Length);
			Initialize(points, indeces, isDynamic);

			Flags = (int)(PolyFlags.VERTEX_SHADER | PolyFlags.PIXEL_SHADER | PolyFlags.DRAW_COLORED );
		}


		void EnumFunc(PipelineState ps, int flag)
		{
			var flags = (PolyFlags)flag;

			ps.VertexInputElements	= VertexInputElement.FromStructure<Gis.GeoPoint>();
			ps.BlendState			= flags.HasFlag(PolyFlags.XRAY)			? BlendState.Additive		: BlendState.AlphaBlend;
			ps.DepthStencilState	= flags.HasFlag(PolyFlags.NO_DEPTH)		? DepthStencilState.None	: DepthStencilState.Default;
			ps.RasterizerState		= flags.HasFlag(PolyFlags.CULL_NONE)	? RasterizerState.CullNone	: RasterizerState.CullCW;

			ps.Primitive = Primitive.TriangleList;
		}


		protected void Initialize(Gis.GeoPoint[] points, int[] indeces, bool isDynamic, bool reInit = false)
		{
			shader		= Game.Content.Load<Ubershader>("globe.Poly.hlsl");
			factory		= shader.CreateFactory(typeof(PolyFlags), EnumFunc);
			factoryXray = shader.CreateFactory(typeof(PolyFlags), Primitive.TriangleList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.Additive, RasterizerState.CullCW, DepthStencilState.None);

			var vbOptions = isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;

			firstBuffer = new VertexBuffer(Game.GraphicsDevice, typeof(Gis.GeoPoint), points.Length, vbOptions);
			firstBuffer.SetData(points);
			currentBuffer = firstBuffer;

			indexBuffer = new IndexBuffer(Game.Instance.GraphicsDevice, indeces.Length);
			indexBuffer.SetData(indeces);

			PointsCpu	= points;
			IndecesCpu	= indeces;

			IndecesToDrawCount = indeces.Length;


			cb			= new ConstantBuffer(Game.GraphicsDevice, typeof(ConstData));
		    if (!reInit)
		    {
		        constData = new ConstData();
		        constData.Data = Vector4.One;
		    }
		}


		public override void Dispose()
		{
			//shader.Dispose();	
			factory.Dispose();
			factoryXray.Dispose();

			firstBuffer.Dispose();
			indexBuffer.Dispose();

			cb.Dispose();

			base.Dispose();
		}


		public void UpdatePointsBuffer()
		{
			if (currentBuffer == null) return;

			currentBuffer.SetData(PointsCpu);
		}

	    public void UpdateIndexBuffer()
	    {
	        if (indexBuffer == null) return;
	        indexBuffer.SetData(IndecesCpu);
	    }


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			if (currentBuffer == null || indexBuffer == null) {
				Log.Warning("Poly layer null reference");
				return;
			}

			if (((PolyFlags) Flags).HasFlag(PolyFlags.XRAY)) {
				Game.GraphicsDevice.PipelineState = factoryXray[Flags];
			}
			else {
				Game.GraphicsDevice.PipelineState = factory[Flags];
			}

			if (((PolyFlags)Flags).HasFlag(PolyFlags.DRAW_TEXTURED)) {
				if(Texture != null)
					Game.GraphicsDevice.PixelShaderResources[0] = Texture; 

				cb.SetData(constData);

				Game.GraphicsDevice.PixelShaderConstants[1] = cb;
			}

		    if (((PolyFlags) Flags).HasFlag(PolyFlags.USE_CONST_COLOR))
		    {
		        cb.SetData(constData);

		        Game.GraphicsDevice.PixelShaderConstants[1] = cb;
            }

			Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;

			Game.GraphicsDevice.PixelShaderSamplers[0] = Sampler;
			Game.GraphicsDevice.PixelShaderSamplers[1] = SamplerState.AnisotropicClamp;

			Game.GraphicsDevice.SetupVertexInput(currentBuffer, indexBuffer);
			Game.GraphicsDevice.DrawIndexed(IndecesToDrawCount, FirstIndex, 0);

			//game.GraphicsDevice.ResetStates();
		}


		public class SelectInfo
		{
			public BoundingBox	BoundingBox;
			public string		NodeName;
			public int			NodeIndex;
			public int			MeshIndex;
			public DMatrix		WorldMatrix;
			public DMatrix		WorldMatrixInvert;
		}

		public SelectInfo[] ObjectsInfo = new SelectInfo[0];

		public class SelectedItem : Gis.SelectedItem
		{
			public int			NodeIndex;
			public BoundingBox	BoundingBox;
			public DMatrix		BoundingBoxTransform;
		}


		public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
		{
			var slelectedList = new List<Gis.SelectedItem>();            
			foreach (var info in ObjectsInfo)
			{
				var localNearPoint	= DVector3.TransformCoordinate(nearPoint, info.WorldMatrixInvert);
				var localFarPoint	= DVector3.TransformCoordinate(farPoint, info.WorldMatrixInvert);

				var ray = new Ray(localNearPoint.ToVector3(), DVector3.Normalize(localFarPoint - localNearPoint).ToVector3());

				float distance;                
				if (info.BoundingBox.Intersects(ref ray, out distance)) {
					Console.WriteLine(info.NodeName);

					slelectedList.Add(new SelectedItem
					{
						Distance	= distance,
						Name		= info.NodeName,
						NodeIndex	= info.NodeIndex,
						BoundingBox = info.BoundingBox
					});

					//if(Gis.Debug != null) Gis.Debug.DrawBoundingBox(info.BoundingBox, info.WorldMatrix);
				}
			}

			return slelectedList;
		}


		public static PolyGisLayer GenerateRegularGrid(Game engine, double left, double right, double top, double bottom, int density, int dimX, int dimY, MapProjection projection)
		{
			int[] indexes;
			Gis.GeoPoint[] vertices;

			CalculateVertices(out vertices, out indexes, density, left, right, top, bottom, projection);

			//var vb = new VertexBuffer(Game.Instance.GraphicsDevice, typeof(Gis.GeoPoint), vertices.Length);
			//var ib = new IndexBuffer(Game.Instance.GraphicsDevice, indexes.Length);
			//ib.SetData(indexes);
			//vb.SetData(vertices, 0, vertices.Length);

			return new PolyGisLayer(engine, vertices, indexes, false);
		}

	    public void Merge(PolyGisLayer second)
	    {
	        var points = new List<Gis.GeoPoint>(PointsCpu);
	        var indeces = new List<int>(IndecesCpu);
	        var c = points.Count;
	        foreach (var p in second.PointsCpu) 
	        {
	            points.Add(p);
	        }
	        foreach (var index in second.IndecesCpu)
	        {
	            indeces.Add(index + c);
	        }
	        //PointsCpu = points.ToArray();
	        //IndecesCpu = indeces.ToArray();
            Dispose();
            Initialize(points.ToArray(), indeces.ToArray(), false, true);
	    }

	    public void MergeList(IEnumerable<PolyGisLayer> list)
	    {
	        var points = new List<Gis.GeoPoint>(PointsCpu);
	        var indeces = new List<int>(IndecesCpu);
            foreach (var second in list)
	        {	            
	            var c = points.Count;
	            foreach (var p in second.PointsCpu)
	            {
	                points.Add(p);
	            }

	            foreach (var index in second.IndecesCpu)
	            {
	                indeces.Add(index + c);
	            }
	        }

	        //PointsCpu = points.ToArray();
	        //IndecesCpu = indeces.ToArray();
	        Dispose();
	        Initialize(points.ToArray(), indeces.ToArray(), false, true);
	    }


        public static PolyGisLayer CreateFromContour(Game engine, DVector2[] lonLatRad, Color color, bool usePalette = true, List<DVector2[]> excludeRad = null, TriangulationAlgorithm method = TriangulationAlgorithm.Dwyer)
		{
			var triangulator = new TriangleNet.Mesh();
			triangulator.Behavior.Algorithm = method;		    
			InputGeometry ig = new InputGeometry();

		    if (lonLatRad.Length < 3 || (lonLatRad[0] - lonLatRad[1]).Length() < float.Epsilon && (lonLatRad[0] - lonLatRad.Aggregate(DVector2.Zero, (v1, v2) => v1 + v2)).Length() < float.Epsilon)
		    {
		        return null;
		    }

		    int i = 5;
			ig.AddPoint(lonLatRad[0].X, lonLatRad[0].Y);
			for (int v = 1; v < lonLatRad.Length; v++) {
				ig.AddPoint(lonLatRad[v].X, lonLatRad[v].Y);
				ig.AddSegment(v-1, v);
			}
		    i = lonLatRad.Length;
			ig.AddSegment(lonLatRad.Length - 1, 0);
		    if ((lonLatRad.First() - lonLatRad.Last()).Length() < float.Epsilon) lonLatRad = lonLatRad.Skip(1).ToArray();
		    if (excludeRad != null && excludeRad.Count > 0)
		    {
		        foreach (var list in excludeRad)
		        {
		            var m = list.ToList().Aggregate(DVector2.Zero, (a, b) => a + b/list.Length);
		            ig.AddPoint(list[0].X, list[0].Y);
                    for (int v = 1; v < list.Length; v++)
		            {
		                ig.AddPoint(list[v].X, list[v].Y);
                        ig.AddSegment(i + v - 1, i + v);
                    }
		            ig.AddSegment(i + list.Length - 1, i);
                    ig.AddHole(m.X, m.Y);                    
		            i += list.Length;
		        }
		    }

		    triangulator.Triangulate(ig);
            

			if (triangulator.Vertices.Count != lonLatRad.Length) {
				//Log.Warning("Vertices count not match");
				//return null;
			}


			var points = new List<Gis.GeoPoint>();
			foreach (var pp in triangulator.Vertices) {
				points.Add(new Gis.GeoPoint {
					Lon		= pp.X,
					Lat		= pp.Y,
					Color	= color
				});
			}


			var indeces = new List<int>();
			foreach (var tr in triangulator.Triangles) {
				indeces.Add(tr.P0);
				indeces.Add(tr.P1);
				indeces.Add(tr.P2);
			}

		    if (usePalette)
		    {
		        return new PolyGisLayer(engine, points.ToArray(), indeces.ToArray(), false)
		        {
		            Flags = (int) (PolyFlags.NO_DEPTH | PolyFlags.DRAW_TEXTURED | PolyFlags.CULL_NONE |
		                           PolyFlags.VERTEX_SHADER | PolyFlags.PIXEL_SHADER | PolyFlags.USE_PALETTE_COLOR)
		        };
		    }
		    else
		    {
		        return new PolyGisLayer(engine, points.ToArray(), indeces.ToArray(), false)
		        {
		            Flags = (int)(PolyFlags.NO_DEPTH | PolyFlags.DRAW_COLORED | PolyFlags.CULL_NONE |
		                          PolyFlags.VERTEX_SHADER | PolyFlags.PIXEL_SHADER | PolyFlags.USE_VERT_COLOR)
		        };
            }		
		}


		public static PolyGisLayer CreateRoadFromLine(DVector2[] lineRad, double[] distances, double width)
		{
			if (lineRad.Length == 0) {
				return null;
			}

			float distMul = 4.0f;

			List<Gis.GeoPoint> vertices = new List<Gis.GeoPoint>();
			List<int> indeces = new List<int>();

			for (int i = 0; i < lineRad.Length - 1; i++)
			{
				var p0 = lineRad[i];
				var p1 = lineRad[i + 1];

				var cPos0 = GeoHelper.SphericalToCartesian(p0);
				var cPos1 = GeoHelper.SphericalToCartesian(p1);

				var normal = DVector3.Normalize(cPos0);

				var dir		= cPos1 - cPos0;
				var sideVec = DVector3.Normalize(DVector3.Cross(normal, dir));

				var sideOffset = sideVec * width;


				// Plane
				var finalPosRight	= cPos0 + sideOffset;
				var finalPosLeft	= cPos0 - sideOffset;

				var lonLatRight = GeoHelper.CartesianToSpherical(finalPosRight);
				var lonLatLeft	= GeoHelper.CartesianToSpherical(finalPosLeft);

				vertices.Add(new Gis.GeoPoint {
					Lon = lonLatRight.X,
					Lat = lonLatRight.Y,
					Color	= Color.Yellow,
					Tex0	= new Vector4((float)distances[i] * distMul, 0.0f, 0.0f, 0.0f),
				});

				vertices.Add(new Gis.GeoPoint {
					Lon = lonLatLeft.X,
					Lat = lonLatLeft.Y,
					Color	= Color.Yellow,
					Tex0	= new Vector4((float)distances[i] * distMul, 1.0f, 0.0f, 0.0f),
				});

				indeces.Add(i * 2);
				indeces.Add(i * 2 + 1);
				indeces.Add((i + 1) * 2);

				indeces.Add(i * 2 + 1);
				indeces.Add((i + 1) * 2 + 1);
				indeces.Add((i + 1) * 2);

			}

			{
				var p0 = lineRad[lineRad.Length - 1];
				var p1 = lineRad[lineRad.Length - 2];

				var cPos0 = GeoHelper.SphericalToCartesian(p0);
				var cPos1 = GeoHelper.SphericalToCartesian(p1);

				var normal = DVector3.Normalize(cPos0);

				var dir		= cPos0 - cPos1;
				var sideVec = DVector3.Normalize(DVector3.Cross(normal, dir));

				var sideOffset = sideVec * width;


				// Plane
				var finalPosRight = cPos0 + sideOffset;
				var finalPosLeft = cPos0 - sideOffset;

				var lonLatRight = GeoHelper.CartesianToSpherical(finalPosRight);
				var lonLatLeft	= GeoHelper.CartesianToSpherical(finalPosLeft);


				vertices.Add(new Gis.GeoPoint {
					Lon = lonLatRight.X,
					Lat = lonLatRight.Y,
					Color	= Color.Yellow,
					Tex0	= new Vector4((float)distances[lineRad.Length - 1] * distMul, 0.0f, 0.0f, 0.0f),
				});

				vertices.Add(new Gis.GeoPoint {
					Lon = lonLatLeft.X,
					Lat = lonLatLeft.Y,
					Color	= Color.Yellow,
					Tex0	= new Vector4((float)distances[lineRad.Length - 1] * distMul, 1.0f, 0.0f, 0.0f),
				});
			}


			return new PolyGisLayer(Game.Instance, vertices.ToArray(), indeces.ToArray(), false) {
				Flags	= (int)(PolyFlags.NO_DEPTH | PolyFlags.DRAW_TEXTURED | PolyFlags.CULL_NONE | PolyFlags.VERTEX_SHADER | PolyFlags.PIXEL_SHADER),
				Sampler = SamplerState.AnisotropicWrap
			};
		}



		public static PolyGisLayer CreatePolyFromLine(DVector2[] lineRad, double width, bool usePalette = true, Color? color = null)
		{
			if(lineRad.Length == 0) {
				return null;
			}

		    if (color == null) color = Color.White;

			List<Gis.GeoPoint> vertices = new List<Gis.GeoPoint>();
			List<int> indeces = new List<int>();

			for(int i = 0; i < lineRad.Length; i++) {

				var pP = i != 0 ? lineRad[i - 1] : lineRad[lineRad.Length - 2];
                var p0 = lineRad[i];
				var p1 = i != lineRad.Length - 1 ? lineRad[i + 1] : lineRad[1];

				var cPosP = GeoHelper.SphericalToCartesian(pP);
				var cPos0 = GeoHelper.SphericalToCartesian(p0);
				var cPos1 = GeoHelper.SphericalToCartesian(p1);

				var normal = DVector3.Normalize(cPos0);

				var forwardDir     = cPos1 - cPos0;
				var sideForwardVec = DVector3.Normalize(DVector3.Cross(normal, forwardDir));

				var prevDir     = cPos0 - cPosP;
				var sidePrevVec = DVector3.Normalize(DVector3.Cross(normal, prevDir));

				var sideVec = sideForwardVec + sidePrevVec;

				var fx = DVector3.Dot(sideForwardVec, sideVec);

				if(fx < 0.00001) fx = 1.0f;

				sideVec = sideVec / fx;

				var sideOffset = sideVec * width;
				
				// Plane
				var finalPosRight	= cPos0;// + sideOffset;
				var finalPosLeft	= cPos0 - sideOffset;

				var lonLatRight = GeoHelper.CartesianToSpherical(finalPosRight);
				var lonLatLeft  = GeoHelper.CartesianToSpherical(finalPosLeft);

				vertices.Add(new Gis.GeoPoint {
					Lon = lonLatRight.X,
					Lat = lonLatRight.Y,
					Color = color.Value,
					Tex0 = Vector4.Zero,
				});

				vertices.Add(new Gis.GeoPoint {
					Lon = lonLatLeft.X,
					Lat = lonLatLeft.Y,
					Color = color.Value,
					Tex0 = Vector4.Zero,
				});

				if(i != lineRad.Length - 1) {
					indeces.Add(i * 2);
					indeces.Add(i * 2 + 1);
					indeces.Add((i + 1) * 2);

					indeces.Add(i * 2 + 1);
					indeces.Add((i + 1) * 2 + 1);
					indeces.Add((i + 1) * 2);
				}
			}

		    if (usePalette)
		    {
		        return new PolyGisLayer(Game.Instance, vertices.ToArray(), indeces.ToArray(), false)
		        {
		            Flags = (int) (PolyFlags.NO_DEPTH | PolyFlags.DRAW_TEXTURED | PolyFlags.CULL_NONE |
		                           PolyFlags.VERTEX_SHADER | PolyFlags.PIXEL_SHADER | PolyFlags.USE_PALETTE_COLOR),
		            Sampler = SamplerState.AnisotropicWrap
		        };
		    }
		    else
		    {
		        return new PolyGisLayer(Game.Instance, vertices.ToArray(), indeces.ToArray(), false)
		        {
		            Flags = (int)(PolyFlags.NO_DEPTH | PolyFlags.DRAW_COLORED | PolyFlags.CULL_NONE |
		                          PolyFlags.VERTEX_SHADER | PolyFlags.PIXEL_SHADER | PolyFlags.USE_VERT_COLOR),
		            Sampler = SamplerState.AnisotropicWrap
		        };
            }
		}



		public static PolyGisLayer CreateFromUtmFbxModel(Game engine, string fileName)
		{
			var scene = engine.Content.Load<Scene>(fileName);

			var s = fileName.Split('_');
			double easting	= double.Parse(s[1].Replace(',', '.'));
			double northing = double.Parse(s[2].Replace(',', '.'));
			string region	= s[3];
			
			var transforms = new Matrix[scene.Nodes.Count];
			scene.ComputeAbsoluteTransforms(transforms);

			List<Gis.GeoPoint>	points = new List<Gis.GeoPoint>();
			List<int>			indeces = new List<int>();

			var oInfo = new SelectInfo[scene.Meshes.Count];

			for (int i = 0; i < scene.Nodes.Count; i++) {

				var meshIndex = scene.Nodes[i].MeshIndex;

				if (meshIndex < 0) {
					continue;
				}


				oInfo[meshIndex] = new SelectInfo {
					MeshIndex	= meshIndex,
					NodeIndex	= i,
					NodeName	= scene.Nodes[i].Name
				};


				int vertexOffset = points.Count;

				var world = transforms[i];

				double worldLon, worldLat;
				Gis.UtmToLatLon(easting + world.TranslationVector.X, northing - world.TranslationVector.Z, region, out worldLon, out worldLat);

				var worldBasis			= GeoHelper.CalculateBasisOnSurface(DMathUtil.DegreesToRadians(new DVector2(worldLon, worldLat)));
				var worldBasisInvert	= DMatrix.Invert(worldBasis);

				oInfo[meshIndex].WorldMatrix		= worldBasis;
				oInfo[meshIndex].WorldMatrixInvert	= worldBasisInvert;
                
				List<Vector3> cartPoints = new List<Vector3>();
				
				foreach (var vert in scene.Meshes[meshIndex].Vertices) {
					var pos = vert.Position;

					var worldPos	= Vector3.TransformCoordinate(pos, world);
					var worldNorm	= Vector3.TransformNormal(vert.Normal, world);


					double lon, lat;
					Gis.UtmToLatLon(easting + (double)worldPos.X, northing - (double)worldPos.Z, region, out lon, out lat);

					DVector3 norm = new DVector3(worldNorm.X, worldNorm.Z, worldNorm.Y);
					norm.Normalize();

					norm = DVector3.TransformNormal(norm, DMatrix.RotationYawPitchRoll(DMathUtil.DegreesToRadians(lon), DMathUtil.DegreesToRadians(lat), 0));
					norm.Normalize();

					norm.Y = -norm.Y;

					lon = DMathUtil.DegreesToRadians(lon) + 0.0000068;
					lat = DMathUtil.DegreesToRadians(lat) + 0.0000113;

					cartPoints.Add(DVector3.TransformCoordinate(GeoHelper.SphericalToCartesian(new DVector2(lon, lat), GeoHelper.EarthRadius + worldPos.Y / 1000.0), worldBasisInvert).ToVector3());

					var point = new Gis.GeoPoint {
						Lon		= lon,
						Lat		= lat,
						Color	= vert.Color0,
						Tex0	= new Vector4(norm.ToVector3(), 0),
						Tex1	= new Vector4(0,0,0, (float)(worldPos.Y/1000.0))
					};
					//point.Color.Alpha = 0.5f;
					points.Add(point);
				}

				oInfo[meshIndex].BoundingBox = BoundingBox.FromPoints(cartPoints.ToArray());

				var inds = scene.Meshes[meshIndex].GetIndices();

				foreach (var ind in inds) {
					indeces.Add(vertexOffset + ind);
				}

			}

			return new PolyGisLayer(engine, points.ToArray(), indeces.ToArray(), false) { ObjectsInfo = oInfo };
		}


		public static void CreateFromUtmFbxModel(Game engine, string fileName, string meshName, out Gis.GeoPoint[] outPoints, out int[] outIndeces)
		{
			outPoints	= null;
			outIndeces	= null;

			var scene = engine.Content.Load<Scene>(fileName);

			var s = fileName.Split('_');
			double easting = double.Parse(s[1].Replace(',', '.'));
			double northing = double.Parse(s[2].Replace(',', '.'));
			string region = s[3];

			var transforms = new Matrix[scene.Nodes.Count];
			scene.ComputeAbsoluteTransforms(transforms);

			List<Gis.GeoPoint> points = new List<Gis.GeoPoint>();

			var node = scene.Nodes.FirstOrDefault(x => x.Name == meshName);

			if (node == null || node.MeshIndex < 0) return;

			var ind			= scene.Nodes.IndexOf(node);
			var meshIndex	= node.MeshIndex;
            var world		= transforms[ind];

			double worldLon, worldLat;
			Gis.UtmToLatLon(easting + world.TranslationVector.X, northing - world.TranslationVector.Z, region, out worldLon, out worldLat);

			foreach (var vert in scene.Meshes[meshIndex].Vertices) {
				var pos = vert.Position;

				var worldPos = Vector3.TransformCoordinate(pos, world);
				var worldNorm = Vector3.TransformNormal(vert.Normal, world);

				double lon, lat;
				Gis.UtmToLatLon(easting + worldPos.X, northing - worldPos.Z, region, out lon, out lat);

				DVector3 norm = new DVector3(worldNorm.X, worldNorm.Z, worldNorm.Y);
				norm.Normalize();

				norm = DVector3.TransformNormal(norm,
					DMatrix.RotationYawPitchRoll(DMathUtil.DegreesToRadians(lon), DMathUtil.DegreesToRadians(lat), 0));
				norm.Normalize();

				norm.Y = -norm.Y;

				lon = DMathUtil.DegreesToRadians(lon) + 0.0000068;
				lat = DMathUtil.DegreesToRadians(lat) + 0.0000113;


				var point = new Gis.GeoPoint {
					Lon = lon,
					Lat = lat,
					Color = vert.Color0,
					Tex0 = new Vector4(norm.ToVector3(), 0),
					Tex1 = new Vector4(0, 0, 0, worldPos.Y / 1000.0f)
				};
				points.Add(point);

				var inds = scene.Meshes[meshIndex].GetIndices();

				outPoints = points.ToArray();
				outIndeces = inds;
			}
		}


		void SwapBuffers()
		{

		}


		static protected void CalculateVertices(out Gis.GeoPoint[] vertices, out int[] indeces, int density, double leftLon, double rightLon, double topLat, double bottomLat, MapProjection projection)
		{
			int RowsCount		= density + 2;
			int ColumnsCount	= RowsCount;

			var ms		= projection;
			var verts	= new List<Gis.GeoPoint>();

			var leftTop		= ms.WorldToTilePos(leftLon, topLat, 0);
			var rightBottom = ms.WorldToTilePos(rightLon, bottomLat, 0);

			double left		= leftTop.X;
			double right	= rightBottom.X;
			double top		= leftTop.Y;
			double bottom	= rightBottom.Y;

			float	step	= 1.0f / (density + 1);
			double	dStep	= 1.0 / (double)(density + 1);

			for (int row = 0; row < RowsCount; row++) {
				for (int col = 0; col < ColumnsCount; col++) {
					double xx = left * (1.0 - dStep * col) + right * dStep * col;
					double yy = top * (1.0 - dStep * row) + bottom * dStep * row;

					var sc = ms.TileToWorldPos(xx, yy, 0);

					var lon = sc.X * Math.PI / 180.0;
					var lat = sc.Y * Math.PI / 180.0;

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
	}
}
