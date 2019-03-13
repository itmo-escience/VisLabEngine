using System;
using System.Collections.Generic;
using System.ComponentModel;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2
{
    public interface UIComponent : INotifyPropertyChanged //, IUIInputAware
    {
        ISlot Placement { get; set; }

        float DesiredWidth { get; }
        float DesiredHeight { get; }

        /*#region Transforms
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

            _localTransform = Matrix3x2.Transformation(1, 1, 0, Placement.Position.X, Y);

            var pTransform = Placement.Parent?.GlobalTransform ?? Matrix.Identity;
            _globalTransform = _transform * _localTransform * pTransform;

            _isTransformDirty = false;
        }

        #endregion

        public virtual RectangleF LocalBoundingBox
        {
            get
            {
                return (Visible ? new RectangleF(0, 0, Width, Height) : new RectangleF(0, 0, 0, 0)).GetBound(_transform * _localTransform);
            }
        }
        */

        object Tag { get; set; }
        string Name { get; set; }

        void Update(GameTime gameTime);
        void Draw(SpriteLayerD2D layer);
    }

    public static class UINameManager
    {
        #region Naming

        private static readonly Dictionary<Type, int> GeneratedCountOfType = new Dictionary<Type, int>();
        public static string GenerateName(Type type)
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
    }
}
