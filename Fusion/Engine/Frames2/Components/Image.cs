using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public sealed class Image : UIComponent
    {
        private float _opacity;
        public float Opacity {
            get => _opacity;
            set {
                SetAndNotify(ref _opacity, value);
            }
        }

        private DrawBitmap _image;

        private string _sourseFile;
        public string SourceFile {
            get => _sourseFile;
            set {
                _sourseFile = value;
                var source = System.Drawing.Image.FromFile(value);
                _image = new DrawBitmap(0, 0, Width, Height, source, Opacity);
            }
        }

        public Image() : base() {
            Opacity = 1;
            _sourseFile = "";

            PropertyChanged += (s, e) =>
            {
                if ((e.PropertyName == nameof(Width)) || (e.PropertyName == nameof(Height)) || (e.PropertyName == nameof(Opacity)))
                {
                    UpdateImage();
                }
            };
        }

        public Image(float x, float y, string file, float opacity = 1) : base(x, y)
        {
            Opacity = opacity;

            var source = System.Drawing.Image.FromFile(file);
            Width = source.Width;
            Height = source.Height;

            _image = new DrawBitmap(0, 0, source, Opacity);

            PropertyChanged += (s, e) =>
            {
                if ((e.PropertyName == nameof(Width)) || (e.PropertyName == nameof(Height)) || (e.PropertyName == nameof(Opacity)))
                {
                    UpdateImage();
                }
            };
        }

        public Image(float x, float y, float width, float height, string file, float opacity = 1) : base(x, y, width, height)
        {
            Opacity = opacity;
            SourceFile = file;

            PropertyChanged += (s, e) =>
            {
                if ((e.PropertyName == nameof(Width)) || (e.PropertyName == nameof(Height)) || (e.PropertyName == nameof(Opacity)))
                {
                    UpdateImage();
                }
            };
        }

        private void UpdateImage()
        {
            var sourceFile = System.Drawing.Image.FromFile(_sourseFile);
            _image = new DrawBitmap(0, 0, Width, Height, sourceFile, Opacity);
        }

        public override void Update(GameTime gameTime) { }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(_image);
        }
    }
}
