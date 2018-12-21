using Fusion.Core;
using Fusion.Engine.Common;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    internal class SpriteLayerD2D : DisposableBase
    {
        private RenderTarget _target;
        private SolidColorBrush _brush;

        public SpriteLayerD2D(RenderTarget renderTarget2D)
        {
            _target = renderTarget2D;

            _brush = new SolidColorBrush(_target, SharpDX.Color.White);
        }

        public void Draw(GameTime gameTime)
        {
            _target.DrawEllipse(new Ellipse(new RawVector2(100, 100), 30, 30), _brush);
            _target.DrawEllipse(new Ellipse(new RawVector2(250, 100), 30, 30), _brush);
            _target.DrawEllipse(new Ellipse(new RawVector2(250, 250), 30, 30), _brush);
            _target.DrawEllipse(new Ellipse(new RawVector2(100, 250), 30, 30), _brush);
        }
    }
}
