using System.Collections.Generic;
using Fusion.Core.Mathematics;
using SharpDX.Direct2D1;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class BrushD2D
    {
        internal Brush Brush { get; }

        internal BrushD2D(Brush brush)
        {
            Brush = brush;
        }
    }

    public class BrushD2DFactory
    {
        internal RenderTarget Target { get; }

        private readonly Dictionary<Color4, BrushD2D> _cachedBrushes = new Dictionary<Color4, BrushD2D>();

        internal BrushD2DFactory(RenderTarget target)
        {
            Target = target;
        }

        public BrushD2D GetOrCreateSolidBrush(Color4 color)
        {
            if (_cachedBrushes.TryGetValue(color, out var brush))
            {
                return brush;
            }

            brush = new BrushD2D(new SolidColorBrush(Target, color.ToRawColor4()));
            _cachedBrushes[color] = brush;
            return brush;
        }
    }
}
