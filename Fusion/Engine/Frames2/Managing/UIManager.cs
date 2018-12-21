using Fusion.Engine.Common;
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Frames2.Managing
{
    public class UIManager
    {
        public UIPainter UIPainter { get; }
        public UIEventProcessor UIEventProcessor { get; }
        public UIContainer Root;

        internal UIManager()
        {
            UIPainter = new UIPainter(Root);
            UIEventProcessor = new UIEventProcessor(Root);
        }

        public void Update(GameTime gameTime)
        {
            UIEventProcessor.Update(gameTime);
        }

        public void Draw(SpriteLayer layer)
        {
            layer.Clear();

            UIPainter.Draw(layer);
        }
    }
}
