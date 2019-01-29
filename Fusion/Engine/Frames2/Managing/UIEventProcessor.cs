using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Input;

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
            _game.Keyboard.KeyUp += (sender, args) => {
                if (args.Key.IsMouseKey())
                {
                    Vector2 mousePosition = _game.Mouse.Position;
                    foreach (var c in UIManager.BFSTraverseForPoint(_root, mousePosition))
                    {
                        c.InvokeClick(this, new ClickEventArgs(args.Key, _game.Mouse.Position));
                    }
                }
            };
        }

        public void Update(GameTime time)
        {
            /*
            var stack = new Stack<UIComponent>();

            stack.Push(_root);

            while (stack.Any())
            {
                var top = stack.Pop();

                if (top is UIContainer)
                {

                }
            }
            */
        }
    }
}
