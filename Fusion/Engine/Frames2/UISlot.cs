using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
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

    public class SimpleControllerSlot : IControllerSlot, ISlotAttachable, ISlotSerializable
    {
        public float X
		{
			get;
			internal set;
		}

        public float Y
		{
			get;
			internal set;
		}

        public float Angle
		{
			get;
			internal set;
		}

        public float Width
		{
			get;
			internal set;
		}

        public float Height
		{
			get;
			internal set;
		}

		public float AvailableWidth => MathUtil.Clamp(Parent.Placement.Width - X, 0, float.MaxValue);
        public float AvailableHeight => MathUtil.Clamp(Parent.Placement.Height - Y, 0, float.MaxValue);

        public string Name { get; private set; }

		public bool Clip
		{
			get;
			set;
		} = true;

		public bool Visible
		{
			get;
			set;
		} = true;

		public IUIContainer Parent { get; }
        public UIComponent Component { get; private set; }

        public SolidBrushD2D DebugBrush { get; } = new SolidBrushD2D(Color4.White);
        public TextFormatD2D DebugTextFormat => UIManager.DefaultDebugTextFormat;

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

        public void WriteToXml(XmlWriter writer)
        {
            writer.WriteStartElement(Name);
            UIComponentSerializer.WriteValue(writer, X);
            UIComponentSerializer.WriteValue(writer, Y);
            UIComponentSerializer.WriteValue(writer, Width);
            UIComponentSerializer.WriteValue(writer, Height);
            UIComponentSerializer.WriteValue(writer, Angle);
            UIComponentSerializer.WriteValue(writer, Clip);
            UIComponentSerializer.WriteValue(writer, Visible);
            UIComponentSerializer.WriteValue(writer, new SeralizableObjectHolder(Component));
            writer.WriteEndElement();
        }

        public void ReadFromXml(XmlReader reader)
        {
            Name = reader.Name;
            reader.ReadStartElement();
            X = UIComponentSerializer.ReadValue<float>(reader);
            Y = UIComponentSerializer.ReadValue<float>(reader);
            Width = UIComponentSerializer.ReadValue<float>(reader);
            Height = UIComponentSerializer.ReadValue<float>(reader);
            Angle = UIComponentSerializer.ReadValue<float>(reader);
            Clip = UIComponentSerializer.ReadValue<bool>(reader);
            Visible = UIComponentSerializer.ReadValue<bool>(reader);
            Attach(UIComponentSerializer.ReadValue<SeralizableObjectHolder>(reader).SerializableFrame);
            reader.ReadEndElement();
        }
    }

    public class ParentFillingSlot : IControllerSlot, ISlotAttachable, ISlotSerializable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public float X => 0;
        public float Y => 0;
        public float Angle => 0;
        public float Width => Parent.Placement.Width;
        public float Height => Parent.Placement.Width;
        public float AvailableWidth => Width;
        public float AvailableHeight => Height;
        public bool Clip => true;
        public bool Visible { get; set; } = true;

        public IUIContainer Parent { get; }
        public UIComponent Component { get; private set; }
        public SolidBrushD2D DebugBrush { get; } = new SolidBrushD2D(Color4.White);
        public TextFormatD2D DebugTextFormat => UIManager.DefaultDebugTextFormat;

        public string Name { get; private set; }

        internal ParentFillingSlot(string name, IUIContainer parent)
        {
            Parent = parent;
            Name = name;
        }

        public void DebugDraw(SpriteLayerD2D layer) { }

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
                    Log.Error("Attempt to attach component from unmodifiable slot");
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

        public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;
        public ObservableCollection<PropertyValueStates> Properties { get; } = new ObservableCollection<PropertyValueStates>();

        public void WriteToXml(XmlWriter writer)
        {
            writer.WriteStartElement(Name);
            UIComponentSerializer.WriteValue(writer, Visible);
            UIComponentSerializer.WriteValue(writer, new SeralizableObjectHolder(Component));
            writer.WriteEndElement();
        }

        public void ReadFromXml(XmlReader reader)
        {
            Name = reader.Name;
            reader.ReadStartElement();
            Visible = UIComponentSerializer.ReadValue<bool>(reader);
            Attach(UIComponentSerializer.ReadValue<SeralizableObjectHolder>(reader).SerializableFrame);
            reader.ReadEndElement();
        }
    }
}