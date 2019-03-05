using Fusion.Drivers.Graphics;
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

        private Texture2D _texture;
        public Texture2D Texture {
            get => _texture;
            set {
                SetAndNotify(ref _texture, value);
            }
        }

        private DrawBitmap _drawCommand;

        public Image() : this(0, 0, 0, 0, null, 1)
        {
        }

        public Image(float x, float y, Texture2D texture, float opacity = 1) : this(x, y, texture.Width, texture.Height, texture, opacity)
        {
        }

        public Image(float x, float y, float width, float height, Texture2D texture, float opacity = 1) : base(x, y, width, height)
        {
            Texture = texture;
            Opacity = opacity;

            UpdateDrawCommand();

            PropertyChanged += (s, e) =>
            {
                if ((e.PropertyName == nameof(Width)) || (e.PropertyName == nameof(Height)) || (e.PropertyName == nameof(Opacity)))
                {
                    UpdateDrawCommand();
                }
            };
        }

        private void UpdateDrawCommand()
        {
            _drawCommand = new DrawBitmap(0, 0, Width, Height, Texture, Opacity);
        }

        public override void Update(GameTime gameTime) { }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(_drawCommand);
        }
    }
}
