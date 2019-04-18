using System.ComponentModel;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Utils;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public class Border : IUIComponent
    {
        public Color4 BackgroundColor { get; set; }
        public Color4 BorderColor { get; set; }
        public float RadiusX { get; set; }
        public float RadiusY { get; set; }

        [XmlIgnore]
        public ISlot Placement { get; set; }
        public UIEventsHolder Events { get; } = new UIEventsHolder();

        public float DesiredWidth { get; set; } = -1;
        public float DesiredHeight { get; set; } = -1;

        public object Tag { get; set; }
        public string Name { get; set; }

        public Border() { }

        public Border(float radiusX, float radiusY) : this(radiusX, radiusX, Color.Zero, Color.White) {}

        public Border(Color background, Color border) : this(0, 0, background, border) {}

        public Border(float radiusX, float radiusY, Color background, Color border)
        {
            RadiusX = radiusX;
            RadiusY = radiusY;
            BackgroundColor = background.ToColor4();
            BorderColor = border.ToColor4();
        }

        public bool IsInside(Vector2 point) => Placement.IsInside(point);

        public void Update(GameTime gameTime) { }

        public void Draw(SpriteLayerD2D layer)
        {
            if (RadiusX == 0 || RadiusY == 0)
            {
                layer.Draw(new FillRect(0, 0, Placement.Width, Placement.Height, new SolidBrushD2D(BackgroundColor)));
                layer.Draw(new Rect(0, 0, Placement.Width, Placement.Height, new SolidBrushD2D(BorderColor)));
            }
            else
            {
                layer.Draw(new FillRoundedRect(0, 0, Placement.Width, Placement.Height, RadiusX, RadiusY, new SolidBrushD2D(BackgroundColor)));
                layer.Draw(new RoundedRect(0, 0, Placement.Width, Placement.Height, RadiusX, RadiusY, new SolidBrushD2D(BorderColor)));
            }
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString() => "Border";

		public void DefaultInit()
		{
			Name = this.GetType().Name;
            DesiredWidth = 100;
            DesiredHeight = 100;
            BackgroundColor = Color4.Zero;
            BorderColor = Color4.White;
        }
	}
}
