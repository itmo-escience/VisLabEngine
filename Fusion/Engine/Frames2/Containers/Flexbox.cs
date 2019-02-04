using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;
using System;

namespace Fusion.Engine.Frames2.Containers
{
    public class Flexbox : UIContainer
    {
        private float _mainSize;
        public float MainSize {
            get => _mainSize;
            set {
                SetAndNotify(ref _mainSize, value);
            }
        }

        public override void Update(GameTime gameTime)
        {
            float bottomBorder = 0;
            float lineWidth = 0;
            float lineHeight = 0;
            float maxLineWidth = 0;

            foreach (var child in Children)
            {
                if ((lineWidth == 0) || (lineWidth + child.Width <= _mainSize))
                {
                    child.X = lineWidth - child.LocalBoundingBox.X;
                    child.Y = bottomBorder - child.LocalBoundingBox.Y;

                    lineWidth += child.Width;
                    lineHeight = Math.Max(lineHeight, child.Height);
                }
                else
                {
                    bottomBorder += lineHeight;
                    maxLineWidth = Math.Min(maxLineWidth, lineWidth);

                    child.X = - child.LocalBoundingBox.X;
                    child.Y = bottomBorder - child.LocalBoundingBox.Y;

                    lineWidth = child.Width;
                    lineHeight = child.Height;
                }
            }

            Width = Math.Min(maxLineWidth, _mainSize);
            Height = bottomBorder + lineHeight;
        }

        public Flexbox() : base()
        {
            _mainSize = 0;
            debugBrush = new SolidBrushD2D(new Color4(1, 0.5f, 1, 1));
            debugTextFormat = new TextFormatD2D("Consolas", 14);
        }

        public Flexbox(float x, float y, float width, float height, bool needClipping = false) : base(x, y, width, height, needClipping)
        {
            _mainSize = width;  //TODO make better
            debugBrush = new SolidBrushD2D(new Color4(1, 0.5f, 0, 1));
            debugTextFormat = new TextFormatD2D("Consolas", 14);
        }

        public override void DebugDraw(SpriteLayerD2D layer)
        {
            base.DebugDraw(layer);
            layer.Draw(new TransformCommand(GlobalTransform));

            //layer.Draw(new Rect(0, 0, Width, Height, debugBrush));

            //
        }
    }
}
