using SharpDX.DirectWrite;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class TextLayoutD2D
    {
        private TextLayout _layout;
        public float Width { get => _layout.Metrics.Width; }
        public float Height { get => _layout.Metrics.Height; }
        public float MaxWidth { get => _layout.MaxWidth; }
        public float MaxHeight { get => _layout.MaxHeight; }
        public readonly string text;
        public readonly TextFormat textFormat;

        public TextLayoutD2D(string text, TextFormatD2D textFormat, float maxWidth, float maxHeight)
        {
            this.text = text;
            this.textFormat = new TextFormatFactory().CreateTextFormat(textFormat);
            _layout = new TextLayout(new Factory(), text, this.textFormat, maxWidth, maxHeight);
        }
    }

    internal class TextLayoutFactory
    {
        private readonly Factory _factory;

        public TextLayoutFactory()
        {
            _factory = new Factory();
        }

        public TextLayout CreateTextLayout(TextLayoutD2D layout)
        {
            return new TextLayout(_factory, layout.text, layout.textFormat, layout.MaxWidth, layout.MaxHeight);
        }
    }
}
