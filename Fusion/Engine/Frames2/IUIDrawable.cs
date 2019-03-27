using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2
{
    public interface IUIDrawable
    {
        void Draw(SpriteLayerD2D layer);
    }

    public interface IUIDebugDrawable
    {
        SolidBrushD2D DebugBrush { get; }
        TextFormatD2D DebugTextFormat { get; }
        void DebugDraw(SpriteLayerD2D layer);
    }
}
