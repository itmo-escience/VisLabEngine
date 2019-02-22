using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Containers
{
    public class FreePlacement : UIContainer
    {
        protected override SolidBrushD2D DebugBrush => new SolidBrushD2D(new Color4(0, 1, 1, 1));
        protected override TextFormatD2D DebugTextFormat => new TextFormatD2D("Consolas", 14);

        public FreePlacement() : base() { }

        public FreePlacement(float x, float y, float width, float height, bool needClipping = false) : base(x, y, width, height, needClipping) { }

        public override void DebugDraw(SpriteLayerD2D layer)
        {
            base.DebugDraw(layer);
            layer.Draw(new TransformCommand(GlobalTransform));

            layer.Draw(new Rect(0, 0, Width, Height, DebugBrush));
        }
    }
}
