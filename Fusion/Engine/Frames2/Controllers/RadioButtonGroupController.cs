using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Controllers
{
    public class RadioButtonGroupSlot : PropertyChangedHelper, IControllerSlot, ISlotAttachable
    {
        internal RadioButtonGroupSlot(RadioButtonGroupController holder, float x, float y, float width, float height)
        {
            InternalHolder = holder;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

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

        internal IUIModifiableContainer<RadioButtonGroupSlot> InternalHolder { get; }
        public IUIContainer Parent => InternalHolder;

        private UIComponent _component;
        public UIComponent Component
        {
            get => _component;
            private set => SetAndNotify(ref _component, value);
        }

        public SolidBrushD2D DebugBrush => new SolidBrushD2D(new Color4(0, 1.0f, 0, 1.0f));
        public TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);

        public ObservableCollection<PropertyValueStates> Properties { get; } = new ObservableCollection<PropertyValueStates>();

        public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;

        public virtual void Attach(UIComponent newComponent)
        {
            var old = Component;

            Component = newComponent;
            newComponent.Placement = this;

            ComponentAttached?.Invoke(this,
                new SlotAttachmentChangedEventArgs(old, newComponent)
            );
        }

        public void DebugDraw(SpriteLayerD2D layer) {}
    }

    public class RadioButtonGroupController : UIController<RadioButtonGroupSlot>, IUIModifiableContainer<RadioButtonGroupSlot>
    {
        private readonly AsyncObservableCollection<RadioButtonGroupSlot> _slots = new AsyncObservableCollection<RadioButtonGroupSlot>();
        protected override IEnumerable<IControllerSlot> MainControllerSlots => _slots;
        protected override IEnumerable<IControllerSlot> AdditionalControllerSlots => new List<IControllerSlot>() { Background };

        public SimpleControllerSlot Background { get; }

        public RadioButtonController CheckedRadioButton { get; private set; }
        
        public RadioButtonGroupController()
        {
            Background = new SimpleControllerSlot("Background", this);

            _slots.CollectionChanged += OnRadioButtonCollectionChange;
        }

        public override void Update(GameTime gameTime)
        {
            float bottomBorder = 0;
            foreach (var slot in _slots)
            {
                slot.X = 0;
                slot.Y = bottomBorder;

                slot.Width = slot.Component.DesiredWidth >= 0 
                    ? Math.Min(slot.Component.DesiredWidth, slot.AvailableWidth) 
                    : slot.AvailableWidth;

                slot.Height = slot.Component.DesiredHeight >= 0 
                    ? Math.Min(slot.Component.DesiredHeight, slot.AvailableHeight) 
                    : slot.AvailableHeight;

                bottomBorder += slot.Height;    
            }
        }

        private void OnRadioButtonCollectionChange(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems != null)
            {
                foreach (RadioButtonGroupSlot slot in args.NewItems)
                {
                    ((RadioButtonController)slot.Component).RadioButtonClick += ChangeCheckedRadioButtonTo;
                }
            }
            if (args.OldItems != null)
            {
                foreach (RadioButtonGroupSlot slot in args.OldItems)
                {
                    ((RadioButtonController)slot.Component).RadioButtonClick -= ChangeCheckedRadioButtonTo;
                }
            }
        }

        private void ChangeCheckedRadioButtonTo(object sender,  RadioButtonController.RadioButtonClickEventArgs args)
        {
            var newButton = args.RadioButton;
            if (newButton == CheckedRadioButton) return;

            CheckedRadioButton?.ChangeState(State.Default);
            CheckedRadioButton = newButton;
        }

        public RadioButtonGroupSlot Insert(UIComponent child, int index)
        {
            var slot = new RadioButtonGroupSlot(this, 0, 0, 100, 100);
            slot.Attach(child);

            _slots.Insert(index, slot);

            return slot;
        }

        public RadioButtonGroupSlot InsertRadioButtonWith(UIComponent body, int index)
        {
            var radioButton = new RadioButtonController
            {
                DesiredWidth = 100,
                DesiredHeight = 25
            };

            radioButton.Background.Attach(new Border(Color.Gray, Color.White) { DesiredWidth = 100, DesiredHeight = 25});
            radioButton.Body.Attach(body);
            radioButton.RadioButton.Attach(new Border(Color.Blue, Color.White) { DesiredWidth = 25, DesiredHeight = 25});

            var radioButtonColor = new PropertyValueStates("BackgroundColor", new Color4(1.0f, 0.0f, 0.0f, 1.0f));
            radioButtonColor[State.Hovered] = new Color4(0.5f, 0.0f, 0.0f, 1.0f);
            radioButtonColor[State.Disabled] = new Color4(1.0f, 0.5f, 0.5f, 1.0f);
            radioButtonColor[RadioButtonController.Pressed] = new Color4(0.5f, 0.5f, 0.0f, 1.0f);
            radioButtonColor[RadioButtonController.Checked] = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
            radioButtonColor[RadioButtonController.CheckedHovered] = new Color4(0.0f, 0.5f, 0.0f, 1.0f);
            radioButtonColor[RadioButtonController.CheckedDisabled] = new Color4(0.5f, 1.0f, 0.5f, 1.0f);

            radioButton.RadioButton.Properties.Add(radioButtonColor);

            var slot = new RadioButtonGroupSlot(this, 0, 0, 100, 100);
            slot.Attach(radioButton);
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
