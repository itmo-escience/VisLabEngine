using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{
    public class VerticalBox : UIContainer
    {
        public override void Update(GameTime gameTime)
        {
            float bottomBorder = this.Y;
            foreach (var child in Children)
            {
                child.X += this.X - child.BoundingBox.X;
                child.Y += bottomBorder - child.BoundingBox.Y;
                bottomBorder += child.BoundingBox.Height;
            }
        }

        public VerticalBox(float x, float y, float width, float height) : base(x, y, width, height)
        {
            debugBrush = new SolidBrushD2D(new Color4(0, 1, 1, 1));
            debugTextFormat = new TextFormatD2D("Consolas", 14);
        }
    }
}
