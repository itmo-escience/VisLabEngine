using Fusion.Engine.Common;
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Frames2.Components
{
    public sealed class Image : UIComponent, IUIMouseAware
    {
        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(SpriteLayer layer)
        {

        }

        public event MouseEvent MouseIn;
        public event MouseEvent MouseOver;
        public event MouseEvent MouseMove;
        public event MouseEvent MouseOut;
        public event MouseEvent MouseDrag;
        public event MouseEvent MouseDown;
        public event MouseEvent MouseUp;
        public event MouseEvent MouseClick;
    }
}
