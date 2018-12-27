using System.Collections.Generic;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using SharpDX.DirectWrite;
using BlendState = Fusion.Drivers.Graphics.BlendState;
using DepthStencilState = Fusion.Drivers.Graphics.DepthStencilState;
using RasterizerState = Fusion.Drivers.Graphics.RasterizerState;
using SamplerState = Fusion.Drivers.Graphics.SamplerState;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class SpriteEngineD2D : GameModule
    {
        private enum Flags
        {
            OPAQUE = 0x0001,
            ALPHA_BLEND = 0x0002,
            ALPHA_BLEND_PREMUL = 0x0004,
            ADDITIVE = 0x0008,
            SCREEN = 0x0010,
            MULTIPLY = 0x0020,
            NEG_MULTIPLY = 0x0040,
            ALPHA_ONLY = 0x0080,
        }

        private readonly RenderSystem _rs;
        private readonly GraphicsDevice _device;
        private Ubershader _shader;
        private StateFactory _factory;

        public SpriteEngineD2D(RenderSystem rs) : base(rs.Game)
        {
            _rs = rs;
            _device = _rs.Device;
        }

        public override void Initialize()
        {
            LoadContent();

            Game.Reloading += (s, e) => LoadContent();
        }

        private void LoadContent()
        {
            _shader = _device.Game.Content.Load<Ubershader>("spriteD2D");
            _factory = _shader.CreateFactory(typeof(Flags), (ps, i) => StateEnum(ps, (Flags)i));
        }

        private void StateEnum(PipelineState ps, Flags flags)
        {
            switch (flags)
            {
                case Flags.OPAQUE: ps.BlendState = BlendState.Opaque; break;
                case Flags.ALPHA_BLEND: ps.BlendState = BlendState.AlphaBlend; break;
                case Flags.ALPHA_BLEND_PREMUL: ps.BlendState = BlendState.AlphaBlendPremul; break;
                case Flags.ADDITIVE: ps.BlendState = BlendState.Additive; break;
                case Flags.SCREEN: ps.BlendState = BlendState.Screen; break;
                case Flags.MULTIPLY: ps.BlendState = BlendState.Multiply; break;
                case Flags.NEG_MULTIPLY: ps.BlendState = BlendState.NegMultiply; break;
                case Flags.ALPHA_ONLY: ps.BlendState = BlendState.AlphaMaskWrite; break;
            }

            ps.RasterizerState = RasterizerState.CullNone;
            ps.DepthStencilState = DepthStencilState.None;
            ps.Primitive = Primitive.TriangleStrip;
            ps.VertexInputElements = VertexInputElement.FromStructure(typeof(SpriteVertex));
        }

        internal void DrawSprites(GameTime gameTime, RenderTargetSurface targetSurface, IList<SpriteLayerD2D> layers)
        {
            _device.ResetStates();

            DrawSprites2D(gameTime, layers);

            _device.SetTargets(null, targetSurface);
            _device.PipelineState = _factory[(int)Flags.ALPHA_BLEND];
            _device.PixelShaderSamplers[0] = SamplerState.PointClamp;

            foreach (var layer in layers)
            {
                _device.PixelShaderResources[0] = layer.ShaderResource;
                _device.Draw(4, 0);
            }
        }

        private void DrawSprites2D(GameTime gameTime, IEnumerable<SpriteLayerD2D> layers)
        {
            foreach (var layer in layers)
            {
                using (new PixEvent("SpriteLayerD2D"))
                {
                    layer.Render(gameTime);
                }
            }
        }
    }
}
