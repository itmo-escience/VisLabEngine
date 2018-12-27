using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Frames2.Drawing;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Managing
{
    public class UIManager
    {
        public UIPainter UIPainter { get; }
        public UIEventProcessor UIEventProcessor { get; }
        public UIContainer Root;

        public UIManager(RenderSystem rs)
        {
            Root = new FreePlacement(0, 0, rs.Width, rs.Height);
            UIPainter = new UIPainter(Root);
            UIEventProcessor = new UIEventProcessor(Root);
        }

        public void Update(GameTime gameTime)
        {
            UIEventProcessor.Update(gameTime);
        }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Clear();

            UIPainter.Draw(layer);
        }
    }
}
