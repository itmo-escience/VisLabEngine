using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Fusion.Engine.Frames2.Controllers
{
    public class CheckBoxController : UIController, IXmlSerializable
    {
        protected override IEnumerable<IControllerSlot> ControllerSlots { get; }

        public static ControllerState Pressed = new ControllerState("Pressed");
        public static ControllerState Checked = new ControllerState("Checked");
        public static ControllerState CheckedPressed = new ControllerState("PressedChecked");
        public static ControllerState CheckedHovered = new ControllerState("HoveredChecked");
        public static ControllerState CheckedDisabled = new ControllerState("DisabledChecked");
        protected override IReadOnlyCollection<ControllerState> NonDefaultStates { get; } = new[] { Pressed, Checked, CheckedPressed, CheckedHovered, CheckedDisabled };

        public SimpleControllerSlot CheckBox { get; private set; }
        public SimpleControllerSlot Body { get; private set;}
        public ParentFillingSlot Background { get; private set;}

        public CheckBoxController()
        {
            CheckBox = new SimpleControllerSlot("CheckBox", this);
            Body = new SimpleControllerSlot("Body", this);
            Background = new ParentFillingSlot("Background", this);
            ControllerSlots = new List<IControllerSlot>() { Background, CheckBox, Body };

            Style = UIStyleManager.Instance.GetStyle(GetType());

            Events.MouseDown += OnMouseDown;
            Events.MouseUp += OnMouseUp;
            Events.MouseUpOutside += OnMouseUpOutside;
            Events.Enter += OnEnter;
            Events.Leave += OnLeave;

            CheckBoxClick += (sender, args) => { };
        }

        public CheckBoxController(IUIComponent body) : this()
        {
            DesiredWidth = 100;
            DesiredHeight = 25;
            Background.Attach(new Border(Color.Gray, Color.White));
            Body.Attach(body);
            CheckBox.Attach(new Border(Color.Blue, Color.White) { DesiredWidth = 25, DesiredHeight = 25});
        }

        public override void DefaultInit()
        {
            base.DefaultInit();
            DesiredWidth = 100;
            DesiredHeight = 25;
            Background.Attach(new Border(Color.Gray, Color.White));
            Body.Attach(new Label(Name, "Calibri", 14) { DesiredWidth = -1, DesiredHeight = -1});
            CheckBox.Attach(new Border(Color.Blue, Color.White) { DesiredWidth = 25, DesiredHeight = 25});
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            CheckBox.X = 0;
            CheckBox.Y = 0;
            CheckBox.Width = CheckBox.Component.DesiredWidth >= 0
                ? Math.Min(CheckBox.Component.DesiredWidth, CheckBox.AvailableWidth)
                : CheckBox.AvailableWidth;
            CheckBox.Height = CheckBox.Component.DesiredHeight >= 0
                ? Math.Min(CheckBox.Component.DesiredHeight, CheckBox.AvailableHeight)
                : CheckBox.AvailableHeight;

            Body.X = CheckBox.Width;
            Body.Y = 0;
            Body.Width = Body.Component.DesiredWidth >= 0
                ? Math.Min(Body.Component.DesiredWidth, Body.AvailableWidth)
                : Body.AvailableWidth;
            Body.Height = Body.Component.DesiredHeight >= 0
                ? Math.Min(Body.Component.DesiredHeight, Body.AvailableHeight)
                : Body.AvailableHeight;
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
            if (CurrentState == ControllerState.Hovered)
            {
                ChangeState(Pressed);
            }
            if (CurrentState == CheckedHovered)
            {
                ChangeState(CheckedPressed);
            }
        }

        private void OnMouseUp(IUIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == Pressed)
            {
                CheckBoxClick?.Invoke(this, new CheckBoxClickEventArgs(this, true));
                ChangeState(CheckedHovered);
            }
            if (CurrentState == CheckedPressed)
            {
                CheckBoxClick?.Invoke(this, new CheckBoxClickEventArgs(this, false));
                ChangeState(ControllerState.Hovered);
            }
        }

        private void OnMouseUpOutside(IUIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == Pressed)
            {
                ChangeState(ControllerState.Default);
            }
            if (CurrentState == CheckedPressed)
            {
                ChangeState(Checked);
            } 
        }

        public event EventHandler<CheckBoxClickEventArgs> CheckBoxClick;

        public class CheckBoxClickEventArgs : EventArgs
        {
            public CheckBoxController CheckBox { get; }
            public bool IsChecked { get; }

            public CheckBoxClickEventArgs(CheckBoxController ctrl, bool isChecked)
            {
                CheckBox = ctrl;
                IsChecked = isChecked;
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

            reader.ReadStartElement();

            reader.ReadStartElement("Slots");
            Background.ReadFromXml(reader);
            Body.ReadFromXml(reader);
            CheckBox.ReadFromXml(reader);
            reader.ReadEndElement();

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
            Body.WriteToXml(writer);
            CheckBox.WriteToXml(writer);
            writer.WriteEndElement();
        }
    }
}
