using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Graphics.Display;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
    public class SceneLayer : Gis.GisLayer
    {
        [Flags]
        public enum Flags
        {
            VERTEX_SHADER = 1 << 0,
            PIXEL_SHADER = 1 << 1,
            DRAW_HEAT = 1 << 2,
            INSTANCED = 1 << 4,

            NO_DEPTH = 1 << 10,
            CULL_NONE = 1 << 11,
            XRAY = 1 << 12,
            UV_TRANSPARENCY = 1 << 13,   
            GLASS = 1 << 14,         
        }    

        [StructLayout(LayoutKind.Explicit)]
        public struct ConstData
        {
            [FieldOffset(0)] public float Time;
            [FieldOffset(4)] public Vector3 SunDirection;
            [FieldOffset(16)] public Vector2 AppearanceParams;
            [FieldOffset(24)] public Vector2 DisappearanceParams;
            [FieldOffset(32)] public Matrix WorldMatrix;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct BuildingData
        {
            [FieldOffset(0)]
            public Vector2 BuildingTime; //days from TimeManager start
            [FieldOffset(8)]
            public Vector2 DestroyingTime; //days from TimeManager start
            [FieldOffset(16)]
            public Vector3 Dummy; //height, dummy
            [FieldOffset(28)]
            public uint RenderType;
            [FieldOffset(32)]
            public Color4 ColorMult;
                        
        }

        public BuildingsLayer BuildingsLayer;

        public Dictionary<string, BuildingParams> Buildings;

        public Dictionary<string, InstanceLayer> InstanceLayers = new Dictionary<string, InstanceLayer>();
        public Dictionary<string, List<InstanceLayer.InstancedDataStruct>> InstanceDataByLayer = new Dictionary<string, List<InstanceLayer.InstancedDataStruct>>();

        private Func<string, BuildingParams> buildDataPlaceholderFunc = s =>
        {
            var ns = s.Split('_');
            var b = new BuildingParams()
            {
                Index = ns[0],
            };
            if (ns[0].ToLower() == "ihc")
            {
                b.SceneName = s;
                b.Index = s;
            }
            if (ns.Length > 1 && (ns[1].ToLower() == "tp" || ns[1].ToLower() == "rtp" || ns[1].ToLower() == "och" ||
                ns[1].ToLower() == "kot" || ns[1].ToLower() == "grp" || ns[1].ToLower() == "gas" || ns[1].ToLower() == "kns" || ns[1].ToLower() == "elst"))
            {
                b.SceneName = s;
            }
            return b;
        };
        private Func<BuildingParams, Color> buildingColorFunc = s =>
            Color.White;

        private bool useBaseParams;
        public SceneLayer(Game engine, string[] sceneNames, Dictionary<string, string> instanceFiles, Dictionary<string, BuildingParams> buildings, float easting, float northing, string region, Color color, bool useBaseParams = false, bool getBuildingFromName = false, Func<string, BuildingParams> buildDataPlaceholderFunc = null, Func<BuildingParams, Color> buildingColorFunc = null) : base(engine)
        {
            this.useBaseParams = useBaseParams;
            if (buildDataPlaceholderFunc != null)
            {
                this.buildDataPlaceholderFunc = buildDataPlaceholderFunc;
            }
            if (buildingColorFunc!= null)
            {
                this.buildingColorFunc = buildingColorFunc;
            }
            var s = sceneNames[0].Split('_');            
            Buildings = buildings;
            double worldLon, worldLat;
            Gis.UtmToLatLon(easting, northing, region, out worldLon, out worldLat);

            foreach (var pair in instanceFiles)
            {
                InstanceLayer layer = new InstanceLayer(engine, new DVector2(worldLon + 1 * DMathUtil.RadiansToDegrees(0.0000068) + 0*0.0002, worldLat + 1 * DMathUtil.RadiansToDegrees(0.0000113) - 0*0.00005), pair.Value, 4096);
                InstanceLayers.Add(pair.Key, layer);
                InstanceDataByLayer[pair.Key] = new List<InstanceLayer.InstancedDataStruct>();
            }
            foreach (var b in buildings)
            {
                BuildingsData.Add(new SceneLayer.BuildingData()
                {
                    ColorMult = this.buildingColorFunc(b.Value),
                    BuildingTime = new Vector2(useBaseParams ? b.Value.StartTimeBase : b.Value.StartTime, useBaseParams ? b.Value.EndTimeBase : b.Value.EndTime),
                    DestroyingTime = new Vector2(useBaseParams ? b.Value.DestroyStartTimeBase : b.Value.DestroyStartTime, useBaseParams ? b.Value.DestroyEndTimeBase : b.Value.DestroyEndTime),
                });
                buildingsIds.Add(b.Key, BuildingsData.Count - 1);
            }
            
            

            LoadModels(engine, sceneNames, easting, northing, region, color, getBuildingFromName);
        }

        public class BuildingParams
        {
            public enum BuildingType
            {
                Unidentyfied = 0,
                Hotel = 1,
                Students = 2,
                Sports = 3,
                Transport = 4,
                Parking = 5,
                House = 6,
                Businass = 7,
                ProgressCenter = 8,
                Services = 9,
                Church = 10,
                Hospital = 11,
                Admin = 12,
                Reserved = 13,                               

                MagistralRoad = 201,
                Street = 202,
                TramRail = 203,
                Railroad = 204,
                RoadBasement = 205,
                CustomRoad = 206,
                VeloRoad = 207,
            }

            public string SceneName;

            //geoJSON params
            public string Id;
            public int? Quarter;
            public BuildingType Type;
            public string TypeName;
            public float BuildingTime;
            public int Number;
            public int Year;
            public int Month;
            public int Horizon;
            public string Index = "Not set";
            public int Floors;
            public int[] Citizens;

            public int RoadDistrictNumber;

            //common params            

            //computed params
            public virtual float StartTime { get; set; } = -1;
            public virtual float EndTime { get; set; } = -0.9f;
            public virtual float StartTimeBase { get; set; } = -1;
            public virtual float EndTimeBase { get; set; } = -0.9f;

            public virtual float DestroyStartTime { get; set; } = 365000;
            public virtual float DestroyEndTime { get; set; } = 366000f;
            public virtual float DestroyStartTimeBase { get; set; } = 365000;
            public virtual float DestroyEndTimeBase { get; set; } = 366000f;
        }

        public class SelectInfo
        {
            public BoundingBox BoundingBox;
            public string NodeName;
            public int NodeIndex;
            public int MeshIndex;            
            public DMatrix WorldMatrix;
            public DMatrix WorldMatrixInvert;
            public List<Vector3> Points;
        }

        public ConstData SceneData;

        public List<SelectInfo> OInfo;

        List <BuildingData> BuildingsData = new List<BuildingData>();
        Dictionary<string, int> buildingsIds = new Dictionary<string, int>();

        void LoadModels(Game engine, string[] sceneNames, double easting, double northing, string region, Color color, bool getBuildingFromName = false) {
            List<ScenePoint> points = new List<SceneLayer.ScenePoint>();
            List<int> indeces = new List<int>();

            double worldLon, worldLat;
            Gis.UtmToLatLon(easting, northing, region, out worldLon, out worldLat);

            SceneData = new ConstData();

            SceneData.WorldMatrix =
                DMatrix.ToFloatMatrix(
                    GeoHelper.CalculateBasisOnSurface(new DVector2(worldLon, worldLat) * Math.PI / 180));

            OInfo = new List<SelectInfo>();

            Random random = new Random();

            foreach (string sceneName in sceneNames) {
                var scene = engine.Content.Load<Scene>(sceneName);
                var s = sceneName.Split('_');
                //double easting = Double.Parse(s[1].Replace(',', '.'));
                //double northing = Double.Parse(s[2].Replace(',', '.'));
                //string region = s[3];                

                var transforms = new Matrix[scene.Nodes.Count];
                scene.ComputeAbsoluteTransforms(transforms);
                
                for (int i = 0; i < scene.Nodes.Count; i++)
                {

                    var meshIndex = scene.Nodes[i].MeshIndex;
                    var world = transforms[i];

                    var ns = scene.Nodes[i].Name.Split('_').ToList();
                    var buildingName = getBuildingFromName ? sceneName : ns[0];
                    if (!Buildings.ContainsKey(buildingName))
                    {
                        var b = buildDataPlaceholderFunc(scene.Nodes[i].Name);
                        if (b.Index != null)
                        {
                            if (!getBuildingFromName) buildingName = b.Index;
                            if (!Buildings.ContainsKey(buildingName))
                            {
                                Buildings.Add(buildingName, b);
                                var bdt = new SceneLayer.BuildingData()
                                {
                                    ColorMult = buildingColorFunc(b),
                                    BuildingTime = new Vector2(useBaseParams ? b.StartTimeBase : b.StartTime, useBaseParams ? b.EndTimeBase : b.EndTime),
                                    DestroyingTime = new Vector2(useBaseParams ? b.DestroyStartTimeBase : b.DestroyStartTime, useBaseParams ? b.DestroyEndTimeBase: b.DestroyEndTime),
                                };
                                BuildingsData.Add(bdt);
                                buildingsIds.Add(buildingName, BuildingsData.Count - 1);
                            }
                        }
                    }

                    if (ns.Count > 1 && (ns[1].ToLower() == "tp" || ns[1].ToLower() == "rtp" || ns[1].ToLower() == "och" ||
                        ns[1].ToLower() == "kot" || ns[1].ToLower() == "grp" || ns[1].ToLower() == "gas" || ns[1].ToLower() == "kns" || ns[1].ToLower() == "elst"))
                    {
                        Buildings[buildingName].SceneName = scene.Nodes[i].Name;
                    }
                    if (!buildingsIds.ContainsKey(buildingName))
                    {
                        BuildingsData.Add(new SceneLayer.BuildingData()
                        {
                            ColorMult = Color.White,
                            BuildingTime = new Vector2(0),
                            DestroyingTime = new Vector2(float.MaxValue),
                        });
                        buildingsIds[buildingName] = BuildingsData.Count - 1;
                    }
                    

                    if (meshIndex < 0)
                    {
                        

                        if (ns.Count >= 2)
                        {
                            if (ns[1] == "house") { ns.Add("house" + random.Next(0, 2));                                
                            }
                            if (ns[1] == "townhouse")
                            {
                                ns.Add("townhouse");                                
                            }                            
                        }

                        if (ns.Count <= 2) continue;

                        if (InstanceLayers.ContainsKey(ns[2]))
                        {
                            float maxDelta = 0.1f;
                            float d1 = random.NextFloat(0, +maxDelta),
                                  d2 = random.NextFloat(-maxDelta, 0);
                            InstanceDataByLayer[ns[2]]
                                .Add(new InstanceLayer.InstancedDataStruct(
                                    world
                                    //scene.Nodes[i].Transform
                                    * Matrix.RotationAxis(Vector3.ForwardLH, MathUtil.DegreesToRadians(2.4f)) 
                                    //* scene.Nodes[scene.Nodes[i].ParentIndex].Transform
                                    , (uint) buildingsIds[buildingName], new Vector2(d1, d2)));
                        }
                        #region House hitboxes
                        if (ns[1] == "house" || ns[1] == "townhouse")
                        {
                            var iinfo = new SelectInfo
                            {
                                MeshIndex = meshIndex,
                                NodeIndex = i,
                                NodeName = buildingName, //scene.Nodes[i].Name
                                WorldMatrix = DMatrix.FromFloatMatrix(world),
                            };

                            var cPoints = new List<Vector3>();
                            var layer = InstanceLayers[ns[2]];
                            var mTransforms = new Matrix[layer.Model.Nodes.Count];
                            InstanceLayers[ns[2]].Model.ComputeAbsoluteTransforms(mTransforms);

                            for (int m = 0; m < layer.Model.Nodes.Count; m++)
                            {
                                if (layer.Model.Nodes[m].MeshIndex < 0) continue;
                                var modelMesh = InstanceLayers[ns[2]].Model.Meshes[layer.Model.Nodes[m].MeshIndex];
                                var mWorld = mTransforms[m] * world; 
                                Gis.UtmToLatLon(easting + mWorld.TranslationVector.X, northing - mWorld.TranslationVector.Z, region,
                                    out worldLon, out worldLat);

                                var mWorldBasis =
                                    GeoHelper.CalculateBasisOnSurface(DMathUtil.DegreesToRadians(new DVector2(worldLon, worldLat)));
                                iinfo.WorldMatrix = mWorldBasis;                                
                                var mWorldBasisInvert = DMatrix.Invert(mWorldBasis);
                                iinfo.WorldMatrixInvert = mWorldBasisInvert;
                                foreach (var vert in modelMesh.Vertices)
                                {
                                    

                                    var pos = new Vector3(vert.Position.X, -vert.Position.Z, vert.Position.Y);
                                    var worldPos = Vector3.TransformCoordinate(pos, mWorld);
                                    double lon, lat;
                                    Gis.UtmToLatLon(easting + worldPos.X, northing + worldPos.Y, region, out lon, out lat);

                                    lon = DMathUtil.DegreesToRadians(lon) + 0.0000068;
                                    lat = DMathUtil.DegreesToRadians(lat) + 0.0000113;

                                    cPoints.Add(DVector3
                                        .TransformCoordinate(
                                            GeoHelper.SphericalToCartesian(new DVector2(lon, lat),
                                                GeoHelper.EarthRadius + worldPos.Z / 1000.0), mWorldBasisInvert)
                                        .ToVector3());                                    
                                }                                                                
                            }
                            iinfo.BoundingBox = BoundingBox.FromPoints(cPoints.ToArray());
                            //if (Gis.Debug != null) Gis.Debug.DrawBoundingBox(iinfo.BoundingBox, iinfo.WorldMatrix);
                            iinfo.Points = cPoints;
                            OInfo.Add(iinfo);
                        }
                        #endregion
                        continue;
                    }
                    var info = new SelectInfo
                    {
                        MeshIndex = meshIndex,
                        NodeIndex = i,
                        NodeName = buildingName, //scene.Nodes[i].Name
                    };
                    OInfo.Add(info);


                    int vertexOffset = points.Count;

                    Gis.UtmToLatLon(easting + world.TranslationVector.X, northing - world.TranslationVector.Z, region,
                        out worldLon, out worldLat);

                    var worldBasis =
                        GeoHelper.CalculateBasisOnSurface(DMathUtil.DegreesToRadians(new DVector2(worldLon, worldLat)));
                    var worldBasisInvert = DMatrix.Invert(worldBasis);

                    info.WorldMatrix = worldBasis;
                    info.WorldMatrixInvert = worldBasisInvert;

                    List<Vector3> cartPoints = new List<Vector3>();

                    var bd = BuildingsData[buildingsIds[buildingName]];
                    bd.Dummy.X = scene.Meshes[meshIndex].Vertices.Max(vert =>
                    {
                        var pos = vert.Position;

                        var worldPos = Vector3.TransformCoordinate(pos, world);
                        var worldNorm = Vector3.TransformNormal(vert.Normal, world);
                        return worldPos.Z / 1000.0f + 0.001f;
                    });
                    bd.Dummy.Y = scene.Meshes[meshIndex].Vertices.Min(vert =>
                    {
                        var pos = vert.Position;

                        var worldPos = Vector3.TransformCoordinate(pos, world);
                        var worldNorm = Vector3.TransformNormal(vert.Normal, world);
                        return worldPos.Z / 1000.0f + 0.001f;
                    });
                    //var delta = scene.Meshes[meshIndex].Vertices.Max(v => v.Position.Z / 1000) - scene.Meshes[meshIndex].Vertices.Min(v => v.Position.Z / 1000);
                    BuildingsData[buildingsIds[buildingName]] = bd;

                    foreach (var vert in scene.Meshes[meshIndex].Vertices)
                    {
                        var pos = vert.Position;

                        var worldPos = Vector3.TransformCoordinate(pos, world);
                        var worldNorm = Vector3.TransformNormal(vert.Normal, world);


                        double lon, lat;
                        Gis.UtmToLatLon(easting + worldPos.X, northing + worldPos.Y, region, out lon, out lat);

                        DVector3 norm = new DVector3(worldNorm.X, worldNorm.Y, worldNorm.Z);
                        norm.Normalize();

                        norm = DVector3.TransformNormal(norm,
                            DMatrix.RotationYawPitchRoll(DMathUtil.DegreesToRadians(lon),
                                DMathUtil.DegreesToRadians(lat),
                                0));
                        norm.Normalize();
                        norm.Y = -norm.Y;

                        lon = DMathUtil.DegreesToRadians(lon) + 0.0000068;
                        lat = DMathUtil.DegreesToRadians(lat) + 0.0000113;

                        cartPoints.Add(DVector3
                            .TransformCoordinate(
                                GeoHelper.SphericalToCartesian(new DVector2(lon, lat),
                                    GeoHelper.EarthRadius + worldPos.Z / 1000.0), worldBasisInvert)
                            .ToVector3());

                        var point = new SceneLayer.ScenePoint
                        {
                            Lon = lon,
                            Lat = lat,
                            Color = vert.Color0 * color,
                            Tex0 = new Vector4(norm.ToVector3(), 0),
                            Tex1 = new Vector4(vert.TexCoord0, 0, (float)(worldPos.Z / 1000.0 + (bd.Dummy.X - bd.Dummy.Y < 0.0001 ? 0.0002 : -0.0002))),
                            ID = (uint) buildingsIds[buildingName],

                        };
                        if (ns.Count > 1 && ns[1].ToLower() == "glass")
                            point.Color.Alpha = 0.5f;
                        points.Add(point);

                    }

                    info.BoundingBox = BoundingBox.FromPoints(cartPoints.ToArray());
                   // if (Gis.Debug != null) Gis.Debug.DrawBoundingBox(info.BoundingBox, info.WorldMatrix);
                    info.Points = cartPoints;
                    var inds = scene.Meshes[meshIndex].GetIndices();

                    foreach (var ind in inds)
                    {
                        indeces.Add(vertexOffset + ind);
                    }
                }
            }
            BuildingsLayer = new BuildingsLayer(engine, points.ToArray(), indeces.ToArray(), BuildingsData.ToArray(), false);            
            BuildingsLayer.SceneData = SceneData;
            BuildingsLayer.Settings.StartDisappearAnimationPercentage = 1.0f;
            BuildingsLayer.Settings.EndDisappearAnimationPercentage = 1.1f;
            foreach (var instanceName in InstanceLayers.Keys)
            {
                //InstanceLayers[instanceName].SetInstanceBuffer(InstanceDataByLayer[instanceName]);
                //InstanceLayers[instanceName].InstancedDataCPU = InstanceDataByLayer[instanceName].ToArray();
                Array.Copy(InstanceDataByLayer[instanceName].ToArray(), InstanceLayers[instanceName].InstancedDataCPU, InstanceDataByLayer[instanceName].Count);
                InstanceLayers[instanceName].InstancedCountToDraw = InstanceDataByLayer[instanceName].Count;

                InstanceLayers[instanceName].SetBuildingsData(BuildingsData.ToArray());
                InstanceLayers[instanceName].SceneData = SceneData;
            }
        }

        public void SetBuildingsData(Dictionary<string, BuildingParams> buildings)
        {
            foreach (var kv in buildingsIds)
            {
                if (buildings.ContainsKey(kv.Key))
                {
                    var b = buildings[kv.Key];
                    var bd = BuildingsData[buildingsIds[kv.Key]];
                    bd.ColorMult = this.buildingColorFunc(b);
                    bd.BuildingTime = new Vector2(useBaseParams ? b.StartTimeBase : b.StartTime,
                        useBaseParams ? b.EndTimeBase : b.EndTime);
                    bd.DestroyingTime = new Vector2(
                        useBaseParams ? b.DestroyStartTimeBase : b.DestroyStartTime,
                        useBaseParams ? b.DestroyEndTimeBase : b.DestroyEndTime);
                    BuildingsData[buildingsIds[kv.Key]] = bd;
                }
                else
                {
                    var bd = BuildingsData[buildingsIds[kv.Key]];
                    bd.ColorMult = Color.White;
                    bd.BuildingTime = new Vector2(0);
                    bd.DestroyingTime = new Vector2(float.MaxValue);
                }
            }
            foreach (var b in buildings)
            {
                Buildings[b.Key] = b.Value;
                if (!buildingsIds.ContainsKey(b.Key))
                {
                    Log.Warning("Building not present in Model");
                    BuildingsData.Add(new SceneLayer.BuildingData()
                    {
                        ColorMult = this.buildingColorFunc(b.Value),
                        BuildingTime = new Vector2(useBaseParams ? b.Value.StartTimeBase : b.Value.StartTime,
                            useBaseParams ? b.Value.EndTimeBase : b.Value.EndTime),
                        DestroyingTime = new Vector2(
                            useBaseParams ? b.Value.DestroyStartTimeBase : b.Value.DestroyStartTime,
                            useBaseParams ? b.Value.DestroyEndTimeBase : b.Value.DestroyEndTime),                        
                    });
                    buildingsIds.Add(b.Key, BuildingsData.Count - 1);
                }                              
            }

            BuildingsLayer.UpdateBuildingsData(BuildingsData.ToArray());
            foreach (var instanceName in InstanceLayers.Keys)
            {
                InstanceLayers[instanceName].SetBuildingsData(BuildingsData.ToArray());
            }            
        }

        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
            if (IsActive)
            {
                foreach (var layer in InstanceLayers.Values)
                {
                    layer.Draw(gameTime, constBuffer);
                }
                BuildingsLayer.Draw(gameTime, constBuffer);
            }
        }

        public void Recolor(Func<BuildingParams, Color> buildingColorFunc)
        {
            this.buildingColorFunc = buildingColorFunc;
            foreach (var kv in buildingsIds)
            {
                var v = BuildingsData[kv.Value];
                v.ColorMult = buildingColorFunc(Buildings[kv.Key]);
                BuildingsData[kv.Value] = v;
            }            
            BuildingsLayer.UpdateBuildingsData(BuildingsData.ToArray());
            foreach (var layer in InstanceLayers)
            {
                layer.Value.UpdateBuildingsData(BuildingsData.ToArray());
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;
            base.Update(gameTime);
            
            //sunPos = Vector3.TransformCoordinate(sunPos, Matrix.RotationX((float)(gameTime.Total.TotalMilliseconds / 10000 * Math.PI)));            
            //SceneData.Time = (uint) (gameTime.Total.TotalMilliseconds) % 40000;
            //Vector3 sunPos = Vector3.Transform(new Vector3(0, 1, 0), Matrix.RotationAxis(new Vector3(1, 0, 0), (float)((float)SceneData.Time / 100 * Math.PI * 2))).ToVector3();
            SceneData.SunDirection = new Vector3(1, 1, 0);
            BuildingsLayer.SceneData = SceneData;
            BuildingsLayer.Update(gameTime);
            foreach (var layer in InstanceLayers.Values)
            {
                layer.SceneData = SceneData;
                layer.Update(gameTime);
            }
        }

        public struct ScenePoint
        {
            [Vertex("TEXCOORD", 0)]
            public double Lon;
            [Vertex("TEXCOORD", 1)]
            public double Lat;
            [Vertex("TEXCOORD", 2)]
            public Vector4 Tex0;
            [Vertex("TEXCOORD", 3)]
            public Vector4 Tex1;
            [Vertex("Color")]
            public Color4 Color;
            [Vertex("StructID")]
            public uint ID;
        }

        public class SelectedItem : Gis.SelectedItem
        {
            public BoundingBox BoundingBox;
            public List<DVector3> Points;
            public BuildingParams BuildingStats;
            public DMatrix WorldMatrix;
        }

        public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
        {            
            var selectedList = new List<Gis.SelectedItem>();
            if (!IsActive || !IsVisible) return selectedList;
            foreach (var info in OInfo)
            {
                var localNearPoint = DVector3.TransformCoordinate(nearPoint, info.WorldMatrixInvert);
                var localFarPoint = DVector3.TransformCoordinate(farPoint, info.WorldMatrixInvert);

                var ray = new Ray(localNearPoint.ToVector3(),
                    DVector3.Normalize(localFarPoint - localNearPoint).ToVector3());

                float distance;
                if (info.BoundingBox.Intersects(ref ray, out distance))
                {
                    Log.Message(info.NodeName);

                    selectedList.Add(new SelectedItem
                    {
                        Distance = distance,
                        Name = info.NodeName,
                        BoundingBox = info.BoundingBox,
                        Points = info.Points.Select(a =>
                        {
                            var v4 = DVector3.Transform(new DVector3(a), info.WorldMatrix);
                            return new DVector3(v4.X, v4.Y, v4.Z);
                        }).ToList(),                        
                        BuildingStats = Buildings.ContainsKey(info.NodeName) ? Buildings[info.NodeName] : null,
                        WorldMatrix = info.WorldMatrix,
                    });

                   // if (Gis.Debug != null) Gis.Debug.DrawBoundingBox(info.BoundingBox, info.WorldMatrix);
                }
            }
            return selectedList;
        }

        public List<Gis.SelectedItem> Select(string buildingName)
        {
            var selectedList = new List<Gis.SelectedItem>();
            if (!IsActive || !IsVisible) return selectedList;
            foreach (var info in OInfo)
            {                
                if (info.NodeName == buildingName)
                {
                    Console.WriteLine(info.NodeName);

                    selectedList.Add(new SelectedItem
                    {
                        Distance = 0,
                        Name = info.NodeName,
                        BoundingBox = info.BoundingBox,
                        Points = info.Points.Select(a =>
                        {
                            var v4 = DVector3.Transform(new DVector3(a), info.WorldMatrix);
                            return new DVector3(v4.X, v4.Y, v4.Z);
                        }).ToList(),
                        BuildingStats = Buildings.ContainsKey(info.NodeName) ? Buildings[info.NodeName] : null,
                        WorldMatrix = info.WorldMatrix,
                    });
                }
            }            
            return selectedList;
        }

        public void UpdateSelection(string selectedName)
        {
            foreach (var kv in buildingsIds)
            {
                var bd = BuildingsData[kv.Value];
                if (selectedName == null)
                {
                    bd.RenderType = 0;
                }
                else
                {
                    if (kv.Key == selectedName) { bd.RenderType = 2;}
                    else { bd.RenderType = 1;}
                }
                BuildingsData[kv.Value] = bd;
            }
            BuildingsLayer.UpdateBuildingsData(BuildingsData.ToArray());
            foreach (var layer in InstanceLayers)
            {
                layer.Value.UpdateBuildingsData(BuildingsData.ToArray());
            }
        }
    }
}
