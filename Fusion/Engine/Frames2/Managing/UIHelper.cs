using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Frames2.Managing
{
    internal class UIHelper
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

        public static List<UIComponent> GetAllComponentsByPoint(UIContainer root, Vector2 innerPoint)
        {
            List<UIComponent> components = new List<UIComponent>();
            foreach (var c in UIHelper.BFSTraverseForPoint(root, innerPoint)) components.Add(c);
            return components;
        }
    }
}
