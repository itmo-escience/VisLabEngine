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
        #region Position and bounds properties
        private float _x;
        public float X
        {
            get => _x;
            set
            {
                if (SetAndNotify(ref _x, value))
                    _isTransformDirty = true;
            }
        }

        private float _y;
        public float Y
        {
            get => _y;
            set
            {
                if (SetAndNotify(ref _y, value))
                    _isTransformDirty = true;
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

        private bool _isTransformDirty = true;

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
                    _isTransformDirty = true;
            }
        }

        private void UpdateTransforms()
        {
            _localTransform = Matrix3x2.Translation(X, Y);

            var pTransform = Parent?.GlobalTransform ?? Matrix.Identity;
            _globalTransform = _transform * _localTransform * pTransform;

            _isTransformDirty = false;
        }

        #endregion

        private UIContainer _parent;
        public UIContainer Parent
        {
            get => _parent;
            internal set
            {
                if (SetAndNotify(ref _parent, value))
                    _isTransformDirty = true;
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

        public IList<IUIController> Controllers { get; } = new List<IUIController>();

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
