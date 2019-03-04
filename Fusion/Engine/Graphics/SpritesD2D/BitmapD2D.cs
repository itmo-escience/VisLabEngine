using Fusion.Drivers.Graphics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class BitmapD2D
    {
        internal Bitmap _bitmap { get; }
        public BitmapD2D(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public float Width { get => _bitmap.Size.Width; }
        public float Height { get => _bitmap.Size.Height; }

        public static BitmapD2D FromTexture2D(Texture2D texture, RenderTargetD2D renderTarget)
        {
            using (var tex = texture.SRV.ResourceAs<SharpDX.Direct3D11.Texture2D>())
            using (var surface = tex.QueryInterfaceOrNull<Surface>())
            {
                if (surface == null)
                {
                    Log.Error("Convertion Texture2D to BitmapD2D failed.");
                }
                var bitmap = new Bitmap(renderTarget.RenderTarget, surface, new BitmapProperties(new PixelFormat(
                    Format.B8G8R8A8_UNorm,
                    AlphaMode.Premultiplied))
                );
                return new BitmapD2D(bitmap);
            }
        }
    }
}
