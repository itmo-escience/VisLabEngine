using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Containers;

namespace Fusion.Engine.Frames2.Controllers
{
    public class ScrollViewerController : UIController, IXmlSerializable
    {
        protected override IEnumerable<IControllerSlot> ControllerSlots { get; }

        public SimpleControllerSlot Content { get; }
        public SimpleControllerSlot HorizontalScrollBar { get; }
        public SimpleControllerSlot VerticalScrollBar { get; }
        public ParentFillingSlot Background { get; }

        private ScrollBarController _horizontalScrollBar;
        private ScrollBarController _verticalScrollBar;

        public ScrollViewerController()
        {
            Content = new SimpleControllerSlot("Content", this);
            HorizontalScrollBar = new SimpleControllerSlot("HorizontalScrollBar", this);
            VerticalScrollBar = new SimpleControllerSlot("VerticalScrollBar", this);
            Background = new ParentFillingSlot("Background", this);
            ControllerSlots = new List<IControllerSlot>() { Background, Content, HorizontalScrollBar, VerticalScrollBar };

            Style = UIStyleManager.Instance.GetStyle(GetType());

            HorizontalScrollBar.X = 0;
            VerticalScrollBar.Y = 0;

            _horizontalScrollBar = new ScrollBarController();
            _horizontalScrollBar.DefaultInit(ScrollBarController.ScrollBarOrintation.Horizontal);
            HorizontalScrollBar.Attach(_horizontalScrollBar);

            _verticalScrollBar = new ScrollBarController();
            _verticalScrollBar.DefaultInit(ScrollBarController.ScrollBarOrintation.Vertical);
            VerticalScrollBar.Attach(_verticalScrollBar);
        }

        public override void DefaultInit()
        {
            base.DefaultInit();
            DesiredWidth = 300;
            DesiredHeight = 300;
            Background.Attach(new Border());
            Content.Attach(new FreePlacement() {DesiredWidth = 400, DesiredHeight = 400});
        }

        public override void Update(GameTime gameTime)
        {
            bool needVerticalScrollBar = Content.Component.DesiredHeight > Placement.Height;
            float borderX = !needVerticalScrollBar ? Placement.Width : Placement.Width - _verticalScrollBar.DesiredWidth;

            bool needHorizontalScrollBar = Content.Component.DesiredWidth > borderX;
            float borderY = !needHorizontalScrollBar ? Placement.Height : Placement.Height - _horizontalScrollBar.DesiredHeight;

            VerticalScrollBar.X = borderX;
            VerticalScrollBar.Width = _verticalScrollBar.DesiredWidth;
            VerticalScrollBar.Height = borderY;

            HorizontalScrollBar.Y = borderY;
            HorizontalScrollBar.Width = borderX;
            HorizontalScrollBar.Height = _horizontalScrollBar.DesiredHeight;

            UpdateContentView();
        }

        private void UpdateContentView()
        {
            _horizontalScrollBar.BarSize = VerticalScrollBar.X;
            _horizontalScrollBar.ContentSize = Content.Component.DesiredWidth;

            _verticalScrollBar.BarSize = HorizontalScrollBar.Y;
            _verticalScrollBar.ContentSize = Content.Component.DesiredHeight;

            Content.X = -_horizontalScrollBar.ScrollPosition;
            Content.Y = -_verticalScrollBar.ScrollPosition;

            Content.Width = VerticalScrollBar.X + _horizontalScrollBar.ScrollPosition;
            Content.Height = HorizontalScrollBar.Y + _verticalScrollBar.ScrollPosition;
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
            reader.ReadStartElement("ScrollViewerController");

            reader.ReadStartElement("Slots");
            Background.ReadFromXml(reader);
            Content.ReadFromXml(reader);
            HorizontalScrollBar.ReadFromXml(reader);
            VerticalScrollBar.ReadFromXml(reader);
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
            Content.WriteToXml(writer);
            HorizontalScrollBar.WriteToXml(writer);
            VerticalScrollBar.WriteToXml(writer);
            writer.WriteEndElement();
        }
    }
}
