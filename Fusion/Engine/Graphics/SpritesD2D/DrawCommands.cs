using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct2D1;
using Matrix3x2 = Fusion.Core.Mathematics.Matrix3x2;
using RectangleF = Fusion.Core.Mathematics.RectangleF;
using Vector2 = Fusion.Core.Mathematics.Vector2;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public interface IDrawCommand
    {
        void Apply(RenderTargetD2D target);
    }

    public sealed class TransformCommand : IDrawCommand
    {
        private readonly Matrix3x2 _transform;
        public TransformCommand(Matrix3x2 transform)
        {
            _transform = transform;
        }

        public static TransformCommand Identity = new TransformCommand(Matrix3x2.Identity);

        public void Apply(RenderTargetD2D target)
        {
            target.Transform = _transform;
        }
    }

    public class Line : IDrawCommand
    {
        public IBrushD2D Brush { get; }
        public Vector2 P0 { get; }
        public Vector2 P1 { get; }

        public Line(Vector2 p0, Vector2 p1, IBrushD2D brush)
        {
            Brush = brush;
            P0 = p0;
            P1 = p1;
        }

        public void Apply(RenderTargetD2D target)
        {
            target.DrawLine(P0, P1, Brush);
        }
    }

    public class LineChain : IDrawCommand
    {
        private readonly List<Line> _lines = new List<Line>();

        public LineChain(List<Vector2> points, IBrushD2D brush)
        {
            for (var i = 1; i < points.Count; i++)
            {
                _lines.Add(new Line(points[i - 1], points[i], brush));
            }
        }

        public void Apply(RenderTargetD2D target)
        {
            foreach (var line in _lines)
            {
                line.Apply(target);
            }
        }
    }

    public class Ellipse : IDrawCommand
    {
        protected readonly float X, Y, R1, R2;
        protected readonly IBrushD2D Brush;

        public Ellipse(float x, float y, float r1, float r2, IBrushD2D brush)
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

    public sealed class Rect : IDrawCommand
    {
        protected readonly float X, Y, W, H;
        private readonly IBrushD2D _brush;
        private RectangleF _rect;

        public Rect(float x, float y, float w, float h, IBrushD2D brush)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
            _brush = brush;
            _rect = new RectangleF(x, y, w, h);
        }

        public void Apply(RenderTargetD2D target)
        {
            target.DrawRect(_rect, _brush);
        }

        public override string ToString()
        {
            return $"Rectangle ({X}, {Y}, {W}, {H})";
        }
    }

    public sealed class Circle : Ellipse
    {
        public Circle(float x, float y, float r, IBrushD2D brush) : base(x, y, r, r, brush) { }

        public override string ToString()
        {
            return $"Circle({X}, {Y}, {R1})";
        }
    }

    public sealed class Label : IDrawCommand
    {
        public readonly string Text;
        public readonly TextFormatD2D Format;
        public readonly IBrushD2D Brush;
        public readonly RectangleF Location;

        public Label(string text, RectangleF location, TextFormatD2D format, IBrushD2D brush)
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

    public class FillEllipse : IDrawCommand
    {
        protected readonly float X, Y, R1, R2;
        protected readonly IBrushD2D Brush;

        public FillEllipse(float x, float y, float r1, float r2, IBrushD2D brush)
        {
            X = x;
            Y = y;
            R1 = r1;
            R2 = r2;
            Brush = brush;
        }

        public void Apply(RenderTargetD2D target)
        {
            target.FillEllipse(new Vector2(X, Y), R1, R2, Brush);
        }

        public override string ToString()
        {
            return $"FillEllipse({X}, {Y}, {R1}, {R2})";
        }
    }

    public sealed class FillCircle : FillEllipse
    {
        public FillCircle(float x, float y, float r, IBrushD2D brush) : base(x, y, r, r, brush) { }

        public override string ToString()
        {
            return $"FillCircle({X}, {Y}, {R1})";
        }
    }

    public sealed class FillRect : IDrawCommand
    {
        protected readonly float X, Y, W, H;
        private readonly IBrushD2D _brush;
        private RectangleF _rect;

        public FillRect(float x, float y, float w, float h, IBrushD2D brush)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
            _brush = brush;
            _rect = new RectangleF(x, y, w, h);
        }

        public void Apply(RenderTargetD2D target)
        {
            target.FillRect(_rect, _brush);
        }

        public override string ToString()
        {
            return $"FillRectangle ({X}, {Y}, {W}, {H})";
        }
    }

    public sealed class DrawBitmap : IDrawCommand
    {
        private readonly float _x, _y;
        private readonly int _w, _h;
        private readonly float _opacity;
        private readonly RectangleF _targetRect;
        private readonly RectangleF _sourceRect;
        private SharpDX.Direct2D1.Bitmap _dxBitmap;
        private readonly System.Drawing.Image _sourceImage;

        public DrawBitmap(float x, float y, System.Drawing.Image image, float opacity = 1)
        {
            _x = x;
            _y = y;
            _w = image.Width;
            _h = image.Height;
            _targetRect = new RectangleF(_x, _y, image.Width, image.Height);
            _sourceRect = new RectangleF(0, 0, _w, _h);
            _opacity = opacity;
            _sourceImage = image;
        }

        public void Apply(RenderTargetD2D target)
        {
            if(_dxBitmap == null)
                _dxBitmap = target.ToBitmap(_sourceImage);

            target.DrawBitmap(_dxBitmap,
                _targetRect,
                _opacity,
                BitmapInterpolationMode.NearestNeighbor,
                _sourceRect
            );
        }

        public override string ToString()
        {
            return $"DrawBitmap ({_x}, {_y}, {_w}, {_h})";
        }
    }
}
