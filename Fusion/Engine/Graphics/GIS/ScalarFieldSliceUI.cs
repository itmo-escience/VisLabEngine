using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
    public class ScalarFieldSliceUI : Gis.GisLayer
    {       
        private readonly LinesGisLayer _lines;
        private readonly TextLabelGisLayer _labels;
        private readonly List<SliceUI> _slices = new List<SliceUI>();

        public ScalarFieldSliceUI(Game engine, GlobeCamera camera, SpriteFont labelFont, SpriteLayer layer) : base(engine)
        {
            _lines = new LinesGisLayer(engine, 2);
            _labels = new TextLabelGisLayer(engine, labelFont, layer, camera);
        }

        public SliceUI Add(List<Tuple<DVector3, string>> XAxis, List<Tuple<DVector3, string>> YAxis)
        {
            var result = new SliceUI();
            
            //_lines.AddLine(XAxis);
            foreach (var p in XAxis)
            {
                var l = _labels.AddLabel(new TextLabelGisLayer.TextLabel(p.Item1, p.Item2, Color.White, Color.Black, TextLabelGisLayer.AnchorPoint.TopLeft));

                result.Labels.Add(l);
            }

            foreach (var p in YAxis)
            {
                var l = _labels.AddLabel(new TextLabelGisLayer.TextLabel(p.Item1, p.Item2, Color.White, Color.Black, TextLabelGisLayer.AnchorPoint.BottomRight));

                result.Labels.Add(l);
            }

            _slices.Add(result);
            return result;
        }

        public void Remove(SliceUI toBeRemoved)
        {
            foreach (var label in toBeRemoved.Labels)
            {
                _labels.RemoveLabel(label);
            }

            _slices.Remove(toBeRemoved);
        }

        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
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
        public List<TextLabelGisLayer.TextLabel> Labels = new List<TextLabelGisLayer.TextLabel>();
    }
}
