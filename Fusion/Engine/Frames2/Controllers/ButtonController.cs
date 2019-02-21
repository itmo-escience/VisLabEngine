using System;
using System.Collections.Generic;
using Fusion.Engine.Frames2.Managing;

namespace Fusion.Engine.Frames2.Controllers
{
    public class ButtonController : UIController
    {
        public static readonly State Hovered = new State("Hovered");
        public static readonly State Pressed = new State("Pressed");

        protected override IEnumerable<State> NonDefaultStates => new List<State> {Hovered, Pressed};

        public Slot Foreground { get; }
        public Slot Background { get; }

        public ButtonController()
        {
            Foreground = new Slot(this, "Foreground");
            Background = new Slot(this, "Background");

            SlotsInternal.Add(Background);
            SlotsInternal.Add(Foreground);

            Background.ComponentAttached += (sender, args) =>
            {
                if (args.Old != null)
                {
                    args.Old.MouseDown -= OnMouseDown;
                    args.Old.MouseUp -= OnMouseUp;
                    args.Old.MouseUpOutside -= OnMouseUp;
                    args.New.Enter -= OnEnter;
                    args.New.Leave -= OnLeave;
                }

                args.New.MouseDown += OnMouseDown;
                args.New.MouseUp += OnMouseUp;
                args.New.MouseUpOutside += OnMouseUp;
                args.New.Enter += OnEnter;
                args.New.Leave += OnLeave;
            };

            ButtonClick += (sender, args) => { };
        }

        private void OnEnter(UIComponent sender)
        {
            if (CurrentState == State.Default)
            {
                ChangeState(Hovered);
            }
        }

        private void OnLeave(UIComponent sender)
        {
            if (CurrentState == Hovered)
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
            if(CurrentState == Pressed)
                ButtonClick?.Invoke(this, new ButtonClickEventArgs(this));

            ChangeState(sender.IsInside(e.position) ? Hovered : State.Default);
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
