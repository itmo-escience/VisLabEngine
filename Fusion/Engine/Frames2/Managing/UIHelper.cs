﻿using Fusion.Core.Mathematics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Fusion.Engine.Frames2.Managing
{
    public abstract class PropertyChangedHelper
    {
        #region PropertyChaged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets field with new value and fires <seealso cref="PropertyChanged"/> event if provided value is different from the old one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">Private field to set.</param>
        /// <param name="value">New value.</param>
        /// <param name="propertyName">Name that will be passed in a PropertyChanged event.</param>
        /// <returns>True if new value is different and PropertyChanged event was fired, false otherwise.</returns>
        protected bool SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            NotifyPropertyChanged(propertyName);

            return true;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public static class UIHelper
    {
        public static IEnumerable<UIComponent> BFSTraverse(UIComponent root)
        {
            var queue = new Queue<UIComponent>();
            queue.Enqueue(root);

            while (queue.Any())
            {
                var c = queue.Dequeue();
                yield return c;

                if (c is IUIContainer<Slot> container)
                {
                    foreach (var child in container.Slots)
                    {
                        queue.Enqueue(child.Component);
                    }
                }
            }
        }

        public static IEnumerable<UIComponent> BFSTraverseForPoint(UIManager manager, UIComponent root, Vector2 point)
        {
            var queue = new Queue<UIComponent>();

            if (InsideComponent(manager, root, point)) queue.Enqueue(root);

            while (queue.Any())
            {
                var c = queue.Dequeue();
                yield return c;

                if (c is IUIContainer<Slot> container)
                {
                    foreach (var child in container.Slots.Select(s => s.Component))
                    {
                        if (InsideComponent(manager, child, point))
                            queue.Enqueue(child);
                    }
                }
            }
        }

        private static bool InsideComponent(UIManager manager, UIComponent component, Vector2 point)
        {
            if (component is IUIContainer<Slot> container && !container.Placement.Clip)
            {
                return true;
            }
            return manager.IsInsideSlotInternal(component.Placement, point);
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

                if (c is IUIContainer<Slot> container)
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

        public static UIComponent GetLowestComponentInHierarchy(UIManager manager, IUIContainer<Slot> root, Vector2 innerPoint)
        {
            if (!InsideComponent(manager, root, innerPoint)) return null;

            var lowestContainer = root;
            while (true)
            {
                var newLowest = lowestContainer.Slots
                    .LastOrDefault(c => InsideComponent(manager, c.Component, innerPoint))?
                    .Component;

                if (newLowest == null) return lowestContainer;
                if (newLowest is IUIContainer<Slot> newContainer)
                {
                    lowestContainer = newContainer;
                }
                else
                {
                    return newLowest;
                }
            }
        }

        public static IEnumerable<IUIContainer<Slot>> Ancestors(UIComponent component)
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

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            return dict.TryGetValue(key, out var result) ? result : defaultValue;
        }
    }
}
