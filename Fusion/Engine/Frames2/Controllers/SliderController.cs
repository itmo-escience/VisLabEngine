using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Utils;
using Fusion.Engine.Graphics.SpritesD2D;
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
    public class SliderController : UIController, IXmlSerializable
    {
        protected override IEnumerable<IControllerSlot> ControllerSlots { get; }

        public static ControllerState Pressed = new ControllerState("Pressed");
        public static ControllerState PressedHovered = new ControllerState("PressedHovered");
        protected override IReadOnlyCollection<ControllerState> NonDefaultStates { get; } = new[] { Pressed, PressedHovered };

        public SimpleControllerSlot LeftTrack { get; }
        public SimpleControllerSlot RightTrack { get; }
        public SimpleControllerSlot Thumb { get; }
        public ParentFillingSlot Background { get; }

        public float MinValue { get; set;}
        public float MaxValue { get; set;}
        private float _value;
        public float Value
        {
            get => _value;
            set { _value = Math.Max(MinValue, Math.Min(MaxValue, value)); }
        }

        private float _xDelta;

        public SliderController()
        {
            LeftTrack = new SimpleControllerSlot("LeftTrack", this);
            RightTrack = new SimpleControllerSlot("RightTrack", this);
            Thumb = new SimpleControllerSlot("Thumb", this);
            Background = new ParentFillingSlot("Background", this);
            ControllerSlots = new List<IControllerSlot>() { Background, LeftTrack, RightTrack, Thumb };

            Style = UIStyleManager.Instance.GetStyle(GetType());

            Events.MouseDown += OnMouseDown;
            Events.MouseUp += OnMouseUp;
            Events.MouseUpOutside += OnMouseUp;
            Events.MouseMove += OnMouseMove;
            Events.MouseMoveOutside += OnMouseMove;
            Thumb.ComponentAttached += (s, e) =>
            {
                if (e.New != null)
                {
                    e.New.Events.Enter += OnThumbEnter;
                    e.New.Events.Leave += OnThumbLeave;
                }
                if (e.Old != null)
                {
                    e.Old.Events.Enter -= OnThumbEnter;
                    e.Old.Events.Leave -= OnThumbLeave;
                }
            };

            SliderValueChange += (sender, args) => { };
        }

        public override void DefaultInit()
        {
            base.DefaultInit();
            MinValue = 0;
            MaxValue = 100;
            Value = 50;
            DesiredWidth = 200;
            DesiredHeight = 25;
            Background.Attach(new Border());
            LeftTrack.Attach(new Border(Color.LightGray, Color.White) { DesiredHeight = 5 });
            RightTrack.Attach(new Border(Color.DarkGray, Color.White) { DesiredHeight = 5 });
            Thumb.Attach(new Border(Color.Gray, Color.White) { DesiredWidth = 10, DesiredHeight = 20 });
        }

        public override void Update(GameTime gameTime)
        {
            var tracksIndent = Thumb.Component.DesiredWidth / 2;

            var tracksRatio = (Value - MinValue) / (MaxValue - MinValue);
            var valuePosition = tracksRatio * (Placement.Width - 2 * tracksIndent) + tracksIndent;

            LeftTrack.X = tracksIndent;
            LeftTrack.Y = (Placement.Height - LeftTrack.Component.DesiredHeight) / 2;
            LeftTrack.Width = valuePosition - tracksIndent;
            LeftTrack.Height = LeftTrack.Component.DesiredHeight;

            RightTrack.X = valuePosition;
            RightTrack.Y = (Placement.Height - RightTrack.Component.DesiredHeight) / 2;
            RightTrack.Width = Placement.Width - valuePosition - tracksIndent;
            RightTrack.Height = RightTrack.Component.DesiredHeight;

            Thumb.X = valuePosition - Thumb.Component.DesiredWidth / 2;
            Thumb.Y = (Placement.Height - Thumb.Component.DesiredHeight) / 2;
            Thumb.Width = Thumb.Component.DesiredWidth;
            Thumb.Height = Thumb.Component.DesiredHeight;
        }

        public override void Draw(SpriteLayerD2D layer)
        {
            //TODO: draw marks (or add TickBar)
        }

        #region Events

        private void OnThumbEnter(IUIComponent sender)
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

        private void OnThumbLeave(IUIComponent sender)
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
                _xDelta = Thumb.X - e.Position.X;
            }
        }

        private void OnMouseUp(IUIComponent sender, ClickEventArgs e)
        {
            if (CurrentState == Pressed)
            {
                ChangeState(ControllerState.Default);
                SliderValueChange.Invoke(this, new SliderValueChangeEventArgs(Value));
            }
            if (CurrentState == PressedHovered)
            {
                ChangeState(ControllerState.Hovered);
                SliderValueChange.Invoke(this, new SliderValueChangeEventArgs(Value));
            }
        }

        private void OnMouseMove(IUIComponent sender, MoveEventArgs e)
        {
            if (CurrentState != Pressed && CurrentState != PressedHovered) return;

            var desiredX = e.Position.X + _xDelta;
            var tracksRatio = desiredX / (DesiredWidth - Thumb.Component.DesiredWidth);
            Value = tracksRatio * (MaxValue - MinValue) + MinValue;
        }

        public event EventHandler<SliderValueChangeEventArgs> SliderValueChange;

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
            MinValue = float.Parse(reader.GetAttribute("MinValue"));
            MaxValue = float.Parse(reader.GetAttribute("MaxValue"));
            Value = float.Parse(reader.GetAttribute("Value"));

            reader.ReadStartElement();

            reader.ReadStartElement("Slots");
            Background.ReadFromXml(reader);
            LeftTrack.ReadFromXml(reader);
            RightTrack.ReadFromXml(reader);
            Thumb.ReadFromXml(reader);
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("DesiredWidth", DesiredWidth.ToString());
            writer.WriteAttributeString("DesiredHeight", DesiredHeight.ToString());
            writer.WriteAttributeString("StyleName", Style.Name);
            writer.WriteAttributeString("MinValue", MinValue.ToString());
            writer.WriteAttributeString("MaxValue", MaxValue.ToString());
            writer.WriteAttributeString("Value", Value.ToString());

            writer.WriteStartElement("Slots");
            Background.WriteToXml(writer);
            LeftTrack.WriteToXml(writer);
            RightTrack.WriteToXml(writer);
            Thumb.WriteToXml(writer);
            writer.WriteEndElement();
        }
    }
}
