using System;
using System.Collections.Generic;
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
            public float TimeFracture;
            public float Dummy;
        }

        public int Flags;

        private Gis.CartPoint[] _pointsCpu;
        private int[] _indices;
        private List<float[]> _values = new List<float[]>();
        private int _currentTime = 0;
       
        private VertexBuffer _pointsBuffer;
        private StructuredBuffer _prevBuffer;
        private StructuredBuffer _nextBuffer;

        private IndexBuffer _indicesBuffer;

        public Texture2D Palette;
        private readonly Ubershader _shader;
        private readonly ConstantBuffer _minMaxValueTimeFractureBuffer;
        private ConstData _constData;

        public ScalarFieldSlice(Game engine, string palette, float minValue, float maxValue) : base(engine)
        {
            _shader = Game.Content.Load<Ubershader>("globe.SFieldSlice.hlsl");
            _factory = _shader.CreateFactory(
                typeof(PointFlags), 
                Primitive.TriangleList, 
                VertexInputElement.FromStructure<Gis.CartPoint>(), 
                BlendState.AlphaBlend, 
                RasterizerState.CullNone, 
                DepthStencilState.Default
            );
            _minMaxValueTimeFractureBuffer = new ConstantBuffer(Game.GraphicsDevice, typeof(ConstData));
            _constData = new ConstData();

            SetPalette(palette);
            SetMinMax(minValue, maxValue);
        }

        public void SetMinMax(float minValue, float maxValue)
        {
            _constData.Min = minValue;
            _constData.Max = maxValue;
            _minMaxValueTimeFractureBuffer.SetData(_constData);
        }

        public void SetPalette(string palette)
        {
            Palette = Game.Content.Load<Texture2D>(palette);
        }

        public void SetPoints(Gis.CartPoint[] newPoints, int[] indices, List<float[]> valuesInTime)
        {
            _pointsCpu = newPoints;
            _indices = indices;
            _values = valuesInTime;

            UpdateValueBuffers(_currentTime);

            _pointsBuffer?.Dispose();
            _indicesBuffer?.Dispose();

            if (_pointsCpu == null)
                return;            

            _pointsBuffer = new VertexBuffer(Game.GraphicsDevice, typeof(Gis.CartPoint), _pointsCpu.Length);
            _pointsBuffer.SetData(_pointsCpu);

            _indicesBuffer = new IndexBuffer(Game.GraphicsDevice, _indices.Length);
            _indicesBuffer.SetData(_indices);           
        }

        public void SetTime(int timeIndex, float timeFracture)
        {            
            _constData.TimeFracture = timeFracture;
            _minMaxValueTimeFractureBuffer.SetData(_constData);

            if(timeIndex != _currentTime)
                UpdateValueBuffers(_currentTime);

            _currentTime = timeIndex;
        }

        private void UpdateValueBuffers(int timeIndex)
        {
            _prevBuffer?.Dispose();
            _nextBuffer?.Dispose();

            if (_values.Count == 0)
                return;

            _prevBuffer = new StructuredBuffer(Game.Instance.GraphicsDevice, typeof(float), _values[timeIndex].Length, StructuredBufferFlags.None);
            _prevBuffer.SetData(_values[timeIndex]);

            _nextBuffer = new StructuredBuffer(Game.Instance.GraphicsDevice, typeof(float), _values[timeIndex].Length, StructuredBufferFlags.None);
            _nextBuffer.SetData(_values[timeIndex + 1 == _values.Count ? timeIndex : timeIndex + 1]);
        }

        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
            if (_pointsBuffer == null || _indicesBuffer == null) return;

            Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;
            Game.GraphicsDevice.VertexShaderConstants[1] = _minMaxValueTimeFractureBuffer;
            Game.GraphicsDevice.PipelineState = _factory[(int) PointFlags.DRAW_TEXTURED_POLY];

            Game.GraphicsDevice.PixelShaderResources[0] = Palette;
            Game.GraphicsDevice.PixelShaderSamplers[0] = SamplerState.LinearClamp;
            Game.GraphicsDevice.VertexShaderResources[1] = _prevBuffer;
            Game.GraphicsDevice.VertexShaderResources[2] = _nextBuffer;

            Game.GraphicsDevice.SetupVertexInput(_pointsBuffer, _indicesBuffer);
            Game.GraphicsDevice.DrawIndexed(_indicesBuffer.Capacity, 0, 0);
        }

        public override void Dispose()
        {
            _shader?.Dispose();
            Palette?.Dispose();

            _factory?.Dispose();

            _pointsBuffer?.Dispose();
            _indicesBuffer?.Dispose();

            base.Dispose();
        }
    }
}
