using Fusion.Engine.Common;

namespace Fusion.Engine.Frames2.Managing
{
    public class UIAnimator
    {
        private UIContainer<ISlot> _root;

        internal UIAnimator(UIContainer<ISlot> root)
        {
            _root = root;
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
