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
    public class VerticalBoxSlot : PropertyChangedHelper, ISlotAttachable
    {
        internal VerticalBoxSlot(VerticalBox holder, float x, float y, float width, float height)
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
            internal set => SetAndNotify(ref _x, value);
        }

        private float _y;
        public float Y
        {
            get => _y;
            internal set => SetAndNotify(ref _y, value);
        }

        private float _angle;
        public float Angle
        {
            get => _angle;
            internal set => SetAndNotify(ref _angle, value);
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

        internal IUIModifiableContainer<VerticalBoxSlot> InternalHolder { get; }
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

        public override string ToString() => $"VerticalBoxSlot with {Component}";
    }

    public enum HorizontalAlignment
    {
        Left, Center, Right
    }

    public class VerticalBox : PropertyChangedHelper, IUIModifiableContainer<VerticalBoxSlot>
    {
        private readonly AsyncObservableCollection<VerticalBoxSlot> _slots = new AsyncObservableCollection<VerticalBoxSlot>();
        public IEnumerable<ISlot> Slots => _slots;

        public ISlot Placement { get; set; }
        public UIEventsHolder Events { get; } = new UIEventsHolder();

        public float DesiredWidth { get; set; } = -1;
        public float DesiredHeight { get; set; } = -1;

        public object Tag { get; set; }
        public string Name { get; set; }

        private HorizontalAlignment _alignment;
        public HorizontalAlignment Alignment
        {
            get => _alignment;
            set => SetAndNotify(ref _alignment, value);
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

        public VerticalBoxSlot Insert(UIComponent child, int index)
        {
            var slot = new VerticalBoxSlot(this, 0, 0, 100, 100);
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
