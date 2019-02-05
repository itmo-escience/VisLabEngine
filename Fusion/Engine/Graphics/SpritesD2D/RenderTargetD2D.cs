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

        /// <inheritdoc cref="Clear(Core.Mathematics.Color4)"/>
        public void Clear() => _target.Clear(null);

        /// <inheritdoc cref="RenderTarget.Clear(RawColor4?)"/>
        public void Clear(Color4 color) => _target.Clear(color.ToRawColor4());

        /// <inheritdoc cref="RenderTarget.Flush()"/>
        public void Flush() => _target.Flush();

        /// <inheritdoc cref="RenderTarget.Flush(out long, out long)"/>
        public void Flush(out long tag1, out long tag2) => _target.Flush(out tag1, out tag2);

        /// <inheritdoc cref="RenderTarget.PushAxisAlignedClip(RawRectangleF, AntialiasMode)"/>
        public void PushAxisAlignedClip(RectangleF clippingRecrangle, AntialiasModeD2D antialiasMode) =>
            _target.PushAxisAlignedClip(createAlignedRectangle(clippingRecrangle.ToRawRectangleF()), antialiasMode.ToAntialiasMode());

        /// <inheritdoc cref="RenderTarget.PopAxisAlignedClip()"/>
        public void PopAxisAlignedClip() => _target.PopAxisAlignedClip();

        private RawVector2 createAlignedVector(RawVector2 vector)
        {
            return new RawVector2(vector.X + 0.5f, vector.Y + 0.5f);
        }

        private RawRectangleF createAlignedRectangle(RawRectangleF rectangle)
        {
            return new RawRectangleF(rectangle.Left + 0.5f, rectangle.Top + 0.5f, rectangle.Right + 0.5f, rectangle.Bottom + 0.5f);
        }

        public void DrawEllipse(Vector2 center, float rX, float rY, IBrushD2D brush) =>
            _target.DrawEllipse(new SharpDX.Direct2D1.Ellipse(createAlignedVector(center.ToRawVector2()), rX, rY), _brushFactory.GetOrCreateBrush(brush));

        public void DrawEllipse(Vector2 center, float rX, float rY, IBrushD2D brush, float strokeWidth) =>
            _target.DrawEllipse(new SharpDX.Direct2D1.Ellipse(createAlignedVector(center.ToRawVector2()), rX, rY), _brushFactory.GetOrCreateBrush(brush), strokeWidth);

        public void DrawLine(Vector2 p0, Vector2 p1, IBrushD2D brush) =>
            _target.DrawLine(createAlignedVector(p0.ToRawVector2()), createAlignedVector(p1.ToRawVector2()), _brushFactory.GetOrCreateBrush(brush));

        public void DrawLine(Vector2 p0, Vector2 p1, IBrushD2D brush, float strokeWidth) =>
            _target.DrawLine(createAlignedVector(p0.ToRawVector2()), createAlignedVector(p1.ToRawVector2()), _brushFactory.GetOrCreateBrush(brush), strokeWidth);

        public void DrawStrokeLine(Vector2 p0, Vector2 p1, IBrushD2D brush, float strokeWidth = 1)
        {
            StrokeStyleProperties prop = new StrokeStyleProperties
            {
                DashStyle = DashStyle.Dash,
            };
            _target.DrawLine(createAlignedVector(p0.ToRawVector2()), createAlignedVector(p1.ToRawVector2()), _brushFactory.GetOrCreateBrush(brush), strokeWidth, new StrokeStyle(_target.Factory, prop));
        }

        public void DrawRect(RectangleF rectangle, IBrushD2D brush) =>
            _target.DrawRectangle(createAlignedRectangle(rectangle.ToRawRectangleF()), _brushFactory.GetOrCreateBrush(brush));

        public void DrawRect(RectangleF rectangle, IBrushD2D brush, float strokeWidth) =>
            _target.DrawRectangle(createAlignedRectangle(rectangle.ToRawRectangleF()), _brushFactory.GetOrCreateBrush(brush), strokeWidth);

        public void DrawStrokeRect(RectangleF rectangle, IBrushD2D brush, float strokeWidth = 1)
        {
            StrokeStyleProperties prop = new StrokeStyleProperties
            {
                DashStyle = DashStyle.Dash,
            };
            _target.DrawRectangle(createAlignedRectangle(rectangle.ToRawRectangleF()), _brushFactory.GetOrCreateBrush(brush), strokeWidth, new StrokeStyle(_target.Factory, prop));
        }

        public void FillEllipse(Vector2 center, float rX, float rY, IBrushD2D brush) =>
            _target.FillEllipse(new SharpDX.Direct2D1.Ellipse(createAlignedVector(center.ToRawVector2()), rX, rY), _brushFactory.GetOrCreateBrush(brush));

        public void FillRect(RectangleF rectangle, IBrushD2D brush) =>
            _target.FillRectangle(createAlignedRectangle(rectangle.ToRawRectangleF()), _brushFactory.GetOrCreateBrush(brush));

        public void DrawText(string text, TextFormatD2D textFormat, RectangleF rectangleF, IBrushD2D brush) =>
            _target.DrawText(text, _dwFactory.CreateTextFormat(textFormat), createAlignedRectangle(rectangleF.ToRawRectangleF()), _brushFactory.GetOrCreateBrush(brush));

        public void DrawTextLayout(Vector2 origin, TextLayoutD2D layout, IBrushD2D brush) =>
            _target.DrawTextLayout(createAlignedVector(origin.ToRawVector2()), _layoutFactory.CreateTextLayout(layout), _brushFactory.GetOrCreateBrush(brush));

        public void DrawBitmap(BitmapD2D bitmap, RectangleF destinationRectangle, float opacity, RectangleF sourceRectangle) =>
            _target.DrawBitmap(bitmap.Bitmap, createAlignedRectangle(destinationRectangle.ToRawRectangleF()), opacity, BitmapInterpolationMode.NearestNeighbor, createAlignedRectangle(sourceRectangle.ToRawRectangleF()));

        public void PushLayer(PathGeometryD2D clippingGeometry, AntialiasModeD2D antialiasMode)
        {
            LayerParameters layerParameters = new LayerParameters
            {
                GeometricMask = clippingGeometry.PathGeometry,
                ContentBounds = RectangleF.Infinite.ToRawRectangleF(),
                MaskAntialiasMode = antialiasMode.ToAntialiasMode(),
                MaskTransform = SharpDX.Matrix3x2.Identity,
                Opacity = 1.0f,
                LayerOptions = LayerOptions.None,
                OpacityBrush = null
            };
            _target.PushLayer(ref layerParameters, new Layer(_target));
        }

        public void PopLayer() =>
            _target.PopLayer();

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

        /// <summary>
        /// Converts Image to Bitmap
        /// </summary>
        /// <param name="image">Image to convert</param>
        /// <returns>A D2D1 Bitmap</returns>
        internal Bitmap ToBitmap(System.Drawing.Image image)
        {
            var bitmap = (System.Drawing.Bitmap) image;
            var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapProperties = new BitmapProperties(new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));

            // Transform pixels from BGRA to RGBA
            int stride = bitmap.Width * sizeof(int);
            using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
            {
                // Lock System.Drawing.Bitmap
                var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                // Convert all pixels
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int offset = bitmapData.Stride * y;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        // Not optimized
                        byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        int rgba = R | (G << 8) | (B << 16) | (A << 24);
                        tempStream.Write(rgba);
                    }

                }
                bitmap.UnlockBits(bitmapData);
                tempStream.Position = 0;

                return new Bitmap(_target, new SharpDX.Size2(bitmap.Width, bitmap.Height), tempStream, stride, bitmapProperties);
            }
        }

        public BitmapD2D LoadBitmap(DataStream bitmapStream, SharpDX.Size2 size)
        {
            var bitmapProperties = new BitmapProperties(new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
            var stride = size.Width * sizeof(int);
            return new BitmapD2D(new Bitmap(_target, size, bitmapStream, stride, bitmapProperties));
        }

        private void ZZZ()
        {
        }
    }

    public class BitmapD2D
    {
        internal Bitmap Bitmap { get; }
        internal BitmapD2D(Bitmap bitmap)
        {
            Bitmap = bitmap;
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
