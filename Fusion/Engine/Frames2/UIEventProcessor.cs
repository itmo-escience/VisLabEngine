using System.Collections.Generic;
using System.Linq;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Interfaces;

namespace Fusion.Engine.Frames2
{
    public class UIEventProcessor
    {
        private readonly Game _game;
        private readonly UIContainer _root;
        internal UIEventProcessor(UIContainer root)
        {
            _root = root;
            _game = Game.Instance;

            SubscribeToInputEvents();
        }

        private void SubscribeToInputEvents()
        {
        }

        public void Update(GameTime time)
        {
            var stack = new Stack<IUIComponent>();

            stack.Push(_root);

            while (stack.Any())
            {
                var top = stack.Pop();

                if (top is UIContainer)
                {

                }
            }
        }
    }
}
