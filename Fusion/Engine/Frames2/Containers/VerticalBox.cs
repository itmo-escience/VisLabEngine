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
    public sealed class VerticalBoxSlot : AttachableSlot
    {
        internal VerticalBoxSlot(VerticalBox holder, float x, float y, float width, float height)
        {
            InternalHolder = holder;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        internal IUIModifiableContainer<VerticalBoxSlot> InternalHolder { get; }

        public override IUIContainer<Slot> Parent => InternalHolder;

        public override SolidBrushD2D DebugBrush => new SolidBrushD2D(new Color4(0, 1.0f, 0, 1.0f));
        public override TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);
        public override void DebugDraw(SpriteLayerD2D layer) { }

        #region ISlotAttachable

        public override void Attach(UIComponent newComponent)
        {
            var old = Component;

            Component = newComponent;
            newComponent.Placement = this;

            ComponentAttached?.Invoke(this,
                new SlotAttachmentChangedEventArgs(old, newComponent)
            );
        }

        public override event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;

        #endregion

        public override string ToString() => $"FreePlacementSlot with {Component}";
    }

    public enum VerticalAlignment
    {
        Left, Center, Right
    }

    public class VerticalBox : PropertyChangedHelper, IUIModifiableContainer<VerticalBoxSlot>
    {
        private readonly AsyncObservableCollection<VerticalBoxSlot> _slots = new AsyncObservableCollection<VerticalBoxSlot>();
        public IEnumerable<VerticalBoxSlot> Slots => _slots;

        public event PropertyChangedEventHandler PropertyChanged;
        public Slot Placement { get; set; }
        public UIEventsHolder Events { get; } = new UIEventsHolder();

        private const float MinSideSize = 10;

        public float DesiredWidth { get; set; } = MinSideSize;
        public float DesiredHeight { get; set; } = MinSideSize;

        public object Tag { get; set; }
        public string Name { get; set; }

        private VerticalAlignment _alignment;
        public VerticalAlignment Alignment {
            get => _alignment;
            set => SetAndNotify(ref _alignment, value);
        }

        public void Update(GameTime gameTime)
        {
            float bottomBorder = 0;
            float maxChildWidth = 0;
            foreach (var slot in Slots)
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
                case VerticalAlignment.Left:
                    deltaXMultiplier = 0;
                    break;
                case VerticalAlignment.Center:
                    deltaXMultiplier = 0.5f;
                    break;
                case VerticalAlignment.Right:
                    deltaXMultiplier = 1;
                    break;
            }

            foreach (var slot in Slots)
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

    /*
    public enum VerticalAlignment
    {
        Left, Center, Right
    }

    public class VerticalBox : UIContainer
    {
        private VerticalAlignment _alignment;
        public VerticalAlignment Alignment {
            get => _alignment;
            set {
                SetAndNotify(ref _alignment, value);
            }
        }

        protected override void UpdateChildrenLayout()
        {
            float bottomBorder = 0;
            float maxChildWidth = 0;
            foreach (var child in Children)
            {
                child.Y += bottomBorder - child.LocalBoundingBox.Y;
                bottomBorder += child.LocalBoundingBox.Height;

                if (maxChildWidth < child.LocalBoundingBox.Width) maxChildWidth = child.LocalBoundingBox.Width;
            }

            Width = maxChildWidth;
            Height = bottomBorder;

            float deltaXMultiplier = 0;
            switch (Alignment)
            {
                case VerticalAlignment.Left:
                    deltaXMultiplier = 0;
                    break;
                case VerticalAlignment.Center:
                    deltaXMultiplier = 0.5f;
                    break;
                case VerticalAlignment.Right:
                    deltaXMultiplier = 1;
                    break;
            }

            foreach (var child in Children)
            {
                child.X += deltaXMultiplier * (maxChildWidth - child.LocalBoundingBox.Width) - child.LocalBoundingBox.X;
            }
        }

        protected override SolidBrushD2D DebugBrush { get; } = new SolidBrushD2D(new Color4(0, 1, 1, 1));
        protected override TextFormatD2D DebugTextFormat { get; } = new TextFormatD2D("Consolas", 14);

        public VerticalBox() : base() {
            Alignment = VerticalAlignment.Left;
        }

        public VerticalBox(float x, float y, float width, float height, VerticalAlignment alignment = VerticalAlignment.Left, bool needClipping = false) : base(x, y, width, height, needClipping)
        {
            DebugBrush = new SolidBrushD2D(new Color4(0, 1, 1, 1));
            DebugTextFormat = new TextFormatD2D("Consolas", 14);
            Alignment = alignment;
        }

        public override void DebugDraw(SpriteLayerD2D layer)
        {
            base.DebugDraw(layer);
            layer.Draw(new TransformCommand(GlobalTransform));

            layer.Draw(new Rect(0, 0, Width, Height, DebugBrush));

            float bottomBorder = 0;
            foreach (var child in Children)
            {
                bottomBorder += child.LocalBoundingBox.Height;
                layer.Draw(new Line(new Vector2(0, bottomBorder), new Vector2(Width, bottomBorder), DebugBrush));
            }
        }
    }
    */
}
