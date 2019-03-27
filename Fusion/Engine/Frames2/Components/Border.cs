using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public class Border : PropertyChangedHelper, UIComponent
    {
        private Color4 _backgroundColor = Color4.Zero;
        public Color4 BackgroundColor 
        {
            get => _backgroundColor;
            set => SetAndNotify(ref _backgroundColor, value);
        }

        private Color4 _borderColor = Color4.White;
        public Color4 BorderColor 
        {
            get => _borderColor;
            set => SetAndNotify(ref _borderColor, value);
        }

        public Slot Placement { get; set; }
        public UIEventsHolder Events { get; } = new UIEventsHolder();

        public float DesiredWidth { get; set; } = -1;
        public float DesiredHeight { get; set; } = -1;

        public object Tag { get; set; }
        public string Name { get; set; }

        public Border() { }

        public Border(Color background, Color border)
        {
            BackgroundColor = background.ToColor4();
            BorderColor = border.ToColor4();
        }

        public void Update(GameTime gameTime) { }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(new FillRect(0, 0, Placement.Width, Placement.Height, new SolidBrushD2D(BackgroundColor)));
            layer.Draw(new Rect(0, 0, Placement.Width, Placement.Height, new SolidBrushD2D(BorderColor)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString() => "Border";
    }
}
