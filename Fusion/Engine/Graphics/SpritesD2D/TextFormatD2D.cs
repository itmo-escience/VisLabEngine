using SharpDX.DirectWrite;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class TextFormatD2D
    {
        internal TextFormat Format { get; }
        internal TextFormatD2D(TextFormat format)
        {
            Format = format;
        }
    }

    public class TextFormatD2DFactory
    {
        private readonly Factory _factory;

        internal TextFormatD2DFactory(Factory dwFactory)
        {
            _factory = dwFactory;
        }

        public TextFormatD2D CreateTextFormat(string fontFamilyName, float fontSize)
        {
            var format = new TextFormat(_factory, fontFamilyName, fontSize);

            return new TextFormatD2D(format);
        }
    }
}
