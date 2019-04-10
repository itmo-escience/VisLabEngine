using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{
    public sealed class FreePlacementSlot : PropertyChangedHelper, ISlotAttachable
    {
        internal FreePlacementSlot(FreePlacement holder, float x, float y, float width, float height)
        {
            InternalHolder = holder;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        #region ISlot
        private float _x;
        public float X
        {
            get => _x;
            set => SetAndNotify(ref _x, value);
        }

        private float _y;
        public float Y
        {
            get => _y;
            set => SetAndNotify(ref _y, value);
        }

        private float _angle;
        public float Angle
        {
            get => _angle;
            set => SetAndNotify(ref _angle, value);
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

        internal IUIModifiableContainer<FreePlacementSlot> InternalHolder { get; }
        public IUIContainer Parent => InternalHolder;

        private UIComponent _component;
        public UIComponent Component
        {
            get => _component;
            private set => SetAndNotify(ref _component, value);
        }

        public SolidBrushD2D DebugBrush => new SolidBrushD2D(new Color4(0, 1.0f, 0, 1.0f));
        public TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);
        public void DebugDraw(SpriteLayerD2D layer) { }
        #endregion

        #region ISlotAttachable

        public void Attach(UIComponent newComponent)
        {
            var old = Component;

            Component = newComponent;
            newComponent.Placement = this;

            ComponentAttached?.Invoke(this,
                new SlotAttachmentChangedEventArgs(old, newComponent)
            );
        }

        public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;

        #endregion

        public override string ToString() => $"FreePlacementSlot with {Component}";
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

        public bool IsInside(Vector2 point) => Placement.IsInside(point);

        public void Update(GameTime gameTime)
        {
            foreach (var slot in _slots)
            {
                // If it's not changed than it's strictly equal
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (slot.Component.DesiredWidth >= 0 && slot.Width != slot.Component.DesiredWidth)
                    slot.Width = slot.Component.DesiredWidth;

                // If it's not changed than it's strictly equal
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (slot.Component.DesiredHeight >= 0 && slot.Height != slot.Component.DesiredHeight)
                    slot.Height = slot.Component.DesiredHeight;
            }
        }

        public void Draw(SpriteLayerD2D layer) { }

        public int IndexOf(UIComponent child)
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

        public bool Contains(UIComponent component) => _slots.Any(slot => slot.Component == component);

        public FreePlacementSlot Insert(UIComponent child, int index)
        {
            var slot = new FreePlacementSlot(this, 0, 0, child.DesiredWidth, child.DesiredHeight);
            slot.Attach(child);

            _slots.Insert(index, slot);

            return slot;
        }

        public bool Remove(UIComponent child)
        {
            var slot = _slots.FirstOrDefault(s => s.Component == child);
            if (slot == null)
                return false;

            slot.Attach(null);

            return true;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("Name");
            reader.ReadStartElement("FreePlacement");
            reader.ReadStartElement("Slots");

            reader.MoveToContent();
            if (!reader.IsEmptyElement)
            {
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("Slot");

                    var x = UIComponentSerializer.ReadValue<float>(reader);
                    var y = UIComponentSerializer.ReadValue<float>(reader);
                    var width = UIComponentSerializer.ReadValue<float>(reader);
                    var height = UIComponentSerializer.ReadValue<float>(reader);

                    var slot = new FreePlacementSlot(this, x, y, width, height)
                    {
                        Angle = UIComponentSerializer.ReadValue<float>(reader),
                        Clip = UIComponentSerializer.ReadValue<bool>(reader),
                        Visible = UIComponentSerializer.ReadValue<bool>(reader)
                    };
                    slot.Attach(UIComponentSerializer.ReadValue<SeralizableObjectHolder>(reader).SerializableFrame);
                    _slots.Add(slot);

                    reader.ReadEndElement();
                    reader.MoveToContent();
                }
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            writer.WriteStartElement("Slots");

            foreach (var slot in _slots)
            {
                writer.WriteStartElement("Slot");

                UIComponentSerializer.WriteValue(writer, slot.X);
                UIComponentSerializer.WriteValue(writer, slot.Y);
                UIComponentSerializer.WriteValue(writer, slot.Width);
                UIComponentSerializer.WriteValue(writer, slot.Height);
                UIComponentSerializer.WriteValue(writer, slot.Angle);
                UIComponentSerializer.WriteValue(writer, slot.Clip);
                UIComponentSerializer.WriteValue(writer, slot.Visible);
                UIComponentSerializer.WriteValue(writer, new SeralizableObjectHolder(slot.Component));

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}
