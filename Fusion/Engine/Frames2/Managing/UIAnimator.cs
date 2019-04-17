using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Frames2.Utils;

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
