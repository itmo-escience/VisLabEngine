using System;
using System.Collections.Generic;
using Fusion.Engine.Frames2.Managing;

namespace Fusion.Engine.Frames2.Controllers
{
    /*
    public class ButtonType : IControllerType { }

    public class ButtonController : UIController<ButtonType>
    {
        public static State<ButtonType> Pressed = new State<ButtonType>("Pressed");
        protected override IEnumerable<State<ButtonType>> NonDefaultStates => new List<State<ButtonType>> {Pressed};

        public Slot<ButtonType> Foreground { get; }
        public Slot<ButtonType> Background { get; }

        public ButtonController(float x, float y) : base(x, y, 100, 100)
        {
            Foreground = new Slot<ButtonType>("Foreground");
            Background = new Slot<ButtonType>("Background");

            SlotsInternal.Add(Background);
            SlotsInternal.Add(Foreground);

            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseUpOutside += OnMouseUp;
            Enter += OnEnter;
            Leave += OnLeave;

            ButtonClick += (sender, args) => { };
        }

        private void OnEnter(UIComponent sender)
        {
            if (CurrentState == State<ButtonType>.Default)
            {
                ChangeState(State<ButtonType>.Hovered);
            }
        }

        private void OnLeave(UIComponent sender)
        {
            if (CurrentState == State<ButtonType>.Hovered)
            {
                ChangeState(State<ButtonType>.Default);
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

            ChangeState(sender.IsInside(e.Position) ? State<ButtonType>.Hovered : State<ButtonType>.Default);
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
    */
}
