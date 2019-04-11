using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;
using Label = Fusion.Engine.Frames2.Components.Label;

namespace Fusion.Engine.Frames2.Controllers
{
    public class TextBoxSlot : IControllerSlot, ISlotAttachable
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

        internal TextBoxSlot(string name, TextBoxController parent)
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
    
    public class TextBoxController : UIController<TextBoxSlot>
    {
        public static ControllerState Editing = new ControllerState("Editing");
        protected override IEnumerable<ControllerState> NonDefaultStates => new List<ControllerState> { Editing };

        private readonly List<TextBoxSlot> _slots;
        protected override IEnumerable<IControllerSlot> MainControllerSlots => _slots;
        protected override IEnumerable<IControllerSlot> AdditionalControllerSlots { get; } = new List<IControllerSlot>();

        public TextBoxSlot Text { get; }
        public TextBoxSlot Background { get; }

        private Label _label;

        public TextBoxController(string styleName = UIStyleManager.DefaultStyle)
        {
            _label = new Label("Label", new TextFormatD2D("Calibri", 12));

            Style = UIStyleManager.Instance.GetStyle(GetType(), styleName);

            Background = new TextBoxSlot("Background", this);
            Text = new TextBoxSlot("Text", this);
            Background.Attach(new Border());
            Text.Attach(_label);

            _slots = new List<TextBoxSlot> { Background, Text };

            Events.Enter += OnEnter;
            Events.Leave += OnLeave;
            Events.MouseDownOutside += OnMouseDownOutside;
            Events.MouseDown += OnMouseDown;
            Events.KeyPress += OnKeyPress;

            Input += (sender, args) => { };
        }

        public TextBoxController():this(UIStyleManager.DefaultStyle) { }

        /*public override void DefaultInit()
        {
            Width = 100;
            Height = 100;

            _label.MaxWidth = Width;
            _label.MaxHeight = Height;
            _label.Width = Width;
            _label.Height = Height;
            _label.Text = "TextBox";

            Background.Attach(new Border(0, 0, Width, Height));
        }*/

        public event EventHandler<InputEventArgs> Input;

        public class InputEventArgs : EventArgs
        {
            public string Text { get; }

            public InputEventArgs(string text)
            {
                Text = text;
            }
        }

        private const char BackspaceCharCode = (char)8;
        private const char EscapeCharCode = (char)27;

        private void OnKeyPress(UIComponent sender, KeyPressEventArgs e)
        {
            if(CurrentState != Editing)
                return;

            switch (e.KeyChar)
            {
                case BackspaceCharCode:
                    _label.Text = _label.Text.Substring(0, _label.Text.Length - 1); break;
                case EscapeCharCode:
                    ChangeState(ControllerState.Default);
                    return;
                default: _label.Text += e.KeyChar; break;
            }

            Input?.Invoke(this, new InputEventArgs(_label.Text));
        }

        private void OnMouseDown(UIComponent sender, ClickEventArgs e)
        {
            if(CurrentState != ControllerState.Disabled)
                ChangeState(Editing);
        }

        private void OnMouseDownOutside(UIComponent sender, ClickEventArgs e)
        {
            if(CurrentState == Editing)
                ChangeState(ControllerState.Default);
        }

        private void OnEnter(UIComponent sender)
        {
            if (CurrentState == ControllerState.Default)
                ChangeState(ControllerState.Hovered);
        }

        private void OnLeave(UIComponent sender)
        {
            if(CurrentState == ControllerState.Hovered)
                ChangeState(ControllerState.Default);
        }
    }
}