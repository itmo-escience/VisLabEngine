using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{
    public sealed class FreePlacementSlot : ISlotAttachable
    {
        internal FreePlacementSlot(FreePlacement holder, float x, float y, float width, float height)
        {
            InternalHolder = holder;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

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
        public IUIContainer<ISlot> Parent => InternalHolder;

        public UIComponent Component { get; private set; }

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
    }

    public class FreePlacement : IUIModifiableContainer<FreePlacementSlot>
    {
        private readonly AsyncObservableCollection<FreePlacementSlot> _slots = new AsyncObservableCollection<FreePlacementSlot>();
        public IEnumerable<FreePlacementSlot> Slots => _slots;

        public event PropertyChangedEventHandler PropertyChanged;
        public ISlot Placement { get; set; }
        public UIEventsHolder Events { get; } = new UIEventsHolder();

        public float DesiredWidth { get; set; } = -1;
        public float DesiredHeight { get; set; } = -1;

        public object Tag { get; set; }
        public string Name { get; set; }

		private readonly object _childrenAccessLock = new object();
		public object ChildrenAccessLock => _childrenAccessLock;

		public void Update(GameTime gameTime)
        {
            foreach (var slot in Slots)
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
			return Insert(child, index, 0, 0, 100, 100);
		}

		public FreePlacementSlot Insert( UIComponent child, int index, float x, float y, float width, float height )
		{
			var slot = new FreePlacementSlot(this, x, y, width, height);
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

		public FreePlacementSlot Add( UIComponent child)
		{
			return Insert(child, int.MaxValue);
		}

		public FreePlacementSlot Add( UIComponent child, float x, float y, float width, float height )
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

			return true;
        }

		public void DefaultInit()
		{
			Name = this.GetType().Name;
		}
	}
}
