using System;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics.GIS
{
    public class ScalarFieldSlice : Gis.GisLayer
    {
        private readonly StateFactory _factory;

        [Flags]
        public enum PointFlags
        {
            DRAW_TEXTURED_POLY = 1 << 0,
            POINT_FADING = 1 << 1,
        }

        private struct ConstData
        {
            public float Min;
            public float Max;
            public float Padding1;
            public float Padding2;
        }

        public int Flags;

        private Gis.CartPoint[] _pointsCpu;
        private VertexBuffer _currentBuffer;
        private IndexBuffer _indicesBuffer;

        public Texture2D Palette;
        private readonly Ubershader _shader;
        private readonly ConstantBuffer _minMaxValueBuffer;

        public ScalarFieldSlice(Game engine) : base(engine)
        {
            _shader = Game.Content.Load<Ubershader>("globe.SFieldSlice.hlsl");
            _factory = _shader.CreateFactory(
                typeof(PointFlags), 
                Primitive.TriangleList, 
                VertexInputElement.FromStructure<Gis.CartPoint>(), 
                BlendState.AlphaBlend, 
                RasterizerState.CullNone, 
                DepthStencilState.None
            );

            Palette = Game.Content.Load<Texture2D>("pallete");

            _minMaxValueBuffer = new ConstantBuffer(Game.GraphicsDevice, typeof(ConstData));
        }

        public void SetPoints(Gis.CartPoint[] newPoints, int[] indices, float minValue, float maxValue)
        {
            _currentBuffer?.Dispose();
            _indicesBuffer?.Dispose();

            _pointsCpu = newPoints;
            _currentBuffer = new VertexBuffer(Game.GraphicsDevice, typeof(Gis.CartPoint), newPoints.Length);
            _currentBuffer.SetData(newPoints);

            _indicesBuffer = new IndexBuffer(Game.GraphicsDevice, indices.Length);
            _indicesBuffer.SetData(indices);

            _minMaxValueBuffer.SetData(new ConstData { Min = minValue, Max = maxValue });
        }

        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
            if (_pointsCpu == null) return;

            Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;
            Game.GraphicsDevice.VertexShaderConstants[1] = _minMaxValueBuffer;
            Game.GraphicsDevice.PipelineState = _factory[(int) PointFlags.DRAW_TEXTURED_POLY];

            Game.GraphicsDevice.PixelShaderResources[0] = Palette;
            Game.GraphicsDevice.PixelShaderSamplers[0] = SamplerState.LinearClamp;            

            Game.GraphicsDevice.SetupVertexInput(_currentBuffer, _indicesBuffer);
            Game.GraphicsDevice.DrawIndexed(_indicesBuffer.Capacity, 0, 0);
        }

        public override void Dispose()
        {
            _shader?.Dispose();
            Palette?.Dispose();

            _factory?.Dispose();

            _currentBuffer?.Dispose();
            _indicesBuffer?.Dispose();

            base.Dispose();
        }
    }
}
