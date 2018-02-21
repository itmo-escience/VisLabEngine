using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI
{
    public class Label : ScalableFrame
    {
        public bool IsFlip = false;

        public Label(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            Font.DrawString(sb, Text, this.GlobalRectangle.X, this.GlobalRectangle.Y, Color.White, 0, 0, false, IsFlip);
        }
    }
}
