using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Graphics.SpritesD2D;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Fusion.Engine.Frames2.Controllers
{
    public class ButtonController : UIController, IXmlSerializable
    {
        public static ControllerState Pressed = new ControllerState("Pressed");
        protected override IEnumerable<ControllerState> NonDefaultStates => new List<ControllerState> { Pressed };

        private readonly List<ParentFillingSlot> _slots;
        protected override IEnumerable<IControllerSlot> MainControllerSlots => _slots;
        protected override IEnumerable<IControllerSlot> AdditionalControllerSlots { get; } = new List<IControllerSlot>();

        public ParentFillingSlot Foreground { get; private set; }
        public ParentFillingSlot Background { get; private set; }

        public ButtonController(string styleName = UIStyleManager.DefaultStyle)
        {
            Style = UIStyleManager.Instance.GetStyle(this.GetType(), styleName);

            Foreground = new ParentFillingSlot("Foreground", this);
            Background = new ParentFillingSlot("Background", this);
			Foreground.Attach(new Label("Button", "Calibri", 12));
			Background.Attach(new Border());

            _slots = new List<ParentFillingSlot> { Background, Foreground};

            Events.MouseDown += OnMouseDown;
            Events.MouseUp += OnMouseUp;
            Events.MouseUpOutside += OnMouseUpOutside;
            Events.Enter += OnEnter;
            Events.Leave += OnLeave;

            ButtonClick += (sender, args) => { };
        }

		public ButtonController():this(UIStyleManager.DefaultStyle) { }

        public override void DefaultInit()
        {
            base.DefaultInit();
            DesiredWidth = 50;
            DesiredHeight = 50;
        }

        #region Events

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
            reader.ReadStartElement("ButtonController");

            reader.ReadStartElement("Slots");
            Background = ParentFillingSlot.ReadFromXml(reader, this);
            Foreground = ParentFillingSlot.ReadFromXml(reader, this);
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
            Foreground.WriteToXml(writer);
            writer.WriteEndElement();
        }
    }
}
