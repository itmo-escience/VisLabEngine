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
        private UIComponent _focusComponent;
        private Keys _lastMouseKey;
        private Keys _lastKey;
        private UIComponent _lastMouseDownComponent;

        internal UIEventProcessor(UIContainer root)
        {
            _root = root;
            _game = Game.Instance;
            _focusComponent = root;

            SubscribeToInputEvents();
        }

        private void SubscribeToInputEvents()
        {
            //wrong Click
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

            //KeyDown
            _game.Keyboard.KeyDown += (sender, args) => {
                _focusComponent.InvokeKeyDown(this, (KeyEventArgs)args);    //TODO invoke for parents?
                _lastKey = args.Key;
            };

            //KeyUp
            _game.Keyboard.KeyUp += (sender, args) => {
                _focusComponent.InvokeKeyUp(this, (KeyEventArgs)args);      //also
            };

            //MouseMove
            _game.Mouse.Move += (sender, args) =>
            {
                Vector2 mousePosition = _game.Mouse.Position;
                foreach (var c in UIManager.BFSTraverseForPoint(_root, mousePosition))
                {
                    c.InvokeMouseMove(this, (MoveEventArgs)args);
                }
            };
            /*_game.Touch.Manipulate += (args) => {
                Vector2 touchPosition = args.Position;
                foreach (var c in UIManager.BFSTraverseForPoint(_root, touchPosition))
                {
                    c.InvokeMouseMove(this, );
                }
            };*/

            //MouseDrag
            _game.Mouse.Move += (sender, args) =>
            {
                if (_game.Keyboard.IsKeyDown(_lastMouseKey))
                {
                    Vector2 mousePosition = _game.Mouse.Position;
                    foreach (var c in UIManager.BFSTraverseForPoint(_root, mousePosition))
                    {
                        c.InvokeMouseDrag(this, new DragEventArgs(_lastMouseKey, args));
                    }
                }
            };

            //MouseDown
            _game.Keyboard.KeyDown += (sender, args) => {
                if (args.Key.IsMouseKey())
                {
                    Vector2 mousePosition = _game.Mouse.Position;
                    UIComponent lastComponent = null;
                    foreach (var c in UIManager.BFSTraverseForPoint(_root, mousePosition))
                    {
                        c.InvokeMouseDown(this, new ClickEventArgs(args.Key, mousePosition));
                        lastComponent = c;
                    }
                    _lastMouseKey = args.Key;
                    _lastMouseDownComponent = lastComponent;
                }
            };
            _game.Touch.Hold += (args) => {
                _focusComponent.InvokeMouseDown(this, new ClickEventArgs(Keys.RightButton, args.Position)); //TODO hold == press?
            };

            //MouseUp
            _game.Keyboard.KeyUp += (sender, args) => {
                if (args.Key.IsMouseKey())
                {
                    Vector2 mousePosition = _game.Mouse.Position;
                    foreach (var c in UIManager.BFSTraverseForPoint(_root, mousePosition))
                    {
                        c.InvokeMouseUp(this, new ClickEventArgs(args.Key, mousePosition));
                    }
                }
            };

            //Click
            //TODO
            _game.Touch.Tap += (args) => {
                foreach (var c in UIManager.BFSTraverseForPoint(_root, args.Position))
                {
                    c.InvokeClick(this, new ClickEventArgs(Keys.LeftButton, args.Position));
                }
            };

            //DoubleClick
            //TODO
            _game.Touch.DoubleTap += (args) => {
                foreach (var c in UIManager.BFSTraverseForPoint(_root, args.Position))
                {
                    c.InvokeDoubleClick(this, new ClickEventArgs(Keys.LeftButton, args.Position));
                }
            };

            //Scroll
            _game.Mouse.Scroll += (sender, args) =>
            {
                _focusComponent.InvokeScroll(this, new ScrollEventArgs(_game.Mouse.Position, args));
            };

            //Enter
            //TODO

            //Leave
            //TODO

            //Focus
            //TODO

            //Blur
            //TODO
        }

        public void Update(GameTime time)
        {
            //KeyPress
            if (_game.Keyboard.IsKeyDown(_lastKey))
            {
                _focusComponent.InvokeKeyPress(this, new KeyEventArgs(_lastKey));
            }
        }
    }
}
