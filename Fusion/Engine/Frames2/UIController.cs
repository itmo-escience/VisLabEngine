using Fusion.Engine.Common;

namespace Fusion.Engine.Frames2
{
    public abstract class UIController
    {
        protected bool IsAttached { get; private set; }
        public UIComponent Host { get; private set; }

        public void AttachTo(UIComponent host)
        {
            Host = host;
            AttachAction();
            IsAttached = true;
        }

        public void Detach()
        {
            DetachAction();
            Host = null;
            IsAttached = false;
        }

        protected abstract void AttachAction();
        protected abstract void DetachAction();

        public abstract void Update(GameTime gameTime);
    }
}
