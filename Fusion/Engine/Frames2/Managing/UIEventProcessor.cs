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

        private const int KeyPressRepetitionFrequency = 50;
        private const int KeyPressRepetitionDelay = 200;

        private enum KeyPressState
        {
            Idle, JustPressed, Pressing
        }
        private KeyPressState _keyPressState = KeyPressState.Idle;

        private double _lastKeyRepetitionMs = 0;

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
            _game.Keyboard.KeyDown += (sender, args) =>
            {
                var keyArgs = (KeyEventArgs) args;
                _focusComponent?.InvokeKeyDown(keyArgs);

                foreach (var c in UIHelper.Ancestors(_focusComponent))
                {
                    if (!keyArgs.ShouldPropagate) break;

                    c.InvokeKeyDown(keyArgs);
                }

                _lastKeyRepetitionMs = Game.Instance.Time.Total.TotalMilliseconds;
                _lastKey = args.Key;
                _keyPressState = KeyPressState.JustPressed;
            };

            //KeyUp
            _game.Keyboard.KeyUp += (sender, args) =>
            {
                if (args.Key == _lastKey)
                {
                    _keyPressState = KeyPressState.Idle;
                }


                var keyArgs = (KeyEventArgs) args;
                _focusComponent?.InvokeKeyUp(keyArgs);

                foreach (var c in UIHelper.Ancestors(_focusComponent))
                {
                    if (!keyArgs.ShouldPropagate) break;

                    c.InvokeKeyUp(keyArgs);
                }
            };

            //MouseMove (mouse)
            _game.Mouse.Move += (sender, args) =>
            {
                Vector2 mousePosition = _game.Mouse.Position;
                var mouseArgs = (MoveEventArgs) args;
                foreach (var c in UIHelper.BFSTraverseForPoint(_root, mousePosition).Reverse())
                {
                    if (!mouseArgs.ShouldPropagate)
                        break;

                    c.InvokeMouseMove(mouseArgs);
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
                if (!_game.Keyboard.IsKeyDown(_lastMouseKey)) return;

                var mousePosition = _game.Mouse.Position;
                var dragArgs = new DragEventArgs(_lastMouseKey, args);

                foreach (var c in UIHelper.BFSTraverseForPoint(_root, mousePosition).Reverse())
                {
                    if (!dragArgs.ShouldPropagate)
                        return;

                    c.InvokeMouseDrag(dragArgs);
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
            _game.Keyboard.KeyDown += (sender, args) =>
            {
                if (!args.Key.IsMouseKey()) return;

                var mousePosition = _game.Mouse.Position;
                var clickArgs = new ClickEventArgs(args.Key, mousePosition);

                foreach (var c in UIHelper.BFSTraverseForPoint(_root, mousePosition).Reverse())
                {
                    if (!clickArgs.ShouldPropagate)
                        break;

                    c.InvokeMouseDown(clickArgs);
                }

                foreach (var c in UIHelper.DFSTraverse(_root))
                {
                    if (!c.IsInside(mousePosition))
                        c.InvokeMouseDownOutside(clickArgs);
                }

                _lastMouseKey = args.Key;
                _lastMouseDownComponent = UIHelper.GetLowestComponentInHierarchy(_root, mousePosition);
                _focusComponent = _lastMouseDownComponent;
            };

            //MouseDown (touch)
            _game.Touch.Hold += (args) =>
            {
                Log.Warning("Touch is not implemented");

                _focusComponent?.InvokeMouseDown(new ClickEventArgs(Keys.RightButton,
                    args.Position)); //TODO hold == press?
            };

            //MouseUp
            _game.Keyboard.KeyUp += (sender, args) =>
            {
                if (!args.Key.IsMouseKey()) return;

                var mousePosition = _game.Mouse.Position;
                var clickArgs = new ClickEventArgs(args.Key, mousePosition);
                foreach (var c in UIHelper.BFSTraverseForPoint(_root, mousePosition).Reverse())
                {
                    if (!clickArgs.ShouldPropagate)
                        break;
                    c.InvokeMouseUp(clickArgs);
                }

                foreach (var c in UIHelper.DFSTraverse(_root))
                {
                    if (!c.IsInside(mousePosition))
                        c.InvokeMouseUpOutside(clickArgs);
                }
            };

            //Click + DoubleClick (mouse)
            _game.Keyboard.KeyUp += (sender, args) =>
            {
                if (args.Key != _lastMouseKey) return;

                var mousePosition = _game.Mouse.Position;
                var currentComponent = UIHelper.GetLowestComponentInHierarchy(_root, mousePosition);
                if (currentComponent != _lastMouseDownComponent) return;

                var clickArgs = new ClickEventArgs(args.Key, mousePosition);
                foreach (var c in UIHelper.BFSTraverseForPoint(_root, mousePosition).Reverse())
                {
                    if (!clickArgs.ShouldPropagate)
                        break;
                    c.InvokeClick(clickArgs);
                }

                if (currentComponent == _lastClickComponent)
                {
                    _clickStopwatch.Stop();
                    if (_clickStopwatch.ElapsedMilliseconds < _clickDelay)
                    {
                        clickArgs = new ClickEventArgs(args.Key, mousePosition);
                        foreach (var c in UIHelper.BFSTraverseForPoint(_root, mousePosition).Reverse())
                        {
                            if (!clickArgs.ShouldPropagate)
                                break;
                            c.InvokeDoubleClick(clickArgs);
                        }

                        _lastClickComponent = null; //TODO is normal?
                    }
                }
                else
                {
                    _lastClickComponent = currentComponent;
                }

                _clickStopwatch.Restart();
            };

            //Click (touch)
            _game.Touch.Tap += (args) =>
            {
                foreach (var c in UIHelper.BFSTraverseForPoint(_root, args.Position))
                {
                    c.InvokeClick(new ClickEventArgs(Keys.LeftButton, args.Position));
                }
            };

            //DoubleClick (touch)
            _game.Touch.DoubleTap += (args) =>
            {
                foreach (var c in UIHelper.BFSTraverseForPoint(_root, args.Position))
                {
                    c.InvokeDoubleClick(new ClickEventArgs(Keys.LeftButton, args.Position));
                }
            };

            //Scroll
            _game.Mouse.Scroll += (sender, args) =>
            {
                _focusComponent?.InvokeScroll(new ScrollEventArgs(_game.Mouse.Position, args));
            };

            //Focus
            //TODO

            //Blur
            //TODO
        }

        public void Update(GameTime time)
        {
            var shouldRepeat =_keyPressState != KeyPressState.Idle && (
                _keyPressState == KeyPressState.JustPressed && (time.Total.TotalMilliseconds - _lastKeyRepetitionMs > KeyPressRepetitionDelay) ||
                _keyPressState == KeyPressState.Pressing && (time.Total.TotalMilliseconds - _lastKeyRepetitionMs > KeyPressRepetitionFrequency)
                );

            //KeyPress
            if (!_lastKey.IsMouseKey() && _game.Keyboard.IsKeyDown(_lastKey) && shouldRepeat)
            {
                _lastKeyRepetitionMs = time.Total.TotalMilliseconds;

                var keyArgs = new KeyEventArgs(_lastKey);
                _focusComponent?.InvokeKeyPress(keyArgs);

                foreach (var c in UIHelper.Ancestors(_focusComponent))
                {
                    if (!keyArgs.ShouldPropagate) break;

                    c.InvokeKeyPress(keyArgs);
                }

                if (_keyPressState == KeyPressState.JustPressed)
                {
                    _keyPressState = KeyPressState.Pressing;
                }
            }

            //Enter + Leave (mouse)
            InvokeEnterAndLeaveComponentsEvents();
        }

        private void InvokeEnterAndLeaveComponentsEvents()
        {
            var newComponentsWithMouse = UIHelper.BFSTraverseForPoint(_root, _game.Mouse.Position).ToList();
            foreach (var c in newComponentsWithMouse.Except(_componentsWithMouse))
            {
                c.InvokeEnter();
            }
            foreach (var c in _componentsWithMouse.Except(newComponentsWithMouse))
            {
                c.InvokeLeave();
            }
            _componentsWithMouse = newComponentsWithMouse;
        }
    }
}
