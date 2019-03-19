using System.Collections.Generic;
using Fusion.Drivers.Graphics;
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

        public override string ToString() => $"Transform: {_transform}";
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
        private bool _isStroked;

        public Rect(float x, float y, float w, float h, IBrushD2D brush, bool isStroked = false)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
            _brush = brush;
            _isStroked = isStroked;
            _rect = new RectangleF(x, y, w, h);
        }

        public void Apply(RenderTargetD2D target)
        {
            if (_isStroked)
            {
                target.DrawStrokeRect(_rect, _brush);
            }
            else
            {
                target.DrawRect(_rect, _brush);
            }
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

    public sealed class Text : IDrawCommand
    {
        public readonly string Value;
        public readonly TextFormatD2D Format;
        public readonly IBrushD2D Brush;
        public readonly RectangleF Location;

        public Text(string value, RectangleF location, TextFormatD2D format, IBrushD2D brush)
        {
            Value = value;
            Format = format;
            Brush = brush;
            Location = location;
        }

        public void Apply(RenderTargetD2D target)
        {
            target.DrawText(Value, Format, Location, Brush);
        }

        public override string ToString() => $"Text: ({Location.X}, {Location.Y}, {Location.Width}, {Location.Height}, {Value})";
    }

    public sealed class LayoutedText : IDrawCommand
    {
        public readonly TextLayoutD2D Layout;
        public readonly IBrushD2D Brush;
        public readonly Vector2 Location;

        public LayoutedText(Vector2 location, TextLayoutD2D layout, IBrushD2D brush)
        {
            Layout = layout;
            Brush = brush;
            Location = location;
        }

        public void Apply(RenderTargetD2D target)
        {
            target.DrawTextLayout(Location, Layout, Brush);
        }

        public override string ToString() => $"LayoutedText: ({Location.X}, {Location.Y}, {Layout.Width}, {Layout.Height}, {Layout.Text})";
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
        public float Width, Height;
        public float Opacity;
        private readonly RectangleF _sourceRect;
        private BitmapD2D _bitmap;
        private readonly Texture2D _texture;
        private bool _bitmapIsLoaded = false;

        public DrawBitmap(float x, float y, Texture2D texture, float opacity = 1) : this(x, y, texture.Width, texture.Height, texture, opacity) {}

        public DrawBitmap(float x, float y, float width, float height, Texture2D texture, float opacity = 1)
        {
            _x = x;
            _y = y;
            Width = width;
            Height = height;
            _sourceRect = new RectangleF(0, 0, texture.Width, texture.Height);
            Opacity = opacity;
            _bitmap = null;
            _texture = texture;
        }

        public void Apply(RenderTargetD2D target)
        {
            if (!_bitmapIsLoaded)
            {
                _bitmap = BitmapD2D.FromTexture2D(_texture, target);
                _bitmapIsLoaded = true;
            }
            if (_bitmap != null)
            {
                target.DrawBitmap(_bitmap, new RectangleF(_x, _y, Width, Height), Opacity, _sourceRect);
            }
        }

        public override string ToString()
        {
            return $"DrawBitmap ({_x}, {_y}, {Width}, {Height})";
        }
    }

    public sealed class StartClippingAlongRectangle : IDrawCommand
    {
        private RectangleF _clippingRecrangle;
        private AntialiasModeD2D _antialiasMode;

        public StartClippingAlongRectangle(RectangleF clippingRecrangle, AntialiasModeD2D antialiasMode)
        {
            _clippingRecrangle = clippingRecrangle;
            _antialiasMode = antialiasMode;
        }

        public void Apply(RenderTargetD2D target)
        {
            target.PushAxisAlignedClip(_clippingRecrangle, _antialiasMode);
        }

        public override string ToString()
        {
            return $"StartClippingAlongRectangle ({_clippingRecrangle.X}, {_clippingRecrangle.Y}, {_clippingRecrangle.Width}, {_clippingRecrangle.Height}, {_antialiasMode})";
        }
    }

    public sealed class EndClippingAlongRectangle : IDrawCommand
    {
        public void Apply(RenderTargetD2D target)
        {
            target.PopAxisAlignedClip();
        }

        public override string ToString()
        {
            return $"EndClippingAlongRectangle";
        }
    }

    public sealed class StartClippingAlongGeometry : IDrawCommand
    {
        private PathGeometryD2D _clippingGeometry;
        private AntialiasModeD2D _antialiasMode;

        public StartClippingAlongGeometry(PathGeometryD2D clippingGeometry, AntialiasModeD2D antialiasMode)
        {
            _clippingGeometry = clippingGeometry;
            _antialiasMode = antialiasMode;
        }

        public void Apply(RenderTargetD2D target)
        {
            target.PushLayer(_clippingGeometry, _antialiasMode);
        }

        public override string ToString()
        {
            return $"StartClippingAlongGeometry ({_antialiasMode})";
        }
    }

    public sealed class EndClippingAlongGeometry : IDrawCommand
    {
        public void Apply(RenderTargetD2D target)
        {
            target.PopLayer();
        }

        public override string ToString()
        {
            return $"EndClippingAlongGeometry";
        }
    }
}
