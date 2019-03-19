namespace Fusion.Engine.Frames2.Events
{
    public class UIEventsHolder : IUIInputAware
    {
        public event KeyDownEvent KeyDown;
        public event KeyUpEvent KeyUp;
        public event KeyPressEvent KeyPress;

        public event MouseMoveEvent MouseMove;
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
        internal void InvokeKeyDown(UIComponent sender, KeyEventArgs args)
        {
            KeyDown?.Invoke(sender, args);
        }

        internal void InvokeKeyUp(UIComponent sender, KeyEventArgs args)
        {
            KeyUp?.Invoke(sender, args);
        }

        internal void InvokeKeyPress(UIComponent sender, KeyPressEventArgs args)
        {
            KeyPress?.Invoke(sender, args);
        }
        #endregion

        #region mouse
        internal void InvokeMouseMove(UIComponent sender, MoveEventArgs args)
        {
            MouseMove?.Invoke(sender, args);
        }

        internal void InvokeMouseDrag(UIComponent sender, DragEventArgs args)
        {
            MouseDrag?.Invoke(sender, args);
        }

        internal void InvokeMouseDown(UIComponent sender, ClickEventArgs args)
        {
            MouseDown?.Invoke(sender, args);
        }

        internal void InvokeMouseDownOutside(UIComponent sender, ClickEventArgs args)
        {
            MouseDownOutside?.Invoke(sender, args);
        }

        internal void InvokeMouseUp(UIComponent sender, ClickEventArgs args)
        {
            MouseUp?.Invoke(sender, args);
        }

        internal void InvokeMouseUpOutside(UIComponent sender, ClickEventArgs args)
        {
            MouseUpOutside?.Invoke(sender, args);
        }

        internal void InvokeClick(UIComponent sender, ClickEventArgs args)
        {
            Click?.Invoke(sender, args);
        }

        internal void InvokeDoubleClick(UIComponent sender, ClickEventArgs args)
        {
            DoubleClick?.Invoke(sender, args);
        }

        internal void InvokeEnter(UIComponent sender)
        {
            Enter?.Invoke(sender);
        }

        internal void InvokeLeave(UIComponent sender)
        {
            Leave?.Invoke(sender);
        }

        internal void InvokeScroll(UIComponent sender, ScrollEventArgs args)
        {
            Scroll?.Invoke(sender, args);
        }
        #endregion

        internal void InvokeFocus(UIComponent sender)
        {
            Focus?.Invoke(sender);
        }

        internal void InvokeBlur(UIComponent sender)
        {
            Blur?.Invoke(sender);
        }
    }
}