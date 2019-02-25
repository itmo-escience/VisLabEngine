using Fusion.Core.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace Fusion.Engine.Frames2.Managing
{
    public class UIHelper
    {
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

        public static IEnumerable<UIComponent> BFSTraverseForPoint(UIComponent root, Vector2 innerPoint)
        {
            var queue = new Queue<UIComponent>();
            if (root.IsInside(innerPoint)) queue.Enqueue(root);

            while (queue.Any())
            {
                var c = queue.Dequeue();
                yield return c;

                if (c is UIContainer container)
                {
                    foreach (var child in container.Children)
                    {
                        if (child.IsInside(innerPoint)) queue.Enqueue(child);
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

        public static UIComponent GetLowestComponentInHierarchy(UIContainer root, Vector2 innerPoint)
        {
            if (!root.IsInside(innerPoint)) return null;

            UIContainer lowestContainer = root;
            while (true)
            {
                UIComponent newLowest = lowestContainer.Children.LastOrDefault(c => c.IsInside(innerPoint));
                if (newLowest == null) return lowestContainer;
                if (newLowest is UIContainer newContainer)
                {
                    lowestContainer = newContainer;
                }
                else
                {
                    return newLowest;
                }
            }
        }

        public static IEnumerable<UIContainer> Ancestors(UIComponent component)
        {
            if(component == null)
                yield break;

            var current = component.Parent;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }
    }
}
