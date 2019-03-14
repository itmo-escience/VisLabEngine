using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    /*
    public sealed class Label : UIComponent
    {
        private bool _isDirtyText;

        public ISlot Placement { get; set; }
        public object Tag { get; set; }
        public string Name { get; set; }

        private TextFormatD2D _textFormat;
        public TextFormatD2D TextFormat {
            get => _textFormat;
            set {
                _textFormat = value;
                //NotifyPropertyChanged();
            }
        }

        private TextLayoutD2D _textLayout;
        public TextLayoutD2D TextLayout
        {
            get => _textLayout;
            set
            {
                _textLayout = value;
                //NotifyPropertyChanged();
            }
        }

        private float _maxWidth;
        public float MaxWidth {
            get => _maxWidth;
            set
            {
                _maxWidth = value;

                //if (SetAndNotify(ref _maxWidth, value))
                //    InvalidateTransform();
            }
        }

        private float _maxHeight;
        public float MaxHeight {
            get => _maxHeight;
            set
            {
                _maxHeight = value;

                //if (SetAndNotify(ref _maxHeight, value))
                //    InvalidateTransform();
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

                //Width = layout.Width < _maxWidth ? layout.Width : _maxWidth;
                //Height = layout.Height < _maxHeight ? layout.Height : _maxHeight;
                //NotifyPropertyChanged();
            }
        }

        private Text _textCommand;

        public Label() : base()
        {
            _isDirtyText = true;
        }

        public Label(string text, string fontName, float fontSize, float x, float y, float width, float height)
            : this(text, new TextFormatD2D(fontName, fontSize), x, y, width, height) { }

        public Label(string text, TextFormatD2D textFormat, float x, float y, float width, float height) : base(x, y, width, height)
        {
            _textFormat = textFormat;
            _maxWidth = width;
            _maxHeight = height;
            Text = text;

            _isDirtyText = true;
        }

        public Label(string text, TextLayoutD2D textLayout, float x, float y, float width, float height) : base(x, y, width, height)
        {
            _textLayout = textLayout;
            _maxWidth = width;
            _maxHeight = height;
            Text = text;

            _isDirtyText = true;
        }

        public void Update(GameTime gameTime)
        {
            if (_isDirtyText)
                _textCommand = new Text(
                    Text,
                    new RectangleF(0, 0, Placement.Width, Placement.Height),
                    _textFormat,
                    new SolidBrushD2D(Color4.White)
                );

            _isDirtyText = false;
        }

        public void Draw(SpriteLayerD2D layer)
        {
            if(_textCommand != null)
                layer.Draw(_textCommand);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }*/
}
