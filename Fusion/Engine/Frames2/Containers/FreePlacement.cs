using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{
    public class FreePlacementSlot : ISlotAttachable
    {
        internal FreePlacementSlot(FreePlacement parent, float x, float y, float width, float height)
        {
            Parent = parent;
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

        private float _width;
        public float Width
        {
            get => _width;
            set => SetAndNotify(ref _width, value);
        }

        private float _height;
        public float Height
        {
            get => _height;
            set => SetAndNotify(ref _height, value);
        }

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

        internal UIContainer<FreePlacementSlot> Parent { get; }
        public UIContainer<ISlot> GetParent() => Parent;

        private UIComponent _component;
        public UIComponent Component
        {
            get => _component;
            set => SetAndNotify(ref _component, value);
        }

        public SolidBrushD2D DebugBrush => new SolidBrushD2D(new Color4(0, 1.0f, 0, 1.0f));
        public TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);
        #endregion

        #region ISlotAttachable
        public void Attach(UIComponent component)
        {
            var old = Component;

            Component = component;

            ComponentAttached?.Invoke(this,
                new SlotAttachmentChangedEventArgs(old, component)
            );
        }

        public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;
        #endregion

        #region PropertyChanges

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets field with new value and fires <seealso cref="PropertyChanged"/> event if provided value is different from the old one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">Private field to set.</param>
        /// <param name="value">New value.</param>
        /// <param name="propertyName">Name that will be passed in a PropertyChanged event.</param>
        /// <returns>True if new value is different and PropertyChanged event was fired, false otherwise.</returns>
        protected bool SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            NotifyPropertyChanged(propertyName);

            return true;
        }

        internal void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion PropertyChanges
    }

    public class FreePlacement : UIContainer<FreePlacementSlot>
    {
        private readonly AsyncObservableCollection<FreePlacementSlot> _slots = new AsyncObservableCollection<FreePlacementSlot>();
        public IEnumerable<FreePlacementSlot> Slots => _slots;

        public event PropertyChangedEventHandler PropertyChanged;
        public ISlot Placement { get; set; }
        public object Tag { get; set; }
        public string Name { get; set; }

        public void Update(GameTime gameTime) { }
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
