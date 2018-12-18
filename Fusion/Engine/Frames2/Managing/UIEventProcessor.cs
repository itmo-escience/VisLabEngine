using System.Collections.Generic;
using System.Linq;
using Fusion.Engine.Common;

namespace Fusion.Engine.Frames2.Managing
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
            var stack = new Stack<UIComponent>();

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
