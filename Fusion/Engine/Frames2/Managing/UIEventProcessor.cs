using System.Collections.Generic;
using System.Diagnostics;
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
        private UIComponent _lastClickComponent;
        private Stopwatch _clickStopwatch;
        private const long _clickDelay = 500;
        private List<UIComponent> _componentsWithMouse;

        internal UIEventProcessor(UIContainer root)
        {
            _root = root;
            _game = Game.Instance;
            _focusComponent = root;
            _clickStopwatch = new Stopwatch();
            _componentsWithMouse = new List<UIComponent>();

            SubscribeToInputEvents();
        }

        private void SubscribeToInputEvents()
        {
            //KeyDown
            _game.Keyboard.KeyDown += (sender, args) => {
                _focusComponent.InvokeKeyDown((KeyEventArgs)args);    //TODO invoke for parents?
                _lastKey = args.Key;
            };

            //KeyUp
            _game.Keyboard.KeyUp += (sender, args) => {
                _focusComponent.InvokeKeyUp((KeyEventArgs)args);      //also
            };

            //MouseMove (mouse)
            _game.Mouse.Move += (sender, args) =>
            {
                Vector2 mousePosition = _game.Mouse.Position;
                foreach (var c in UIHelper.BFSTraverseForPoint(_root, mousePosition))
                {
                    c.InvokeMouseMove((MoveEventArgs)args);
                }
            };

            /*//MouseMove (touch)
            _game.Touch.Manipulate += (args) => {
                Vector2 touchPosition = args.Position;
                foreach (var c in UIManager.BFSTraverseForPoint(_root, touchPosition))
                {
                    c.InvokeMouseMove( );
                }
            };*/

            //MouseDrag (mouse)
            _game.Mouse.Move += (sender, args) =>
            {
                if (_game.Keyboard.IsKeyDown(_lastMouseKey))
                {
                    Vector2 mousePosition = _game.Mouse.Position;
                    foreach (var c in UIHelper.BFSTraverseForPoint(_root, mousePosition))
                    {
                        c.InvokeMouseDrag(new DragEventArgs(_lastMouseKey, args));
                    }
                }
            };

            /*//MouseDrag (touch)
            _game.Touch.Hold += (args) => {
                foreach (var c in UIHelper.BFSTraverseForPoint(_root, args.Position))
                {
                    c.InvokeMouseDrag(new DragEventArgs(Keys.LeftButton, ));
                }
            };*/

            //MouseDown (mouse)
            _game.Keyboard.KeyDown += (sender, args) => {
                if (args.Key.IsMouseKey())
                {
                    Vector2 mousePosition = _game.Mouse.Position;
                    foreach (var c in UIHelper.BFSTraverseForPoint(_root, mousePosition))
                    {
                        c.InvokeMouseDown(new ClickEventArgs(args.Key, mousePosition));
                    }

                    _lastMouseKey = args.Key;
                    _lastMouseDownComponent = UIHelper.GetLowestComponentInHierarchy(_root, mousePosition);
                }
            };

            //MouseDown (touch)
            _game.Touch.Hold += (args) => {
                _focusComponent.InvokeMouseDown(new ClickEventArgs(Keys.RightButton, args.Position)); //TODO hold == press?
            };

            //MouseUp
            _game.Keyboard.KeyUp += (sender, args) => {
                if (args.Key.IsMouseKey())
                {
                    Vector2 mousePosition = _game.Mouse.Position;
                    foreach (var c in UIHelper.BFSTraverseForPoint(_root, mousePosition))
                    {
                        c.InvokeMouseUp(new ClickEventArgs(args.Key, mousePosition));
                    }
                }
            };

            //Click + DoubleClick (mouse)
            _game.Keyboard.KeyUp += (sender, args) => {
                if (args.Key == _lastMouseKey)
                {
                    Vector2 mousePosition = _game.Mouse.Position;
                    UIComponent currentComponent = UIHelper.GetLowestComponentInHierarchy(_root, mousePosition);
                    if (currentComponent == _lastMouseDownComponent)
                    {
                        currentComponent?.InvokeClick(new ClickEventArgs(args.Key, mousePosition));

                        if (currentComponent == _lastClickComponent)
                        {
                            _clickStopwatch.Stop();
                            if (_clickStopwatch.ElapsedMilliseconds < _clickDelay)
                            {
                                currentComponent?.InvokeDoubleClick(new ClickEventArgs(args.Key, mousePosition));
                                _lastClickComponent = null;     //TODO is normal?
                            }
                        } else {
                            _lastClickComponent = currentComponent;
                        }
                        _clickStopwatch.Restart();
                    }
                }
            };

            //Click (touch)
            _game.Touch.Tap += (args) => {
                foreach (var c in UIHelper.BFSTraverseForPoint(_root, args.Position))
                {
                    c.InvokeClick(new ClickEventArgs(Keys.LeftButton, args.Position));
                }
            };

            //DoubleClick (touch)
            _game.Touch.DoubleTap += (args) => {
                foreach (var c in UIHelper.BFSTraverseForPoint(_root, args.Position))
                {
                    c.InvokeDoubleClick(new ClickEventArgs(Keys.LeftButton, args.Position));
                }
            };

            //Scroll
            _game.Mouse.Scroll += (sender, args) =>
            {
                _focusComponent.InvokeScroll(new ScrollEventArgs(_game.Mouse.Position, args));
            };

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
                _focusComponent.InvokeKeyPress(new KeyEventArgs(_lastKey));
            }

            //Enter + Leave (mouse)
            InvokeEnterAndLeaveComponentsEvents();
        }

        private void InvokeEnterAndLeaveComponentsEvents()
        {
            List<UIComponent> newComponentsWithMouse = UIHelper.GetAllComponentsByPoint(_root, _game.Mouse.Position);
            foreach (UIComponent c in newComponentsWithMouse.Except(_componentsWithMouse))
            {
                c.InvokeEnter();
            }
            foreach (UIComponent c in _componentsWithMouse.Except(newComponentsWithMouse))
            {
                c.InvokeLeave();
            }
            _componentsWithMouse = newComponentsWithMouse;
        }
    }
}
