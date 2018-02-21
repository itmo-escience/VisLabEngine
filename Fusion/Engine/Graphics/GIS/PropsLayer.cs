//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;
//using Fusion.Core.Mathematics;
//using Fusion.Drivers.Graphics;
//using Fusion.Engine.Common;
//using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
//using Fusion.Engine.Graphics.GIS.GlobeMath;

//namespace Fusion.Engine.Graphics.GIS
//{
//    public class PropsLayer : BuildingsLayer
//    {
        
//        public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
//        {
//            return null;            
//        }

//        [StructLayout(LayoutKind.Explicit)]
//        public struct InstancedDataStruct
//        {
//            [FieldOffset(0)]
//            public Matrix World;
//            [FieldOffset(80)] public uint ID;
//            [FieldOffset(84)] public Vector3 Dummy;

//            public InstancedDataStruct(Matrix world, uint id)
//            {
//                World = world;
//                ID = id;
//                Dummy = Vector3.Zero;
//            }
//        }

//        public static PropsLayer CreateFromFbx(Game engine, double easting, double northing, string region, string modelName)
//        {
//            var scene = engine.Content.Load<Scene>(modelName);

//            double worldLon = 0, worldLat = 0;
//            Gis.UtmToLatLon(easting, northing, region, out worldLon, out worldLat);
//            var zeroLat = worldLat / 180 * Math.PI;
//            var zeroLon = worldLon / 180 * Math.PI;


//            var transforms = new Matrix[scene.Nodes.Count];
//            scene.ComputeAbsoluteTransforms(transforms);

//            List<SceneLayer.ScenePoint> points = new List<SceneLayer.ScenePoint>();
//            List<int> indeces = new List<int>();

//            for (int i = 0; i < scene.Nodes.Count; i++)
//            {

//                var meshIndex = scene.Nodes[i].MeshIndex;
//                var world = transforms[i];

//                var ns = scene.Nodes[i].Name.Split('_');
//                if (meshIndex < 0)
//                {
//                    continue;
//                }                

//                int vertexOffset = points.Count;

//                Gis.UtmToLatLon(easting + world.TranslationVector.X, northing - world.TranslationVector.Z, region,
//                    out worldLon, out worldLat);

//                var worldBasis =
//                    GeoHelper.CalculateBasisOnSurface(DMathUtil.DegreesToRadians(new DVector2(worldLon, worldLat)));
//                var worldBasisInvert = DMatrix.Invert(worldBasis);


//                List<Vector3> cartPoints = new List<Vector3>();

//                foreach (var vert in scene.Meshes[meshIndex].Vertices)
//                {
//                    var pos = vert.Position;

//                    var worldPos = Vector3.TransformCoordinate(pos, world);
//                    var worldNorm = Vector3.TransformNormal(vert.Normal, world);


//                    double lon, lat;
//                    Gis.UtmToLatLon(easting + worldPos.X, northing - worldPos.Y, region, out lon, out lat);

//                    DVector3 norm = new DVector3(worldNorm.X, worldNorm.Y, worldNorm.Z);
//                    norm.Normalize();

//                    norm = DVector3.TransformNormal(norm,
//                        DMatrix.RotationYawPitchRoll(DMathUtil.DegreesToRadians(lon), DMathUtil.DegreesToRadians(lat),
//                            0));
//                    norm.Normalize();

//                    norm.Z = -norm.Z;

//                    lon = DMathUtil.DegreesToRadians(lon) + 0.0000068;
//                    lat = DMathUtil.DegreesToRadians(lat) + 0.0000113;

//                    cartPoints.Add(DVector3
//                        .TransformCoordinate(
//                            GeoHelper.SphericalToCartesian(new DVector2(lon, lat),
//                                GeoHelper.EarthRadius + worldPos.Z / 1000.0f), worldBasisInvert)
//                        .ToVector3());

//                    var point = new SceneLayer.ScenePoint
//                    {
//                        Lon = lon,
//                        Lat = lat,
//                        Color = vert.Color0,
//                        Tex0 = new Vector4(norm.ToVector3(), 0),
//                        Tex1 = new Vector4(0, 0, 0, worldPos.Z / 1000.0f),
//                        ID = 0,

//                    };
//                    //point.Color.Alpha = 0.5f;
//                    points.Add(point);

//                }
                

//                var inds = scene.Meshes[meshIndex].GetIndices();

//                foreach (var ind in inds)
//                {
//                    indeces.Add(vertexOffset + ind);
//                }
//            }
//            return new PropsLayer(engine, points.ToArray(), indeces.ToArray(), new SceneLayer.BuildingData[] { new SceneLayer.BuildingData(), },
//                false)
//            {
//                SceneData = new SceneLayer.ConstData()
//                {
//                }
//            };
//        }

//        public PropsLayer(Game engine, SceneLayer.ScenePoint[] points, int[] indeces, SceneLayer.BuildingData[] buildings, bool isDynamic, int maxInstanceCount = 4096) : base(engine, points, indeces, buildings, isDynamic)
//        {            
//            Flags |= (int) SceneLayer.Flags.INSTANCED;
//            InstancedDataCPU = new InstancedDataStruct[maxInstanceCount];
//            instDataGpu = new StructuredBuffer(engine.GraphicsDevice, typeof(InstancedDataStruct), maxInstanceCount, StructuredBufferFlags.None);
//            SetInstanceBuffer(new List<InstancedDataStruct>() { new InstancedDataStruct(Matrix.Identity, 0) });
//        }

//        public int InstanceCountToDraw;
//        public InstancedDataStruct[] InstancedDataCPU { get; protected set; }

//        StructuredBuffer instDataGpu;

//        public void SetInstanceBuffer(List<InstancedDataStruct> data)
//        {
//            InstanceCountToDraw = data.Count;
//            Array.Copy(data.ToArray(), InstancedDataCPU, data.Count);
//            instDataGpu.SetData(InstancedDataCPU);
//        }

//        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
//        {
//            if (currentBuffer == null || indexBuffer == null)
//            {
//                Log.Warning("Poly layer null reference");
//                return;
//            }

//            Game.GraphicsDevice.PipelineState = factory[Flags];

//            SceneData.AppearanceParams = new Vector2(Settings.StartAppearAnimationPercentage, Settings.EndAppearAnimationPercentage);
//            SceneData.DisappearanceParams = new Vector2(Settings.StartDisappearAnimationPercentage, Settings.EndDisappearAnimationPercentage);
//            sceneBuffer.SetData(SceneData);

//            Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;
//            Game.GraphicsDevice.VertexShaderConstants[1] = sceneBuffer;
//            Game.GraphicsDevice.PixelShaderConstants[1] = sceneBuffer;

//            Game.GraphicsDevice.VertexShaderResources[3] = BuildingsData;
//            Game.GraphicsDevice.PixelShaderResources[3] = BuildingsData;
//            Game.GraphicsDevice.VertexShaderResources[4] = instDataGpu;

//            Game.GraphicsDevice.PixelShaderSamplers[0] = Sampler;
//            Game.GraphicsDevice.PixelShaderSamplers[1] = SamplerState.AnisotropicClamp;

            

//            Game.GraphicsDevice.SetupVertexInput(currentBuffer, indexBuffer);
//            Game.GraphicsDevice.DrawInstancedIndexed(indexBuffer.Capacity, InstanceCountToDraw, 0, 0, 0);
//        }
//    }
//}
