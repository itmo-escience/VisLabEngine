using System;
using System.Collections.Generic;
using Fusion.Engine.Frames2.Managing;

namespace Fusion.Engine.Frames2.Controllers
{
    public class ButtonController : UIController
    {
        public static State Pressed = new State("Pressed");
        protected override IEnumerable<State> NonDefaultStates => new List<State> {Pressed};

        public Slot Foreground { get; }
        public Slot Background { get; }

        public ButtonController(float x, float y, float width, float height) : base(x, y, width, height)
        {
            Foreground = new Slot("Foreground");
            Background = new Slot("Background");

            SlotsInternal.Add(Background);
            SlotsInternal.Add(Foreground);

            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseUpOutside += OnMouseUp;
            Enter += OnEnter;
            Leave += OnLeave;
        }

        private void OnEnter(UIComponent sender)
        {
            if (CurrentState == State.Default)
            {
                ChangeState(State.Hovered);
            }
        }

        private void OnLeave(UIComponent sender)
        {
            if (CurrentState == State.Hovered)
            {
                ChangeState(State.Default);
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
            if (CurrentState == Pressed)
            {
                ButtonClick?.Invoke(this, new ButtonClickEventArgs(this));
                ChangeState(sender.IsInside(e.Position) ? State.Hovered : State.Default);
            }
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
