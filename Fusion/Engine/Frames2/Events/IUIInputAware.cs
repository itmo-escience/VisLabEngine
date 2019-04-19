using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2.Utils;
using Fusion.Engine.Input;

namespace Fusion.Engine.Frames2.Events
{
    public interface IUIInputAware
    {
        event KeyDownEvent KeyDown;
        event KeyUpEvent KeyUp;
        event KeyPressEvent KeyPress;

        event FocusEvent Focus;
        event BlurEvent Blur;

        event MouseMoveEvent MouseMove;
        event MouseDragEvent MouseDrag;
        event MouseDownEvent MouseDown;
        event MouseUpEvent MouseUp;

        event MouseDownEvent MouseDownOutside;
        event MouseUpEvent MouseUpOutside;

        event ClickEvent Click;
        event DoubleClickEvent DoubleClick;
        event EnterEvent Enter;
        event LeaveEvent Leave;
        event ScrollEvent Scroll;
    }
    public delegate void KeyDownEvent(IUIComponent sender, KeyEventArgs e);
    public delegate void KeyUpEvent(IUIComponent sender, KeyEventArgs e);
    public delegate void KeyPressEvent(IUIComponent sender, KeyPressEventArgs e);

    public delegate void FocusEvent(IUIComponent sender);
    public delegate void BlurEvent(IUIComponent sender);


    public delegate void MouseMoveEvent(IUIComponent sender, MoveEventArgs e);

    //mouse + touch START
    public delegate void MouseDragEvent(IUIComponent sender, DragEventArgs e);

    public delegate void MouseDownEvent(IUIComponent sender, ClickEventArgs e);
    public delegate void MouseUpEvent(IUIComponent sender, ClickEventArgs e);

    public delegate void ClickEvent(IUIComponent sender, ClickEventArgs e);
    public delegate void DoubleClickEvent(IUIComponent sender, ClickEventArgs e);

    public delegate void EnterEvent(IUIComponent sender);
    public delegate void LeaveEvent(IUIComponent sender);
    //mouse + touch END

    public delegate void ScrollEvent(IUIComponent sender, ScrollEventArgs e);


    public class BubblingEventArgs : EventArgs
    {
        public bool ShouldPropagate { get; set; } = true;
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

    public class KeyPressEventArgs : BubblingEventArgs
    {
        public char KeyChar { get; }

        public KeyPressEventArgs(char keyChar)
        {
            KeyChar = keyChar;
        }

        public static explicit operator KeyPressEventArgs(Input.KeyPressArgs args)
        {
            return new KeyPressEventArgs(args.KeyChar);
        }
    }

}
