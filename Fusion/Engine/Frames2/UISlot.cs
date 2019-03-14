using System;
using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2
{
    public interface ISlot : INotifyPropertyChanged
    {
        float X { get; }
        float Y { get; }
        float Angle { get; }

        /* Current width and height */
        float Width { get; }
        float Height { get; }

        /* Container specifies available dimensions for component */
        float AvailableWidth { get; }
        float AvailableHeight { get; }

        Matrix3x2 Transform { get; }
        bool Clip { get; }
        bool Visible { get; }

        UIContainer<ISlot> Parent { get; }
        UIComponent Component { get; }

        SolidBrushD2D DebugBrush { get; }
        TextFormatD2D DebugTextFormat { get; }

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
    }

    public interface ISlotAttachable : ISlot
    {
        void Attach(UIComponent component);

        event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;
    }

    public class SlotAttachmentChangedEventArgs : EventArgs
    {
        public UIComponent Old { get; }
        public UIComponent New { get; }

        public SlotAttachmentChangedEventArgs(UIComponent oldComponent, UIComponent newComponent)
        {
            Old = oldComponent;
            New = newComponent;
        }
    }

    public static class SlotExtensions
    {
        internal static Matrix3x2 LocalTransform(this ISlot slot)
        {
            var m = Matrix3x2.Transformation(1.0f, 1.0f, slot.Angle, 0, 0);
            return m * Matrix3x2.Translation(slot.X, slot.Y);
        }

        public static RectangleF BoundingBox(this ISlot slot)
        {
            var rectangle = slot.Visible
                ? new RectangleF(0, 0, slot.Width, slot.Height)
                : new RectangleF(0, 0, 0, 0);
            return rectangle.GetBound(slot.Transform);
        }

        public static bool IsInside(this ISlot slot, Vector2 point)
        {
            var invertTransform = slot.Transform;
            invertTransform.Invert();
            var localPoint = Matrix3x2.TransformPoint(invertTransform, point);
            return ((localPoint.X >= 0) && (localPoint.Y >= 0) && (localPoint.X < slot.Width) && (localPoint.Y < slot.Height));
        }

        public static void DebugDraw(this ISlot slot, SpriteLayerD2D layer)
        {
            var b = slot.BoundingBox();
            layer.Draw(TransformCommand.Identity);
            layer.Draw(new Rect(b.X, b.Y, b.Width, b.Height, slot.DebugBrush));

            var debugText = $"{slot.Component.Name} X:{b.X:0.00} Y:{b.Y:0.00} W:{b.Width:0.00} H:{b.Height:0.00}";
            var dtl = new TextLayoutD2D(debugText, slot.DebugTextFormat, float.MaxValue, float.MaxValue);
            layer.Draw(new Text(debugText, new RectangleF(b.X, b.Y - dtl.Height, dtl.Width + 1, dtl.Height), slot.DebugTextFormat, slot.DebugBrush));
        }
    }
}