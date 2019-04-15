using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Input;

namespace Fusion.Engine.Frames2.Events
{
    public class UIEventProcessor
    {
        private readonly Game _game;
        public IUIContainer Root { get; internal set; }
        private readonly UIManager _manager;

        private UIComponent _focusComponent;

        private Keys _lastMouseKey;

        private UIComponent _lastMouseDownComponent;
        private UIComponent _lastClickComponent;

        private readonly Stopwatch _clickStopwatch;
        private const long ClickDelay = 500;

        private List<UIComponent> _componentsWithMouse;

        internal UIEventProcessor(UIManager manager, IUIContainer root)
        {
            Root = root;
            _game = Game.Instance;
            _manager = manager;
            _focusComponent = root;
            _clickStopwatch = new Stopwatch();
            _componentsWithMouse = new List<UIComponent>();

            SubscribeToInputEvents();
        }

        private void SubscribeToInputEvents()
        {
            #region keyboard
            _game.Keyboard.FormKeyDown += (sender, args) =>
            {
                var keyArgs = (KeyEventArgs) args;
                var s = _focusComponent;
                _focusComponent?.Events.InvokeKeyDown(s, keyArgs);

                foreach (var c in UIHelper.Ancestors(_focusComponent))
                {
                    if (!keyArgs.ShouldPropagate) break;

                    c.Events.InvokeKeyDown(s, keyArgs);
                }
            };

            _game.Keyboard.FormKeyUp += (sender, args) =>
            {
                var keyArgs = (KeyEventArgs) args;
                var s = _focusComponent;
                _focusComponent?.Events.InvokeKeyUp(s, keyArgs);

                foreach (var c in UIHelper.Ancestors(_focusComponent))
                {
                    if (!keyArgs.ShouldPropagate) break;

                    c.Events.InvokeKeyUp(s, keyArgs);
                }
            };

            _game.Keyboard.FormKeyPress += (sender, args) =>
            {
                var keyArgs = new KeyPressEventArgs(args.KeyChar);
                var s = _focusComponent;
                _focusComponent?.Events.InvokeKeyPress(s, keyArgs);

                foreach (var c in UIHelper.Ancestors(_focusComponent))
                {
                    if (!keyArgs.ShouldPropagate) break;

                    c.Events.InvokeKeyPress(s, keyArgs);
                }
            };
            #endregion

            #region mouse
            _game.Mouse.Move += (sender, args) =>
            {
                var mousePosition = _game.Mouse.Position;
                var mouseArgs = (MoveEventArgs) args;

                UIComponent s = null;
                foreach (var c in UIHelper.BFSTraverseForPoint(_manager, Root, mousePosition).Reverse())
                {
                    if (s == null)
                        s = c;
                    if (!mouseArgs.ShouldPropagate)
                        break;

                    c.Events.InvokeMouseMove(s, mouseArgs);
                }
            };

            _game.Mouse.Move += (sender, args) =>
            {
                if (!_game.Keyboard.IsKeyDown(_lastMouseKey)) return;

                var mousePosition = _game.Mouse.Position;
                var dragArgs = new DragEventArgs(_lastMouseKey, args);

                UIComponent s = null;
                foreach (var c in UIHelper.BFSTraverseForPoint(_manager, Root, mousePosition).Reverse())
                {
                    if (s == null) s = c;

                    if (!dragArgs.ShouldPropagate)
                        return;

                    c.Events.InvokeMouseDrag(s, dragArgs);
                }
            };

            _game.Keyboard.KeyDown += (sender, args) =>
            {
                if (!args.Key.IsMouseKey()) return;

                var mousePosition = _game.Mouse.Position;
                var clickArgs = new ClickEventArgs(args.Key, mousePosition);

                UIComponent s = null;
                foreach (var c in UIHelper.BFSTraverseForPoint(_manager, Root, mousePosition).Reverse())
                {
                    if (s == null) s = c;
                    if (!clickArgs.ShouldPropagate)
                        break;

                    c.Events.InvokeMouseDown(s, clickArgs);
                }

                foreach (var c in UIHelper.DFSTraverse(Root))
                {
                    if (!_manager.IsInsideSlotInternal(c.Placement, mousePosition))
                        c.Events.InvokeMouseDownOutside(s, clickArgs);
                }

                _lastMouseKey = args.Key;
                _lastMouseDownComponent = UIHelper.GetLowestComponentInHierarchy(_manager, Root, mousePosition);
                _focusComponent = _lastMouseDownComponent;
            };

            //MouseUp
            _game.Keyboard.KeyUp += (sender, args) =>
            {
                if (!args.Key.IsMouseKey()) return;

                var mousePosition = _game.Mouse.Position;
                var clickArgs = new ClickEventArgs(args.Key, mousePosition);
                UIComponent s = null;
                foreach (var c in UIHelper.BFSTraverseForPoint(_manager, Root, mousePosition).Reverse())
                {
                    if (s == null) s = c;
                    if (!clickArgs.ShouldPropagate)
                        break;
                    c.Events.InvokeMouseUp(s, clickArgs);
                }

                foreach (var c in UIHelper.DFSTraverse(Root))
                {
                    if (!_manager.IsInsideSlotInternal(c.Placement, mousePosition))
                        c.Events.InvokeMouseUpOutside(s, clickArgs);
                }
            };

            //Click + DoubleClick (mouse)
            _game.Keyboard.KeyUp += (sender, args) =>
            {
                if (args.Key != _lastMouseKey) return;

                var mousePosition = _game.Mouse.Position;
                var currentComponent = UIHelper.GetLowestComponentInHierarchy(_manager, Root, mousePosition);
                if (currentComponent != _lastMouseDownComponent) return;

                var clickArgs = new ClickEventArgs(args.Key, mousePosition);
                foreach (var c in UIHelper.BFSTraverseForPoint(_manager, Root, mousePosition).Reverse())
                {
                    if (!clickArgs.ShouldPropagate)
                        break;
                    c.Events.InvokeClick(currentComponent, clickArgs);
                }

                if (currentComponent == _lastClickComponent)
                {
                    _clickStopwatch.Stop();
                    if (_clickStopwatch.ElapsedMilliseconds < ClickDelay)
                    {
                        clickArgs = new ClickEventArgs(args.Key, mousePosition);
                        foreach (var c in UIHelper.BFSTraverseForPoint(_manager, Root, mousePosition).Reverse())
                        {
                            if (!clickArgs.ShouldPropagate)
                                break;
                            c.Events.InvokeDoubleClick(currentComponent, clickArgs);
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

            //Scroll
            _game.Mouse.Scroll += (sender, args) =>
            {
                _focusComponent?.Events.InvokeScroll(_focusComponent, new ScrollEventArgs(_game.Mouse.Position, args));
            };
            #endregion

            #region touch

            /*
            //MouseMove (touch)
            _game.Touch.Manipulate += (args) => {
                Vector2 touchPosition = args.Position;
                foreach (var c in UIManager.BFSTraverseForPoint(_root, touchPosition))
                {
                    c.InvokeMouseMove( );
                }
            };

            //MouseDrag (touch)
            _game.Touch.Hold += (args) => {
                foreach (var c in UIHelper.BFSTraverseForPoint(_root, args.Position))
                {
                    c.InvokeMouseDrag(new DragEventArgs(Keys.LeftButton, ));
                }
            };

            //MouseDown (touch)
            _game.Touch.Hold += (args) =>
            {
                Log.Warning("Touch is not implemented");

                _focusComponent?.InvokeMouseDown(new ClickEventArgs(Keys.RightButton,
                    args.Position)); //TODO hold == press?
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
            */
            #endregion

            //TODO
            //Focus
            //Blur
        }

        public void Update(GameTime time)
        {
            //Enter + Leave (mouse)
            InvokeEnterAndLeaveComponentsEvents();
        }

        private void InvokeEnterAndLeaveComponentsEvents()
        {
            var newComponentsWithMouse = UIHelper.BFSTraverseForPoint(_manager, Root, _game.Mouse.Position).ToList();
            foreach (var c in newComponentsWithMouse.Except(_componentsWithMouse))
            {
                c.Events.InvokeEnter(c);
            }
            foreach (var c in _componentsWithMouse.Except(newComponentsWithMouse))
            {
                c.Events.InvokeLeave(c);
            }
            _componentsWithMouse = newComponentsWithMouse;
        }
    }
}
