using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using SharpDX.MediaFoundation;

namespace Fusion.Engine.Graphics.GIS
{
    public class HeightmapBatch : Gis.GisLayer
    {
        public HeightmapBatch(Game engine) : base(engine)
        {
            cB = new ConstantBuffer(Game.GraphicsDevice, typeof(MCBatch.ConstData));
            parameters.Color = new Vector4(0.25f, 0.25f, 0.25f, 1);
            shader = Game.Content.Load<Ubershader>("globe.Heightmap.hlsl"); 
            factory = shader.CreateFactory(typeof(FieldFlags), EnumFunc);            
        }

        public float Width
        {
            get => parameters.FieldSize.X;
            set => parameters.FieldSize.X = value;
        }

        public float Height
        {
            get => parameters.FieldSize.Y;
            set => parameters.FieldSize.Y = value;
        }

        public Vector4 Forward
        {
            get => parameters.Forward;
            set => parameters.Forward = value;
        }

        public Vector4 Right
        {
            get => parameters.Right;
            set => parameters.Right = value;
        }

        public int Size
        {
            get => (int)parameters.dimX;
            set => parameters.dimX = parameters.dimY = (uint)value;
        }
        public Texture2D Data;

        public DVector2 LonLat
        {
            get => new DVector2(parameters.Lon, parameters.Lat);
            set
            {
                parameters.Lon = value.X;
                parameters.Lat = value.Y;
            }
        }

        Ubershader shader;
        StateFactory factory;

        public bool isInit = false;

        [Flags]
        public enum FieldFlags : int
        {
            None = 0,
            DrawLocal = 1 << 0,
        }     

        SamplerState Sampler = SamplerState.LinearClamp;

        void EnumFunc(PipelineState ps, int flag)
        {
            var flags = (FieldFlags)flag;

            ps.VertexInputElements = null;

            ps.BlendState = BlendState.AlphaBlend;
            ps.DepthStencilState = DepthStencilState.Default;
            ps.RasterizerState = RasterizerState.CullNone;

            ps.Primitive = Primitive.PointList;
        }

        protected MCBatch.ConstData parameters;

        public float ScaleMult
        {
            get => parameters.IsolineValue;
            set => parameters.IsolineValue = value;
        }
        protected ConstantBuffer cB;


        public override void Dispose()
        {
            factory.Dispose();        

            Data.Dispose();

            cB.Dispose();

            base.Dispose();
        }

        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
            if (!isInit) return;           
            cB.SetData(parameters);
            Game.GraphicsDevice.GeometryShaderConstants[0] = constBuffer;
            Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;     
            Game.GraphicsDevice.VertexShaderConstants[1] = cB;
            Game.GraphicsDevice.GeometryShaderConstants[1] = cB;
            Game.GraphicsDevice.PixelShaderConstants[1] = cB;

            Game.GraphicsDevice.GeometryShaderSamplers[0] = Sampler;

            Game.GraphicsDevice.GeometryShaderResources[0] = Data;

            var flags = FieldFlags.DrawLocal;

            Game.GraphicsDevice.PipelineState = factory[(int)flags];

            Game.GraphicsDevice.SetupVertexInput(null, null);

            Game.GraphicsDevice.Draw((Size - 1) * (Size - 1), 0);    
            
        }

        
    }
}
