using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class BuildingsLayer : PolyGisLayer
    {

        public class LayerParams
        {
            public float StartAppearAnimationPercentage { get; set; }= 0.1f;
            public float EndAppearAnimationPercentage { get; set; } = 0.4f;
            public float StartDisappearAnimationPercentage { get; set; } = 0.8f;
            public float EndDisappearAnimationPercentage { get; set; } = 1.0f;

        }

        public LayerParams Settings = new LayerParams();

        protected BuildingsLayer(Game engine) : base(engine)
        {
        }

        public SceneLayer.ScenePoint[]	PointsCpu	{ get; protected set; }
	    public int[]					Indeces		{ get; protected set; }

		public SceneLayer.ConstData SceneData;
        protected ConstantBuffer sceneBuffer;

        public StructuredBuffer BuildingsData;

        public BuildingsLayer(Game engine, SceneLayer.ScenePoint[] points, int[] indeces, SceneLayer.BuildingData[] buildings, bool isDynamic) : base(engine)
        {
            //Console.WriteLine(points.Length + " _ " + indeces.Length);
            Initialize(points, indeces, isDynamic);            
            SetBuildingsData(buildings);
            Flags = (int) (SceneLayer.Flags.VERTEX_SHADER | SceneLayer.Flags.PIXEL_SHADER | SceneLayer.Flags.CULL_NONE);
            sceneBuffer = new ConstantBuffer(engine.GraphicsDevice, typeof(SceneLayer.ConstData));            
        }


        protected void EnumFunc(PipelineState ps, int flag)
        {
            var flags = (SceneLayer.Flags) flag;

            ps.VertexInputElements = VertexInputElement.FromStructure<SceneLayer.ScenePoint>();
            ps.BlendState = BlendState.AlphaBlend;
            ps.DepthStencilState = flags.HasFlag(SceneLayer.Flags.NO_DEPTH)
                ? DepthStencilState.None
                : (flags.HasFlag(SceneLayer.Flags.GLASS) ? DepthStencilState.Readonly : DepthStencilState.Default);
            ps.RasterizerState = flags.HasFlag(SceneLayer.Flags.CULL_NONE) ? RasterizerState.CullNone : RasterizerState.CullCW;

            ps.Primitive = Primitive.TriangleList;
        }

        public void SetBuildingsData(SceneLayer.BuildingData[] buildings)
        {
            BuildingsData = new StructuredBuffer(Game.GraphicsDevice, typeof(SceneLayer.BuildingData), buildings.Length, StructuredBufferFlags.None);
            BuildingsData.SetData(buildings);
        }

        public void UpdateBuildingsData(SceneLayer.BuildingData[] buildings)
        {
            BuildingsData.SetData(buildings, 0, Math.Min(buildings.Length, BuildingsData.StructureCapacity));
        }

        protected virtual void Initialize(SceneLayer.ScenePoint[] points, int[] indeces, bool isDynamic)
        {
            shader = Game.Content.Load<Ubershader>("globe.Scene.hlsl");
            factory = shader.CreateFactory(typeof(SceneLayer.Flags), EnumFunc);

            var vbOptions = isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;

            firstBuffer = new VertexBuffer(Game.GraphicsDevice, typeof(SceneLayer.ScenePoint), points.Length, vbOptions);
            firstBuffer.SetData(points);
            currentBuffer = firstBuffer;            

            indexBuffer = new IndexBuffer(Game.Instance.GraphicsDevice, indeces.Length);
            indexBuffer.SetData(indeces);

            PointsCpu = points;

            cb = new ConstantBuffer(Game.GraphicsDevice, typeof(ConstData));
            constData = new ConstData();
            constData.Data = Vector4.One;

	        Indeces = indeces;
        }


        public override void Dispose()
        {
            //shader.Dispose();	
            factory.Dispose();

            firstBuffer.Dispose();
            indexBuffer.Dispose();

            cb.Dispose();

            base.Dispose();
        }


        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
            if (currentBuffer == null || indexBuffer == null)
            {
                Log.Warning("Poly layer null reference");
                return;
            }
            
            Game.GraphicsDevice.PipelineState = factory[Flags];

            SceneData.AppearanceParams = new Vector2(Settings.StartAppearAnimationPercentage, Settings.EndAppearAnimationPercentage);
            SceneData.DisappearanceParams = new Vector2(Settings.StartDisappearAnimationPercentage, Settings.EndDisappearAnimationPercentage);
            sceneBuffer.SetData(SceneData);

            Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;
            Game.GraphicsDevice.VertexShaderConstants[1] = sceneBuffer;
            Game.GraphicsDevice.PixelShaderConstants[1] = sceneBuffer;

            Game.GraphicsDevice.VertexShaderResources[3] = BuildingsData;
            Game.GraphicsDevice.PixelShaderResources[3] = BuildingsData;

            Game.GraphicsDevice.PixelShaderSamplers[0] = Sampler;
            Game.GraphicsDevice.PixelShaderSamplers[1] = SamplerState.AnisotropicClamp;

            Game.GraphicsDevice.SetupVertexInput(currentBuffer, indexBuffer);
            Game.GraphicsDevice.DrawIndexed(indexBuffer.Capacity, 0, 0);

            //game.GraphicsDevice.ResetStates();
        }


        public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
        {
            var slelectedList = new List<Gis.SelectedItem>();

            foreach (var info in ObjectsInfo)
            {
                var localNearPoint = DVector3.TransformCoordinate(nearPoint, info.WorldMatrixInvert);
                var localFarPoint = DVector3.TransformCoordinate(farPoint, info.WorldMatrixInvert);

                var ray = new Ray(localNearPoint.ToVector3(),
                    DVector3.Normalize(localFarPoint - localNearPoint).ToVector3());

                float distance;
                if (info.BoundingBox.Intersects(ref ray, out distance))
                {
                    Console.WriteLine(info.NodeName);

                    slelectedList.Add(new SelectedItem
                    {
                        Distance = distance,
                        Name = info.NodeName,
                        NodeIndex = info.NodeIndex,
                        BoundingBox = info.BoundingBox
                    });

                    if (Gis.Debug != null) Gis.Debug.DrawBoundingBox(info.BoundingBox, info.WorldMatrix);
                }
            }

            return slelectedList;
        }
    }
}
