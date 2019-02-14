using Fusion.Core.Mathematics;
using Fusion.Engine.Input;
using System;

namespace Fusion.Engine.Frames2.Managing
{
    public interface IUIInputAware
    {
        event KeyDownEvent KeyDown;
        event KeyUpEvent KeyUp;
        event KeyPressEvent KeyPress;

        event MouseMoveEvent MouseMove;
        event MouseDragEvent MouseDrag;

        event MouseDownEvent MouseDown;
        event MouseUpEvent MouseUp;

        event ClickEvent Click;
        event DoubleClickEvent DoubleClick;

        event EnterEvent Enter;
        event LeaveEvent Leave;

        event ScrollEvent Scroll;

        event FocusEvent Focus;
        event BlurEvent Blur;
    }

    public delegate void KeyDownEvent(UIComponent sender, KeyEventArgs e);
    public delegate void KeyUpEvent(UIComponent sender, KeyEventArgs e);
    public delegate void KeyPressEvent(UIComponent sender, KeyEventArgs e);

    public delegate void MouseMoveEvent(UIComponent sender, MoveEventArgs e);

    //mouse + touch START
    public delegate void MouseDragEvent(UIComponent sender, DragEventArgs e);

    public delegate void MouseDownEvent(UIComponent sender, ClickEventArgs e);
    public delegate void MouseUpEvent(UIComponent sender, ClickEventArgs e);

    public delegate void ClickEvent(UIComponent sender, ClickEventArgs e);
    public delegate void DoubleClickEvent(UIComponent sender, ClickEventArgs e);

    public delegate void EnterEvent(UIComponent sender);
    public delegate void LeaveEvent(UIComponent sender);
    //mouse + touch END

    public delegate void ScrollEvent(UIComponent sender, ScrollEventArgs e);

    //public delegate void HoldEvent(UIComponent sender, ClickEventArgs e);
    //public delegate void CanselEvent(UIComponent sender, MoveEventArgs e);

    public delegate void FocusEvent(UIComponent sender);
    public delegate void BlurEvent(UIComponent sender);

    public class KeyEventArgs : EventArgs
    {
        public Keys key;

        public KeyEventArgs(Keys key)
        {
            this.key = key;
        }

        public static explicit operator KeyEventArgs(Input.KeyEventArgs args)
        {
            return new KeyEventArgs(args.Key);
        }
    }

    public class MoveEventArgs : EventArgs
    {
        public Vector2 position;
        public Vector2 offset;

        public static explicit operator MoveEventArgs(MouseMoveEventArgs args)
        {
            MoveEventArgs temp = new MoveEventArgs
            {
                position = args.Position,
                offset = args.Offset
            };
            return temp;
        }
    }

    public class ClickEventArgs : EventArgs
    {
        public Keys key;
        public Vector2 position;

        public ClickEventArgs(Keys key, Vector2 position)
        {
            this.key = key;
            this.position = position;
        }
    }

    public class DragEventArgs : EventArgs
    {
        public Keys key;
        public Vector2 position;
        public Vector2 offset;

        public DragEventArgs(Keys key, MouseMoveEventArgs args)
        {
            this.key = key;
            position = args.Position;
            offset = args.Offset;
        }
    }

    public class ScrollEventArgs : EventArgs
    {
        public Vector2 position;
        public int wheelDelta;

        public ScrollEventArgs(Vector2 position, MouseScrollEventArgs args)
        {
            this.position = position;
            wheelDelta = args.WheelDelta;
        }
    }
}