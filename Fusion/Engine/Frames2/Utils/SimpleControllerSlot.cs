using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Frames2.Controllers;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Utils
{
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
        public IUIComponent Component { get; private set; }

        public SolidBrushD2D DebugBrush { get; } = new SolidBrushD2D(Color4.White);
        public TextFormatD2D DebugTextFormat => UIManager.DefaultDebugTextFormat;

        public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;
        public event PropertyChangedEventHandler PropertyChanged;

        internal SimpleControllerSlot(string name, IUIContainer parent)
        {
            Parent = parent;
            Name = name;
        }

        public void Attach(IUIComponent component)
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

            IUIComponent old = null;
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
}