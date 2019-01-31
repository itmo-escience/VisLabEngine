using Fusion.Engine.Common;

namespace Fusion.Engine.Frames2.Controllers
{
    public class ButtonController : UIController
    {
        public ButtonController()
        {

        }

        protected override void AttachAction()
        {
            Host.Click += (processor, args) =>
            {
                Log.Message("Clicked");
            };
        }

        protected override void DetachAction() { }

        public override void Update(GameTime gameTime) { }
    }

    public class ToggleController : UIController
    {
        public bool Toggled { get; private set; }
        public ToggleController()
        {

        }

        protected override void AttachAction()
        {
            Host.Click += (processor, args) =>
            {
                Toggled = !Toggled;
                Log.Message("Toggle state: {0}", Toggled);
            };
        }

        protected override void DetachAction() { }

        public override void Update(GameTime gameTime) { }
    }
}
