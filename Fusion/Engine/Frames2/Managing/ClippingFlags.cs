using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Managing
{
    internal sealed class TransformDrawable : IUIDrawable
    {
        private readonly TransformCommand _command;
        public TransformDrawable(Matrix3x2 matrix)
        {
            _command = new TransformCommand(matrix);
        }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(_command);
        }
    }

    internal sealed class StartClippingFlag : IUIDrawable
    {
        private readonly PathGeometryD2D _pathGeometry;

        public StartClippingFlag(PathGeometryD2D pathGeometry)
        {
            _pathGeometry = pathGeometry;
        }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(new StartClippingAlongGeometry(_pathGeometry, AntialiasModeD2D.Aliased));
        }
    }

    internal sealed class EndClippingFlag : IUIDrawable
    {
        public void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(new EndClippingAlongGeometry());
        }
    }
}
