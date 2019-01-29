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

    public delegate void KeyDownEvent(UIEventProcessor eventProcessor, KeyEventArgs e);
    public delegate void KeyUpEvent(UIEventProcessor eventProcessor, KeyEventArgs e);
    public delegate void KeyPressEvent(UIEventProcessor eventProcessor, KeyEventArgs e);

    public delegate void MouseMoveEvent(UIEventProcessor eventProcessor, MoveEventArgs e);

    //mouse + touch START
    public delegate void MouseDragEvent(UIEventProcessor eventProcessor, DragEventArgs e);

    public delegate void MouseDownEvent(UIEventProcessor eventProcessor, ClickEventArgs e);
    public delegate void MouseUpEvent(UIEventProcessor eventProcessor, ClickEventArgs e);

    public delegate void ClickEvent(UIEventProcessor eventProcessor, ClickEventArgs e);
    public delegate void DoubleClickEvent(UIEventProcessor eventProcessor, ClickEventArgs e);

    public delegate void EnterEvent(UIEventProcessor eventProcessor);
    public delegate void LeaveEvent(UIEventProcessor eventProcessor);
    //mouse + touch END

    public delegate void ScrollEvent(UIEventProcessor eventProcessor, ScrollEventArgs e);

    //public delegate void HoldEvent(UIEventProcessor eventProcessor, ClickEventArgs e);
    //public delegate void CanselEvent(UIEventProcessor eventProcessor, MoveEventArgs e);

    public delegate void FocusEvent(UIEventProcessor eventProcessor);
    public delegate void BlurEvent(UIEventProcessor eventProcessor);

    public class KeyEventArgs : EventArgs
    {
        public Keys key;

        public static explicit operator KeyEventArgs(Input.KeyEventArgs args)
        {
            KeyEventArgs temp = new KeyEventArgs
            {
                key = args.Key
            };
            return temp;
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
    }
}