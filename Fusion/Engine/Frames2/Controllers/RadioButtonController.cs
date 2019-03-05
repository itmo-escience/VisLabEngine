using Fusion.Engine.Frames2.Managing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Frames2.Controllers
{
    public class RadioButtonController : UIController
    {
        public static State Pressed = new State("Pressed");
        public static State Checked = new State("Checked");
        public static State CheckedHovered = new State("HoveredChecked");
        public static State CheckedDisabled = new State("DisabledChecked");
        protected override IEnumerable<State> NonDefaultStates => new List<State> { Pressed, Checked, CheckedHovered, CheckedDisabled };

        public Slot RadioButton { get; }
        public Slot Text { get; }
        public Slot Background { get; }

        public RadioButtonController(float x, float y) : base(x, y, 100, 25)
        {
            RadioButton = new Slot("RadioButton");
            Text = new Slot("Text");
            Background = new Slot("Background");

            SlotsInternal.Add(RadioButton);
            SlotsInternal.Add(Text);
            SlotsInternal.Add(Background);

            RadioButton.ComponentAttached += (s, e) =>
            {
                UIComponent oldComponent = e.Old;
                UIComponent newComponent = e.New;

                if (oldComponent != null)
                {
                    oldComponent.MouseDown -= OnMouseDown;
                    oldComponent.MouseUp -= OnMouseUp;
                    oldComponent.MouseUpOutside -= OnMouseUp;
                    oldComponent.Enter -= OnEnter;
                    oldComponent.Leave -= OnLeave;
                }

                newComponent.MouseDown += OnMouseDown;
                newComponent.MouseUp += OnMouseUp;
                newComponent.MouseUpOutside += OnMouseUp;
                newComponent.Enter += OnEnter;
                newComponent.Leave += OnLeave;
            };

            RadioButtonClick += (sender, args) => { };
        }

        private void OnEnter(UIComponent sender)
        {
            if (CurrentState == State.Default)
            {
                ChangeState(State.Hovered);
            }
            if (CurrentState == Checked)
            {
                ChangeState(CheckedHovered);
            }
        }

        private void OnLeave(UIComponent sender)
        {
            if (CurrentState == State.Hovered)
            {
                ChangeState(State.Default);
            }
            if (CurrentState == CheckedHovered)
            {
                ChangeState(Checked);
            }
        }

        private void OnMouseDown(UIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == State.Disabled || CurrentState == CheckedDisabled)
                return;

            if (CurrentState == Pressed)
                return;

            ChangeState(Pressed);
        }

        private void OnMouseUp(UIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == State.Disabled || CurrentState == CheckedDisabled)
                return;

            if (CurrentState == Pressed)
            {
                RadioButtonClick?.Invoke(this, new RadioButtonClickEventArgs(this));
                ChangeState(sender.IsInside(e.Position) ? CheckedHovered : Checked);
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
