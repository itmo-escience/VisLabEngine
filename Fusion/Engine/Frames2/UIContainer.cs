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
        public ObservableCollection<UIComponent> Children {
            get { return _children; }
            set {
                foreach (UIComponent child in value)
                {
                    Add(child);
                }
                NotifyPropertyChanged();
            }
        }

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

        /*public override RectangleF BoundingBox
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
        }*/

        protected UIContainer() : base()
        {
            _children = new ObservableCollection<UIComponent>();
            Children = new ObservableCollection<UIComponent>(_children);
            _needClipping = false;
        }

        protected UIContainer(float x, float y, float width, float height, bool needClipping = false) : base(x, y, width, height)
        {
            _children = new ObservableCollection<UIComponent>();
            Children = new ObservableCollection<UIComponent>(_children);
            _needClipping = needClipping;
        }

        public virtual void Add(UIComponent child)
        {
			AddAt(child, int.MaxValue);
		}

		public virtual void AddAt( UIComponent child, int index )
		{
			if (_children.Contains(child))
				return;

			child.Parent = this;
			if (index >= Children.Count)
			{
				Children.Add(child);
			} else
			{
				Children.Insert(index, child);
			}

            UpdateChildrenLayout();
        }

        protected virtual void UpdateChildrenLayout() { }

        public virtual bool Remove(UIComponent child)
        {
            if(!Children.Contains(child))
                return false;

            child.Parent = null;
            _children.Remove(child);
            return true;
        }

        #region ZOrder

        public void AddAtFront(UIComponent child)
        {
            Add(child);
        }

        public void AddAtBack(UIComponent child)
        {
            AddAt(child, 0);
        }

        public void AddInFrontOf(UIComponent child, UIComponent otherChild)
        {
            if (!Children.Contains(otherChild))
                return;

            AddAt(child, Children.IndexOf(otherChild) + 1);
        }

        public void MoveTo(UIComponent child, int index)
        {
            if (!Children.Contains(child))
                return;

            if (index < 0) index = 0;

            Remove(child);
            AddAt(child, index);
        }

        public void BringToFront(UIComponent child)
        {
            MoveTo(child, Children.Count);
        }

        public void SendToBack(UIComponent child)
        {
            MoveTo(child, 0);
        }

        public void BringForward(UIComponent child)
        {
            MoveTo(child, Children.IndexOf(child) + 1);
        }

        public void SendBackward(UIComponent child)
        {
            MoveTo(child, Children.IndexOf(child) - 1);
        }

        #endregion

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
            sink.Dispose();

            return new PathGeometryD2D(geometry);
        }

        internal void RestoreParents()
        {
            foreach (var child in Children)
            {
                child.Parent = this;
                child.NotifyPropertyChanged("Parent");
                if (child is UIContainer container) {
                    container.RestoreParents();
                }
            }
        }
    }
}
