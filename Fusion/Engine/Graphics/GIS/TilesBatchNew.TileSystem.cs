using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using ContainmentType = Fusion.Engine.Graphics.GIS.GlobeMath.ContainmentType;

namespace Fusion.Engine.Graphics.GIS
{
    partial class TilesBatchNew
    {
        int lowestLod = 9;
        int minLod = 3;

        int tileDensity = 8;

        class Node
        {
            public int X, Y, Z;

            public Node Parent;
            public Node[] Childs;
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

            //public VertexBuffer VertexBuf;
            //public IndexBuffer	IndexBuf;


            public void Dispose()
            {
                //VertexBuf.Dispose();
                //IndexBuf.Dispose();
            }
        }

        Dictionary<string, GlobeTile> tilesToRender = new Dictionary<string, GlobeTile>();        
              
        private class NodeInfo
        {
            public int x;
            public int y;
            public int zoom;
            public MapTile tile;
            public NodeInfo parent;
            public NodeInfo tlChild, trChild, blChild, brChild;
            public DVector3 Position;            

            public bool IsChildrenInit()
            {
                return (tlChild?.IsInit() ?? false) && (trChild?.IsInit() ?? false) && (blChild?.IsInit() ?? false) && (brChild?.IsInit() ?? false);
            }

            public bool IsInit()
            {
                return tile.IsLoaded;
            }

            public HashSet<NodeInfo> RequestSplit(TilesBatchNew tileSystem)
            {
                var ret = new HashSet<NodeInfo>();
                if (tlChild == null) ret.Add(tlChild = new NodeInfo()
                {
                    x = x * 2,
                    y = y * 2,
                    zoom = zoom + 1,
                    parent = this,
                    tile = tileSystem.CurrentMapSource.GetTile(x * 2, y * 2, zoom + 1), 
                    Position = tileSystem.GetTileCenterPosition(x * 2, y * 2, zoom + 1)
                });
                if (trChild == null) ret.Add(trChild = new NodeInfo()
                {
                    x = x * 2 + 1,
                    y = y * 2,
                    zoom = zoom + 1,
                    parent = this,
                    tile = tileSystem.CurrentMapSource.GetTile(x * 2 + 1, y * 2, zoom + 1),
                    Position = tileSystem.GetTileCenterPosition(x * 2 + 1, y * 2, zoom + 1)
                });
                if (blChild == null) ret.Add(blChild = new NodeInfo()
                {
                    x = x * 2,
                    y = y * 2 + 1,
                    zoom = zoom + 1,
                    parent = this,
                    tile = tileSystem.CurrentMapSource.GetTile(x * 2, y * 2 + 1, zoom + 1),
                    Position = tileSystem.GetTileCenterPosition(x * 2, y * 2 + 1, zoom + 1)
                });
                if (brChild == null) ret.Add(brChild = new NodeInfo()
                {
                    x = x * 2 + 1,
                    y = y * 2 + 1,
                    zoom = zoom + 1,
                    parent = this,
                    tile = tileSystem.CurrentMapSource.GetTile(x * 2 + 1, y * 2 + 1, zoom + 1),
                    Position = tileSystem.GetTileCenterPosition(x * 2 + 1, y * 2 + 1, zoom + 1)
                });
                return ret;
            }

        }

        NodeInfo rootTile;
        private HashSet<NodeInfo> lastNodes = new HashSet<NodeInfo>();

