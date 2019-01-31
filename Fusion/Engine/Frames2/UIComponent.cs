﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
                if (!Visible)
                {
                    return new RectangleF(X, Y, 0, 0);
                }

                var p0 = Matrix3x2.TransformPoint(GlobalTransform, Vector2.Zero);
                var p1 = Matrix3x2.TransformPoint(GlobalTransform, new Vector2(0, Height));
                var p2 = Matrix3x2.TransformPoint(GlobalTransform, new Vector2(Width, Height));
                var p3 = Matrix3x2.TransformPoint(GlobalTransform, new Vector2(Width, 0));

                return RectangleF.Bounding(p0, p1, p2, p3);
            }
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
            X = x;
            Y = y;
            Width = width;
            Height = height;
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

        #region Controllers

        private readonly Dictionary<string, UIController> _controllers = new Dictionary<string, UIController>();

        public bool AttachController(string key, UIController controller)
        {
            if (_controllers.ContainsKey(key))
                return false;

            controller.AttachTo(this);
            _controllers[key] = controller;
            return true;
        }

        public bool DetachController(string key)
        {
            if (!_controllers.TryGetValue(key, out var controller))
                return false;

            controller.Detach();
            _controllers.Remove(key);
            return true;
        }

        public UIController GetController(string key)
        {
            if (!_controllers.TryGetValue(key, out var result))
            {
                return null;
            }

            return result;
        }

        private void UpdateControllers(GameTime gameTime)
        {
            foreach (var controller in _controllers.Values)
            {
                controller.Update(gameTime);
            }
        }

        #endregion

        internal void InternalUpdate(GameTime gameTime)
        {
            UpdateTransforms();

            UpdateControllers(gameTime);

            Update(gameTime);

            UpdateTransforms();
        }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteLayerD2D layer);

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
