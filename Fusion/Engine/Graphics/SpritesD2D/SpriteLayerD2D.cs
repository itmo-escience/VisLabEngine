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

        public RenderTargetD2D WrapperTarget { get; private set; }

        public BrushD2DFactory BrushFactory { get; private set; }

        public TextFormatD2DFactory TextFormatFactory { get; private set; }

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
            WrapperTarget = new RenderTargetD2D(_target);
            BrushFactory = new BrushD2DFactory(_target);
            TextFormatFactory = new TextFormatD2DFactory(_rs.SpriteEngineD2D.DWriteFactory);

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
                command.Apply(WrapperTarget);
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
            _target.EndDraw();
        }

        public void DrawEllipse(float x, float y, float r1, float r2, Color4 color)
        {
            _isDirty = true;
            Draw(new Ellipse(x, y, r1, r2, BrushFactory.GetOrCreateSolidBrush(color)));
        }

        public void DrawRectangle(float x, float y, float w, float h, Color4 color)
        {
            _isDirty = true;
            Draw(new Rect(x, y, w, h, BrushFactory.GetOrCreateSolidBrush(color)));
        }

        public void DrawCircle(float x, float y, float r, Color4 color)
        {
            _isDirty = true;
            Draw(new Ellipse(x, y, r, r, BrushFactory.GetOrCreateSolidBrush(color)));
        }
    }

    public interface IDrawCommand
    {
        void Apply(RenderTargetD2D target);
    }

    public class Ellipse : IDrawCommand
    {
        protected readonly float X, Y, R1, R2;
        protected readonly BrushD2D Brush;

        public Ellipse(float x, float y, float r1, float r2, BrushD2D brush)
        {
            X = x;
            Y = y;
            R1 = r1;
            R2 = r2;
            Brush = brush;
        }

        public void Apply(RenderTargetD2D target)
        {
            target.DrawEllipse(new Vector2(X, Y), R1, R2, Brush, 5);
        }

        public override string ToString()
        {
            return $"Ellipse({X}, {Y}, {R1}, {R2})";
        }
    }

    public class Rect : IDrawCommand
    {
        protected readonly float X, Y, W, H;
        protected readonly BrushD2D Brush;

        public Rect(float x, float y, float w, float h, BrushD2D brush)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
            Brush = brush;
        }

        public void Apply(RenderTargetD2D target)
        {
            target.DrawRect(new RectangleF(X, Y, W, H), Brush);
        }

        public override string ToString()
        {
            return $"Rectangle ({X}, {Y}, {W}, {H})";
        }
    }

    public class Circle : Ellipse
    {
        public Circle(float x, float y, float r, BrushD2D brush) : base(x, y, r, r, brush) { }

        public override string ToString()
        {
            return $"Circle({X}, {Y}, {R1})";
        }
    }

    public class Label : IDrawCommand
    {
        public readonly string Text;
        public readonly TextFormatD2D Format;
        public readonly BrushD2D Brush;
        public readonly RectangleF Location;

        public Label(string text, RectangleF location, TextFormatD2D format, BrushD2D brush)
        {
            Text = text;
            Format = format;
            Brush = brush;
            Location = location;
        }

        public void Apply(RenderTargetD2D target)
        {
            target.DrawText(Text, Format, Location, Brush);
        }
    }
}
