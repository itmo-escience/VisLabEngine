﻿using System;
using Fusion.Core.Mathematics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Fusion.Core.Utils;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Frames2.Utils;

namespace Fusion.Engine.Frames2.Managing
{
    //public abstract class PropertyChangedHelper
    //{
    //    #region PropertyChaged implementation
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    /// <summary>
    //    /// Sets field with new value and fires <seealso cref="PropertyChanged"/> event if provided value is different from the old one.
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="field">Private field to set.</param>
    //    /// <param name="value">New value.</param>
    //    /// <param name="propertyName">Name that will be passed in a PropertyChanged event.</param>
    //    /// <returns>True if new value is different and PropertyChanged event was fired, false otherwise.</returns>
    //    protected bool SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    //    {
    //        if (EqualityComparer<T>.Default.Equals(field, value))
    //            return false;

    //        field = value;
    //        NotifyPropertyChanged(propertyName);

    //        return true;
    //    }

    //    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion
    //}

    public static class UIHelper
    {
        public static IEnumerable<IUIComponent> BFSTraverse(IUIComponent root)
        {
            var queue = new Queue<IUIComponent>();
            queue.Enqueue(root);
            var result = new List<IUIComponent>();

            while (queue.Any())
            {
                var c = queue.Dequeue();

				if(c != null)
                    result.Add(c);

                if (c is IUIContainer container)
                {
                    foreach (var child in container.Slots)
                    {
                        queue.Enqueue(child.Component);
                    }
                }
            }

            return result;
        }

        public static IEnumerable<IUIComponent> BFSTraverseForPoint(UIManager manager, IUIComponent root, Vector2 point)
        {
            var queue = new Queue<IUIComponent>();

            if (InsideComponent(manager, root, point)) queue.Enqueue(root);

            while (queue.Any())
            {
                var c = queue.Dequeue();
                yield return c;

                if (c is IUIContainer container)
                {
                    foreach (var child in container.Slots.Select(s => s.Component))
                    {
                        if (InsideComponent(manager, child, point))
                            queue.Enqueue(child);
                    }
                }
            }
        }

        public static bool InsideComponent(UIManager manager, IUIComponent component, Vector2 point)
        {
            if (component is IUIContainer container && !container.Placement.Clip)
            {
                return true;
            }
			if (component == null)
				return false;
            return manager.IsInsideSlotInternal(component.Placement, point);
        }

        public static IEnumerable<IUIComponent> DFSTraverse(IUIComponent root)
        {
            var stack = new Stack<IUIComponent>();
            var stack2 = new Stack<IUIComponent>();
            stack.Push(root);

            while (stack.Any())
            {
                var c = stack.Pop();
                stack2.Push(c);

                if (c is IUIContainer container)
                {
                    foreach (var child in container.Slots)
                    {
                        stack.Push(child.Component);
                    }
                }
            }

            var result = new List<IUIComponent>();
            while (stack2.Any())
            {
                var c = stack2.Pop();
				if(c != null)
                    result.Add(c);
            }

            return result;
        }

        public static IEnumerable<IUIComponent> DFSTraverse(IUIComponent root, Predicate<IUIContainer> shouldGoDeeper)
        {
            var stack = new Stack<IUIComponent>();
            var stack2 = new Stack<IUIComponent>();
            stack.Push(root);

            while (stack.Any())
            {
                var c = stack.Pop();
                stack2.Push(c);

                if (c is IUIContainer container && shouldGoDeeper(container))
                {
                    foreach (var child in container.Slots)
                    {
                        stack.Push(child.Component);
                    }
                }
            }

            var result = new List<IUIComponent>();
            while (stack2.Any())
            {
                var c = stack2.Pop();
                if(c != null)
                    result.Add(c);
            }

            return result;
        }

		public static IUIComponent GetLowestComponentInHierarchy( UIManager manager, IUIContainer root, Vector2 innerPoint)
		{
			return GetLowestComponentInHierarchy(manager, root, innerPoint, new List<IUIComponent>());
		}

		public static IUIComponent GetLowestComponentInHierarchy( UIManager manager, IUIContainer root, Vector2 innerPoint, List<IUIComponent> ignoreComponents )
        {
            if (!InsideComponent(manager, root, innerPoint)) return null;

            var lowestContainer = root;
            while (true)
            {
                var newLowest = lowestContainer.Slots.Where(c=> !ignoreComponents.Contains(c.Component))
                    .LastOrDefault(c => InsideComponent(manager, c.Component, innerPoint))?
                    .Component;

                if (newLowest == null) return lowestContainer;
                if (newLowest is IUIContainer newContainer)
                {
                    lowestContainer = newContainer;
                }
                else
                {
                    return newLowest;
                }
            }
        }

		public static IUIComponent GetComponentInChildren( UIManager manager, IUIContainer root, Vector2 innerPoint )
		{
			return GetComponentInChildren(manager, root, innerPoint, new List<IUIComponent>());
		}

		public static IUIComponent GetComponentInChildren( UIManager manager, IUIContainer root, Vector2 innerPoint, List<IUIComponent> ignoreComponents )
		{
			if (!InsideComponent(manager, root, innerPoint)) return null;

			var container = root;
			var component = container.Slots.Where(c => !ignoreComponents.Contains(c.Component))
				.FirstOrDefault(c => InsideComponent(manager, c.Component, innerPoint))?
				.Component;

				return component;
		}

		public static IEnumerable<IUIContainer> Ancestors(IUIComponent component)
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
