using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public sealed class Label : UIComponent
    {
        private ISlot _placement;
        public ISlot Placement
        {
            get => _placement;
            set
            {
                if (_placement != null)
                {
                    _placement.PropertyChanged -= SlotChanged;
                }
                _placement = value;

                if (_placement != null)
                    _placement.PropertyChanged += SlotChanged;
            }
        }

        public UIEventsHolder Events { get; } = new UIEventsHolder();

        private void SlotChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ISlot.Width) || e.PropertyName == nameof(ISlot.Height))
                _isDirtyLayout = true;
        }

        private bool _autoSize = true;
        public bool AutoSize
        {
            get => _autoSize;
            set => _autoSize = value;
        }

        private float _desiredWidth = -1;

        public float DesiredWidth
        {
            get => _desiredWidth;
            set {
                _desiredWidth = value;
                AutoSize = false;
            }
        }

        private float _desiredHeight = -1;
        public float DesiredHeight
        {
            get => _desiredHeight;
            set {
                _desiredHeight = value;
                AutoSize = false;
            }
        }

        public object Tag { get; set; }
        public string Name { get; set; }

        private TextFormatD2D _textFormat;
        public TextFormatD2D TextFormat {
            get => _textFormat;
            set {
                _textFormat = value;
                _isDirtyLayout = true;
                //NotifyPropertyChanged();
            }
        }

        private TextLayoutD2D _textLayout;

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                _isDirtyLayout = true;
            }
        }

        private LayoutedText _textCommand;

        private bool _isDirtyLayout;

        public Label() : this("", "Arial", 10) { }

        public Label(string text, string fontName, float fontSize) : this(text, new TextFormatD2D(fontName, fontSize)) { }

        public Label(string text, TextFormatD2D textFormat)
        {
            _textFormat = textFormat;
            Text = text;

            _isDirtyLayout = true;
        }

        public void Update(GameTime gameTime)
        {
            if (!_isDirtyLayout) return;

            if (AutoSize)
            {
                _textLayout = new TextLayoutD2D(_text, _textFormat, Placement.AvailableWidth, Placement.AvailableHeight);
                _desiredWidth = _textLayout.Width;
                _desiredHeight = _textLayout.Height;
            }
            else
            {
                _textLayout = new TextLayoutD2D(_text, _textFormat, Placement.Width, Placement.Height);
            }

            _textCommand = new LayoutedText(new Vector2(), _textLayout, new SolidBrushD2D(Color4.White));

            _isDirtyLayout = false;
        }

        public void Draw(SpriteLayerD2D layer)
        {
            if(_textCommand != null)
                layer.Draw(_textCommand);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString() => $"Label: {Text}";
    }
}
