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

        public FreePlacement(float x, float y, float width, float height) : base(x, y, width, height)
        {
            debugBrush = new SolidBrushD2D(new Color4(0, 1, 1, 1));
            debugTextFormat = new TextFormatD2D("Consolas", 14);
        }
    }
}
