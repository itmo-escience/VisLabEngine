using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class SpriteLayerD2D : DisposableBase
    {
        private readonly RenderSystem _rs;
        private SolidColorBrush _brush;
        private RenderTarget _target;
        private RenderTarget2D _renderTargetFusion;

        public ShaderResource ShaderResource { get; private set; }

        public SpriteLayerD2D(RenderSystem rs)
        {
            _rs = rs;

            Initialize();
        }

        private void Initialize()
        {
            var w = _rs.DisplayBounds.Width;
            var h = _rs.DisplayBounds.Height;

            _renderTargetFusion = new RenderTarget2D(_rs.Device, ColorFormat.Bgra8, w, h, false, false, true);

            var shaderResourceView = new ShaderResourceView(_rs.Device.Device, _renderTargetFusion.Surface.Resource);
            ShaderResource = new ShaderResource(_rs.Device, shaderResourceView, w, h, 1);

            var factory = new SharpDX.Direct2D1.Factory();


            using (var res = _renderTargetFusion.Surface.RTV.Resource)
            using (var surface = res.QueryInterface<Surface>())
            {
                _target = new RenderTarget(
                    factory,
                    surface,
                    new RenderTargetProperties(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied))
                );
            }

            _brush = new SolidColorBrush(_target, SharpDX.Color.White);
        }

        public void Draw(GameTime gameTime)
        {
            _rs.Device.SetTargets(null, _renderTargetFusion);

            _target.BeginDraw();

            _target.DrawEllipse(new Ellipse(new RawVector2(100, 100), 30, 30), _brush);
            _target.DrawEllipse(new Ellipse(new RawVector2(250, 100), 30, 30), _brush);
            _target.DrawEllipse(new Ellipse(new RawVector2(250, 250), 30, 30), _brush);
            _target.DrawEllipse(new Ellipse(new RawVector2(100, 250), 30, 30), _brush);

            _target.EndDraw();
        }
    }
}
