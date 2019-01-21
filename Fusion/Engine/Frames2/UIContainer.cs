using System.Collections.ObjectModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2
{
    public abstract class UIContainer : UIComponent
    {
        private readonly ObservableCollection<UIComponent> _children;
        public ReadOnlyObservableCollection<UIComponent> Children { get; }

        public override void InvalidateTransform()
        {
            base.InvalidateTransform();
            foreach (var child in Children)
            {
                child.InvalidateTransform();
            }
        }

        public override RectangleF BoundingBox
        {
            get
            {
                var b = base.BoundingBox;

                foreach (var child in Children)
                {
                    b = RectangleF.Union(b, child.BoundingBox);
                }

                return b;
            }
        }

        protected UIContainer(float x, float y, float width, float height) : base(x, y, width, height)
        {
            _children = new ObservableCollection<UIComponent>();
            Children = new ReadOnlyObservableCollection<UIComponent>(_children);
        }

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
