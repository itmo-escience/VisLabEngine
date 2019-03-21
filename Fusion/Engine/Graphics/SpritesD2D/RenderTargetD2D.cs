using System.Collections.Generic;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SharpDX;
using Color4 = Fusion.Core.Mathematics.Color4;
using Matrix3x2 = Fusion.Core.Mathematics.Matrix3x2;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using RectangleF = Fusion.Core.Mathematics.RectangleF;
using Size2 = Fusion.Core.Mathematics.Size2;
using Vector2 = Fusion.Core.Mathematics.Vector2;
using Device = SharpDX.Direct3D11.Device;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    /// <summary>
    /// Decorator class for SharpDX.Direct2D1.RenderTarget.
    /// </summary>
    public sealed class RenderTargetD2D
    {
        internal readonly RenderTarget RenderTarget;
        private readonly BrushFactory _brushFactory;
        private readonly TextFormatFactory _dwFactory;
        private readonly TextLayoutFactory _layoutFactory;

        /// <remarks>This constructor must be internal in order to encapsulate Direct2D dependency.</remarks>
        internal RenderTargetD2D(RenderTarget renderTarget)
        {
            RenderTarget = renderTarget;
            _brushFactory = new BrushFactory(RenderTarget);
            _dwFactory = new TextFormatFactory();
            _layoutFactory = TextLayoutFactory.Instance;
        }

        /// <inheritdoc cref="RenderTarget.BeginDraw()"/>
        public void BeginDraw() => RenderTarget.BeginDraw();

        /// <inheritdoc cref="RenderTarget.EndDraw()"/>
        public void EndDraw() => RenderTarget.EndDraw();

        /// <inheritdoc cref="RenderTarget.EndDraw(out long, out long)"/>
        public void EndDraw(out long tag1, out long tag2) => RenderTarget.EndDraw(out tag1, out tag2);

        /// <inheritdoc cref="Clear(Core.Mathematics.Color4)"/>
        public void Clear() => RenderTarget.Clear(null);

        /// <inheritdoc cref="RenderTarget.Clear(RawColor4?)"/>
        public void Clear(Color4 color) => RenderTarget.Clear(color.ToRawColor4());

        /// <inheritdoc cref="RenderTarget.Flush()"/>
        public void Flush() => RenderTarget.Flush();

        /// <inheritdoc cref="RenderTarget.Flush(out long, out long)"/>
        public void Flush(out long tag1, out long tag2) => RenderTarget.Flush(out tag1, out tag2);

        /// <inheritdoc cref="RenderTarget.PushAxisAlignedClip(RawRectangleF, AntialiasMode)"/>
        public void PushAxisAlignedClip(RectangleF clippingRecrangle, AntialiasModeD2D antialiasMode) =>
            RenderTarget.PushAxisAlignedClip(clippingRecrangle.ToRawRectangleF(), antialiasMode.ToAntialiasMode());

        /// <inheritdoc cref="RenderTarget.PopAxisAlignedClip()"/>
        public void PopAxisAlignedClip() => RenderTarget.PopAxisAlignedClip();

        public void DrawEllipse(Vector2 center, float rX, float rY, IBrushD2D brush) =>
            RenderTarget.DrawEllipse(new SharpDX.Direct2D1.Ellipse(center.ToRawVector2(), rX - 1, rY - 1), _brushFactory.GetOrCreateBrush(brush));

        public void DrawEllipse(Vector2 center, float rX, float rY, IBrushD2D brush, float strokeWidth) =>
            RenderTarget.DrawEllipse(new SharpDX.Direct2D1.Ellipse(center.ToRawVector2(), rX - strokeWidth, rY - strokeWidth), _brushFactory.GetOrCreateBrush(brush), strokeWidth);

        public void DrawLine(Vector2 p0, Vector2 p1, IBrushD2D brush) =>
            RenderTarget.DrawLine(p0.ToRawVector2(), p1.ToRawVector2(), _brushFactory.GetOrCreateBrush(brush));

        public void DrawLine(Vector2 p0, Vector2 p1, IBrushD2D brush, float strokeWidth) =>
            RenderTarget.DrawLine(p0.ToRawVector2(), p1.ToRawVector2(), _brushFactory.GetOrCreateBrush(brush), strokeWidth);

        public void DrawStrokeLine(Vector2 p0, Vector2 p1, IBrushD2D brush, float strokeWidth = 1)
        {
            StrokeStyleProperties prop = new StrokeStyleProperties
            {
                DashStyle = DashStyle.Dash,
            };
            RenderTarget.DrawLine(p0.ToRawVector2(), p1.ToRawVector2(), _brushFactory.GetOrCreateBrush(brush), strokeWidth, new StrokeStyle(RenderTarget.Factory, prop));
        }

        public void DrawRect(RectangleF rectangle, IBrushD2D brush) =>
            RenderTarget.DrawRectangle(createAlignedRectangle(rectangle.ToRawRectangleF()), _brushFactory.GetOrCreateBrush(brush));

        public void DrawRect(RectangleF rectangle, IBrushD2D brush, float strokeWidth) =>
            RenderTarget.DrawRectangle(createAlignedRectangle(rectangle.ToRawRectangleF(), strokeWidth), _brushFactory.GetOrCreateBrush(brush), strokeWidth);

        public void DrawStrokeRect(RectangleF rectangle, IBrushD2D brush, float strokeWidth = 1)
        {
            StrokeStyleProperties prop = new StrokeStyleProperties
            {
                DashStyle = DashStyle.Dash,
            };
            RenderTarget.DrawRectangle(createAlignedRectangle(rectangle.ToRawRectangleF(), strokeWidth), _brushFactory.GetOrCreateBrush(brush), strokeWidth, new StrokeStyle(RenderTarget.Factory, prop));
        }

        public void FillEllipse(Vector2 center, float rX, float rY, IBrushD2D brush) =>
            RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(center.ToRawVector2(), rX, rY), _brushFactory.GetOrCreateBrush(brush));

        public void FillRect(RectangleF rectangle, IBrushD2D brush) =>
            RenderTarget.FillRectangle(rectangle.ToRawRectangleF(), _brushFactory.GetOrCreateBrush(brush));

        public void DrawText(string text, TextFormatD2D textFormat, RectangleF rectangleF, IBrushD2D brush) =>
            RenderTarget.DrawText(text, _dwFactory.CreateTextFormat(textFormat), rectangleF.ToRawRectangleF(), _brushFactory.GetOrCreateBrush(brush));

        public void DrawTextLayout(Vector2 origin, TextLayoutD2D layout, IBrushD2D brush) =>
            RenderTarget.DrawTextLayout(origin.ToRawVector2(), _layoutFactory.CreateTextLayout(layout), _brushFactory.GetOrCreateBrush(brush));

        public void DrawBitmap(BitmapD2D bitmap, RectangleF destinationRectangle, float opacity, RectangleF sourceRectangle) =>
            RenderTarget.DrawBitmap(bitmap._bitmap, destinationRectangle.ToRawRectangleF(), opacity, BitmapInterpolationMode.NearestNeighbor, sourceRectangle.ToRawRectangleF());

        public void DrawPathGeometry(PathGeometryD2D geometry, IBrushD2D brush) =>
            RenderTarget.DrawGeometry(geometry.PathGeometry, _brushFactory.GetOrCreateBrush(brush));

        public void DrawPathGeometry(PathGeometryD2D geometry, IBrushD2D brush, float strokeWidth) =>
            RenderTarget.DrawGeometry(geometry.PathGeometry, _brushFactory.GetOrCreateBrush(brush), strokeWidth);

        private RawRectangleF createAlignedRectangle(RawRectangleF rectangle, float borderThickness = 1)
        {
            borderThickness /= 2;
            return new RawRectangleF(rectangle.Left + borderThickness, rectangle.Top + borderThickness, rectangle.Right - borderThickness, rectangle.Bottom - borderThickness);
        }

        private readonly Stack<Layer> _layers = new Stack<Layer>();
        public void PushLayer(PathGeometryD2D clippingGeometry, AntialiasModeD2D antialiasMode)
        {
            var layerParameters = new LayerParameters
            {
                GeometricMask = clippingGeometry.PathGeometry,
                ContentBounds = RectangleF.Infinite.ToRawRectangleF(),
                MaskAntialiasMode = antialiasMode.ToAntialiasMode(),
                MaskTransform = SharpDX.Matrix3x2.Identity,
                Opacity = 1.0f,
                LayerOptions = LayerOptions.None,
                OpacityBrush = null
            };

            var layer = new Layer(RenderTarget);
            _layers.Push(layer);
            RenderTarget.PushLayer(ref layerParameters, layer);
        }

        public void PopLayer()
        {
            var l = _layers.Pop();
            l.Dispose();
            RenderTarget.PopLayer();
        }

        public DrawingStateBlockD2D SaveDrawingState()
        {
            var s = new DrawingStateBlock(RenderTarget.Factory);
            RenderTarget.SaveDrawingState(s);
            return new DrawingStateBlockD2D(s);
        }

        public void RestoreDrawingState(DrawingStateBlockD2D state) => RenderTarget.RestoreDrawingState(state.State);

        public Matrix3x2 Transform
        {
            get => RenderTarget.Transform.ToMatrix3x2();
            set => RenderTarget.Transform = value.ToRawMatrix3X2();
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
