using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public sealed class Label : UIComponent
    {
        private bool _isDirtyText;

        private TextFormatD2D _textFormat;
        public TextFormatD2D TextFormat {
            get => _textFormat;
            set {
                _textFormat = value;
                NotifyPropertyChanged();
            }
        }


        private float _maxWidth;
        public float MaxWidth {
            get => _maxWidth;
            set {
                if (SetAndNotify(ref _maxWidth, value))
                    InvalidateTransform();
            }
        }

        private float _maxHeight;
        public float MaxHeight {
            get => _maxHeight;
            set {
                if (SetAndNotify(ref _maxHeight, value))
                    InvalidateTransform();
            }
        }

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _isDirtyText = true;
                _text = value;

                var layout = new TextLayoutD2D(_text, _textFormat, _maxWidth, _maxHeight);

                Width = layout.Width < _maxWidth ? layout.Width : _maxWidth;
                Height = layout.Height < _maxHeight ? layout.Height : _maxHeight;
                NotifyPropertyChanged();
            }
        }

        private Graphics.SpritesD2D.Label _label;

        public Label() : base() {}

        public Label(string text, TextFormatD2D textFormat, float x, float y, float width, float height) : base(x, y, width, height)
        {
            _textFormat = textFormat;
            _maxWidth = width;
            _maxHeight = height;
            Text = text;
        }

        public override void Update(GameTime gameTime)
        {
            if (_isDirtyText)
                _label = new Graphics.SpritesD2D.Label(
                    Text,
                    new RectangleF(0, 0, Width, Height),
                    _textFormat,
                    new SolidBrushD2D(Color4.White)
                );

            _isDirtyText = false;
        }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(_label);
        }
    }
}
