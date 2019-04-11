using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{
    public class VerticalBoxSlot : ISlotAttachable
    {
        internal VerticalBoxSlot(VerticalBox holder, float x, float y, float width, float height)
        {
            InternalHolder = holder;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

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

        internal IUIModifiableContainer<VerticalBoxSlot> InternalHolder { get; }
        public IUIContainer Parent => InternalHolder;

        public UIComponent Component
        {
            get;
            private set;
        }

        public SolidBrushD2D DebugBrush => new SolidBrushD2D(new Color4(0, 1.0f, 0, 1.0f));
        public TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);
        public void DebugDraw(SpriteLayerD2D layer) { }
        #endregion

        #region ISlotAttachable

        public virtual void Attach(UIComponent newComponent)
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

		public override string ToString() => $"VerticalBoxSlot with {Component}";
    }

    public class VerticalBox : IUIModifiableContainer<VerticalBoxSlot>
    {
        private readonly AsyncObservableCollection<VerticalBoxSlot> _slots = new AsyncObservableCollection<VerticalBoxSlot>();
        public IEnumerable<ISlot> Slots => _slots;

        public ISlot Placement { get; set; }
        public UIEventsHolder Events { get; } = new UIEventsHolder();

        public float DesiredWidth { get; set; } = -1;
        public float DesiredHeight { get; set; } = -1;

        public object Tag { get; set; }
        public string Name { get; set; }

		private readonly object _childrenAccessLock = new object();
		public object ChildrenAccessLock => _childrenAccessLock;


		public event PropertyChangedEventHandler PropertyChanged;

		public HorizontalAlignment Alignment
        {
            get;
            set;
        }
        
        public enum HorizontalAlignment
        {
            Left, Center, Right
        }

        public bool IsInside(Vector2 point) => Placement.IsInside(point);

        public void Update(GameTime gameTime)
        {
            float bottomBorder = 0;
            float maxChildWidth = 0;
            foreach (var slot in _slots)
            {
                slot.Width = slot.Component.DesiredWidth;
                slot.Height = slot.Component.DesiredHeight;

                slot.Y = bottomBorder;
                bottomBorder += slot.Height;

                if (maxChildWidth < slot.Width) maxChildWidth = slot.Width;
            }

            DesiredWidth = maxChildWidth;
            DesiredHeight = bottomBorder;

            float deltaXMultiplier = 0;
            switch (Alignment)
            {
                case HorizontalAlignment.Left:
                    deltaXMultiplier = 0;
                    break;
                case HorizontalAlignment.Center:
                    deltaXMultiplier = 0.5f;
                    break;
                case HorizontalAlignment.Right:
                    deltaXMultiplier = 1;
                    break;
            }

            foreach (var slot in _slots)
            {
                slot.X = deltaXMultiplier * (DesiredWidth - slot.Width);
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

		public VerticalBoxSlot Insert( UIComponent child, int index )
		{
			return Insert(child, index, 0, 0, 100, 100);
		}

		public VerticalBoxSlot Insert( UIComponent child, int index, float x, float y, float width, float height )
		{
			var slot = new VerticalBoxSlot(this, x, y, width, height);
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

		public VerticalBoxSlot Add( UIComponent child )
		{
			return Insert(child, int.MaxValue);
		}

		public VerticalBoxSlot Add( UIComponent child, float x, float y, float width, float height )
		{
			return Insert(child, int.MaxValue, x, y, width, height);
		}

		public bool Remove(UIComponent child)
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

		public void DefaultInit()
		{
			Name = this.GetType().Name;
		}
	}
}
