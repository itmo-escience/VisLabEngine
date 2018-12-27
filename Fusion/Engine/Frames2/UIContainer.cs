using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2
{
    public abstract class UIContainer : UIComponent
    {
        private readonly List<UIComponent> _children = new List<UIComponent>();
        public IReadOnlyList<UIComponent> Children => _children;

        public override RectangleF BoundingBox =>
                Children.Where(c => c.Visible)
                .Aggregate(
                    RectangleF.Empty,
                    (bb, child) => RectangleF.Union(child.BoundingBox, bb)
                );

        protected UIContainer(float x, float y, float width, float height) : base(x, y, width, height) { }

        public virtual void Add(UIComponent child)
        {
            if(_children.Contains(child))
                return;

            child.Parent = this;
            _children.Add(child);
        }

        public virtual bool Remove(UIComponent child)
        {
            if(!Children.Contains(child))
                return false;

            child.Parent = null;
            _children.Remove(child);
            return true;
        }

        public override void Draw(SpriteLayerD2D layer) { }
    }
}
