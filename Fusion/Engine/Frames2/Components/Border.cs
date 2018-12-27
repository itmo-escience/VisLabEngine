using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public class Border : UIComponent
    {
        private Rect _rect;
        public Border(float x, float y, float width, float height) : base(x, y, width, height)
        {
            _rect = new Rect(x, y, width, height, new SolidBrushD2D(Color4.White));
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(_rect);
        }
    }
}
