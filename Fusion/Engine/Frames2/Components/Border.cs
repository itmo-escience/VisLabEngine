using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public class Border : UIComponent
    {
        public Color4 BackgroundColor { get; set; } = Color4.Zero;
        public Color4 Color { get; set; } = Color4.White;

        public ISlot Placement { get; set; }

        public float DesiredWidth { get; set; } = -1;
        public float DesiredHeight { get; set; } = -1;

        public object Tag { get; set; }
        public string Name { get; set; }

        public void Update(GameTime gameTime) { }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(new FillRect(0, 0, Placement.Width, Placement.Height, new SolidBrushD2D(BackgroundColor)));
            layer.Draw(new Rect(0, 0, Placement.Width, Placement.Height, new SolidBrushD2D(Color)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
