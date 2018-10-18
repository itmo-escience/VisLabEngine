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
        public enum AnchorPoint
        {
            TopLeft,       Top,    TopRight,
            Left,                     Right,
            BottomLeft, Bottom, BottomRight,
        }

        public class TextLabel
        {
            public DVector3 Position;
            public Color TextColor;
            public Color BackgroundColor;
            public string Text;
            public float Angle;
            public bool Visible = true;
            public AnchorPoint AnchorPoint;

            public TextLabel(DVector3 position, string text, Color textColor)
                : this(position, text, textColor, backgroundColor: Color.Zero, anchorPoint: AnchorPoint.TopLeft, angle: 0) { }

            public TextLabel(DVector3 position, string text, Color textColor, Color backgroundColor)
                : this(position, text, textColor, backgroundColor, anchorPoint: AnchorPoint.TopLeft, angle: 0) { }

            public TextLabel(DVector3 position, string text, Color textColor, Color backgroundColor, AnchorPoint anchorPoint)
                : this(position, text, textColor, backgroundColor, anchorPoint, angle: 0) { }

            public TextLabel(DVector3 position, string text, Color textColor, Color backgroundColor, AnchorPoint anchorPoint, float angle)
            {
                Position = position;
                Text = text;
                TextColor = textColor;
                BackgroundColor = backgroundColor;
                Angle = angle;
                AnchorPoint = anchorPoint;
            }
        }

        public double MaxVisibleDistance = 3500;

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
            void Display(TextLabel label, Rectangle rect, Color bgColor)
            {
                _spriteLayer.DrawSprite(Game.RenderSystem.WhiteTexture,
                    rect.X + rect.Width / 2, rect.Y + rect.Height / 2,
                    rect.Width, rect.Height,
                    0, bgColor
                );

                _font.DrawString(
                    _spriteLayer,
                    label.Text,
                    rect.X,
                    rect.Y,
                    label.TextColor,
                    useBaseLine: false
                );
            }
            _spriteLayer.Clear();

            var viewport = _camera.Viewport.Bounds;

            var sortedLabels = _labels
                .Where(l => l.Visible)
                .Where(l => viewport.Contains(GetBoundingRect(l)))
                .Select(l => new Tuple<TextLabel, double>(l, (l.Position - _camera.FinalCamPosition).Length()))
                .Where(t => t.Item2 < MaxVisibleDistance)
                .OrderBy(t => t.Item2)
                .Select(t => t.Item1)
                .ToList();

            if(sortedLabels.Count == 0)
                return;

            var lastDisplayedRect = GetBoundingRect(sortedLabels[0]);
            Display(sortedLabels[0], lastDisplayedRect, sortedLabels[0].BackgroundColor);
            for (var i = 1; i < sortedLabels.Count; i++)
            {
                var label = sortedLabels[i];
                var rect = GetBoundingRect(label);

                if (!rect.Intersects(lastDisplayedRect))
                {
                    Display(label, rect, label.BackgroundColor);

                    lastDisplayedRect = rect;
                }
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

        public Rectangle GetBoundingRect(TextLabel label)
        {
            var screenPos = _camera.CartesianToScreen(label.Position);

            var textRect = _font.MeasureStringF(label.Text);

            var x = screenPos.X;
            var y = screenPos.Y;
            switch (label.AnchorPoint)
            {
                case AnchorPoint.TopLeft:
                    break;
                case AnchorPoint.Top:
                    x -= textRect.Width / 2;
                    break;
                case AnchorPoint.TopRight:
                    x -= textRect.Width;
                    break;
                case AnchorPoint.BottomLeft:
                    y -= textRect.Height;
                    break;
                case AnchorPoint.Bottom:
                    x -= textRect.Width / 2;
                    y -= textRect.Height;
                    break;
                case AnchorPoint.BottomRight:
                    x -= textRect.Width;
                    y -= textRect.Height;
                    break;
                case AnchorPoint.Left:
                    y -= textRect.Height / 2;
                    break;
                case AnchorPoint.Right:
                    x -= textRect.Width;
                    y -= textRect.Height / 2;
                    break;
            }

            return new Rectangle((int) x, (int) y, (int) textRect.Width, (int) textRect.Height);
        }
    }
}
