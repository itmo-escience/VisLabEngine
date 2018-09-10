using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI.Elements
{
    public class RenderTargetDraw : ScalableFrame
    {

        public RenderTarget2D Target;
        private TargetTexture rtTexture;
        public RenderTargetDraw(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor, RenderTarget2D target = null) : base(ui, x, y, w, h, text, backColor)
        {
            Target = target;
        }

        protected override void initialize()
        {
            base.initialize();

            Target = Target ?? new RenderTarget2D(Game.GraphicsDevice, ColorFormat.Rgba8, 512, 512);
            rtTexture = new TargetTexture(Target);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex)
        {
            base.DrawFrame(gameTime, spriteLayer, clipRectIndex);

            spriteLayer.Draw(rtTexture, this.GetBorderedRectangle(), Color.White, clipRectIndex);
        }
    }
}
