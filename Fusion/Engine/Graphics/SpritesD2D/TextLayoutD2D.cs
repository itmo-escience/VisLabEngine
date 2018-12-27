using SharpDX.DirectWrite;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class TextLayoutD2D
    {
        public TextLayout Layout { get; }

        internal TextLayoutD2D(TextLayout layout)
        {
            Layout = layout;
        }
    }

    public class TextLayoutD2DFactory
    {
        private Factory _factory;
        internal TextLayoutD2DFactory(Factory factory)
        {
            _factory = factory;
        }
    }
}
