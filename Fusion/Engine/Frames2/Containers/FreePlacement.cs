using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{
    public class FreePlacementSlot : PropertyChangedHelper, ISlotAttachable
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

        public float AvailableWidth => MathUtil.Clamp(Holder.Placement.Width - X, 0, float.MaxValue);
        public float AvailableHeight => MathUtil.Clamp(Holder.Placement.Height - Y, 0, float.MaxValue);

        private Matrix3x2 _transform = Matrix3x2.Identity;
        public Matrix3x2 Transform
        {
            get => _transform;
            set => SetAndNotify(ref _transform, value);
        }

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
        public IUIContainer<ISlot> Holder => InternalHolder;

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

        #endregion
    }

    public class FreePlacement : IUIModifiableContainer<FreePlacementSlot>
    {
        private readonly AsyncObservableCollection<FreePlacementSlot> _slots = new AsyncObservableCollection<FreePlacementSlot>();
        public IEnumerable<FreePlacementSlot> Slots => _slots;

        public event PropertyChangedEventHandler PropertyChanged;
        public ISlot Placement { get; set; }

        public float DesiredWidth { get; set; } = -1;
        public float DesiredHeight { get; set; } = -1;

        public object Tag { get; set; }
        public string Name { get; set; }

        public void Update(GameTime gameTime)
        {
            foreach (var slot in Slots)
            {
                if (slot.Component.DesiredWidth >= 0)
                    slot.Width = slot.Component.DesiredWidth;

                if (slot.Component.DesiredHeight >= 0)
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
