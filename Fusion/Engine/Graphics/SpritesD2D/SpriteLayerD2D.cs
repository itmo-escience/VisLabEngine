using System.Collections.Generic;
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
        private bool _isDirty = false;

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
            _isDirty = true;
        }

        public void Render(GameTime gameTime)
        {
            if (!_isDirty) return;
            _rs.Device.SetTargets(null, _renderTargetFusion);

            _target.BeginDraw();
            _target.Clear(null);

            foreach (var command in _drawCommands)
            {
                command.Apply(_target);
            }

            _target.EndDraw();

            _isDirty = false;
        }

        private readonly List<IDrawCommand> _drawCommands = new List<IDrawCommand>();

        public void Clear()
        {
            _drawCommands.Clear();

            _target.BeginDraw();
            _target.Clear(null);
            _target.EndDraw();
        }

        public void DrawEllipse(float x, float y, float r1, float r2)
        {
            _isDirty = true;
            _drawCommands.Add(new Ellipse(x, y, r1, r2, _brush));
        }
    }

    internal interface IDrawCommand
    {
        void Apply(RenderTarget target);
    }

    internal class Ellipse : IDrawCommand
    {
        private readonly float _x, _y, _r1, _r2;
        private readonly Brush _brush;

        public Ellipse(float x, float y, float r1, float r2, Brush brush)
        {
            _x = x;
            _y = y;
            _r1 = r1;
            _r2 = r2;
            _brush = brush;
        }

        public void Apply(RenderTarget target)
        {
            target.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(_x, _y), _r1, _r2), _brush);
        }
    }
}
