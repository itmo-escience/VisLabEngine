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
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.Graph;

namespace FusionUI.UI.Elements
{
    public class GisRenderTargtDraw : RenderTargetDraw
    {
        public Gis Gis;
        public GlobeCamera Camera;

        public List<Gis.GisLayer> layers;

        public GisRenderTargtDraw(FrameProcessor ui, float x, float y, float w, float h) : base(ui, x, y, w, h, "", Color.White, null)
        {
            Target = new RenderTarget2D(Game.GraphicsDevice, ColorFormat.Rgba8, 1024, 1024);
            Gis = new Gis(Game);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex)
        {
            DepthStencilSurface depth;
            RenderTargetSurface[] surfaces;
            var dev = Game.GraphicsDevice;
            dev.GetTargets(out depth, out surfaces);
            dev.SetTargets(null, Target);

            Gis.Draw(gameTime, StereoEye.Mono, layers);

            dev.SetTargets(depth, surfaces);

            base.DrawFrame(gameTime, spriteLayer, clipRectIndex);
        }
    }
}

