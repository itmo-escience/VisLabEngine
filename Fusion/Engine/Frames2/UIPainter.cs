using System.Collections.Generic;
using System.Linq;
using Fusion.Engine.Frames2.Interfaces;
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Frames2
{
    public class UIPainter
    {
        private readonly UIContainer _root;

        public UIPainter(UIContainer root)
        {
            _root = root;
        }

        public void Draw(SpriteLayer layer)
        {
            layer.Clear();

            DrawNonRecursive(layer);
        }

        private void DrawNonRecursive(SpriteLayer layer)
        {
            var queue = new Queue<IUIComponent>();
            queue.Enqueue(_root);

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
                else
                {
                    c.Draw(layer);
                }
            }

            _root.Draw(layer);
        }
    }
}
