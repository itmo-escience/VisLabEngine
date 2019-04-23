using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class StackOfPages : IUIModifiableContainer<ParentFillingSlot>, IXmlSerializable
    {
        private readonly AsyncObservableCollection<ParentFillingSlot> _slots = new AsyncObservableCollection<ParentFillingSlot>();
        public IEnumerable<ISlot> Slots => _slots;

        public event PropertyChangedEventHandler PropertyChanged;
        public ISlot Placement { get; set; }
        public UIEventsHolder Events { get; } = new UIEventsHolder();

        public float DesiredWidth { get; set; } = -1;
        public float DesiredHeight { get; set; } = -1;

        public object Tag { get; set; }
        public string Name { get; set; }

        public object ChildrenAccessLock { get; } = new object();
        public bool IsInside(Vector2 point) => Placement.IsInside(point);

		public void Update( GameTime gameTime ) {}

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

		public ParentFillingSlot Insert( IUIComponent child, int index)
		{
			var slot = new ParentFillingSlot(child.Name, this);
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

		public ParentFillingSlot Add( IUIComponent child)
		{
			return Insert(child, int.MaxValue);
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

            reader.ReadStartElement();

            _slots.Clear();
            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement("Slots");
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    var slot = new ParentFillingSlot(this);
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
