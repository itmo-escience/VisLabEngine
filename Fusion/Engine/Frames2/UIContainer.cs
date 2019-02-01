using System.Collections.ObjectModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.SpritesD2D;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace Fusion.Engine.Frames2
{
    public abstract class UIContainer : UIComponent
    {
        private readonly ObservableCollection<UIComponent> _children;
        public ReadOnlyObservableCollection<UIComponent> Children { get; }
        private bool _needClipping;
        public bool NeedClipping {
            get => _needClipping;
            set {
                SetAndNotify(ref _needClipping, value);
            }
        }

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

        protected UIContainer(float x, float y, float width, float height, bool needClipping = false) : base(x, y, width, height)
        {
            _children = new ObservableCollection<UIComponent>();
            Children = new ReadOnlyObservableCollection<UIComponent>(_children);
            _needClipping = needClipping;
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

        public PathGeometryD2D GetClippingGeometry(SpriteLayerD2D layer)
        {
            PathGeometry geometry = new PathGeometry(layer.Factory);
            GeometrySink sink = geometry.Open();
            sink.SetFillMode(FillMode.Winding);

            RawVector2 p0 = Matrix3x2.TransformPoint(GlobalTransform, new Vector2(0, 0)).ToRawVector2();
            RawVector2 p1 = Matrix3x2.TransformPoint(GlobalTransform, new Vector2(0, Height)).ToRawVector2();
            RawVector2 p2 = Matrix3x2.TransformPoint(GlobalTransform, new Vector2(Width, Height)).ToRawVector2();
            RawVector2 p3 = Matrix3x2.TransformPoint(GlobalTransform, new Vector2(Width, 0)).ToRawVector2();

            sink.BeginFigure(p0, FigureBegin.Filled);
            sink.AddLine(p1);
            sink.AddLine(p2);
            sink.AddLine(p3);
            sink.EndFigure(FigureEnd.Closed);
            sink.Close();
            return new PathGeometryD2D(geometry);
        }
    }
}
