using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Controllers
{
    public sealed class ButtonSlot : AttachableSlot, IHasProperties
    {
        public override float X => 0;
        public override float Y => 0;
        public override float Angle => 0;
        public override float Width => Parent.Placement.Width;
        public override float Height => Parent.Placement.Width;
        public override float AvailableWidth => Width;
        public override float AvailableHeight => Height;
        public override bool Clip => true;

        public override IUIContainer<Slot> Parent { get; }
        public override SolidBrushD2D DebugBrush => new SolidBrushD2D(Color4.White);
        public override TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);

        public string Name { get; }

        internal ButtonSlot(string name, ButtonController parent)
        {
            Parent = parent;
            Name = name;
            Visible = false;
        }

        public override void DebugDraw(SpriteLayerD2D layer) { }

        public ObservableCollection<PropertyValueStates> Properties { get; } = new ObservableCollection<PropertyValueStates>();
    }

    public class ButtonController : UIController<ButtonSlot>
    {
        public static State Pressed = new State("Pressed");
        protected override IEnumerable<State> NonDefaultStates => new List<State> { Pressed };

        private readonly List<ButtonSlot> _slots;
        public override IEnumerable<ButtonSlot> Slots => _slots;

        public ButtonSlot Foreground { get; }
        public ButtonSlot Background { get; }

        public ButtonController()
        {
            Foreground = new ButtonSlot("Foreground", this);
            Background = new ButtonSlot("Background", this);

            _slots = new List<ButtonSlot> { Background, Foreground};

            Events.MouseDown += OnMouseDown;
            Events.MouseUp += OnMouseUp;
            Events.MouseUpOutside += OnMouseUpOutside;
            Events.Enter += OnEnter;
            Events.Leave += OnLeave;

            ButtonClick += (sender, args) => { };
        }

        private void OnMouseUpOutside(UIComponent sender, ClickEventArgs e)
        {
            ChangeState(State.Default);
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
            if(CurrentState == Pressed)
                ButtonClick?.Invoke(this, new ButtonClickEventArgs(this));

            ChangeState(State.Hovered);
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
