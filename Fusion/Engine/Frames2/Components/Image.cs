using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.SpritesD2D;
using System.Xml.Serialization;

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
		[XmlIgnore]
		public Texture2D Texture {
			get
			{
				if ((_texture == null) && (!string.IsNullOrEmpty(TextureName)))
				{
					_texture = Game.Instance.Content.Load<Texture2D>(_textureName);
				}
                return _texture;
			}
			set {
                SetAndNotify(ref _texture, value);
				SetAndNotify(ref _textureName, null);
			}
        }


		private string _textureName;

		public string TextureName
		{
			get
			{
				return _textureName;
			}
			set
			{
				SetAndNotify(ref _textureName, value);
				try
				{
					if ((_texture == null) && (!string.IsNullOrEmpty(_textureName)))
					{
						_texture = Game.Instance.Content.Load<Texture2D>(_textureName);
						UpdateDrawCommand();
					}
				}
				catch (System.Exception)
				{

					//throw;
				}
			}
		}
		private DrawBitmap _drawCommand;

        public Image() : this(0, 0, 0, 0, (Texture2D) null, 1)
        {
        }

        public Image(float x, float y, Texture2D texture, float opacity = 1) : this(x, y, texture.Width, texture.Height, texture, opacity)
        {
        }

		public Image( float x, float y, string textureName, float opacity = 1 ) : base(x, y)
		{
			_textureName = textureName;

			if (Texture != null)
			{
				Width = Texture.Width;
				Height = Texture.Height; 
			}

            Opacity = opacity;

			UpdateDrawCommand();

			PropertyChanged += ( s, e ) =>
			{
				if ((e.PropertyName == nameof(Width)) || (e.PropertyName == nameof(Height)) || (e.PropertyName == nameof(Opacity)))
				{
					UpdateDrawCommand();
				}
			};
		}

        public Image( float x, float y, float width, float height, string textureName, float opacity = 1 ) : base(x, y, width, height)
        {
            _textureName = textureName;

            Opacity = opacity;

            UpdateDrawCommand();

            PropertyChanged += ( s, e ) =>
            {
                if ((e.PropertyName == nameof(Width)) || (e.PropertyName == nameof(Height)) || (e.PropertyName == nameof(Opacity)))
                {
                    UpdateDrawCommand();
                }
            };
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

        public override void DefaultInit()
        {
            Width = 100;
            Height = 100;
        }

        private void UpdateDrawCommand()
        {
            _drawCommand = (Texture != null) ? new DrawBitmap(0, 0, Width, Height, Texture, Opacity) : null;
        }

        public override void Update(GameTime gameTime) { }

        public override void Draw(SpriteLayerD2D layer)
        {
			if (_drawCommand!= null)
			{
				layer.Draw(_drawCommand);
			}
			else
			{
				var groupCommand = new DrawGroup(
					new FillRect(0, 0, Width, Height, new SolidBrushD2D(new Core.Mathematics.Color4(0, 0, 0, 1))),
					new Line(new Core.Mathematics.Vector2(0, 0), new Core.Mathematics.Vector2(Width, Height), new SolidBrushD2D(new Core.Mathematics.Color4(1, 0, 0, 1))),
					new Line(new Core.Mathematics.Vector2(0, Height), new Core.Mathematics.Vector2(Width, 0), new SolidBrushD2D(new Core.Mathematics.Color4(1, 0, 0, 1))),
					new Rect(0, 0, Width, Height, new SolidBrushD2D(new Core.Mathematics.Color4(1, 1, 1, 1)))
					);
				layer.Draw(groupCommand);
			}


		}
    }
}
