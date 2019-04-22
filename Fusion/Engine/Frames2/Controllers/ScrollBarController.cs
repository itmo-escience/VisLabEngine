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
    public class ScrollBarController : UIController, IXmlSerializable
    {
        protected override IEnumerable<IControllerSlot> ControllerSlots { get; }

        public static ControllerState Pressed = new ControllerState("Pressed");
        public static ControllerState PressedHovered = new ControllerState("PressedHovered");
        protected override IReadOnlyCollection<ControllerState> NonDefaultStates { get; } = new[] { Pressed, PressedHovered };

        public SimpleControllerSlot ScrollBar { get; }
        public ParentFillingSlot Background { get; }

        public enum ScrollBarOrintation
        {
            Horizontal, Vertical
        }

        public ScrollBarOrintation Orintation { get; set;}

        public float BarSize { get; set;}
        public float ContentSize { get; set;}
        private float _scrollPosition;
        public float ScrollPosition
        {
            get => _scrollPosition;
            set { _scrollPosition = Math.Max(0, Math.Min(ContentSize - BarSize, value)); }
        }

        private float _xyDelta;

        public ScrollBarController()
        {
            ScrollBar = new SimpleControllerSlot("ScrollBar", this);
            Background = new ParentFillingSlot("Background", this);
            ControllerSlots = new List<IControllerSlot>() { Background, ScrollBar };

            Style = UIStyleManager.Instance.GetStyle(GetType());

            Events.MouseDown += OnMouseDown;
            Events.MouseUp += OnMouseUp;
            Events.MouseUpOutside += OnMouseUp;
            Events.MouseMove += OnMouseMove;
            Events.MouseMoveOutside += OnMouseMove;
            ScrollBar.ComponentAttached += (s, e) =>
            {
                if (e.New != null)
                {
                    e.New.Events.Enter += OnBarEnter;
                    e.New.Events.Leave += OnBarLeave;
                }
                if (e.Old != null)
                {
                    e.Old.Events.Enter -= OnBarEnter;
                    e.Old.Events.Leave -= OnBarLeave;
                }
            };

            ScrollPositionChange += (sender, args) => { };
        }

        public void DefaultInit(ScrollBarOrintation orintation)
        {
            base.DefaultInit();
            BarSize = 25;
            ContentSize = 100;
            ScrollPosition = 0;
            Orintation = orintation;
            Background.Attach(new Border(Color.LightGray, Color.White));
            switch (orintation)
            {
                case ScrollBarOrintation.Horizontal:
                    DesiredWidth = 200;
                    DesiredHeight = 15;
                    ScrollBar.Attach(new Border(Color.DarkGray, Color.DarkGray));
                    break;
                case ScrollBarOrintation.Vertical:
                    DesiredWidth = 15;
                    DesiredHeight = 200;
                    ScrollBar.Attach(new Border(Color.DarkGray, Color.DarkGray));
                    break;
            }
            
        }

        public override void DefaultInit() => DefaultInit(ScrollBarOrintation.Horizontal);

        public override void Update(GameTime gameTime)
        {
            var positionRatio = ScrollPosition / ContentSize;

            switch (Orintation)
            {
                case ScrollBarOrintation.Horizontal:
                    ScrollBar.X = positionRatio * Placement.Width;
                    ScrollBar.Y = 0;
                    ScrollBar.Width = BarSize / ContentSize * Placement.Width;
                    ScrollBar.Height = Placement.Height;
                    break;

                case ScrollBarOrintation.Vertical:
                    ScrollBar.X = 0;
                    ScrollBar.Y = positionRatio * Placement.Height;
                    ScrollBar.Width = Placement.Width;
                    ScrollBar.Height = BarSize / ContentSize * Placement.Height;
                    break;
            }
        }

        #region Events

        private void OnBarEnter(IUIComponent sender)
        {
            if (CurrentState == ControllerState.Default)
            {
                ChangeState(ControllerState.Hovered);
            }
            if (CurrentState == Pressed)
            {
                ChangeState(PressedHovered);
            }
        }

        private void OnBarLeave(IUIComponent sender)
        {
            if (CurrentState == ControllerState.Hovered)
            {
                ChangeState(ControllerState.Default);
            }
            if (CurrentState == PressedHovered)
            {
                ChangeState(Pressed);
            }
        }

        private void OnMouseDown(IUIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == ControllerState.Hovered)
            {
                ChangeState(PressedHovered);
                switch (Orintation)
                {
                    case ScrollBarOrintation.Horizontal:
                        _xyDelta = ScrollBar.X - e.Position.X;
                        break;
                    case ScrollBarOrintation.Vertical:
                        _xyDelta = ScrollBar.Y - e.Position.Y;
                        break;
                }
            }
        }

        private void OnMouseUp(IUIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == Pressed)
            {
                ChangeState(ControllerState.Default);
                ScrollPositionChange.Invoke(this, new SliderValueChangeEventArgs(ScrollPosition));
            }
            if (CurrentState == PressedHovered)
            {
                ChangeState(ControllerState.Hovered);
                ScrollPositionChange.Invoke(this, new SliderValueChangeEventArgs(ScrollPosition));
            }
        }

        private void OnMouseMove(IUIComponent sender, MoveEventArgs e)
        {
            if (CurrentState != Pressed && CurrentState != PressedHovered) return;

            float tracksRatio = 0;
            switch (Orintation)
            {
                case ScrollBarOrintation.Horizontal:
                    var desiredX = e.Position.X + _xyDelta;
                    tracksRatio = desiredX / Placement.Width;
                    break;
                case ScrollBarOrintation.Vertical:
                    var desiredY = e.Position.Y + _xyDelta;
                    tracksRatio = desiredY / Placement.Height;
                    break;
            }
            
            ScrollPosition = tracksRatio * ContentSize;
        }

        public event EventHandler<SliderValueChangeEventArgs> ScrollPositionChange;

        public class SliderValueChangeEventArgs : EventArgs
        {
            public float NewValue { get; }

            public SliderValueChangeEventArgs(float newValue)
            {
                NewValue = newValue;
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
            Orintation = (ScrollBarOrintation) Enum.Parse(typeof(ScrollBarOrintation), reader.GetAttribute("Orintation"));
            BarSize = float.Parse(reader.GetAttribute("BarSize"));
            ContentSize = float.Parse(reader.GetAttribute("ContentSize"));

            reader.ReadStartElement();

            reader.ReadStartElement("Slots");
            Background.ReadFromXml(reader);
            ScrollBar.ReadFromXml(reader);
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("DesiredWidth", DesiredWidth.ToString());
            writer.WriteAttributeString("DesiredHeight", DesiredHeight.ToString());
            writer.WriteAttributeString("StyleName", Style.Name);
            writer.WriteAttributeString("Orintation", Orintation.ToString());
            writer.WriteAttributeString("BarSize", BarSize.ToString());
            writer.WriteAttributeString("ContentSize", ContentSize.ToString());

            writer.WriteStartElement("Slots");
            Background.WriteToXml(writer);
            ScrollBar.WriteToXml(writer);
            writer.WriteEndElement();
        }
    }
}
