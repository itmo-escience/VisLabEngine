using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2.Controllers;
using Fusion.Engine.Frames2.Managing;
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

        bool Clip { get; }
        bool Visible { get; }

        IUIContainer Parent { get; }
        UIComponent Component { get; }

        SolidBrushD2D DebugBrush { get; }
        TextFormatD2D DebugTextFormat { get; }
        void DebugDraw(SpriteLayerD2D layer);
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
        internal static Matrix3x2 Transform(this ISlot slot) => Matrix3x2.Transformation(1.0f, 1.0f, slot.Angle, slot.X, slot.Y);

        public static RectangleF BoundingBox(this ISlot slot)
        {
            var rectangle = slot.Visible
                ? new RectangleF(0, 0, slot.Width, slot.Height)
                : new RectangleF(0, 0, 0, 0);
            return rectangle;
        }

        public static bool IsInside(this ISlot slot, Vector2 point)
        {
            Matrix3x2 invertTransform = Transform(slot);
            invertTransform.Invert();
            Vector2 localPoint = Matrix3x2.TransformPoint(invertTransform, point);
            return ((localPoint.X >= 0) && (localPoint.Y >= 0) && (localPoint.X < slot.Width) && (localPoint.Y < slot.Height));
        }

        public static void Detach(this ISlotAttachable slot)
        {
            slot.Attach(null);
        }
    }

    public class SimpleControllerSlot : PropertyChangedHelper, IControllerSlot, ISlotAttachable
    {
        private float _x;
        public float X
        {
            get => _x;
            internal set => SetAndNotify(ref _x, value);
        }

        private float _y;
        public float Y
        {
            get => _y;
            internal set => SetAndNotify(ref _y, value);
        }

        private float _angle;
        public float Angle
        {
            get => _angle;
            internal set => SetAndNotify(ref _angle, value);
        }

        private float _width;
        public float Width
        {
            get => _width;
            internal set => SetAndNotify(ref _width, value);
        }

        private float _height;
        public float Height
        {
            get => _height;
            internal set => SetAndNotify(ref _height, value);
        }

        public float AvailableWidth => MathUtil.Clamp(Parent.Placement.Width - X, 0, float.MaxValue);
        public float AvailableHeight => MathUtil.Clamp(Parent.Placement.Height - Y, 0, float.MaxValue);

        public string Name { get; }

        private bool _clip = true;
        public bool Clip
        {
            get => _clip;
            set => SetAndNotify(ref _clip, value);
        }

        private bool _visible = true;
        public bool Visible
        {
            get => _visible;
            set => SetAndNotify(ref _visible, value);
        }

        public IUIContainer Parent { get; }
        public UIComponent Component { get; private set; }

        public SolidBrushD2D DebugBrush => new SolidBrushD2D(Color4.White);
        public TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);

        public ObservableCollection<PropertyValueStates> Properties { get; } = new ObservableCollection<PropertyValueStates>();

        public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;
        public event PropertyChangedEventHandler PropertyChanged;

        internal SimpleControllerSlot(string name, IUIContainer parent)
        {
            Parent = parent;
            Name = name;
        }

        public void Attach(UIComponent component)
        {
            var s = component.Placement;

            if (s != null)
            {
                if (s is ISlotAttachable sa)
                {
                    sa.Detach();
                }
                else
                {
                    Log.Error("Attempt to attach component from unmodifiable");
                    return;
                }
            }

            UIComponent old = null;
            if (Component != null)
            {
                old = Component;
                Component.Placement = null;
            }

            component.Placement = this;
            Component = component;
            ComponentAttached?.Invoke(this, new SlotAttachmentChangedEventArgs(old, component));
        }

        public void DebugDraw(SpriteLayerD2D layer)
        {
        }
    }
}