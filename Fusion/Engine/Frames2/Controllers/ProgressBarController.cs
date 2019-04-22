using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Components;
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
    public class ProgressBarController : UIController, IXmlSerializable
    {
        protected override IEnumerable<IControllerSlot> ControllerSlots { get; }

        public SimpleControllerSlot Track { get; }
        public ParentFillingSlot Background { get; }

        public float MinValue { get; set;}
        public float MaxValue { get; set;}
        private float _value;
        public float Value
        {
            get => _value;
            set { _value = Math.Max(MinValue, Math.Min(MaxValue, value)); }
        }

        private bool _isIndeterminate;
        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                _isIndeterminate = value;
                _indeterminateValue = -_indeterminateValuePart * (MaxValue - MinValue);
            }
        }
        private const float _indeterminateValuePart = 0.25f;
        private float _indeterminateValue;

        public ProgressBarController()
        {
            Track = new SimpleControllerSlot("Track", this);
            Background = new ParentFillingSlot("Background", this);
            ControllerSlots = new List<IControllerSlot>() { Background, Track };

            Style = UIStyleManager.Instance.GetStyle(GetType());
        }

        public override void DefaultInit()
        {
            base.DefaultInit();
            MinValue = 0;
            MaxValue = 100;
            Value = 0;
            IsIndeterminate = false;
            DesiredWidth = 200;
            DesiredHeight = 25;
            Background.Attach(new Border(Color.LightGray, Color.Gray));
            Track.Attach(new Border(Color.Green, Color.Gray));
        }

        public override void Update(GameTime gameTime)
        {
            Track.Y = 0;
            Track.Height = Track.Component.DesiredHeight;

            if (!IsIndeterminate)
            {
                var trackRatio = (Value - MinValue) / (MaxValue - MinValue);
                Track.X = 0;
                Track.Width = trackRatio * DesiredWidth;
            }
            else
            {
                _indeterminateValue++;
                if (_indeterminateValue > MaxValue) 
                    _indeterminateValue = -_indeterminateValuePart * (MaxValue - MinValue);

                if (_indeterminateValue < MinValue)
                {
                    Track.X = 0;
                }
                else
                {
                    var trackRatio = (_indeterminateValue - MinValue) / (MaxValue - MinValue);
                    Track.X = trackRatio * DesiredWidth;
                }

                var trackEndValue = _indeterminateValue + _indeterminateValuePart * (MaxValue - MinValue);

                if (trackEndValue > MaxValue)
                {
                    Track.Width = DesiredWidth - Track.X;
                }
                else
                {
                    var trackRatio = (trackEndValue - MinValue) / (MaxValue - MinValue);
                    Track.Width = trackRatio * DesiredWidth - Track.X;
                }
            }
        }

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
            reader.ReadStartElement("ProgressBarController");

            reader.ReadStartElement("Slots");
            Background.ReadFromXml(reader);
            Track.ReadFromXml(reader);
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

            writer.WriteStartElement("Slots");
            Background.WriteToXml(writer);
            Track.WriteToXml(writer);
            writer.WriteEndElement();
        }
    }
}
