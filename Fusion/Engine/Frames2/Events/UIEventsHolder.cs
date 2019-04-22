using Fusion.Engine.Frames2.Utils;

namespace Fusion.Engine.Frames2.Events
{
    public class UIEventsHolder : IUIInputAware
    {
        public event KeyDownEvent KeyDown;
        public event KeyUpEvent KeyUp;
        public event KeyPressEvent KeyPress;

        public event MouseMoveEvent MouseMove;
        public event MouseMoveEvent MouseMoveOutside;
        public event MouseDragEvent MouseDrag;
        public event MouseDownEvent MouseDown;
        public event MouseDownEvent MouseDownOutside;
        public event MouseUpEvent MouseUp;
        public event MouseUpEvent MouseUpOutside;

        public event ClickEvent Click;
        public event DoubleClickEvent DoubleClick;
        public event EnterEvent Enter;
        public event LeaveEvent Leave;
        public event ScrollEvent Scroll;

        public event FocusEvent Focus;
        public event BlurEvent Blur;

        #region keyboard
        internal void InvokeKeyDown(IUIComponent sender, KeyEventArgs args)
        {
            KeyDown?.Invoke(sender, args);
        }

        internal void InvokeKeyUp(IUIComponent sender, KeyEventArgs args)
        {
            KeyUp?.Invoke(sender, args);
        }

        internal void InvokeKeyPress(IUIComponent sender, KeyPressEventArgs args)
        {
            KeyPress?.Invoke(sender, args);
        }
        #endregion

        #region mouse
        internal void InvokeMouseMove(IUIComponent sender, MoveEventArgs args)
        {
            MouseMove?.Invoke(sender, args);
        }

        internal void InvokeMouseMoveOutside(IUIComponent sender, MoveEventArgs args)
        {
            MouseMoveOutside?.Invoke(sender, args);
        }

        internal void InvokeMouseDrag(IUIComponent sender, DragEventArgs args)
        {
            MouseDrag?.Invoke(sender, args);
        }

        internal void InvokeMouseDown(IUIComponent sender, ClickEventArgs args)
        {
            MouseDown?.Invoke(sender, args);
        }

        internal void InvokeMouseDownOutside(IUIComponent sender, ClickEventArgs args)
        {
            MouseDownOutside?.Invoke(sender, args);
        }

        internal void InvokeMouseUp(IUIComponent sender, ClickEventArgs args)
        {
            MouseUp?.Invoke(sender, args);
        }

        internal void InvokeMouseUpOutside(IUIComponent sender, ClickEventArgs args)
        {
            MouseUpOutside?.Invoke(sender, args);
        }

        internal void InvokeClick(IUIComponent sender, ClickEventArgs args)
        {
            Click?.Invoke(sender, args);
        }

        internal void InvokeDoubleClick(IUIComponent sender, ClickEventArgs args)
        {
            DoubleClick?.Invoke(sender, args);
        }

        internal void InvokeEnter(IUIComponent sender)
        {
            Enter?.Invoke(sender);
        }

        internal void InvokeLeave(IUIComponent sender)
        {
            Leave?.Invoke(sender);
        }

        internal void InvokeScroll(IUIComponent sender, ScrollEventArgs args)
        {
            Scroll?.Invoke(sender, args);
        }
        #endregion

        internal void InvokeFocus(IUIComponent sender)
        {
            Focus?.Invoke(sender);
        }

        internal void InvokeBlur(IUIComponent sender)
        {
            Blur?.Invoke(sender);
        }
    }
}