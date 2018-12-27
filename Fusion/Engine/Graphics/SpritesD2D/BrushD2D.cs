using System.Collections.Generic;
using Fusion.Core.Mathematics;
using SharpDX.Direct2D1;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public interface IBrushD2D
    {
        Color4 Color { get; }
    }

    public class SolidBrushD2D : IBrushD2D
    {
        public Color4 Color { get; }
        public SolidBrushD2D(Color4 color)
        {
            Color = color;
        }
    }

    public class BrushFactory
    {
        internal RenderTarget Target { get; }

        private readonly Dictionary<IBrushD2D, Brush> _cachedBrushes = new Dictionary<IBrushD2D, Brush>();

        internal BrushFactory(RenderTarget target)
        {
            Target = target;
        }

        public Brush GetOrCreateBrush(IBrushD2D brush)
        {
            if (_cachedBrushes.TryGetValue(brush, out var result))
            {
                return result;
            }

            if (brush is SolidBrushD2D solid)
            {
                result = new SolidColorBrush(Target, solid.Color.ToRawColor4());
            }
            else // TODO: other options
            {
                result = new SolidColorBrush(Target, brush.Color.ToRawColor4());
            }

            _cachedBrushes[brush] = result;
            return result;
        }
    }
}
