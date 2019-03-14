﻿using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.SpritesD2D;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace Fusion.Engine.Frames2
{
    public interface UIReadonlyContainer<out T> : UIComponent where T : ISlot
    {
        IEnumerable<T> Slots { get; }

        int IndexOf(UIComponent child);

        bool Contains(UIComponent component);
    }

    public interface UIContainer<out T> : UIReadonlyContainer<T> where T : ISlot
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

    public static class UIContainerExtensions
    {
        public static int IndexOf<T>(this UIReadonlyContainer<T> c, UIComponent child) where T : ISlot
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

        public static bool Contains<T>(this UIReadonlyContainer<T> c, UIComponent component) where T : ISlot
        {
            var holder = c.Slots.FirstOrDefault(slot => slot.Component == component);

            return holder != null;
        }

        public static IEnumerable<UIComponent> GetChildren<T>(this UIReadonlyContainer<T> container) where T : ISlot
        {
            return container.Slots.Select(slot => slot.Component);
        }

        internal static PathGeometryD2D GetClippingGeometry(this ISlot slot, SpriteLayerD2D layer)
        {
            var geometry = new PathGeometry(layer.Factory);
            var sink = geometry.Open();
            sink.SetFillMode(FillMode.Winding);

            var p0 = Matrix3x2.TransformPoint(slot.Transform, new Vector2(0, 0)).ToRawVector2();
            var p1 = Matrix3x2.TransformPoint(slot.Transform, new Vector2(0, slot.Width)).ToRawVector2();
            var p2 = Matrix3x2.TransformPoint(slot.Transform, new Vector2(slot.Width, slot.Height)).ToRawVector2();
            var p3 = Matrix3x2.TransformPoint(slot.Transform, new Vector2(slot.Width, 0)).ToRawVector2();

            sink.BeginFigure(p0, FigureBegin.Filled);
            sink.AddLine(p1);
            sink.AddLine(p2);
            sink.AddLine(p3);
            sink.EndFigure(FigureEnd.Closed);
            sink.Close();
            sink.Dispose();

            return new PathGeometryD2D(geometry);
        }
    }
}
