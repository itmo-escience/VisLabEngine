using System;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.Graph;

namespace FusionUI.UI.Elements
{
    public class GraphRenderTargtDraw : RenderTargetDraw
    {
        public GraphLayer layer;
        public bool Clear = true;

        [Obsolete("Please use constructor without FrameProcessor")]
        public GraphRenderTargtDraw(FrameProcessor ui, float x, float y, float w, float h) : this(x, y, w, h) { }

        public GraphRenderTargtDraw(float x, float y, float w, float h) : base(x, y, w, h, "", Color.White, null)
        {
            Target = new RenderTarget2D(Game.Instance.GraphicsDevice, ColorFormat.Rgba8, 2048, 2048);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex)
        {
            DepthStencilSurface depth;
            RenderTargetSurface[] surfaces;
            var dev = Game.Instance.GraphicsDevice;
            dev.SetViewport(0, 0, GlobalRectangle.Width, GlobalRectangle.Height);
            dev.GetTargets(out depth, out surfaces);
            dev.SetTargets(null, Target);
            if (Clear)
            {
                dev.Clear(Target.Surface, Color.Black);
            }

            layer.Draw(gameTime, StereoEye.Mono);


            if (depth != null)
            {
                dev.SetTargets(depth, surfaces);
            }

            base.DrawFrame(gameTime, spriteLayer, clipRectIndex);
        }
    }
}