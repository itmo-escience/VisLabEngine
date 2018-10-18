using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
    public class ScalarFieldSliceUI : Gis.GisLayer
    {
        private readonly GlobeCamera _camera;
        private readonly LinesGisLayer _lines;
        private readonly TextLabelGisLayer _labels;
        private readonly List<SliceUI> _slices = new List<SliceUI>();
        public SpriteLayer SpriteLayer { get; private set; }

        public ScalarFieldSliceUI(Game engine, GlobeCamera camera, SpriteFont labelFont, SpriteLayer layer) : base(engine)
        {
            _camera = camera;
            _lines = new LinesGisLayer(engine, 2);
            _labels = new TextLabelGisLayer(engine, labelFont, layer, camera);
            SpriteLayer = layer;
        }

        public SliceUI Add(List<DVector3> XAxis, List<string> XLabels, List<DVector3> YAxis, List<string> YLabels)
        {
            var result = new SliceUI();
            result.XAxis = XAxis;
            result.YAxis = YAxis;
            result.AxisColor = Color4.White;

            _lines.AddLine(XAxis, result.AxisColor);
            for(var i = 0; i < XLabels.Count; i++)
            {
                var l = _labels.AddLabel(new TextLabelGisLayer.TextLabel(XAxis[i], XLabels[i], Color.White, Color.Zero));

                result.XLabels.Add(l);
            }

            _lines.AddLine(YAxis, result.AxisColor);
            for (var i = 0; i < YLabels.Count; i++)
            {
                var l = _labels.AddLabel(new TextLabelGisLayer.TextLabel(YAxis[i], YLabels[i], Color.White, Color.Zero));

                result.YLabels.Add(l);
            }

            _slices.Add(result);
            return result;
        }

        public void Remove(SliceUI toBeRemoved)
        {
            foreach (var label in toBeRemoved.XLabels)
            {
                _labels.RemoveLabel(label);
            }

            foreach (var label in toBeRemoved.YLabels)
            {
                _labels.RemoveLabel(label);
            }

            _slices.Remove(toBeRemoved);

            _lines.Clear();
            foreach (var slice in _slices)
            {
                _lines.AddLine(slice.XAxis, slice.AxisColor);
                _lines.AddLine(slice.YAxis, slice.AxisColor);
            }
        }

        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
            foreach (var slice in _slices)
            {
                var p0 = _camera.CartesianToScreen(slice.XLabels[0].Position);
                var p1 = _camera.CartesianToScreen(slice.XLabels[slice.XLabels.Count - 1].Position);

                var showXLabelsAtRight = p1.X < p0.X ^ p0.Y < p1.Y;
                var showYLabelsAtLeft = p1.X < p0.X;

                foreach (var label in slice.XLabels)
                {
                    label.AnchorPoint = showXLabelsAtRight
                        ? TextLabelGisLayer.AnchorPoint.TopRight
                        : TextLabelGisLayer.AnchorPoint.TopLeft;
                }

                foreach (var label in slice.YLabels)
                {
                    label.AnchorPoint = showYLabelsAtLeft
                        ? TextLabelGisLayer.AnchorPoint.BottomLeft
                        : TextLabelGisLayer.AnchorPoint.BottomRight;
                }
            }

            _lines.Draw(gameTime, constBuffer);
            _labels.Draw(gameTime, constBuffer);

            base.Draw(gameTime, constBuffer);
        }

        public override void Update(GameTime gameTime)
        {
            _lines.Update(gameTime);
            _labels.Update(gameTime);

            base.Update(gameTime);
        }
    }

    public class SliceUI
    {
        public List<TextLabelGisLayer.TextLabel> XLabels = new List<TextLabelGisLayer.TextLabel>();
        public List<TextLabelGisLayer.TextLabel> YLabels = new List<TextLabelGisLayer.TextLabel>();

        public List<DVector3> XAxis = new List<DVector3>();
        public List<DVector3> YAxis = new List<DVector3>();
        public Color4 AxisColor = Color4.White;
    }
}
