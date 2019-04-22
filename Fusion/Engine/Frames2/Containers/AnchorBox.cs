using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Frames2.Utils;
using Fusion.Engine.Graphics.SpritesD2D;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

		public IUIComponent Component
		{
			get;
			private set;
		}

		public SolidBrushD2D DebugBrush => UIManager.DefaultDebugBrush;
		public TextFormatD2D DebugTextFormat => UIManager.DefaultDebugTextFormat;
		public void DebugDraw( SpriteLayerD2D layer ) { }
		#endregion

		#region ISlotAttachable

		public virtual void Attach( IUIComponent newComponent )
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
            var f = UIComponentSerializer.ReadValue<Fixators>(reader);
            Fixators = f;
            Clip = UIComponentSerializer.ReadValue<bool>(reader);
            Visible = UIComponentSerializer.ReadValue<bool>(reader);
            Attach(UIComponentSerializer.ReadValue<SeralizableObjectHolder>(reader).SerializableFrame);
            reader.ReadEndElement();
        }
	}

	public class Fixators : INotifyPropertyChanged
	{
	    private float _left   = 0;
	    private float _right  = -1;
	    private float _top    = 0;
	    private float _bottom = -1;

		public float Left
		{
		    get => _left;
            set {
                if (_right < 0 && value < 0)
                {
                    _right = 0;
                }

                _left = value;
	        }
		}

	    public float Top
	    {
            get => _top;
	        set {
	            if (_bottom < 0 && value < 0)
	            {
	                _bottom = 0;
	            }

	            _top = value;
	        }
	    }

		public float Right
		{
		    get => _right;
		    set {
		        if (_left < 0 && value < 0)
		        {
		            _left = 0;
		        }

		        _right = value;
		    }
		}

		public float Bottom
		{
		    get => _bottom;
		    set {
		        if (_top < 0 && value < 0)
		        {
		            _top = 0;
		        }

		        _bottom = value;
		    }
	    }

		public event PropertyChangedEventHandler PropertyChanged;

	    public override string ToString() => $"Fixators: left={Left}, top={Top}, right={Right}, bottom={Bottom}";
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

	    public object ChildrenAccessLock { get; } = new object();

	    public bool IsInside(Vector2 point) => Placement.IsInside(point);

		public void Update( GameTime gameTime )
		{
			foreach (var slot in _slots)
			{
				if (slot.Fixators.Left >= 0)
				{
					slot.X = slot.Fixators.Left;
					if (slot.Fixators.Right >= 0)
						slot.Width = Placement.Width - (slot.Fixators.Left + slot.Fixators.Right);
					else
					{
					    if (slot.Component.DesiredWidth < 0)
					        slot.Width = slot.AvailableWidth;
					    else
					        slot.Width = Math.Min(slot.Component.DesiredWidth, slot.AvailableWidth);
					}
				}
				else // left < 0, right >=0
				{
                    Debug.Assert(slot.Fixators.Right >= 0);

				    slot.X = Math.Max(0, Placement.Width - slot.Component.DesiredWidth - slot.Fixators.Right);
				    slot.Width = Math.Min(slot.AvailableWidth, slot.Component.DesiredWidth);
				}


			    if (slot.Fixators.Top >= 0)
			    {
			        slot.Y = slot.Fixators.Top;
			        if (slot.Fixators.Bottom >= 0)
			            slot.Height = Placement.Height - (slot.Fixators.Top + slot.Fixators.Bottom);
			        else // top > 0, bottom <= 0
			        {
			            if (slot.Component.DesiredHeight < 0)
			                slot.Height = slot.AvailableHeight;
			            else
			                slot.Height = Math.Min(slot.Component.DesiredHeight, slot.AvailableHeight);
			        }
			    }
			    else // top < 0, bottom >=0
			    {
			        Debug.Assert(slot.Fixators.Bottom >= 0);

			        slot.Y = Math.Max(0, Placement.Height - slot.Component.DesiredHeight - slot.Fixators.Bottom);
			        slot.Height = Math.Min(slot.AvailableHeight, slot.Component.DesiredHeight);
			    }
			}
		}

		public void Draw( SpriteLayerD2D layer ) { }

		public int IndexOf( IUIComponent child )
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

		public bool Contains( IUIComponent component ) => _slots.Any(slot => slot.Component == component);

		public AnchorBoxSlot Insert( IUIComponent child, int index)
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

		public AnchorBoxSlot Add( IUIComponent child )
		{
			return Insert(child, int.MaxValue);
		}

		public bool Remove( IUIComponent child )
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
            DesiredWidth = 50;
            DesiredHeight = 50;
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
