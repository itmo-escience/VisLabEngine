using Fusion.Engine.Common;

namespace Fusion.Engine.Frames2.Managing
{
    public class UIAnimator
    {
        private IUIModifiableContainer<Slot> _root;

        internal UIAnimator(IUIModifiableContainer<Slot> root)
        {
            _root = root;
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
