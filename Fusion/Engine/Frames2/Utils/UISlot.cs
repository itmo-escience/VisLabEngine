using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Utils
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
        IUIComponent Component { get; }

        SolidBrushD2D DebugBrush { get; }
        TextFormatD2D DebugTextFormat { get; }
        void DebugDraw(SpriteLayerD2D layer);
    }

    public interface ISlotAttachable : ISlot
    {
        void Attach(IUIComponent component);

        event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;
    }

    public class SlotAttachmentChangedEventArgs : EventArgs
    {
        public IUIComponent Old { get; }
        public IUIComponent New { get; }

        public SlotAttachmentChangedEventArgs(IUIComponent oldComponent, IUIComponent newComponent)
        {
            Old = oldComponent;
            New = newComponent;
        }
    }

    public interface ISlotSerializable : ISlot
    {
        void WriteToXml(XmlWriter writer);
        void ReadFromXml(XmlReader reader);
    }

    public static class SlotExtensions
    {
        public static Matrix3x2 Transform(this ISlot slot) => Matrix3x2.Rotation(slot.Angle) * Matrix3x2.Translation(slot.X, slot.Y);

        //public static RectangleF BoundingBox(this ISlot slot)
        //{
        //    var rectangle = slot.Visible
        //        ? new RectangleF(0, 0, slot.Width, slot.Height)
        //        : new RectangleF(0, 0, 0, 0);
        //    return rectangle;
        //}

        public static void ReleaseComponent(this ISlot slot)
        {
            slot.Component.Placement = null;

            if (slot.GetType().GetField("PropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(slot) is Delegate handler) {
                foreach (var subscriber in handler.GetInvocationList()) {
                    slot.PropertyChanged -= subscriber as PropertyChangedEventHandler;
                }
            }
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
}