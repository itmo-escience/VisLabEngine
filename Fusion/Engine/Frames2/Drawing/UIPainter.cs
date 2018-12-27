using System.Collections.Generic;
using System.Linq;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Drawing
{
    public class UIPainter
    {
        private readonly UIContainer _root;

        public UIPainter(UIContainer root)
        {
            _root = root;
        }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Clear();

            DrawNonRecursive(layer);
        }

        private void DrawNonRecursive(SpriteLayerD2D layer)
        {
            var queue = new Queue<UIComponent>();
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
