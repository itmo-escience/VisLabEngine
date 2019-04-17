using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Graphics.SpritesD2D;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Frames2.Events;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using Fusion.Core.Utils;
using Fusion.Engine.Frames2.Utils;
using Fusion.Engine.Frames2.Containers;

namespace Fusion.Engine.Frames2.Controllers
{
    public class RadioButtonController : UIController, IXmlSerializable
    {
        protected override IEnumerable<IControllerSlot> MainControllerSlots { get; }
        protected override IEnumerable<IControllerSlot> AdditionalControllerSlots { get; } = Enumerable.Empty<IControllerSlot>();

        public static ControllerState Pressed = new ControllerState("Pressed");
        public static ControllerState Checked = new ControllerState("Checked");
        public static ControllerState CheckedPressed = new ControllerState("PressedChecked");
        public static ControllerState CheckedHovered = new ControllerState("HoveredChecked");
        public static ControllerState CheckedDisabled = new ControllerState("DisabledChecked");
        protected override IReadOnlyCollection<ControllerState> NonDefaultStates { get; } = new[] { Pressed, Checked, CheckedHovered, CheckedDisabled };

        public SimpleControllerSlot RadioButton { get; private set; }
        public SimpleControllerSlot Body { get; private set;}
        public ParentFillingSlot Background { get; private set;}

        private RadioButtonGroup _group;
        [XmlIgnore]
        public RadioButtonGroup Group
        {
            get => _group;
            set
            {
                if (_group != null)
                {
                    _group.CheckedRadioButtonChange -= RespondToStateChanges;
                    _group.Remove(this);
                }

                _group = value;
                
                if (_group != null)
                {
                    _group.Add(this);
                    _group.CheckedRadioButtonChange += RespondToStateChanges;
                }
            }
        }

        public RadioButtonController(RadioButtonGroup group = null)
        {
            RadioButton = new SimpleControllerSlot("RadioButton", this);
            Body = new SimpleControllerSlot("Body", this);
            Background = new ParentFillingSlot("Background", this);
            MainControllerSlots = new List<IControllerSlot>() { Background, RadioButton, Body };

            Style = UIStyleManager.Instance.GetStyle(GetType());

            Events.MouseDown += OnMouseDown;
            Events.MouseUp += OnMouseUp;
            Events.MouseUpOutside += OnMouseUpOutside;
            Events.Enter += OnEnter;
            Events.Leave += OnLeave;

            RadioButtonClick += (sender, args) => { };

            Group = group;
        }

        public RadioButtonController():this(null) { }

        public RadioButtonController(IUIComponent body, RadioButtonGroup group = null) : this(group)
        {
            DesiredWidth = 100;
            DesiredHeight = 25;
            Background.Attach(new Border(Color.Gray, Color.White));
            Body.Attach(body);
            RadioButton.Attach(new Border(Color.Blue, Color.White) { DesiredWidth = 25, DesiredHeight = 25});
        }

        public override void DefaultInit()
        {
            base.DefaultInit();
            DesiredWidth = 100;
            DesiredHeight = 25;
            Background.Attach(new Border(Color.Gray, Color.White));
            Body.Attach(new Label(Name, "Calibri", 14) { DesiredWidth = -1, DesiredHeight = -1});
            RadioButton.Attach(new Border(Color.Blue, Color.White) { DesiredWidth = 25, DesiredHeight = 25});
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            RadioButton.X = 0;
            RadioButton.Y = 0;
            RadioButton.Width = RadioButton.Component.DesiredWidth >= 0
                ? Math.Min(RadioButton.Component.DesiredWidth, RadioButton.AvailableWidth)
                : RadioButton.AvailableWidth;
            RadioButton.Height = RadioButton.Component.DesiredHeight >= 0
                ? Math.Min(RadioButton.Component.DesiredHeight, RadioButton.AvailableHeight)
                : RadioButton.AvailableHeight;

            Body.X = RadioButton.Width;
            Body.Y = 0;
            Body.Width = Body.Component.DesiredWidth >= 0
                ? Math.Min(Body.Component.DesiredWidth, Body.AvailableWidth)
                : Body.AvailableWidth;
            Body.Height = Body.Component.DesiredHeight >= 0
                ? Math.Min(Body.Component.DesiredHeight, Body.AvailableHeight)
                : Body.AvailableHeight;
        }

        private void RespondToStateChanges(object sender, RadioButtonGroup.CheckedRadioButtonChangeEventArgs args)
        {
            if (args.OldRadioButton == this) ChangeState(ControllerState.Default);
        }

        #region Events

        private void OnEnter(IUIComponent sender)
        {
            if (CurrentState == ControllerState.Default)
            {
                ChangeState(ControllerState.Hovered);
            }
            if (CurrentState == Checked)
            {
                ChangeState(CheckedHovered);
            }
        }

        private void OnLeave(IUIComponent sender)
        {
            if (CurrentState == ControllerState.Hovered)
            {
                ChangeState(ControllerState.Default);
            }
            if (CurrentState == CheckedHovered)
            {
                ChangeState(Checked);
            }
        }

        private void OnMouseDown(IUIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == ControllerState.Disabled || CurrentState == CheckedDisabled)
                return;

            if (CurrentState == Pressed || CurrentState == CheckedPressed)
                return;

            if (CurrentState == CheckedHovered)
            {
                ChangeState(CheckedPressed);
                return;
            }

            ChangeState(Pressed);
        }

        private void OnMouseUp(IUIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == ControllerState.Disabled || CurrentState == CheckedDisabled)
                return;

            if (CurrentState == Pressed || CurrentState == CheckedPressed)
            {
                RadioButtonClick?.Invoke(this, new RadioButtonClickEventArgs(this));
                ChangeState(CheckedHovered);
            }
        }

        private void OnMouseUpOutside(IUIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == Pressed)
                ChangeState(ControllerState.Default);
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

        #endregion

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("Name");
            var groupName = reader.GetAttribute("GroupName");
            var groupId = int.Parse(reader.GetAttribute("GroupId"));
            Group = RadioButtonManager.GetGroupBy(groupName, groupId);
            DesiredWidth = float.Parse(reader.GetAttribute("DesiredWidth"));
            DesiredHeight = float.Parse(reader.GetAttribute("DesiredHeight"));
            var styleName = reader.GetAttribute("StyleName");
            Style = UIStyleManager.Instance.GetStyle(GetType(), styleName);
            reader.ReadStartElement("RadioButtonController");

            reader.ReadStartElement("Slots");
            Background.ReadFromXml(reader);
            Body.ReadFromXml(reader);
            RadioButton.ReadFromXml(reader);
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("GroupName", Group.Name);
            writer.WriteAttributeString("GroupId", Group.Id.ToString());
            writer.WriteAttributeString("DesiredWidth", DesiredWidth.ToString());
            writer.WriteAttributeString("DesiredHeight", DesiredHeight.ToString());
            writer.WriteAttributeString("StyleName", Style.Name);

            writer.WriteStartElement("Slots");
            Background.WriteToXml(writer);
            Body.WriteToXml(writer);
            RadioButton.WriteToXml(writer);
            writer.WriteEndElement();
        }
    }
}
