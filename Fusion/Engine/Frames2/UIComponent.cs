﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2
{
    public abstract class UIComponent : INotifyPropertyChanged
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
            get {
                return (Visible ? new RectangleF(0, 0, Width, Height) : new RectangleF(0, 0, 0, 0)).GetBound(_transform * _localTransform);
            }
        }

        public virtual bool IsInside(Vector2 point)
        {
            return BoundingBox.Contains(point);
        }

        private UIContainer _parent;
        public UIContainer Parent
        {
            get => _parent;
            internal set
            {
                if (SetAndNotify(ref _parent, value))
                    InvalidateTransform();
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
            set => SetAndNotify(ref _name, value);
        }

        protected UIComponent(float x, float y, float width, float height)
        {
            Name = GenerateName(GetType());
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _isTransformDirty = true;
        }

        public IEnumerable<UIContainer> Ancestors()
        {
            var current = Parent;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }

        // TODO: Anchors

        public IList<IUIController> Controllers { get; } = new List<IUIController>();

        internal void InternalUpdate(GameTime gameTime)
        {
            UpdateTransforms();

            Update(gameTime);

            UpdateTransforms();
        }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteLayerD2D layer);

        protected SolidBrushD2D debugBrush = new SolidBrushD2D(new Color4(0, 1, 0, 1));
        protected TextFormatD2D debugTextFormat = new TextFormatD2D("Consolas", 12);
        public virtual void DebugDraw(SpriteLayerD2D layer)
        {
            var b = BoundingBox;
            layer.Draw(TransformCommand.Identity);
            layer.Draw(new Rect(b.X, b.Y, b.Width, b.Height, debugBrush));

            string debugText = $"{Name} X:{b.X:0.00} Y:{b.Y:0.00} W:{b.Width:0.00} H:{b.Height:0.00}";
            TextLayoutD2D dtl = new TextLayoutD2D(debugText, debugTextFormat, float.MaxValue, float.MaxValue);
            layer.Draw(new Label(debugText, new RectangleF(b.X, b.Y - dtl.Height, dtl.Width + 1, dtl.Height), debugTextFormat, debugBrush));
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

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion PropertyChanges
    }
}
