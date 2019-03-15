using Fusion.Engine.Common;

namespace Fusion.Engine.Frames2.Managing
{
    public class UIAnimator
    {
        private IUIModifiableContainer<ISlot> _root;

        internal UIAnimator(IUIModifiableContainer<ISlot> root)
        {
            _root = root;
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
