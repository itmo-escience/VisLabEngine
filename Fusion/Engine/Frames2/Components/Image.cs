using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public sealed class Image : UIComponent
    {
        private readonly float _opacity;
        private readonly DrawBitmap _image;

        public Image(float x, float y, string file, float opacity = 1) : base(x, y)
        {
            _opacity = opacity;

            var source = System.Drawing.Image.FromFile(file);
            Width = source.Width;
            Height = source.Height;

            _image = new DrawBitmap(0, 0, source, _opacity);
        }

        public Image(float x, float y, float width, float height, string file, float opacity = 1) : base(x, y, width, height)
        {
            _opacity = opacity;

            var source = System.Drawing.Image.FromFile(file);
            _image = new DrawBitmap(0, 0, width, height, source, _opacity);
        }

        public override void Update(GameTime gameTime) { }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(_image);
        }
    }
}
