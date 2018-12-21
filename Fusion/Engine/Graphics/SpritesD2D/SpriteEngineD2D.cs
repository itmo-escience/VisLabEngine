using System.Collections.Generic;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using BlendState = Fusion.Drivers.Graphics.BlendState;
using DepthStencilState = Fusion.Drivers.Graphics.DepthStencilState;
using RasterizerState = Fusion.Drivers.Graphics.RasterizerState;
using SamplerState = Fusion.Drivers.Graphics.SamplerState;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class SpriteEngineD2D : GameModule
    {
        enum Flags
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

        private RenderTarget _renderTargetD2D;
        private RenderTarget2D _renderTargetFusion;
        private ShaderResource _shaderResource;

        private readonly List<SpriteLayerD2D> _layers = new List<SpriteLayerD2D>();

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

        void LoadContent()
        {
            var w = _rs.DisplayBounds.Width;
            var h = _rs.DisplayBounds.Height;

            _shader = _device.Game.Content.Load<Ubershader>("spriteD2D");
            _factory = _shader.CreateFactory(typeof(Flags), (ps, i) => StateEnum(ps, (Flags)i));

            _renderTargetFusion = new RenderTarget2D(_device, ColorFormat.Bgra8, w, h, false, false, true);

            var shaderResourceView = new ShaderResourceView(_device.Device, _renderTargetFusion.Surface.Resource);
            _shaderResource = new ShaderResource(_device, shaderResourceView, w, h, 1);

            var factory = new SharpDX.Direct2D1.Factory();


            using (var res = _renderTargetFusion.Surface.RTV.Resource)
            using (var surface = res.QueryInterface<Surface>())
            {
                _renderTargetD2D = new RenderTarget(
                    factory,
                    surface,
                    new RenderTargetProperties(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied))
                );
                surface.Dispose();
            }

            _layers.Add(CreateSpriteLayerD2D());
        }

        internal SpriteLayerD2D CreateSpriteLayerD2D()
        {
            return new SpriteLayerD2D(_renderTargetD2D);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="flags"></param>
        void StateEnum(PipelineState ps, Flags flags)
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


        /// <summary>
        /// Draws sprite layers
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawSprites(GameTime gameTime)
        {
            _device.ResetStates();
            _device.SetTargets(null, _renderTargetFusion);

            DrawSpritesRecursive(gameTime, _layers);

            _device.SetTargets(null, _device.BackbufferColor.Surface);
            _device.PipelineState = _factory[(int)Flags.ADDITIVE];
            _device.PixelShaderSamplers[0] = SamplerState.PointClamp;
            _device.PixelShaderResources[0] = _shaderResource;
            _device.Draw(4, 0);
        }

        /// <summary>
        /// Draw sprite layers
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="stereoEye"></param>
        /// <param name="layers"></param>
        void DrawSpritesRecursive(GameTime gameTime, IEnumerable<SpriteLayerD2D> layers)
        {
            _renderTargetD2D.BeginDraw();
            foreach (var layer in layers)
            {
                using (new PixEvent("SpriteLayerD2D"))
                {
                    layer.Draw(gameTime);
                }
            }
            _renderTargetD2D.EndDraw();
        }
    }
}
