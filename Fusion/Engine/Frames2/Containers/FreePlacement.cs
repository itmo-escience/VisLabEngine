using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Frames2.Utils;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{
    public sealed class FreePlacementSlot : ISlotAttachable, ISlotSerializable
    {
        internal FreePlacementSlot(FreePlacement holder, float x, float y)
        {
            InternalHolder = holder;
            X = x;
            Y = y;
        }

        internal FreePlacementSlot(FreePlacement holder) : this(holder, 0, 0) {}

        #region ISlot
        public float X { get; set; }

        public float Y { get; set; }

        public float Angle { get; set; }

        public float Width { get; internal set; }

        public float Height { get; internal set; }

		public float AvailableWidth => MathUtil.Clamp(Parent.Placement.Width - X, 0, float.MaxValue);
        public float AvailableHeight => MathUtil.Clamp(Parent.Placement.Height - Y, 0, float.MaxValue);

		public bool Clip { get; set; } = true;

		public bool Visible { get; set; } = true;

		internal IUIModifiableContainer<FreePlacementSlot> InternalHolder { get; }
        public IUIContainer Parent => InternalHolder;

        public IUIComponent Component { get; private set; }

        public SolidBrushD2D DebugBrush => UIManager.DefaultDebugBrush;
        public TextFormatD2D DebugTextFormat => UIManager.DefaultDebugTextFormat;
        public void DebugDraw(SpriteLayerD2D layer) { }
        #endregion

        #region ISlotAttachable

        public void Attach(IUIComponent newComponent)
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

		public void SetTransform(Matrix3x2 newMatrix)
		{
			this.X = newMatrix.M31;
			this.Y = newMatrix.M32;
			//TODO
			//Add Angle and Scale
			this.Angle = 0;
		}

        public override string ToString() => $"FreePlacementSlot with {Component}";

        public void WriteToXml(XmlWriter writer)
        {
            writer.WriteStartElement("FreePlacementSlot");
            UIComponentSerializer.WriteValue(writer, X);
            UIComponentSerializer.WriteValue(writer, Y);
            UIComponentSerializer.WriteValue(writer, Angle);
            UIComponentSerializer.WriteValue(writer, Clip);
            UIComponentSerializer.WriteValue(writer, Visible);
            UIComponentSerializer.WriteValue(writer, new SeralizableObjectHolder(Component));
            writer.WriteEndElement();
        }

        public void ReadFromXml(XmlReader reader)
        {
            reader.ReadStartElement("FreePlacementSlot");
            X = UIComponentSerializer.ReadValue<float>(reader);
            Y = UIComponentSerializer.ReadValue<float>(reader);
            Angle = UIComponentSerializer.ReadValue<float>(reader);
            Clip = UIComponentSerializer.ReadValue<bool>(reader);
            Visible = UIComponentSerializer.ReadValue<bool>(reader);
            Attach(UIComponentSerializer.ReadValue<SeralizableObjectHolder>(reader).SerializableFrame);
            reader.ReadEndElement();
        }
    }

    public class FreePlacement : IUIModifiableContainer<FreePlacementSlot>, IXmlSerializable
    {
        private readonly AsyncObservableCollection<FreePlacementSlot> _slots = new AsyncObservableCollection<FreePlacementSlot>();
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
                var component = slot.Component;

                if (component.DesiredWidth < 0)
                {
                    slot.Width = slot.AvailableWidth;
                }
                else {
                    // If it's not changed than it's strictly equal
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (component.DesiredWidth >= 0 && slot.Width != component.DesiredWidth)
                        slot.Width = component.DesiredWidth;
                }

                if (component.DesiredHeight < 0)
                {
                    slot.Height = slot.AvailableHeight;
                }
                else
                {
                    // If it's not changed than it's strictly equal
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (component.DesiredHeight >= 0 && slot.Height != component.DesiredHeight)
                        slot.Height = component.DesiredHeight;
                }
            }
        }

        public void Draw(SpriteLayerD2D layer) { }

        public int IndexOf(IUIComponent child)
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

        public bool Contains(IUIComponent component) => _slots.Any(slot => slot.Component == component);

        public FreePlacementSlot Insert(IUIComponent child, int index)
        {
			return Insert(child, index, 0, 0);
		}

		public FreePlacementSlot Insert( IUIComponent child, int index, float x, float y)
		{
			var slot = new FreePlacementSlot(this, x, y);
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

		public FreePlacementSlot Add( IUIComponent child)
		{
			return Insert(child, int.MaxValue);
		}

		public FreePlacementSlot Add( IUIComponent child, float x, float y)
		{
			return Insert(child, int.MaxValue, x, y);
		}


		public bool Remove(IUIComponent child)
        {
            var slot = _slots.FirstOrDefault(s => s.Component == child);
            if (slot == null)
                return false;

			_slots.Remove(slot);
			slot.ReleaseComponent();

			return true;
        }

        public void DefaultInit()
        {
            Name = this.GetType().Name;
            DesiredWidth = UIManager.DefaultContainerSize;
            DesiredHeight = UIManager.DefaultContainerSize;
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
            reader.ReadStartElement("FreePlacement");

            _slots.Clear();
            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement("Slots");
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    var slot = new FreePlacementSlot(this);
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
