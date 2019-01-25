using Fusion.Engine.Common;

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
        }
    }
}
