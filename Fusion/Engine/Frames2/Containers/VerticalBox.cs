using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{
    /*
    public enum VerticalAlignment
    {
        Left, Center, Right
    }

    public class VerticalBox : UIContainer
    {
        private VerticalAlignment _alignment;
        public VerticalAlignment Alignment {
            get => _alignment;
            set {
                SetAndNotify(ref _alignment, value);
            }
        }

        protected override void UpdateChildrenLayout()
        {
            float bottomBorder = 0;
            float maxChildWidth = 0;
            foreach (var child in Children)
            {
                child.Y += bottomBorder - child.LocalBoundingBox.Y;
                bottomBorder += child.LocalBoundingBox.Height;

                if (maxChildWidth < child.LocalBoundingBox.Width) maxChildWidth = child.LocalBoundingBox.Width;
            }

            Width = maxChildWidth;
            Height = bottomBorder;

            float deltaXMultiplier = 0;
            switch (Alignment)
            {
                case VerticalAlignment.Left:
                    deltaXMultiplier = 0;
                    break;
                case VerticalAlignment.Center:
                    deltaXMultiplier = 0.5f;
                    break;
                case VerticalAlignment.Right:
                    deltaXMultiplier = 1;
                    break;
            }

            foreach (var child in Children)
            {
                child.X += deltaXMultiplier * (maxChildWidth - child.LocalBoundingBox.Width) - child.LocalBoundingBox.X;
            }
        }

        protected override SolidBrushD2D DebugBrush { get; } = new SolidBrushD2D(new Color4(0, 1, 1, 1));
        protected override TextFormatD2D DebugTextFormat { get; } = new TextFormatD2D("Consolas", 14);

        public VerticalBox() : base() {
            Alignment = VerticalAlignment.Left;
        }

        public VerticalBox(float x, float y, float width, float height, VerticalAlignment alignment = VerticalAlignment.Left, bool needClipping = false) : base(x, y, width, height, needClipping)
        {
            DebugBrush = new SolidBrushD2D(new Color4(0, 1, 1, 1));
            DebugTextFormat = new TextFormatD2D("Consolas", 14);
            Alignment = alignment;
        }

        public override void DebugDraw(SpriteLayerD2D layer)
        {
            base.DebugDraw(layer);
            layer.Draw(new TransformCommand(GlobalTransform));

            layer.Draw(new Rect(0, 0, Width, Height, DebugBrush));

            float bottomBorder = 0;
            foreach (var child in Children)
            {
                bottomBorder += child.LocalBoundingBox.Height;
                layer.Draw(new Line(new Vector2(0, bottomBorder), new Vector2(Width, bottomBorder), DebugBrush));
            }
        }
    }
    */
}
