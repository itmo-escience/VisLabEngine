using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;

namespace FusionUI.UI.Elements
{
    public class GisRenderTargtDraw : RenderTargetDraw
    {
        public Gis Gis;
        public GlobeCamera Camera;

        public List<Gis.GisLayer> layers;

        [Obsolete("Please use constructor without FrameProcessor")]
        public GisRenderTargtDraw(FrameProcessor ui, float x, float y, float w, float h)
            : this(x, y, w, h) { }

        public GisRenderTargtDraw(float x, float y, float w, float h) : base(x, y, w, h, "", Color.White, null)
        {
            Target = new RenderTarget2D(Game.Instance.GraphicsDevice, ColorFormat.Rgba8, 1024, 1024);
            Gis = new Gis(Game.Instance);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex)
        {
            DepthStencilSurface depth;
            RenderTargetSurface[] surfaces;
            var dev = Game.Instance.GraphicsDevice;
            dev.GetTargets(out depth, out surfaces);
            dev.SetTargets(null, Target);

            Gis.Draw(gameTime, StereoEye.Mono, layers);

            dev.SetTargets(depth, surfaces);

            base.DrawFrame(gameTime, spriteLayer, clipRectIndex);
        }
    }
}

