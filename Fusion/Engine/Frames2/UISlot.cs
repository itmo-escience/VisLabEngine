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
        float Width { get; }
        float Height { get; }
        Matrix3x2 Transform { get; }
        bool Clip { get; }
        bool Visible { get; }

        UIContainer<ISlot> GetParent();
        UIComponent Component { get; }

        SolidBrushD2D DebugBrush { get; }
        TextFormatD2D DebugTextFormat { get; }
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
        public static RectangleF BoundingBox(this ISlot slot)
        {
            var rectangle = slot.Visible
                ? new RectangleF(0, 0, slot.Width, slot.Height)
                : new RectangleF(0, 0, 0, 0);
            return rectangle.GetBound(slot.Transform);
        }

        public static bool IsInside(this ISlot slot, Vector2 point)
        {
            Matrix3x2 invertTransform = slot.Transform;
            invertTransform.Invert();
            Vector2 localPoint = Matrix3x2.TransformPoint(invertTransform, point);
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