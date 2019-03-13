using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Controllers
{
    public class ButtonController : UIController
    {
        public static State Pressed = new State("Pressed");
        protected override IEnumerable<State> NonDefaultStates => new List<State> {Pressed};

        public Slot Foreground { get; }
        public Slot Background { get; }

        public ButtonController() : this(0,  0,  0,  0)
        {
        }

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

        public override void DefaultInit()
        {
            Width = 100;
            Height = 100;

            Background.Attach(new Border(0, 0, 100, 100));
            Foreground.Attach(new Label("Button", new TextFormatD2D("Calibry", 12), 0, 0, 100, 100));


            var bgColor = new UIController.PropertyValue("BackgroundColor", Color4.White);
            bgColor[UIController.State.Hovered] = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
            bgColor[ButtonController.Pressed] = new Color4(0.0f, 1.0f, 1.0f, 1.0f);

            var color = new UIController.PropertyValue("BackgroundColor", new Color4(1.0f, 1.0f, 0.0f, 1.0f));
            color[UIController.State.Hovered] = new Color4(1.0f, 0.0f, 1.0f, 1.0f);
            color[ButtonController.Pressed] = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

            Background.Properties.Add(bgColor);
            Background.Properties.Add(color);
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
            if (CurrentState != Pressed) return;

            if (sender.IsInside(e.Position))
            {
                ButtonClick?.Invoke(this, new ButtonClickEventArgs(this));
                ChangeState(State.Hovered);
            }
            else
            {
                ChangeState(State.Default);
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
