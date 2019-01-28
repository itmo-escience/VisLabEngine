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
        private verticalAlignment alignment;

        public override void Update(GameTime gameTime)
        {
            float bottomBorder = 0;
            float maxChildWidth = 0;
            foreach (var child in Children)
            {
                child.Y += bottomBorder - child.LocalBoundingBox.Y;
                bottomBorder += child.BoundingBox.Height;

                if (maxChildWidth < child.BoundingBox.Width) maxChildWidth = child.BoundingBox.Width;
            }

            float deltaXMultiplier = 0;
            switch (alignment)
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
                child.X += deltaXMultiplier * (maxChildWidth - child.BoundingBox.Width) - child.LocalBoundingBox.X;
            }
        }

        public VerticalBox(float x, float y, float width, float height, verticalAlignment alignment = verticalAlignment.LEFT) : base(x, y, width, height)
        {
            debugBrush = new SolidBrushD2D(new Color4(0, 1, 1, 1));
            debugTextFormat = new TextFormatD2D("Consolas", 14);
            this.alignment = alignment;
        }
    }
}
