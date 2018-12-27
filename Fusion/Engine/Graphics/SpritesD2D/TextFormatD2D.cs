using SharpDX.DirectWrite;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class TextFormatD2D
    {
        public string FontFamily { get; }
        public float Size { get; }
        public TextFormatD2D(string fontFamilyName, float fontSize)
        {
            FontFamily = fontFamilyName;
            Size = fontSize;
        }
    }

    internal class TextFormatFactory
    {
        private readonly Factory _factory;

        public TextFormatFactory()
        {
            _factory = new Factory();
        }

        public TextFormat CreateTextFormat(TextFormatD2D format)
        {
            return new TextFormat(_factory, format.FontFamily, format.Size);
        }
    }
}
