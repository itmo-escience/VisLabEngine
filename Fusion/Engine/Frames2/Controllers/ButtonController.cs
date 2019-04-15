using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Controllers
{
    public class ButtonController : UIController
    {
        public static ControllerState Pressed = new ControllerState("Pressed");
        protected override IEnumerable<ControllerState> NonDefaultStates => new List<ControllerState> { Pressed };

        private readonly List<ParentFillingSlot> _slots;
        protected override IEnumerable<IControllerSlot> MainControllerSlots => _slots;
        protected override IEnumerable<IControllerSlot> AdditionalControllerSlots { get; } = new List<IControllerSlot>();

        public ParentFillingSlot Foreground { get; }
        public ParentFillingSlot Background { get; }

        public ButtonController(string styleName = UIStyleManager.DefaultStyle)
        {
            Style = UIStyleManager.Instance.GetStyle(this.GetType(), styleName);

            Foreground = new ParentFillingSlot("Foreground", this);
            Background = new ParentFillingSlot("Background", this);
			Foreground.Attach(new Label("Button", "Calibri", 12));
			Background.Attach(new Border());

            _slots = new List<ParentFillingSlot> { Background, Foreground};

            Events.MouseDown += OnMouseDown;
            Events.MouseUp += OnMouseUp;
            Events.MouseUpOutside += OnMouseUpOutside;
            Events.Enter += OnEnter;
            Events.Leave += OnLeave;

            ButtonClick += (sender, args) => { };
        }

		public ButtonController():this(UIStyleManager.DefaultStyle) { }


		private void OnMouseUpOutside(UIComponent sender, ClickEventArgs e)
        {
            ChangeState(ControllerState.Default);
        }

        private void OnEnter(UIComponent sender)
        {
            if (CurrentState == ControllerState.Default)
            {
                ChangeState(ControllerState.Hovered);
            }
        }

        private void OnLeave(UIComponent sender)
        {
            if (CurrentState == ControllerState.Hovered)
            {
                ChangeState(ControllerState.Default);
            }
        }

        private void OnMouseDown(UIComponent sender, ClickEventArgs e)
        {
            if(CurrentState == Pressed)
                return;

            ChangeState(Pressed);
        }

        private void OnMouseUp(UIComponent sender, ClickEventArgs e)
        {
            if(CurrentState == Pressed)
                ButtonClick?.Invoke(this, new ButtonClickEventArgs(this));

            ChangeState(ControllerState.Hovered);
        }

        public event EventHandler<ButtonClickEventArgs> ButtonClick;

        public class ButtonClickEventArgs : EventArgs
        {
            public ButtonController Button { get; }

            public ButtonClickEventArgs(ButtonController ctrl)
            {
                Button = ctrl;
            }
        }
    }
}
