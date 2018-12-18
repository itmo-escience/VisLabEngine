using Fusion.Engine.Frames.Abstract;

namespace Fusion.Engine.Frames2
{
    public interface IUIMouseAware
    {
        /*
         * Movement related
         */
        event MouseEvent MouseIn;
        event MouseEvent MouseOver;
        event MouseEvent MouseMove;
        event MouseEvent MouseOut;
        event MouseEvent MouseDrag;

        /*
         * Click related
         */
        event MouseEvent MouseDown;
        event MouseEvent MouseUp;
        event MouseEvent MouseClick;
    }

    public delegate void MouseEvent(object sender, MouseEventArgs args);
}
