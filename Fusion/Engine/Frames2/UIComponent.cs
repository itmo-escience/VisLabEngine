using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2
{
    public abstract class UIComponent : INotifyPropertyChanged, IUIInputAware
    {
        #region Position
        private float _x;
        public float X
        {
            get => _x;
            set
            {
                if (SetAndNotify(ref _x, value))
                    InvalidateTransform();
            }
        }

        private float _y;
        public float Y
        {
            get => _y;
            set
            {
                if (SetAndNotify(ref _y, value))
                    InvalidateTransform();
            }
        }

        private float _width;
        public float Width
        {
            get => _width;
            set => SetAndNotify(ref _width, value);
        }

        private float _height;
        public float Height
        {
            get => _height;
            set => SetAndNotify(ref _height, value);
        }

        private float _angle;
        public float Angle
        {
            get => _angle;
            set
            {
                if(SetAndNotify(ref _angle, value))
                    InvalidateTransform();
            }
        }

        public void SetPositionAndSize(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        #endregion

        #region Transforms

        private bool _isTransformDirty = true;

        public virtual void InvalidateTransform()
        {
            _isTransformDirty = true;
        }

        private Matrix3x2 _localTransform = Matrix3x2.Identity;
        internal Matrix3x2 LocalTransform
        {
            get
            {
                if (_isTransformDirty)
                    UpdateTransforms();

                return _localTransform;
            }
        }

        private Matrix3x2 _globalTransform = Matrix3x2.Identity;
        public Matrix3x2 GlobalTransform
        {
            get
            {
                if(_isTransformDirty)
                    UpdateTransforms();

                return _globalTransform;
            }
        }

        private Matrix3x2 _transform = Matrix3x2.Identity;
        public Matrix3x2 Transform
        {
            get => _transform;
            set
            {
                if (SetAndNotify(ref _transform, value))
                    InvalidateTransform();
            }
        }

        private void UpdateTransforms()
        {
            if(!_isTransformDirty) return;

            _localTransform = Matrix3x2.Transformation(1, 1, _angle, X, Y);

            var pTransform = Parent?.GlobalTransform ?? Matrix.Identity;
            _globalTransform = _transform * _localTransform * pTransform;

            _isTransformDirty = false;
        }

        #endregion

        public virtual RectangleF BoundingBox
        {
            get
            {
                return (Visible ? new RectangleF(0, 0, Width, Height) : new RectangleF(0, 0, 0, 0)).GetBound(GlobalTransform);
            }
        }

        public virtual RectangleF LocalBoundingBox
        {
            get
            {
                return (Visible ? new RectangleF(0, 0, Width, Height) : new RectangleF(0, 0, 0, 0)).GetBound(_transform * _localTransform);
            }
        }

        public float GlobalAngle
        {
            get
            {
                float angle = _angle;
                UIComponent parent = _parent;
                while (parent != null)
                {
                    angle += parent.Angle;
                    parent = parent.Parent;
                }
                return angle;
            }
        }

        public virtual bool IsInside(Vector2 point)
        {
            Matrix3x2 invertTransform = GlobalTransform;
            invertTransform.Invert();
            Vector2 localPoint = Matrix3x2.TransformPoint(invertTransform, point);
            return ((localPoint.X >= 0) && (localPoint.Y >= 0) && (localPoint.X < Width) && (localPoint.Y < Height));
        }

        private UIContainer _parent;
        [XmlIgnore]
        public UIContainer Parent
        {
            get => _parent;
            internal set
            {
                if (SetAndNotify(ref _parent, value))
                    InvalidateTransform();
            }
        }

        public UIComponent RootParent {
            get {
                UIComponent parent = this;
                while (parent.Parent != null)
                {
                    parent = parent.Parent;
                }
                return parent;
            }
        }

        private bool _visible = true;
        public bool Visible
        {
            get => _visible;
            set => SetAndNotify(ref _visible, value);
        }

        private object _tag;
        public object Tag
        {
            get => _tag;
            set => SetAndNotify(ref _tag, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set {
                if (UIManager.IsComponentNameValid(value, RootParent, this)) SetAndNotify(ref _name, value);
            }
        }

        protected UIComponent() : this(0, 0, 0, 0)
        { }

        protected UIComponent(float x, float y) : this(x, y, 0, 0)
        { }

        protected UIComponent(float x, float y, float width, float height)
        {
            _name = GenerateName(GetType());
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _isTransformDirty = true;
        }

        public abstract void DefaultInit();

        internal void InternalUpdate(GameTime gameTime)
        {
            UpdateTransforms();

            Update(gameTime);

            UpdateTransforms();
        }

        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(SpriteLayerD2D layer) { }

        protected virtual SolidBrushD2D DebugBrush { get; } = new SolidBrushD2D(new Color4(0, 1, 0, 1));
        protected virtual TextFormatD2D DebugTextFormat { get; } = new TextFormatD2D("Consolas", 12);
        public virtual void DebugDraw(SpriteLayerD2D layer)
        {
            var b = BoundingBox;
            layer.Draw(TransformCommand.Identity);
            layer.Draw(new Rect(b.X, b.Y, b.Width, b.Height, DebugBrush));

            var debugText = $"{Name} X:{b.X:0.00} Y:{b.Y:0.00} W:{b.Width:0.00} H:{b.Height:0.00}";
            var dtl = new TextLayoutD2D(debugText, DebugTextFormat, float.MaxValue, float.MaxValue);
            layer.Draw(new Text(debugText, new RectangleF(b.X, b.Y - dtl.Height, dtl.Width + 1, dtl.Height), DebugTextFormat, DebugBrush));
        }

        #region Naming

        private static readonly Dictionary<Type, int> GeneratedCountOfType = new Dictionary<Type, int>();
        private static string GenerateName(Type type)
        {
            if (GeneratedCountOfType.TryGetValue(type, out var value))
            {
                GeneratedCountOfType[type] = value + 1;
            }
            else
            {
                GeneratedCountOfType[type] = 1;
            }

            return $"{type.Name}_{value}";
        }

        #endregion Naming

        #region PropertyChanges

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets field with new value and fires <seealso cref="PropertyChanged"/> event if provided value is different from the old one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">Private field to set.</param>
        /// <param name="value">New value.</param>
        /// <param name="propertyName">Name that will be passed in a PropertyChanged event.</param>
        /// <returns>True if new value is different and PropertyChanged event was fired, false otherwise.</returns>
        protected bool SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            NotifyPropertyChanged(propertyName);

            return true;
        }

        internal void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion PropertyChanges

        public event KeyDownEvent       KeyDown;
        public event KeyUpEvent         KeyUp;
        public event KeyPressEvent      KeyPress;
        public event MouseMoveEvent     MouseMove;
        public event MouseMoveEvent     MouseMoveOutside;
        public event MouseDragEvent     MouseDrag;
        public event MouseDownEvent     MouseDown;
        public event MouseDownEvent     MouseDownOutside;
        public event MouseUpEvent       MouseUp;
        public event MouseUpEvent       MouseUpOutside;
        public event ClickEvent         Click;
        public event DoubleClickEvent   DoubleClick;
        public event ScrollEvent        Scroll;
        public event EnterEvent         Enter;
        public event LeaveEvent         Leave;
        public event FocusEvent         Focus;
        public event BlurEvent          Blur;

        internal virtual void InvokeKeyDown         (KeyEventArgs e)       => KeyDown?.Invoke(this, e);
        internal virtual void InvokeKeyUp           (KeyEventArgs e)       => KeyUp?.Invoke(this, e);
        internal virtual void InvokeKeyPress        (KeyPressEventArgs e)  => KeyPress?.Invoke(this, e);
        internal virtual void InvokeMouseMove       (MoveEventArgs e)      => MouseMove?.Invoke(this, e);
        internal virtual void InvokeMouseMoveOutside(MoveEventArgs e)      => MouseMoveOutside?.Invoke(this, e);
        internal virtual void InvokeMouseDrag       (DragEventArgs e)      => MouseDrag?.Invoke(this, e);
        internal virtual void InvokeMouseDown       (ClickEventArgs e)     => MouseDown?.Invoke(this, e);
        internal virtual void InvokeMouseDownOutside(ClickEventArgs e)     => MouseDownOutside?.Invoke(this, e);
        internal virtual void InvokeMouseUp         (ClickEventArgs e)     => MouseUp?.Invoke(this, e);
        internal virtual void InvokeMouseUpOutside  (ClickEventArgs e)     => MouseUpOutside?.Invoke(this, e);
        internal virtual void InvokeClick           (ClickEventArgs e)     => Click?.Invoke(this, e);
        internal virtual void InvokeDoubleClick     (ClickEventArgs e)     => DoubleClick?.Invoke(this, e);
        internal virtual void InvokeScroll          (ScrollEventArgs e)    => Scroll?.Invoke(this, e);
        internal virtual void InvokeEnter           ()                     => Enter?.Invoke(this);
        internal virtual void InvokeLeave           ()                     => Leave?.Invoke(this);
        internal virtual void InvokeFocus           ()                     => Focus?.Invoke(this);
        internal virtual void InvokeBlur            ()                     => Blur?.Invoke(this);
    }
}
