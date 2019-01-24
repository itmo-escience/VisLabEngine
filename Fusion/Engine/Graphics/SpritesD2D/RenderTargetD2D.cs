using Fusion.Core.Mathematics;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    /// <summary>
    /// Decorator class for SharpDX.Direct2D1.RenderTarget.
    /// </summary>
    public sealed class RenderTargetD2D
    {
        private readonly RenderTarget _target;
        private readonly BrushFactory _brushFactory;
        private readonly TextFormatFactory _dwFactory;
        private readonly TextLayoutFactory _layoutFactory;

        /// <remarks>This constructor must be internal in order to encapsulate Direct2D dependency.</remarks>
        internal RenderTargetD2D(RenderTarget renderTarget)
        {
            _target = renderTarget;
            _brushFactory = new BrushFactory(_target);
            _dwFactory = new TextFormatFactory();
            _layoutFactory = new TextLayoutFactory();
        }

        /// <inheritdoc cref="RenderTarget.BeginDraw()"/>
        public void BeginDraw() => _target.BeginDraw();

        /// <inheritdoc cref="RenderTarget.EndDraw()"/>
        public void EndDraw() => _target.EndDraw();

        /// <inheritdoc cref="RenderTarget.EndDraw(out long, out long)"/>
        public void EndDraw(out long tag1, out long tag2) => _target.EndDraw(out tag1, out tag2);

        /// <inheritdoc cref="Clear(Color4)"/>
        public void Clear() => _target.Clear(null);

        /// <inheritdoc cref="RenderTarget.Clear(RawColor4?)"/>
        public void Clear(Color4 color) => _target.Clear(color.ToRawColor4());

        /// <inheritdoc cref="RenderTarget.Flush()"/>
        public void Flush() => _target.Flush();

        /// <inheritdoc cref="RenderTarget.Flush(out long, out long)"/>
        public void Flush(out long tag1, out long tag2) => _target.Flush(out tag1, out tag2);

        /// <inheritdoc cref="RenderTarget.PushAxisAlignedClip(RawRectangleF, AntialiasMode)"/>
        public void PushAxisAlignedClip(RectangleF clip, AntialiasModeD2D antialiasMode) =>
            _target.PushAxisAlignedClip(clip.ToRawRectangleF(), antialiasMode.ToAntialiasMode());

        /// <inheritdoc cref="RenderTarget.PopAxisAlignedClip()"/>
        public void PopAxisAlignedClip() => _target.PopAxisAlignedClip();

        public void DrawEllipse(Vector2 center, float rX, float rY, IBrushD2D brush) =>
            _target.DrawEllipse(new SharpDX.Direct2D1.Ellipse(center.ToRawVector2(), rX, rY), _brushFactory.GetOrCreateBrush(brush));

        public void DrawEllipse(Vector2 center, float rX, float rY, IBrushD2D brush, float strokeWidth) =>
            _target.DrawEllipse(new SharpDX.Direct2D1.Ellipse(center.ToRawVector2(), rX, rY), _brushFactory.GetOrCreateBrush(brush), strokeWidth);

        public void DrawLine(Vector2 p0, Vector2 p1, IBrushD2D brush) =>
            _target.DrawLine(p0.ToRawVector2(), p1.ToRawVector2(), _brushFactory.GetOrCreateBrush(brush));

        public void DrawLine(Vector2 p0, Vector2 p1, IBrushD2D brush, float strokeWidth) =>
            _target.DrawLine(p0.ToRawVector2(), p1.ToRawVector2(), _brushFactory.GetOrCreateBrush(brush), strokeWidth);

        public void DrawRect(RectangleF rectangle, IBrushD2D brush) =>
            _target.DrawRectangle(rectangle.ToRawRectangleF(), _brushFactory.GetOrCreateBrush(brush));

        public void DrawRect(RectangleF rectangle, IBrushD2D brush, float strokeWidth) =>
            _target.DrawRectangle(rectangle.ToRawRectangleF(), _brushFactory.GetOrCreateBrush(brush), strokeWidth);

        public void FillEllipse(Vector2 center, float rX, float rY, IBrushD2D brush) =>
            _target.FillEllipse(new SharpDX.Direct2D1.Ellipse(center.ToRawVector2(), rX, rY), _brushFactory.GetOrCreateBrush(brush));

        public void FillRect(RectangleF rectangle, IBrushD2D brush) =>
            _target.FillRectangle(rectangle.ToRawRectangleF(), _brushFactory.GetOrCreateBrush(brush));

        public void DrawText(string text, TextFormatD2D textFormat, RectangleF rectangleF, IBrushD2D brush) =>
            _target.DrawText(text, _dwFactory.CreateTextFormat(textFormat), rectangleF.ToRawRectangleF(), _brushFactory.GetOrCreateBrush(brush));

        public void DrawTextLayout(Vector2 origin, TextLayoutD2D layout, IBrushD2D brush) =>
            _target.DrawTextLayout(origin.ToRawVector2(), _layoutFactory.CreateTextLayout(layout), _brushFactory.GetOrCreateBrush(brush));


        public DrawingStateBlockD2D SaveDrawingState()
        {
            var s = new DrawingStateBlock(_target.Factory);
            _target.SaveDrawingState(s);
            return new DrawingStateBlockD2D(s);
        }

        public void RestoreDrawingState(DrawingStateBlockD2D state) => _target.RestoreDrawingState(state.State);

        public Matrix3x2 Transform
        {
            get => _target.Transform.ToMatrix3x2();
            set => _target.Transform = value.ToRawMatrix3X2();
        }

        private void ZZZ()
        {
        }
    }

    public class DrawingStateBlockD2D
    {
        internal DrawingStateBlock State;
        internal DrawingStateBlockD2D(DrawingStateBlock state)
        {
            State = state;
        }
    }

    public enum AntialiasModeD2D
    {
        /// <inheritdoc cref="AntialiasMode.Aliased"/>
        Aliased,

        /// <inheritdoc cref="AntialiasMode.PerPrimitive"/>
        PerPrimitive
    }

    internal static class D2DExtensions
    {
        public static RawColor4 ToRawColor4(this Color4 c) => new RawColor4(c.Red, c.Green, c.Blue, c.Alpha);
        public static RawRectangleF ToRawRectangleF(this RectangleF r) => new RawRectangleF(r.Left, r.Top, r.Right, r.Bottom);
        public static RawVector2 ToRawVector2(this Vector2 vec) => new RawVector2(vec.X, vec.Y);
        public static RawMatrix3x2 ToRawMatrix3X2(this Matrix3x2 m) => new RawMatrix3x2(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);
        public static Matrix3x2 ToMatrix3x2(this RawMatrix3x2 m) => new Matrix3x2(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);


        public static AntialiasMode ToAntialiasMode(this AntialiasModeD2D mode)
        {
            return mode == AntialiasModeD2D.Aliased ? AntialiasMode.Aliased : AntialiasMode.PerPrimitive;
        }

        public static AntialiasModeD2D ToD2DAntialiasMode(this AntialiasMode mode)
        {
            return mode == AntialiasMode.Aliased ? AntialiasModeD2D.Aliased : AntialiasModeD2D.PerPrimitive;
        }
    }
}
