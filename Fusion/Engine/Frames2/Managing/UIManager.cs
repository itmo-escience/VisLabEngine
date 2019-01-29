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

                layer.Draw(new TransformCommand(c.GlobalTransform));
                c.Draw(layer);

                if (DebugEnabled)
                {
                    layer.Draw(new TransformCommand(c.GlobalTransform));
                    c.DebugDraw(layer);
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

        public static IEnumerable<UIComponent> BFSTraverseForPoint(UIComponent root, Vector2 point)
        {
            var queue = new Queue<UIComponent>();
            if (root.IsInside(point)) queue.Enqueue(root);

            while (queue.Any())
            {
                var c = queue.Dequeue();
                yield return c;

                if (c is UIContainer container)
                {
                    foreach (var child in container.Children)
                    {
                        if (child.IsInside(point)) queue.Enqueue(child);
                    }
                }
            }
        }

        public static IEnumerable<UIComponent> DFSTraverse(UIComponent root)
        {
            var stack = new Stack<UIComponent>();
            var stack2 = new Stack<UIComponent>();
            stack.Push(root);

            while (stack.Any())
            {
                var c = stack.Pop();
                stack2.Push(c);

                if (c is UIContainer container)
                {
                    foreach (var child in container.Children)
                    {
                        stack.Push(child);
                    }
                }
            }

            while (stack2.Any())
            {
                var c = stack2.Pop();
                yield return c;
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
