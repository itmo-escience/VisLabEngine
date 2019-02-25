using System;
using System.Collections.Generic;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;
using Fusion.Engine.Input;
using KeyEventArgs = Fusion.Engine.Frames2.Managing.KeyEventArgs;
using Label = Fusion.Engine.Frames2.Components.Label;

namespace Fusion.Engine.Frames2.Controllers
{
    public class TextBoxController : UIController
    {
        public static State Editing = new State("Editing");
        protected override IEnumerable<State> NonDefaultStates => new List<State> { Editing };

        public Slot Background { get; }
        public Slot Text { get; }

        private Label _label;

        public TextBoxController(float x, float y) : base(x, y, 100, 100)
        {
            _label = new Label("z", new TextFormatD2D("Calibri", 12), 0, 0, 100, 100);

            Background = new Slot("Background");
            Text = new Slot("Text");

            SlotsInternal.Add(Background);
            SlotsInternal.Add(Text);

            Text.Attach(_label);

            Enter += OnEnter;
            Leave += OnLeave;
            MouseDownOutside += OnMouseDownOutside;
            MouseDown += OnMouseDown;
            KeyPress += OnKeyPress;

            Input += (sender, args) => { };
        }

        public event EventHandler<InputEventArgs> Input;

        public class InputEventArgs : EventArgs
        {
            public string Text { get; }

            public InputEventArgs(string text)
            {
                Text = text;
            }
        }

        private void OnKeyPress(UIComponent sender, KeyEventArgs e)
        {
            if(CurrentState != Editing)
                return;

            if (e.Key == Keys.Escape)
            {
                ChangeState(State.Default);
                return;
            }

            _label.Text += e.Key.ToString();

            Input?.Invoke(this, new InputEventArgs(_label.Text));
        }

        private void OnMouseDown(UIComponent sender, ClickEventArgs e)
        {
            if(CurrentState != State.Disabled)
                ChangeState(Editing);
        }

        private void OnMouseDownOutside(UIComponent sender, ClickEventArgs e)
        {
            if(CurrentState == Editing)
                ChangeState(State.Default);
        }

        private void OnEnter(UIComponent sender)
        {
            if (CurrentState == State.Default)
                ChangeState(State.Hovered);
        }

        private void OnLeave(UIComponent sender)
        {
            if(CurrentState == State.Hovered)
                ChangeState(State.Default);
        }
    }
}
