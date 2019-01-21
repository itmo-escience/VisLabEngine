using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Managing
{
    public class UIManager
    {
        public UIEventProcessor UIEventProcessor { get; }
        public UIContainer Root;

        public bool DebugEnabled { get; set; }
        private readonly SolidBrushD2D _debugBrush = new SolidBrushD2D(new Color4(0, 1, 0, 1));

        public UIManager(RenderSystem rs)
        {
            Root = new FreePlacement(0, 0, rs.Width, rs.Height);

            UIEventProcessor = new UIEventProcessor(Root);
        }

        public void Update(GameTime gameTime)
        {
            UIEventProcessor.Update(gameTime);

            foreach (var c in DFSTraverse(Root).Where(c => typeof(UIContainer) != c.GetType()))
            {
                c.Update(gameTime);
            }
        }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Clear();

            DrawBFS(layer, Root);
        }

        private void DrawBFS(SpriteLayerD2D layer, UIContainer root)
        {
            var queue = new Queue<UIComponent>();
            queue.Enqueue(root);

            while (queue.Any())
            {
                var c = queue.Dequeue();

                if(!c.Visible) continue;

                if (c is UIContainer container)
                {
                    foreach (var child in container.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
                else {
                    layer.Draw(new TransformCommand(c.GlobalTransform));
                    c.Draw(layer);

                    if (DebugEnabled)
                    {
                        var b = c.BoundingBox;
                        layer.Draw(TransformCommand.Identity);
                        layer.Draw(new Rect(b.X, b.Y, b.Width, b.Height, _debugBrush));
                    }
                }
            }

            layer.Draw(TransformCommand.Identity);
        }

        public static IEnumerable<UIComponent> BFSTraverse(UIComponent root)
        {
            var queue = new Queue<UIComponent>();
            queue.Enqueue(root);

            while (queue.Any())
            {
                var c = queue.Dequeue();
                yield return c;

                if (c is UIContainer container)
                {
                    foreach (var child in container.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }

        public static IEnumerable<UIComponent> DFSTraverse(UIComponent root)
        {
            var stack = new Stack<UIComponent>();
            stack.Push(root);

            while (stack.Any())
            {
                var c = stack.Pop();
                yield return c;

                if (c is UIContainer container)
                {
                    foreach (var child in container.Children)
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        public IEnumerable<UIComponent> ComponentsAt(Vector2 point, bool includeContainers = true)
        {
            if (!Root.BoundingBox.Contains(point))
                yield break;

            var stack = new Stack<UIComponent>();
            stack.Push(Root);
            while (true)
            {
                var current = stack.Pop();
                yield return current;

                if (current is UIContainer container)
                {
                    foreach (var child in container.Children.Where(c => c.BoundingBox.Contains(point)))
                    {
                        stack.Push(child);
                    }
                }
                else break;
            }
        }
    }
}
