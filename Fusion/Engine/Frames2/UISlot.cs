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

        public static void Detach(this ISlotAttachable slot)
        {
            slot.Attach(null);
        }
    }
}