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
            result.AxisColor = Color4.White;

            var xLineWithTicks = new List<DVector3>();
            xLineWithTicks.Add(XAxis[0]);
            for (var i = 1; i < XAxis.Count - 1; i++)
            {
                xLineWithTicks.Add(XAxis[i]);

                var s = GeoHelper.CartesianToSpherical(XAxis[i]);
                var t = GeoHelper.SphericalToCartesian(s, GeoHelper.EarthRadius + 4);
                xLineWithTicks.Add(t);

                xLineWithTicks.Add(XAxis[i]);
            }
            xLineWithTicks.Add(XAxis[XAxis.Count - 1]);
            result.XAxis = xLineWithTicks;

            for(var i = 0; i < XLabels.Count; i++)
            {
                var l = _labels.AddLabel(new TextLabelGisLayer.TextLabel(XAxis[i], XLabels[i], Color.White, Color.Zero));

                result.XLabels.Add(l);
            }

            var yLineWithTicks = new List<DVector3>();
            var padding = (XAxis[0] - XAxis[1]).Normalized() * 3;
            for (var i = 0; i < YLabels.Count; i++)
            {
                var l = _labels.AddLabel(new TextLabelGisLayer.TextLabel(YAxis[i] + padding, YLabels[i], Color.White, Color.Zero));

                result.YLabels.Add(l);
                yLineWithTicks.Add(YAxis[i]);
                yLineWithTicks.Add(YAxis[i] - padding / 2);
                yLineWithTicks.Add(YAxis[i]);
            }
            result.YAxis = yLineWithTicks;

            _lines.AddLine(result.XAxis, result.AxisColor);
            _lines.AddLine(result.YAxis, result.AxisColor);

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
                        ? TextLabelGisLayer.AnchorPoint.Left
                        : TextLabelGisLayer.AnchorPoint.Right;
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
