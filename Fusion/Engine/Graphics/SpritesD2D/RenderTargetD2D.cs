using System;
using Fusion.Core.Mathematics;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    /// <summary>
    /// Decorator class for SharpDX.Direct2D1.RenderTarget.
    /// </summary>
    public sealed class RenderTargetD2D
    {
        private readonly RenderTarget _target;

        /// <remarks>This constructor must be internal in order to encapsulate Direct2D dependency.</remarks>
        internal RenderTargetD2D(RenderTarget renderTarget)
        {
            _target = renderTarget;
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

        public void DrawEllipse(Vector2 center, float rX, float rY, BrushD2D brush) =>
            _target.DrawEllipse(new SharpDX.Direct2D1.Ellipse(center.ToRawVector2(), rX, rY), brush.Brush);

        public void DrawEllipse(Vector2 center, float rX, float rY, BrushD2D brush, float strokeWidth) =>
            _target.DrawEllipse(new SharpDX.Direct2D1.Ellipse(center.ToRawVector2(), rX, rY), brush.Brush, strokeWidth);

        public void DrawLine(Vector2 p0, Vector2 p1, BrushD2D brush) =>
            _target.DrawLine(p0.ToRawVector2(), p1.ToRawVector2(), brush.Brush);

        public void DrawLine(Vector2 p0, Vector2 p1, BrushD2D brush, float strokeWidth) =>
            _target.DrawLine(p0.ToRawVector2(), p1.ToRawVector2(), brush.Brush, strokeWidth);

        public void DrawRect(RectangleF rectangle, BrushD2D brush) =>
            _target.DrawRectangle(rectangle.ToRawRectangleF(), brush.Brush);

        public void DrawRect(RectangleF rectangle, BrushD2D brush, float strokeWidth) =>
            _target.DrawRectangle(rectangle.ToRawRectangleF(), brush.Brush, strokeWidth);

        public void FillEllipse(Vector2 center, float rX, float rY, BrushD2D brush) =>
            _target.FillEllipse(new SharpDX.Direct2D1.Ellipse(center.ToRawVector2(), rX, rY), brush.Brush);

        public void FillRect(RectangleF rectangle, BrushD2D brush) =>
            _target.FillRectangle(rectangle.ToRawRectangleF(), brush.Brush);

        public void DrawText(string text, TextFormatD2D textFormat, RectangleF rectangleF, BrushD2D brush) =>
            _target.DrawText(text, textFormat.Format, rectangleF.ToRawRectangleF(), brush.Brush);

        private void ZZZ()
        {

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
