using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Graphics.SpritesD2D;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Frames2.Events;
using System.ComponentModel;

namespace Fusion.Engine.Frames2.Controllers
{
    public class RadioButtonSlot : IControllerSlot, ISlotAttachable
    {
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

		public float Angle => 0;

        public float AvailableWidth => MathUtil.Clamp(Parent.Placement.Width - X, 0, float.MaxValue);
        public float AvailableHeight => MathUtil.Clamp(Parent.Placement.Height - Y, 0, float.MaxValue);

        public bool Clip => true;
        public bool Visible => true;

        public IUIContainer Parent { get; }

        public UIComponent Component
		{
			get;
			private set;
		}

		public SolidBrushD2D DebugBrush => new SolidBrushD2D(new Color4(0, 1.0f, 0, 1.0f));
        public TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);

        public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;
		public event PropertyChangedEventHandler PropertyChanged;

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

    public class RadioButtonController : UIController
    {
        protected override IEnumerable<IControllerSlot> MainControllerSlots => new List<IControllerSlot>() { Background, RadioButton, Body };
        protected override IEnumerable<IControllerSlot> AdditionalControllerSlots => Enumerable.Empty<IControllerSlot>();

        public static ControllerState Pressed = new ControllerState("Pressed");
        public static ControllerState Checked = new ControllerState("Checked");
        public static ControllerState CheckedPressed = new ControllerState("PressedChecked");
        public static ControllerState CheckedHovered = new ControllerState("HoveredChecked");
        public static ControllerState CheckedDisabled = new ControllerState("DisabledChecked");
        protected override IEnumerable<ControllerState> NonDefaultStates { get; } = new[] { Pressed, Checked, CheckedHovered, CheckedDisabled };

        public RadioButtonSlot RadioButton { get; }
        public SimpleControllerSlot Body { get; }
        public SimpleControllerSlot Background { get; }

        private RadioButtonGroup _group;
        public RadioButtonGroup Group
        {
            get => _group;
            set
            {
                if (_group != null)
                {
                    _group.CheckedRadioButtonChange -= RespondToStateChanges;
                    _group.Remove(this);
                }

                _group = value;
                
                if (_group != null)
                {
                    _group.Add(this);
                    _group.CheckedRadioButtonChange += RespondToStateChanges;
                }
            }
        }

        public RadioButtonController(RadioButtonGroup group = null)
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

            Group = group;
        }

        public RadioButtonController(UIComponent body, RadioButtonGroup group = null) : this(group)
        {
            DesiredWidth = 100;
            DesiredHeight = 25;
            Background.Attach(new Border(Color.Gray, Color.White) { DesiredWidth = 100, DesiredHeight = 25});
            Body.Attach(body);
            RadioButton.Attach(new Border(Color.Blue, Color.White) { DesiredWidth = 25, DesiredHeight = 25});
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

        private void RespondToStateChanges(object sender, RadioButtonGroup.CheckedRadioButtonChangeEventArgs args)
        {
            if (args.OldRadioButton == this) ChangeState(ControllerState.Default);
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

            if (CurrentState == Pressed || CurrentState == CheckedPressed)
                return;

            if (CurrentState == CheckedHovered)
            {
                ChangeState(CheckedPressed);
                return;
            }

            ChangeState(Pressed);
        }

        private void OnMouseUp(UIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == ControllerState.Disabled || CurrentState == CheckedDisabled)
                return;

            if (CurrentState == Pressed || CurrentState == CheckedPressed)
            {
                RadioButtonClick?.Invoke(this, new RadioButtonClickEventArgs(this));
                ChangeState(CheckedHovered);
            }
        }

        private void OnMouseUpOutside(UIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == Pressed)
                ChangeState(ControllerState.Default);
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
