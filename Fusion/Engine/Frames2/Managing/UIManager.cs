using System.Collections.Generic;
using System.Linq;
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
            UIPainter = new UIPainter();
            UIEventProcessor = new UIEventProcessor(Root);
        }

        public void Update(GameTime gameTime)
        {
            UIEventProcessor.Update(gameTime);

            UpdateDFS(Root, gameTime);
        }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Clear();

            DrawBFS(layer, Root);
        }

        private void UpdateDFS(UIContainer root, GameTime gameTime)
        {
            var queue = new Stack<UIComponent>();
            queue.Push(root);

            while (queue.Any())
            {
                var c = queue.Pop();

                c.Update(gameTime);

                if (c is UIContainer container)
                {
                    foreach (var child in container.Children)
                    {
                        queue.Push(child);
                    }
                }
            }
        }

        private void DrawBFS(SpriteLayerD2D layer, UIContainer root)
        {
            var queue = new Queue<UIComponent>();
            queue.Enqueue(root);

            while (queue.Any())
            {
                var c = queue.Dequeue();

                if (c is UIContainer container)
                {
                    foreach (var child in container.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
                else if (c.Visible)
                {
                    layer.Draw(new TransformCommand(c.GlobalTransform));
                    c.Draw(layer);
                }
            }
        }
    }
}
