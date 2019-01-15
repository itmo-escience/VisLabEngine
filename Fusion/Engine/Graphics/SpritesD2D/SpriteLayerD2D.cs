using System.Collections.Generic;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class SpriteLayerD2D : DisposableBase
    {
        private readonly RenderSystem _rs;
        private RenderTarget _target;
        private RenderTarget2D _renderTargetFusion;
        private bool _isDirty = false;

        private RenderTargetD2D _wrapperTarget;
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
            _wrapperTarget = new RenderTargetD2D(_target);

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
                command.Apply(_wrapperTarget);
            }

            _target.EndDraw();

            _isDirty = false;
        }

        private readonly List<IDrawCommand> _drawCommands = new List<IDrawCommand>();

        public void Draw(IDrawCommand command)
        {
            _isDirty = true;
            _drawCommands.Add(command);
        }

        public void Clear()
        {
            _drawCommands.Clear();

            _target.BeginDraw();
            _target.Clear(null);
            _target.Transform = Matrix3x2.Identity.ToRawMatrix3X2();
            _target.EndDraw();
        }

        public void DrawEllipse(float x, float y, float r1, float r2, Color4 color)
        {
            _isDirty = true;
            Draw(new Ellipse(x, y, r1, r2, new SolidBrushD2D(color)));
        }

        public void DrawRectangle(float x, float y, float w, float h, Color4 color)
        {
            _isDirty = true;
            Draw(new Rect(x, y, w, h, new SolidBrushD2D(color)));
        }

        public void DrawCircle(float x, float y, float r, Color4 color)
        {
            _isDirty = true;
            Draw(new Ellipse(x, y, r, r, new SolidBrushD2D(color)));
        }
    }
}
