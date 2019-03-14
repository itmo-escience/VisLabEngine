using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public sealed class Image : UIComponent
    {
        public float DesiredWidth { get; set; } = -1;
        public float DesiredHeight { get; set; } = -1;

        private float _opacity;
        public float Opacity {
            get => _opacity;
            set
            {
                _opacity = value;
                //SetAndNotify(ref _opacity, value);
            }
        }

        private Texture2D _texture;
        public Texture2D Texture {
            get => _texture;
            set
            {
                _texture = value;
                //SetAndNotify(ref _texture, value);
            }
        }

        private DrawBitmap _drawCommand;

        public Image() : this(null) { }

        public Image(Texture2D texture, float opacity = 1)
        {
            Texture = texture;
            Opacity = opacity;
        }

        public Image(float width, float height, Texture2D texture, float opacity = 1)
        {
            Texture = texture;
            Opacity = opacity;
            DesiredWidth = width;
            DesiredHeight = height;
        }

        public ISlot Placement { get; set; }
        public object Tag { get; set; }
        public string Name { get; set; }

        public void Update(GameTime gameTime)
        {
            if (Texture != null &&
                (_drawCommand == null ||
                !MathUtil.NearEqual(_drawCommand.Width, Placement.Width) ||
                !MathUtil.NearEqual(_drawCommand.Height, Placement.Height)))
            {
                _drawCommand = new DrawBitmap(0, 0, Placement.Width, Placement.Height, Texture, Opacity);
            }
        }

        public void Draw(SpriteLayerD2D layer)
        {
            if(_drawCommand != null)
                layer.Draw(_drawCommand);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
