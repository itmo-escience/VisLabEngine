using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
    public class TextLabelGisLayer : Gis.GisLayer
    {
        public class TextLabel
        {
            public DVector3 Position;
            public Color TextColor;
            public Color BackgroundColor;
            public string Text;
            public float Angle;
            public bool Visible = true;

            public TextLabel(DVector3 position, string text, Color textColor) : this(position, text, textColor, backgroundColor: Color.Zero, angle: 0) { }

            public TextLabel(DVector3 position, string text, Color textColor, Color backgroundColor)
                : this(position, text, textColor, backgroundColor, angle: 0) { }

            public TextLabel(DVector3 position, string text, Color textColor, Color backgroundColor, float angle)
            {
                Position = position;
                Text = text;
                TextColor = textColor;
                BackgroundColor = backgroundColor;
                Angle = angle;
            }
        }

        public double MaxVisibleDistance = 1000;

        private readonly SpriteFont _font;
        private readonly SpriteLayer _spriteLayer;
        private readonly GlobeCamera _camera;
        private readonly List<TextLabel> _labels = new List<TextLabel>();

        public TextLabelGisLayer(Game engine, SpriteFont font, SpriteLayer layer, GlobeCamera camera) : base(engine)
        {
            _font = font;
            _spriteLayer = layer;
            _camera = camera;
        }

        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
            _spriteLayer.Clear();

            var whiteTex = Game.RenderSystem.WhiteTexture;
            var sortedLabels = _labels
                .Where(l => l.Visible)
                .Select(l => new Tuple<TextLabel, double>(l, (l.Position - _camera.FinalCamPosition).Length()))
                .Where(t => t.Item2 < MaxVisibleDistance)
                .OrderBy(t => -t.Item2)
                .Select(t => t.Item1);

            foreach (var label in sortedLabels)
            {
                var screenPos = _camera.CartesianToScreen(label.Position);
                
                var textRect = _font.MeasureStringF(label.Text);

                _spriteLayer.DrawSprite(whiteTex, 
                    screenPos.X, screenPos.Y, 
                    textRect.Width, textRect.Height, 
                    0, label.BackgroundColor
                );

                _font.DrawString(
                    _spriteLayer,
                    label.Text,
                    screenPos.X - textRect.Width / 2,
                    screenPos.Y - textRect.Height / 2,
                    label.TextColor,
                    useBaseLine: false
                );
            }
        }

        public TextLabel AddLabel(DVector3 position, string text, Color color)
        {
            var label = new TextLabel(position, text, color);
            _labels.Add(label);

            return label;
        }

        public TextLabel AddLabel(TextLabel label)
        {           
            _labels.Add(label);

            return label;
        }

        public void RemoveLabel(TextLabel label)
        {
            _labels.Remove(label);
        }

        public void Clear()
        {
            _labels.Clear();
        }
    }
}
