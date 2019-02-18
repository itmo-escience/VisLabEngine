using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{
    public class FreePlacement : UIContainer
    {
        public override void Update(GameTime gameTime)
        {

        }

        public FreePlacement() : base() {
            debugBrush = new SolidBrushD2D(new Color4(0, 1, 1, 1));
            debugTextFormat = new TextFormatD2D("Consolas", 14);
        }

        public FreePlacement(float x, float y, float width, float height, bool needClipping = false) : base(x, y, width, height, needClipping)
        {
            debugBrush = new SolidBrushD2D(new Color4(0, 1, 1, 1));
            debugTextFormat = new TextFormatD2D("Consolas", 14);
        }

        public override void DebugDraw(SpriteLayerD2D layer)
        {
            base.DebugDraw(layer);
            layer.Draw(new TransformCommand(GlobalTransform));

            layer.Draw(new Rect(0, 0, Width, Height, debugBrush));
        }
    }
}
