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

    public class BubblingEventArgs : EventArgs
    {
        public bool ShouldPropagate { get; set; } = true;
    }

    public class KeyEventArgs : BubblingEventArgs
    {
        public Keys Key { get; }

        public KeyEventArgs(Keys key)
        {
            Key = key;
        }

        public static explicit operator KeyEventArgs(Input.KeyEventArgs args)
        {
            return new KeyEventArgs(args.Key);
        }
    }

    public class MoveEventArgs : BubblingEventArgs
    {
        public Vector2 Position { get; }
        public Vector2 Offset { get; }

        public MoveEventArgs(Vector2 position, Vector2 offset)
        {
            Position = position;
            Offset = offset;
        }

        public static explicit operator MoveEventArgs(MouseMoveEventArgs args)
        {
            return new MoveEventArgs(args.Position, args.Offset);
        }
    }

    public class ClickEventArgs : BubblingEventArgs
    {
        public Keys Key { get; }
        public Vector2 Position { get; }

        public ClickEventArgs(Keys key, Vector2 position)
        {
            Key = key;
            Position = position;
        }
    }

    public class DragEventArgs : BubblingEventArgs
    {
        public Keys Key { get; }
        public Vector2 Position { get; }
        public Vector2 Offset { get; }

        public DragEventArgs(Keys key, MouseMoveEventArgs args)
        {
            Key = key;
            Position = args.Position;
            Offset = args.Offset;
        }
    }

    public class ScrollEventArgs : BubblingEventArgs
    {
        public Vector2 Position { get; }
        public int WheelDelta { get; }

        public ScrollEventArgs(Vector2 position, MouseScrollEventArgs args)
        {
            Position = position;
            WheelDelta = args.WheelDelta;
        }
    }
}