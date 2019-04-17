using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Fusion.Engine.Frames2.Containers
{
	public class AnchorBoxSlot : ISlotAttachable, ISlotSerializable
	{
		internal AnchorBoxSlot( AnchorBox holder, Fixators fixators)
		{
			InternalHolder = holder;
            Fixators = fixators;
        }

        internal AnchorBoxSlot(AnchorBox holder) : this(holder, new Fixators()) {}

		public Fixators Fixators { get; set; }

		#region ISlot
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

		internal IUIModifiableContainer<AnchorBoxSlot> InternalHolder { get; }
		public IUIContainer Parent => InternalHolder;

		public UIComponent Component
		{
			get;
			private set;
		}

		public SolidBrushD2D DebugBrush => UIManager.DefaultDebugBrush;
		public TextFormatD2D DebugTextFormat => UIManager.DefaultDebugTextFormat;
		public void DebugDraw( SpriteLayerD2D layer ) { }
		#endregion

		#region ISlotAttachable

		public virtual void Attach( UIComponent newComponent )
		{
			var old = Component;

			Component = newComponent;
			newComponent.Placement = this;

			ComponentAttached?.Invoke(this,
				new SlotAttachmentChangedEventArgs(old, newComponent)
			);
		}

		public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		public override string ToString() => $"AnchorSlot with {Component}";

        public void WriteToXml(XmlWriter writer)
        {
            writer.WriteStartElement("AnchorBoxSlot");
            UIComponentSerializer.WriteValue(writer, Angle);
            UIComponentSerializer.WriteValue(writer, Fixators);
            UIComponentSerializer.WriteValue(writer, Clip);
            UIComponentSerializer.WriteValue(writer, Visible);
            UIComponentSerializer.WriteValue(writer, new SeralizableObjectHolder(Component));
            writer.WriteEndElement();
        }

        public void ReadFromXml(XmlReader reader)
        {
            reader.ReadStartElement("AnchorBoxSlot");
            Angle = UIComponentSerializer.ReadValue<float>(reader);
            Fixators = UIComponentSerializer.ReadValue<Fixators>(reader);
            Clip = UIComponentSerializer.ReadValue<bool>(reader);
            Visible = UIComponentSerializer.ReadValue<bool>(reader);
            Attach(UIComponentSerializer.ReadValue<SeralizableObjectHolder>(reader).SerializableFrame);
            reader.ReadEndElement();
        }
	}

	public class Fixators
	{
		public float Left = -1;
		public float Top = -1;
		public float Right = -1;
		public float Bottom = -1;
    };

	public class AnchorBox : IUIModifiableContainer<AnchorBoxSlot>, IXmlSerializable
	{
		private readonly AsyncObservableCollection<AnchorBoxSlot> _slots = new AsyncObservableCollection<AnchorBoxSlot>();
		public IEnumerable<ISlot> Slots => _slots;

		public event PropertyChangedEventHandler PropertyChanged;
		public ISlot Placement { get; set; }
		public UIEventsHolder Events { get; } = new UIEventsHolder();

		public float DesiredWidth { get; set; } = -1;
		public float DesiredHeight { get; set; } = -1;

		public object Tag { get; set; }
		public string Name { get; set; }

		private readonly object _childrenAccessLock = new object();
		public object ChildrenAccessLock => _childrenAccessLock;
        public bool IsInside(Vector2 point) => Placement.IsInside(point);

		public void Update( GameTime gameTime )
		{
			foreach (var slot in _slots)
			{
				if (slot.Fixators.Left >= 0)
				{
					slot.X = slot.Fixators.Left;
					if (slot.Fixators.Right >= 0)
						slot.Width = DesiredWidth - slot.Fixators.Left - slot.Fixators.Right;
					else
						slot.Width = slot.Component.DesiredWidth;
				}
				else
				{
					if (slot.Fixators.Right >= 0)
						slot.X = DesiredWidth - slot.Fixators.Right - slot.Component.DesiredWidth;
					else
					{
						slot.Fixators.Left = 0;
						slot.X = slot.Fixators.Left;
					}
					slot.Width = slot.Component.DesiredWidth;
				}


				if (slot.Fixators.Top >= 0)
				{
					slot.Y = slot.Fixators.Top;
					if (slot.Fixators.Bottom >= 0)
						slot.Height = DesiredHeight - slot.Fixators.Top - slot.Fixators.Bottom;
					else
						slot.Height = slot.Component.DesiredHeight;
				}
				else
				{
					if (slot.Fixators.Bottom >= 0)
						slot.Y = DesiredHeight - slot.Fixators.Bottom - slot.Component.DesiredHeight;
					else
					{
						slot.Fixators.Top = 0;
						slot.X = slot.Fixators.Top;
					}
					slot.Height = slot.Component.DesiredHeight;
				}

				//if (slot.Component.DesiredWidth >= 0)
				//	slot.Width = slot.Component.DesiredWidth;

				//if (slot.Component.DesiredHeight >= 0)
				//	slot.Height = slot.Component.DesiredHeight;
			}
		}

		public void Draw( SpriteLayerD2D layer ) { }

		public int IndexOf( UIComponent child )
		{
			var idx = 0;
			foreach (var slot in _slots)
			{
				if (slot.Component == child)
					return idx;
				idx++;
			}

			return idx;
		}

		public bool Contains( UIComponent component ) => _slots.Any(slot => slot.Component == component);

		public AnchorBoxSlot Insert( UIComponent child, int index)
		{
			var slot = new AnchorBoxSlot(this);
			slot.Attach(child);

			lock (ChildrenAccessLock)
			{
				if (index < _slots.Count)
					_slots.Insert(index, slot);
				else
					_slots.Add(slot);
			}
			return slot;
		}

		public AnchorBoxSlot Add( UIComponent child )
		{
			return Insert(child, int.MaxValue);
		}

		public bool Remove( UIComponent child )
		{
			var slot = _slots.FirstOrDefault(s => s.Component == child);
			if (slot == null)
				return false;

			_slots.Remove(slot);
			slot.Component.Placement = null;

			var handler = slot.GetType().GetField("PropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(slot) as Delegate;
			if (handler == null)
			{
				//no subscribers
			}
			else
			{
				foreach (var subscriber in handler.GetInvocationList())
				{
					slot.PropertyChanged -= subscriber as PropertyChangedEventHandler;
				}
				//now you have the subscribers
			}

			return true;
		}

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("Name");
            DesiredWidth = float.Parse(reader.GetAttribute("DesiredWidth"));
            DesiredHeight = float.Parse(reader.GetAttribute("DesiredHeight"));
            reader.ReadStartElement("AnchorBox");
            reader.ReadStartElement("Slots");

            _slots.Clear();
            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement("Slots");
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    var slot = new AnchorBoxSlot(this);
                    slot.ReadFromXml(reader);
                    _slots.Add(slot);
                }
                reader.ReadEndElement();
            }
            else
            {
                reader.ReadStartElement("Slots");
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("DesiredWidth", DesiredWidth.ToString());
            writer.WriteAttributeString("DesiredHeight", DesiredHeight.ToString());
            writer.WriteStartElement("Slots");

            foreach (var slot in _slots)
            {
                slot.WriteToXml(writer);
            }

            writer.WriteEndElement();
        }
    }
}
