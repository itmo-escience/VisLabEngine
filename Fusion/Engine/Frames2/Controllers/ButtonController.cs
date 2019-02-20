using System;
using System.Collections.Generic;
using Fusion.Engine.Frames2.Managing;

namespace Fusion.Engine.Frames2.Controllers
{
    public class ButtonController : UIController
    {       
        private static readonly State Hovered  = new State("Hovered");
        private static readonly State Pressed  = new State("Pressed");

        protected override IEnumerable<State> NonDefaultStates => new List<State>
        {
            Hovered, Pressed
        };

        public Slot Foreground { get; }
        public Slot Background { get; }
        public override IReadOnlyList<Slot> Slots => new List<Slot> {Foreground, Background};

        public ButtonController()
        {
            Foreground = new Slot("Foreground");
            Background = new Slot("Background");

            Foreground.ComponentAttached += (sender, args) =>
            {
                if (args.Old != null)
                {
                    args.Old.MouseDown -= OnMouseDown;
                    args.Old.MouseUp -= OnMouseUp;
                }

                args.New.MouseDown += OnMouseDown;
                args.New.MouseUp += OnMouseUp;
            };

            ButtonClick += (sender, args) => { };
        }

        private void OnMouseDown(UIComponent sender, ClickEventArgs e)
        {
            ChangeState(Pressed);
            Log.Message("Down");
        }

        private void OnMouseUp(UIComponent sender, ClickEventArgs e)
        {
            if(CurrentState == Pressed)
                ButtonClick?.Invoke(this, new ButtonClickEventArgs(this));
            
            ChangeState(State.Default);
            Log.Message("Up");
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
