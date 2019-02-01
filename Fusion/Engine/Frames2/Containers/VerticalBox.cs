using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{

    public enum verticalAlignment
    {
        LEFT, CENTER, RIGHT
    }

    public class VerticalBox : UIContainer
    {
        private readonly verticalAlignment _alignment;

        public override void Update(GameTime gameTime)
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
            switch (_alignment)
            {
                case verticalAlignment.LEFT:
                    deltaXMultiplier = 0;
                    break;
                case verticalAlignment.CENTER:
                    deltaXMultiplier = 0.5f;
                    break;
                case verticalAlignment.RIGHT:
                    deltaXMultiplier = 1;
                    break;
            }

            foreach (var child in Children)
            {
                child.X += deltaXMultiplier * (maxChildWidth - child.LocalBoundingBox.Width) - child.LocalBoundingBox.X;
            }
        }

        public VerticalBox(float x, float y, float width, float height, verticalAlignment alignment = verticalAlignment.LEFT, bool needClipping = false) : base(x, y, width, height, needClipping)
        {
            debugBrush = new SolidBrushD2D(new Color4(0, 1, 1, 1));
            debugTextFormat = new TextFormatD2D("Consolas", 14);
            _alignment = alignment;
        }

        public override void Draw(SpriteLayerD2D layer)
        {
            base.Draw(layer);
            layer.Draw(new Rect(0, 0, Width, Height, debugBrush));

            float bottomBorder = 0;
            foreach (var child in Children)
            {
                bottomBorder += child.LocalBoundingBox.Height;
                layer.Draw(new Line(new Vector2(0, bottomBorder), new Vector2(Width, bottomBorder), debugBrush));
            }
        }
    }
}
