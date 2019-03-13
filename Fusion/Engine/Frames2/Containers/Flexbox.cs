using Fusion.Core.Mathematics;
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

        protected override void UpdateChildrenLayout() {
            float bottomBorder = 0;
            float lineWidth = 0;
            float lineHeight = 0;
            float maxLineWidth = 0;

            foreach (var child in Children)
            {
                if ((lineWidth == 0) || (lineWidth + child.LocalBoundingBox.Width <= _mainSize))
                {
                    child.X += lineWidth - child.LocalBoundingBox.X;
                    child.Y += bottomBorder - child.LocalBoundingBox.Y;

                    lineWidth += child.LocalBoundingBox.Width;
                    lineHeight = Math.Max(lineHeight, child.LocalBoundingBox.Height);
                }
                else
                {
                    bottomBorder += lineHeight;

                    child.X += -child.LocalBoundingBox.X;
                    child.Y += bottomBorder - child.LocalBoundingBox.Y;

                    lineWidth = child.LocalBoundingBox.Width;
                    lineHeight = child.LocalBoundingBox.Height;
                }
                maxLineWidth = Math.Max(maxLineWidth, lineWidth);
            }

            Width = Math.Min(maxLineWidth, _mainSize);
            Height = bottomBorder + lineHeight;
        }

        protected override SolidBrushD2D DebugBrush => new SolidBrushD2D(new Color4(1, 0.5f, 1, 1));
        protected override TextFormatD2D DebugTextFormat => new TextFormatD2D("Consolas", 14);

        public Flexbox() : base()
        {
            _mainSize = 0;
        }

        public Flexbox(float x, float y, float width, float height, bool needClipping = false) : base(x, y, width, height, needClipping)
        {
            _mainSize = width;  //TODO make better
        }

        public override void DefaultInit()
        {
            Width = 100;
            Height = 100;
            _mainSize = Width;
        }

        public override void DebugDraw(SpriteLayerD2D layer)
        {
            base.DebugDraw(layer);
            layer.Draw(new TransformCommand(GlobalTransform));

            layer.Draw(new Rect(0, 0, Width, Height, DebugBrush));

            layer.Draw(new Rect(0, 0, _mainSize, Height, DebugBrush, true));
        }
    }
}
