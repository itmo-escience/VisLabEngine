using System.Collections.Generic;
using Fusion.Core.Mathematics;

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
}