        private bool determineInProcess;
        private Object tilesLock = new object();
        void DetermineTilesNew()
        {
            if (determineInProcess) return;
            determineInProcess = true;
            Gis.ResourceWorker.Post(r =>
            {
                r.ProcessQueue.Post(t =>
                {
                    HashSet<NodeInfo> drawnNodes = new HashSet<NodeInfo>();

                    var tilesToRender = new Dictionary<string, GlobeTile>();
                    if (rootTile == null)
                    {
                        rootTile = new NodeInfo()
                        {
                            x = 0,
                            y = 0,
                            zoom = 0,
                            tile = CurrentMapSource.GetTile(0, 0, 0),
                        };
                        lastNodes.Add(rootTile);
                    }


                    var cameraPos = camera.CameraPosition;

                    var frustum = camera.Frustum;

                    foreach (var node in lastNodes)
                    {
                        var nodePos = GetTileCenterPosition(node.x, node.y, node.zoom);

                        if (drawnNodes.Contains(node.parent)) continue;

                        if (node.parent != null && node.parent.zoom > 1 &&
                            (cameraPos - nodePos).Length() > GetOptimalDistanceForLevel(node.zoom - 1))
                        {
                            drawnNodes.Add(node.parent);
                        }
                        else
                        {
                            if (node.zoom == 0 || ((cameraPos - nodePos).Length() <
                                                   GetOptimalDistanceForLevel(node.zoom + 1)
                                                   && //IsTileInFrustum(node.x, node.y, node.zoom, frustum) &&
                                                   node.IsInit()))
                            {
                                if (node.IsChildrenInit())
                                {
                                    drawnNodes.Add(node.tlChild);
                                    drawnNodes.Add(node.blChild);
                                    drawnNodes.Add(node.trChild);
                                    drawnNodes.Add(node.brChild);
                                    continue;
                                }
                                else
                                {
                                    node.RequestSplit(this);
                                    drawnNodes.Add(node);
                                }
                            }
                            else
                            {
                                drawnNodes.Add(node);
                            }
                        }
                    }

                    foreach (var node in drawnNodes
                        //.Where(a => a.zoom <= 5 || (a.Position - cameraPos).Length() < cameraPos.Length())
                        .Where(a => IsTileInFrustum(a.x, a.y, a.zoom, frustum))
                        .OrderBy(a => (cameraPos - a.Position).Length()))
                    {
                        if (tilesToRender.Count < tilesLimit)
                        {
                            tilesToRender.Add(GenerateKey(node.x, node.y, node.zoom), new GlobeTile()
                            {
                                Isloaded = true,
                                X = node.x,
                                Y = node.y,
                                Z = node.zoom,
                            });
                        }
                    }
                    lock (tilesLock)
                    {
                        this.tilesToRender = tilesToRender;
                    }
                    lastNodes = drawnNodes;
                    determineInProcess = false;
                }, null, int.MaxValue);
            }, null, int.MaxValue);
        }        

        private double GetLevelScreenSpaceError(int zoom, double distance)
        {
            double eps = 256.0 / (1 << zoom);
            double xx = camera.Viewport.Height;
            double dd = distance;
            double eta = DMathUtil.DegreesToRadians(camera.Parameters.CameraFovDegrees);

            double p = (eps * xx) / (2 * dd * Math.Tan(eta));

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

        private bool IsTileInFrustum(int x, int y, int z, DBoundingFrustum frustum)
        {
            long numTiles = 1 << z;            

            double x0 = ((double)(x + 0) / (double)numTiles);
            double y0 = ((double)(y + 0) / (double)numTiles);
            double x1 = ((double)(x + 1) / (double)numTiles);
            double y1 = ((double)(y + 1) / (double)numTiles);

            var p1 = GetCartesianCoord(x0, y0);
            var p2 = GetCartesianCoord(x0, y1);
            var p3 = GetCartesianCoord(x1, y0);
            var p4 = GetCartesianCoord(x1, y1);

            var sphere = DBoundingSphere.FromPoints(new[] {p1, p2, p3, p4});
            sphere.Radius *= 3f;
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

        DVector3 GetTileCenterPosition(int x, int y, int z)
        {
            long numTiles = 1 << z;

            double x0 = ((double)(x + 0) / (double)numTiles);
            double y0 = ((double)(y + 0) / (double)numTiles);
            double x1 = ((double)(x + 1) / (double)numTiles);
            double y1 = ((double)(y + 1) / (double)numTiles);

            var xHalf = (x0 + x1) / 2.0;
            var yHalf = (y0 + y1) / 2.0;

            var lonLat = CurrentMapSource.Projection.TileToWorldPos(xHalf, yHalf);
            var ret = GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(lonLat), GeoHelper.EarthRadius);
            return ret;
        }


        DVector3 GetCartesianCoord(double x, double y)
        {
            var lonLat = CurrentMapSource.Projection.TileToWorldPos(x, y);
            if (y == 0) lonLat.Y = 0;
            if (y == 1) lonLat.Y = Math.PI;
            DVector3 result = GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(lonLat), GeoHelper.EarthRadius);
            return result;
        }

        static string GenerateKey(int x, int y, int zoom)
        {
            return x + "_" + y + "_" + zoom;
        }
    }
}
