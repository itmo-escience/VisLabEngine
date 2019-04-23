using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Utils;
using Fusion.Engine.Graphics.SpritesD2D;
using Label = Fusion.Engine.Frames2.Components.Label;

namespace Fusion.Engine.Frames2.Controllers
{
    public class TextBoxController : UIController, IXmlSerializable
    {
        public static ControllerState Editing = new ControllerState("Editing");
        protected override IReadOnlyCollection<ControllerState> NonDefaultStates { get; } = new List<ControllerState> { Editing };

        private readonly List<ParentFillingSlot> _slots;
        protected override IEnumerable<IControllerSlot> ControllerSlots => _slots;

        public ParentFillingSlot Text { get; private set; }
        public ParentFillingSlot Background { get; private set; }

        private Label _label;

        public TextBoxController(string styleName = UIStyleManager.DefaultStyle)
        {
            _label = new Label("Label", new TextFormatD2D("Calibri", 12));

            Style = UIStyleManager.Instance.GetStyle(GetType(), styleName);

            Background = new ParentFillingSlot("Background", this);
            Text = new ParentFillingSlot("Text", this);
            Background.Attach(new Border());
            Text.Attach(_label);

            _slots = new List<ParentFillingSlot> { Background, Text };

            Events.Enter += OnEnter;
            Events.Leave += OnLeave;
            Events.MouseDownOutside += OnMouseDownOutside;
            Events.MouseDown += OnMouseDown;
            Events.KeyPress += OnKeyPress;

            Input += (sender, args) => { };
        }

        public TextBoxController():this(UIStyleManager.DefaultStyle) { }

        public override void DefaultInit()
        {
            base.DefaultInit();

            DesiredWidth = 100;
            DesiredHeight = 25;

            _label.DesiredWidth = 100;
            _label.DesiredHeight = 25;
            _label.Text = "TextBox";

            Background.Attach(new Border());
        }

        #region Events

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

        private void OnKeyPress(IUIComponent sender, KeyPressEventArgs e)
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

        private void OnMouseDown(IUIComponent sender, ClickEventArgs e)
        {
            if(CurrentState != ControllerState.Disabled)
                ChangeState(Editing);
        }

        private void OnMouseDownOutside(IUIComponent sender, ClickEventArgs e)
        {
            if(CurrentState == Editing)
                ChangeState(ControllerState.Default);
        }

        private void OnEnter(IUIComponent sender)
        {
            if (CurrentState == ControllerState.Default)
                ChangeState(ControllerState.Hovered);
        }

        private void OnLeave(IUIComponent sender)
        {
            if(CurrentState == ControllerState.Hovered)
                ChangeState(ControllerState.Default);
        }

        #endregion

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("Name");
            DesiredWidth = float.Parse(reader.GetAttribute("DesiredWidth"));
            DesiredHeight = float.Parse(reader.GetAttribute("DesiredHeight"));
            var styleName = reader.GetAttribute("StyleName");
            Style = UIStyleManager.Instance.GetStyle(GetType(), styleName);

            reader.ReadStartElement();

            reader.ReadStartElement("Slots");
            Background.ReadFromXml(reader);
            Text.ReadFromXml(reader);
            reader.ReadEndElement();

            _label = (Label)Text.Component;

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("DesiredWidth", DesiredWidth.ToString());
            writer.WriteAttributeString("DesiredHeight", DesiredHeight.ToString());
            writer.WriteAttributeString("StyleName", Style.Name);

            writer.WriteStartElement("Slots");
            Background.WriteToXml(writer);
            Text.WriteToXml(writer);
            writer.WriteEndElement();
        }
    }
}