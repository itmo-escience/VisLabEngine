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
            DRAW_SLICE = 1 << 0,
            DRAW_BORDER = 1 << 1,
        }

        private struct ConstData
        {
            public float Min;
            public float Max;
            public float TimeFracture;
            public float Dummy;
        }

        public int Flags;

        private Gis.CartPoint[] _slicePoints;
        private Gis.CartPoint[] _borderPoints;
        private int[] _indices;
        private int[] _borderIndices;
        private List<float[]> _values = new List<float[]>();
        private int _currentTime;
       
        private VertexBuffer _pointsBuffer;
        private VertexBuffer _borderPointsBuffer;

        private StructuredBuffer _prevBuffer;
        private StructuredBuffer _nextBuffer;

        private IndexBuffer _indicesBuffer;
        private IndexBuffer _borderIndicesBuffer;

        public Texture2D Palette;
        private readonly Ubershader _shader;
        private readonly ConstantBuffer _minMaxValueTimeFractureBuffer;
        private ConstData _constData;

        public ScalarFieldSlice(Game engine, string palette, float minValue, float maxValue) : base(engine)
        {
            _shader = _game.Content.Load<Ubershader>("globe.SFieldSlice.hlsl");
            _factory = _shader.CreateFactory(
                typeof(PointFlags), 
                Primitive.TriangleList, 
                VertexInputElement.FromStructure<Gis.CartPoint>(), 
                BlendState.AlphaBlend, 
                RasterizerState.CullNone,
                DepthStencilState.Default
            );
            _minMaxValueTimeFractureBuffer = new ConstantBuffer(_game.GraphicsDevice, typeof(ConstData));
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
            Palette = _game.Content.Load<Texture2D>(palette);
        }

        public void SetPoints(
            Gis.CartPoint[] points, int[] indices,
            Gis.CartPoint[] borderPoints, int[] borderIndices, 
            List<float[]> valuesInTime)
        {
            _slicePoints = points;
            _indices = indices;
            _borderPoints = borderPoints;
            _borderIndices = borderIndices;
            _values = valuesInTime;

            UpdateValueBuffers(_currentTime);

            _pointsBuffer?.Dispose();
            _indicesBuffer?.Dispose();

            _borderPointsBuffer?.Dispose();
            _borderIndicesBuffer?.Dispose();

            if (_slicePoints == null || _slicePoints.Length == 0)
                return;            

            _pointsBuffer = new VertexBuffer(_game.GraphicsDevice, typeof(Gis.CartPoint), _slicePoints.Length);
            _pointsBuffer.SetData(_slicePoints);

            _indicesBuffer = new IndexBuffer(_game.GraphicsDevice, _indices.Length);
            _indicesBuffer.SetData(_indices);

            _borderPointsBuffer = new VertexBuffer(_game.GraphicsDevice, typeof(Gis.CartPoint), _borderPoints.Length);
            _borderPointsBuffer.SetData(_borderPoints);

            _borderIndicesBuffer = new IndexBuffer(_game.GraphicsDevice, _borderIndices.Length);
            _borderIndicesBuffer.SetData(_borderIndices);
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

            _game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;
            _game.GraphicsDevice.VertexShaderConstants[1] = _minMaxValueTimeFractureBuffer;
            _game.GraphicsDevice.PipelineState = _factory[(int) PointFlags.DRAW_SLICE];

            _game.GraphicsDevice.PixelShaderResources[0] = Palette;
            _game.GraphicsDevice.PixelShaderSamplers[0] = SamplerState.LinearClamp;
            _game.GraphicsDevice.VertexShaderResources[1] = _prevBuffer;
            _game.GraphicsDevice.VertexShaderResources[2] = _nextBuffer;

            _game.GraphicsDevice.SetupVertexInput(_pointsBuffer, _indicesBuffer);
            _game.GraphicsDevice.DrawIndexed(_indicesBuffer.Capacity, 0, 0);

            _game.GraphicsDevice.PipelineState = _factory[(int)PointFlags.DRAW_BORDER];
            _game.GraphicsDevice.SetupVertexInput(_borderPointsBuffer, _borderIndicesBuffer);
            _game.GraphicsDevice.DrawIndexed(_borderIndicesBuffer.Capacity, 0, 0);
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
