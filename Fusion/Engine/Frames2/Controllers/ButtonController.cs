using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Controllers
{
    public class ButtonSlot : IControllerSlot, ISlotAttachable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public float X => 0;
        public float Y => 0;
        public float Angle => 0;
        public float Width => Parent.Placement.Width;
        public float Height => Parent.Placement.Width;
        public float AvailableWidth => Width;
        public float AvailableHeight => Height;
        public bool Clip => true;
        public bool Visible { get; set; } = true;

        public IUIContainer Parent { get; }
        public UIComponent Component { get; private set; }
        public SolidBrushD2D DebugBrush => new SolidBrushD2D(Color4.White);
        public TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);

        public string Name { get; }

        internal ButtonSlot(string name, ButtonController parent)
        {
            Parent = parent;
            Name = name;
        }

        public void DebugDraw(SpriteLayerD2D layer) { }

        public void Attach(UIComponent component)
        {
            var s = component.Placement;

            if (s != null)
            {
                if (s is ISlotAttachable sa)
                {
                    sa.Detach();
                }
                else
                {
                    Log.Error("Attempt to attach component from unmodifiable slot");
                    return;
                }
            }

            UIComponent old = null;
            if (Component != null)
            {
                old = Component;
                Component.Placement = null;
            }

            component.Placement = this;
            Component = component;
            ComponentAttached?.Invoke(this, new SlotAttachmentChangedEventArgs(old, component));
        }

        public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;
        public ObservableCollection<PropertyValueStates> Properties { get; } = new ObservableCollection<PropertyValueStates>();
    }

    public class ButtonController : UIController<ButtonSlot>
    {
        public static ControllerState Pressed = new ControllerState("Pressed");
        protected override IEnumerable<ControllerState> NonDefaultStates => new List<ControllerState> { Pressed };

        private readonly List<ButtonSlot> _slots;
        protected override IEnumerable<IControllerSlot> MainControllerSlots => _slots;
        protected override IEnumerable<IControllerSlot> AdditionalControllerSlots { get; } = new List<IControllerSlot>();

        public ButtonSlot Foreground { get; }
        public ButtonSlot Background { get; }

        public ButtonController(string styleName = UIStyleManager.DefaultStyle)
        {
            Style = UIStyleManager.Instance.GetStyle(this.GetType(), styleName);

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
