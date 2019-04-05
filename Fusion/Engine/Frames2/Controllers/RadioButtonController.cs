//using Fusion.Engine.Frames2.Managing;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Fusion.Core.Mathematics;
//using Fusion.Engine.Graphics.SpritesD2D;
//using Fusion.Engine.Frames2.Components;

//namespace Fusion.Engine.Frames2.Controllers
//{
//    public class RadioButtonController : UIController
//    {
//        public static State Pressed = new State("Pressed");
//        public static State Checked = new State("Checked");
//        public static State CheckedHovered = new State("HoveredChecked");
//        public static State CheckedDisabled = new State("DisabledChecked");
//        protected override IEnumerable<State> NonDefaultStates => new List<State> { Pressed, Checked, CheckedHovered, CheckedDisabled };

//        public Slot RadioButton { get; }
//        public Slot Text { get; }
//        public Slot Background { get; }

//        public RadioButtonController() : this(0, 0, 0, 0) {}

//        public RadioButtonController(float x, float y, float width, float height) : base(x, y, width, height)
//        {
//            RadioButton = new Slot("RadioButton");
//            Text = new Slot("Text");
//            Background = new Slot("Background");

//            SlotsInternal.Add(RadioButton);
//            SlotsInternal.Add(Text);
//            SlotsInternal.Add(Background);

//            MouseDown += OnMouseDown;
//            MouseUp += OnMouseUp;
//            MouseUpOutside += OnMouseUp;
//            Enter += OnEnter;
//            Leave += OnLeave;
//        }

//        public override void DefaultInit()
//        {
//            Width = 100;
//            Height = 25;
                
//            Background.Attach(new Border(0, 0, Width, Height));
//            RadioButton.Attach(new Border(0, 0, Height, Height));
//            Text.Attach(new Label("RadioButton", new TextFormatD2D("Calibry", 12), Height, 0, Width - Height, Height));

//            var radioButtoncolor = new UIController.PropertyValue("BackgroundColor", new Color4(1.0f, 0.0f, 0.0f, 1.0f));
//            radioButtoncolor[UIController.State.Hovered] = new Color4(0.5f, 0.0f, 0.0f, 1.0f);
//            radioButtoncolor[UIController.State.Disabled] = new Color4(1.0f, 0.5f, 0.5f, 1.0f);
//            radioButtoncolor[RadioButtonController.Pressed] = new Color4(0.5f, 0.5f, 0.0f, 1.0f);
//            radioButtoncolor[RadioButtonController.Checked] = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
//            radioButtoncolor[RadioButtonController.CheckedHovered] = new Color4(0.0f, 0.5f, 0.0f, 1.0f);
//            radioButtoncolor[RadioButtonController.CheckedDisabled] = new Color4(0.5f, 1.0f, 0.5f, 1.0f);

//            RadioButton.Properties.Add(radioButtoncolor);
//        }

//        private void OnEnter(UIComponent sender)
//        {
//            if (CurrentState == State.Default)
//            {
//                ChangeState(State.Hovered);
//            }
//            if (CurrentState == Checked)
//            {
//                ChangeState(CheckedHovered);
//            }
//        }

//        private void OnLeave(UIComponent sender)
//        {
//            if (CurrentState == State.Hovered)
//            {
//                ChangeState(State.Default);
//            }
//            if (CurrentState == CheckedHovered)
//            {
//                ChangeState(Checked);
//            }
//        }

//        private void OnMouseDown(UIComponent sender, ClickEventArgs e)
//        {
//            if (CurrentState == State.Disabled || CurrentState == CheckedDisabled)
//                return;

//            if (CurrentState == Pressed)
//                return;

//            ChangeState(Pressed);
//        }

//        private void OnMouseUp(UIComponent sender, ClickEventArgs e)
//        {
//            if (CurrentState == State.Disabled || CurrentState == CheckedDisabled)
//                return;

//            if (CurrentState == Pressed)
//            {
//                RadioButtonClick?.Invoke(this, new RadioButtonClickEventArgs(this));
//                ChangeState(sender.IsInside(e.Position) ? CheckedHovered : Checked);
//            }
//        }

//        public event EventHandler<RadioButtonClickEventArgs> RadioButtonClick;

//        public class RadioButtonClickEventArgs : EventArgs
//        {
//            public RadioButtonController RadioButton { get; }

//            public RadioButtonClickEventArgs(RadioButtonController ctrl)
//            {
//                RadioButton = ctrl;
//            }
//        }
//    }
//}
