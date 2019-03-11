using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public class Border : UIComponent
    {
        public Color4 BackgroundColor { get; set; } = Color4.Zero;
        public Color4 Color { get; set; } = Color4.White;

        public Border() : base() { }

        public Border(float x, float y, float width, float height) : base(x, y, width, height)
        {
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(new FillRect(0, 0, Width, Height, new SolidBrushD2D(BackgroundColor)));
            layer.Draw(new Rect(0, 0, Width, Height, new SolidBrushD2D(Color)));
        }
    }
}
