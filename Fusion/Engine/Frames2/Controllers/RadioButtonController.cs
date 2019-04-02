using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Frames2.Events;

namespace Fusion.Engine.Frames2.Controllers
{
    public class RadioButtonSlot : PropertyChangedHelper, IControllerSlot, ISlotAttachable
    {
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

        public float Angle => 0;

        public float AvailableWidth => MathUtil.Clamp(Parent.Placement.Width - X, 0, float.MaxValue);
        public float AvailableHeight => MathUtil.Clamp(Parent.Placement.Height - Y, 0, float.MaxValue);

        public bool Clip => true;
        public bool Visible => true;

        public IUIContainer Parent { get; }

        private UIComponent _component;
        public UIComponent Component
        {
            get => _component;
            private set => SetAndNotify(ref _component, value);
        }

        public SolidBrushD2D DebugBrush => new SolidBrushD2D(new Color4(0, 1.0f, 0, 1.0f));
        public TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);

        public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;

        public string Name { get; }

        internal RadioButtonSlot(string name, RadioButtonController parent)
        {
            Parent = parent;
            Name = name;
        }

        public void Attach(UIComponent newComponent)
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

    public class RadioButtonController : UIController<RadioButtonSlot>
    {
        protected override IEnumerable<IControllerSlot> MainControllerSlots => new List<IControllerSlot>() { Background, RadioButton, Body };
        protected override IEnumerable<IControllerSlot> AdditionalControllerSlots { get; } = new List<IControllerSlot>();

        public static ControllerState Pressed = new ControllerState("Pressed");
        public static ControllerState Checked = new ControllerState("Checked");
        public static ControllerState CheckedHovered = new ControllerState("HoveredChecked");
        public static ControllerState CheckedDisabled = new ControllerState("DisabledChecked");
        protected override IEnumerable<ControllerState> NonDefaultStates => new List<ControllerState> { Pressed, Checked, CheckedHovered, CheckedDisabled };

        public RadioButtonSlot RadioButton { get; }
        public SimpleControllerSlot Body { get; }
        public SimpleControllerSlot Background { get; }

        public RadioButtonController()
        {
            RadioButton = new RadioButtonSlot("RadioButton", this);
            Body = new SimpleControllerSlot("Body", this);
            Background = new SimpleControllerSlot("Background", this);

            Style = UIStyleManager.Instance.GetStyle(GetType());

            Events.MouseDown += OnMouseDown;
            Events.MouseUp += OnMouseUp;
            Events.MouseUpOutside += OnMouseUpOutside;
            Events.Enter += OnEnter;
            Events.Leave += OnLeave;

            RadioButtonClick += (sender, args) => { };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Background.X = 0;
            Background.Y = 0;
            Background.Width = Background.AvailableWidth;
            Background.Height = Background.AvailableHeight;

            RadioButton.X = 0;
            RadioButton.Y = 0;
            RadioButton.Width = RadioButton.Component.DesiredWidth >= 0
                ? Math.Min(RadioButton.Component.DesiredWidth, RadioButton.AvailableWidth)
                : RadioButton.AvailableWidth;
            RadioButton.Height = RadioButton.Component.DesiredHeight >= 0
                ? Math.Min(RadioButton.Component.DesiredHeight, RadioButton.AvailableHeight)
                : RadioButton.AvailableHeight;

            Body.X = RadioButton.Width;
            Body.Y = 0;
            Body.Width = Body.Component.DesiredWidth >= 0
                ? Math.Min(Body.Component.DesiredWidth, Body.AvailableWidth)
                : Body.AvailableWidth;
            Body.Height = Body.Component.DesiredHeight >= 0
                ? Math.Min(Body.Component.DesiredHeight, Body.AvailableHeight)
                : Body.AvailableHeight;
        }

        private void OnEnter(UIComponent sender)
        {
            if (CurrentState == ControllerState.Default)
            {
                ChangeState(ControllerState.Hovered);
            }
            if (CurrentState == Checked)
            {
                ChangeState(CheckedHovered);
            }
        }

        private void OnLeave(UIComponent sender)
        {
            if (CurrentState == ControllerState.Hovered)
            {
                ChangeState(ControllerState.Default);
            }
            if (CurrentState == CheckedHovered)
            {
                ChangeState(Checked);
            }
        }

        private void OnMouseDown(UIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == ControllerState.Disabled || CurrentState == CheckedDisabled)
                return;

            if (CurrentState == Pressed)
                return;

            ChangeState(Pressed);
        }

        private void OnMouseUp(UIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == ControllerState.Disabled || CurrentState == CheckedDisabled)
                return;

            if (CurrentState == Pressed)
            {
                RadioButtonClick?.Invoke(this, new RadioButtonClickEventArgs(this));
                ChangeState(CheckedHovered);
            }
        }

        private void OnMouseUpOutside(UIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == ControllerState.Disabled || CurrentState == CheckedDisabled)
                return;

            if (CurrentState == Pressed)
            {
                RadioButtonClick?.Invoke(this, new RadioButtonClickEventArgs(this));
                ChangeState(Checked);
            }
        }

        public event EventHandler<RadioButtonClickEventArgs> RadioButtonClick;

        public class RadioButtonClickEventArgs : EventArgs
        {
            public RadioButtonController RadioButton { get; }

            public RadioButtonClickEventArgs(RadioButtonController ctrl)
            {
                RadioButton = ctrl;
            }
        }
    }
}
