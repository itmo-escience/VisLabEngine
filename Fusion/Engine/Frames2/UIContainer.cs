using System.Collections.Generic;
using System.Linq;

namespace Fusion.Engine.Frames2
{
    public interface IUIContainer : UIComponent
    {
        IEnumerable<ISlot> Slots { get; }

        int IndexOf(UIComponent child);

        bool Contains(UIComponent component);
    }

    public interface IUIModifiableContainer<out T> : IUIContainer where T : ISlot
    {
        T Insert(UIComponent child, int index);

        bool Remove(UIComponent child);

        /*
        #region ZOrder
        public void InsertAtFront(UIComponent child)
        {
            Insert(child, Slots.Count);
        }

        public void InsertAtBack(UIComponent child)
        {
            Insert(child, 0);
        }

        public void InsertInFrontOf(UIComponent child, UIComponent otherChild)
        {
            if (!Contains(otherChild))
                return;

            Insert(child, IndexOf(otherChild) + 1);
        }

        public void MoveTo(UIComponent child, int index)
        {
            if (!Contains(child))
                return;

            if (index < 0) index = 0;

            Remove(child);
            Insert(child, index);
        }

        public void BringToFront(UIComponent child)
        {
            MoveTo(child, Count);
        }

        public void SendToBack(UIComponent child)
        {
            MoveTo(child, 0);
        }

        public void BringForward(UIComponent child)
        {
            MoveTo(child, IndexOf(child) + 1);
        }

        public void SendBackward(UIComponent child)
        {
            MoveTo(child, IndexOf(child) - 1);
        }
        #endregion
        */
    }

    public static class ContainerExtensions
    {
        public static int IndexOf(this IUIContainer c, UIComponent child)
        {
            var i = 0;
            foreach (var slot in c.Slots)
            {
                if (slot.Component == child)
                    return i;
                i++;
            }

            return -1;
        }

        public static bool Contains(this IUIContainer c, UIComponent component)
        {
            return c.Slots.Any(slot => slot.Component == component);
        }

        public static IEnumerable<UIComponent> GetChildren(this IUIContainer container)
        {
            return container.Slots.Select(slot => slot.Component);
        }
    }
}
