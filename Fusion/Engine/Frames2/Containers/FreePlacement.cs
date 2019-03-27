using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{
    public sealed class FreePlacementSlot : AttachableSlot
    {
        internal FreePlacementSlot(FreePlacement holder, float x, float y, float width, float height)
        {
            InternalHolder = holder;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        internal IUIModifiableContainer<FreePlacementSlot> InternalHolder { get; }

        public override IUIContainer<Slot> Parent => InternalHolder;

        public override SolidBrushD2D DebugBrush => new SolidBrushD2D(new Color4(0, 1.0f, 0, 1.0f));
        public override TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);
        public override void DebugDraw(SpriteLayerD2D layer) { }

        public override string ToString() => $"FreePlacementSlot with {Component}";
    }

    public class FreePlacement : IUIModifiableContainer<FreePlacementSlot>
    {
        private readonly AsyncObservableCollection<FreePlacementSlot> _slots = new AsyncObservableCollection<FreePlacementSlot>();
        public IEnumerable<FreePlacementSlot> Slots => _slots;

        public event PropertyChangedEventHandler PropertyChanged;
        public Slot Placement { get; set; }
        public UIEventsHolder Events { get; } = new UIEventsHolder();

        public float DesiredWidth { get; set; } = -1;
        public float DesiredHeight { get; set; } = -1;

        public object Tag { get; set; }
        public string Name { get; set; }

        public void Update(GameTime gameTime)
        {
            foreach (var slot in Slots)
            {
                if (slot.Component.DesiredWidth >= 0)
                    slot.Width = Math.Min(slot.Component.DesiredWidth, Placement.Width - slot.Component.Placement.X);

                if (slot.Component.DesiredHeight >= 0)
                    slot.Height = Math.Min(slot.Component.DesiredHeight, Placement.Height - slot.Component.Placement.Y);
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
            var slot = new FreePlacementSlot(this, 0, 0, 100, 100);
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
    }
}
