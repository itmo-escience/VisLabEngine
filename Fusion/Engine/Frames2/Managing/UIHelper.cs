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

                if (c is UIContainer<ISlot> container)
                {
                    foreach (var child in container.Slots)
                    {
                        queue.Enqueue(child.Component);
                    }
                }
            }
        }

        public static IEnumerable<UIComponent> BFSTraverseForPoint(UIComponent root, Vector2 innerPoint)
        {
            var queue = new Queue<UIComponent>();
            if (root.Placement.IsInside(innerPoint)) queue.Enqueue(root);

            while (queue.Any())
            {
                var c = queue.Dequeue();
                yield return c;

                if (c is UIContainer<ISlot> container)
                {
                    foreach (var child in container.Slots)
                    {
                        if (child.IsInside(innerPoint)) queue.Enqueue(child.Component);
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

                if (c is UIContainer<ISlot> container)
                {
                    foreach (var child in container.Slots)
                    {
                        stack.Push(child.Component);
                    }
                }
            }

            while (stack2.Any())
            {
                var c = stack2.Pop();
                yield return c;
            }
        }

        public static UIComponent GetLowestComponentInHierarchy(UIContainer<ISlot> root, Vector2 innerPoint)
        {
            if (!root.Placement.IsInside(innerPoint)) return null;

            UIContainer<ISlot> lowestContainer = root;
            while (true)
            {
                var newLowest = lowestContainer.Slots.LastOrDefault(c => c.IsInside(innerPoint))?.Component;
                if (newLowest == null) return lowestContainer;
                if (newLowest is UIContainer<ISlot> newContainer)
                {
                    lowestContainer = newContainer;
                }
                else
                {
                    return newLowest;
                }
            }
        }

        public static IEnumerable<UIContainer<ISlot>> Ancestors(UIComponent component)
        {
            if(component == null)
                yield break;

            var current = component.Placement.Parent;
            while (current != null)
            {
                yield return current;
                current = current.Placement.Parent;
            }
        }
    }
}
