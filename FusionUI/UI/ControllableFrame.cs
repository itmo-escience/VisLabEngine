﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;

namespace FusionUI.UI
{
    public class ControllableFrame : Frame
    {
        public bool ClickToFront = false;

        public ControllableFrame(FrameProcessor ui) : base(ui)
        {
            ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (ClickToFront) this.ZOrder = 100000;
            };
        }

        public ControllableFrame(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (ClickToFront) this.ZOrder = 100000;
            };
        }

        protected virtual void initialize()
        {

        }

        private string TooltipText = "";
        public virtual string Tooltip
        {
            get
            {
                if (Text != null && Font.MeasureString(Text).Width + TextOffsetX > Width)
                {
                    return Text;
                }
                return TooltipText;
            }
            set { TooltipText = value; }
        }

        private ControlActionArgs currentEventArgs = new ControlActionArgs();
        private static float doubleClickTime = 0.3f;

        private Vector2? lastMousePos;

        public void AddMouseActions()
        {
            Game.Mouse.Move += MoueMoveAction = (sender, args) =>
            {
                currentEventArgs.IsTouch = false;
                currentEventArgs.Position = (Point)args.Position;
                currentEventArgs.MoveDelta = (Game.Mouse.Position - lastMousePos) ?? Vector2.Zero;
                lastMousePos = Game.Mouse.Position;
                if (!this.Active) return;
                if (oldPos != null && (currentEventArgs.Position - oldPos.Value).Length() > 2)
                    this.InnerActionDrag(currentEventArgs);
            };
            DateTime? lastClickTime = null;
            Keys lastClickKey = Keys.None;
            Game.Keyboard.KeyDown += KeyDownAction = (sender, args) =>
            {
                lastMousePos = Game.Mouse.Position;
                currentEventArgs.IsTouch = false;
                currentEventArgs.Key = args.Key;
                currentEventArgs.Position = Game.Mouse.Position;
                currentEventArgs.MoveDelta = Vector2.Zero;
                currentEventArgs.IsClick = args.Key == Keys.LeftButton;
                currentEventArgs.IsAltClick = args.Key == Keys.RightButton;
                currentEventArgs.ActionType = ControlActionArgs.ClickActionType.ActionDown;

                if (base.Ghost || !Active) return;
                var rootFrame = ApplicationInterface.Instance.rootFrame;
                var hoveredFrame = MainFrame.GetHoveredFrame(rootFrame, (Point)currentEventArgs.Position);
                if (MainFrame.IsChildOf(rootFrame, hoveredFrame, this))
                {
                    this.InnerActionDown(currentEventArgs);
                    oldPos = currentEventArgs.Position;
                }
                else
                {
                    InnerActionOut(currentEventArgs);
                    currentEventArgs.IsClick = false;
                    currentEventArgs.IsAltClick = false;
                    currentEventArgs.IsDoubleClick = false;
                }
            };
            Game.Keyboard.KeyUp += KeyUpAction = (sender, args) =>
            {
                lastMousePos = Game.Mouse.Position;
                currentEventArgs.IsTouch = false;
                currentEventArgs.Key = args.Key;
                currentEventArgs.Position = Game.Mouse.Position;
                currentEventArgs.MoveDelta = Game.Mouse.PositionDelta;
                currentEventArgs.IsClick = args.Key == Keys.LeftButton;
                currentEventArgs.IsAltClick = args.Key == Keys.RightButton;
                currentEventArgs.ActionType = ControlActionArgs.ClickActionType.ActionUp;
                if (base.Ghost || !Active) return;
                var rootFrame = ApplicationInterface.Instance.rootFrame;
                var hoveredFrame = MainFrame.GetHoveredFrame(rootFrame, (Point)currentEventArgs.Position);
                if (MainFrame.IsChildOf(rootFrame, hoveredFrame, this) && Selected)
                {
                    if (oldPos != null && (currentEventArgs.Position - oldPos.Value).Length() < 2)
                    {
                        currentEventArgs.ActionType |= ControlActionArgs.ClickActionType.ActionClick;
                        if (lastClickTime != null && (DateTime.Now - lastClickTime.Value).TotalSeconds <
                            doubleClickTime && (currentEventArgs.IsClick || currentEventArgs.IsAltClick)  && lastClickKey == args.Key)
                        {
                            currentEventArgs.IsDoubleClick = true;
                            lastClickTime = DateTime.MinValue;
                            lastClickKey = Keys.None;
                        }
                        else
                        {
                            lastClickTime = DateTime.Now;
                            lastClickKey = args.Key;
                        }
                        this.InnerActionClick(currentEventArgs);
                    }
                    this.InnerActionUp(currentEventArgs);
                }
                else
                {
                    this.InnerActionOut(currentEventArgs);
                }
                currentEventArgs.IsClick = false;
                currentEventArgs.IsAltClick = false;
                currentEventArgs.IsDoubleClick = false;
                oldPos = null;
            };

            Game.Touch.Tap += TouchTapAction = (args) =>
            {
                //currentEventArgs.Key = Game.Keyboard.IsKeyDown;
                currentEventArgs.IsTouch = true;
                currentEventArgs.Position = args.Position;
                currentEventArgs.IsClick = true;
                currentEventArgs.ActionType = ControlActionArgs.ClickActionType.ActionDown | ControlActionArgs.ClickActionType.ActionUp | ControlActionArgs.ClickActionType.ActionClick;
                if (base.Ghost || !Active) return;
                var rootFrame = ApplicationInterface.Instance.rootFrame;
                var hoveredFrame = MainFrame.GetHoveredFrame(rootFrame, (Point)currentEventArgs.Position);
                if (MainFrame.IsChildOf(rootFrame, hoveredFrame, this))
                {
                    this.InnerActionDown(currentEventArgs);
                    this.InnerActionClick(currentEventArgs);
                    this.InnerActionUp(currentEventArgs);
                    oldPos = currentEventArgs.Position;
                }
                else
                {
                    InnerActionOut(currentEventArgs);
                    currentEventArgs.IsClick = false;
                    currentEventArgs.IsAltClick = false;
                    currentEventArgs.IsDoubleClick = false;
                }
            };

            Game.Touch.SecondaryTap += TouchSecondaryTapAction = (args) =>
            {
                //currentEventArgs.Key = Game.Keyboard.IsKeyDown;
                currentEventArgs.IsTouch = true;
                currentEventArgs.Position = args.Position;
                currentEventArgs.IsClick = true;
                currentEventArgs.ActionType = ControlActionArgs.ClickActionType.ActionDown | ControlActionArgs.ClickActionType.ActionUp| ControlActionArgs.ClickActionType.ActionClick;
                if (base.Ghost || !Active) return;
                var rootFrame = ApplicationInterface.Instance.rootFrame;
                var hoveredFrame = MainFrame.GetHoveredFrame(rootFrame, (Point)currentEventArgs.Position);
                if (MainFrame.IsChildOf(rootFrame, hoveredFrame, this))
                {
                    this.InnerActionDown(currentEventArgs);
                    this.InnerActionClick(currentEventArgs);
                    this.InnerActionUp(currentEventArgs);
                    oldPos = currentEventArgs.Position;
                }
                else
                {
                    InnerActionOut(currentEventArgs);
                    currentEventArgs.IsClick = false;
                    currentEventArgs.IsAltClick = false;
                    currentEventArgs.IsDoubleClick = false;
                }
            };

            Game.Touch.DoubleTap += TouchDoubleTapAction = (args) =>
            {
                //currentEventArgs.Key = Game.Keyboard.IsKeyDown;
                currentEventArgs.IsTouch = true;
                currentEventArgs.Position = args.Position;
                currentEventArgs.IsClick = true;
                currentEventArgs.IsDoubleClick = true;
                currentEventArgs.ActionType = ControlActionArgs.ClickActionType.ActionDown | ControlActionArgs.ClickActionType.ActionUp | ControlActionArgs.ClickActionType.ActionClick;
                if (base.Ghost || !Active) return;
                var rootFrame = ApplicationInterface.Instance.rootFrame;
                var hoveredFrame = MainFrame.GetHoveredFrame(rootFrame, (Point)currentEventArgs.Position);
                if (MainFrame.IsChildOf(rootFrame, hoveredFrame, this))
                {
                    this.InnerActionDown(currentEventArgs);
                    this.InnerActionClick(currentEventArgs);
                    this.InnerActionUp(currentEventArgs);
                    oldPos = currentEventArgs.Position;
                }
                else
                {
                    InnerActionOut(currentEventArgs);
                    currentEventArgs.IsClick = false;
                    currentEventArgs.IsAltClick = false;
                    currentEventArgs.IsDoubleClick = false;
                }
            };

            Game.Touch.Hold += TouchHoldAction = (args) =>
            {
                currentEventArgs.IsTouch = true;
                currentEventArgs.Position = args.Position;
                currentEventArgs.IsClick = true;
                currentEventArgs.IsAltClick = true;
                currentEventArgs.ActionType = ControlActionArgs.ClickActionType.ActionDown | ControlActionArgs.ClickActionType.ActionUp | ControlActionArgs.ClickActionType.ActionClick;
                if (base.Ghost || !Active) return;
                var rootFrame = ApplicationInterface.Instance.rootFrame;
                var hoveredFrame = MainFrame.GetHoveredFrame(rootFrame, (Point)currentEventArgs.Position);
                if (MainFrame.IsChildOf(rootFrame, hoveredFrame, this))
                {
                    this.InnerActionDown(currentEventArgs);
                    this.InnerActionClick(currentEventArgs);
                    this.InnerActionUp(currentEventArgs);
                    oldPos = currentEventArgs.Position;
                }
                else
                {
                    InnerActionOut(currentEventArgs);
                    currentEventArgs.IsClick = false;
                    currentEventArgs.IsAltClick = false;
                    currentEventArgs.IsDoubleClick = false;
                }
            };

            Game.Touch.Manipulate += args =>
            {
                if (!args.IsEventBegin) currentEventArgs.MoveDelta = args.Position - (Vector2)currentEventArgs.Position;
                else currentEventArgs.MoveDelta = Vector2.Zero;
                currentEventArgs.Position = (Point)args.Position;
                currentEventArgs.IsTouch = true;
                currentEventArgs.ScaleDelta = args.ScaleDelta;
                currentEventArgs.RotationDelta = args.RotationDelta;
                currentEventArgs.FingerCount = args.FingersCount;
                if (args.IsEventBegin)
                {
                    if (base.Ghost || !Active) return;
                    var rootFrame = ApplicationInterface.Instance.rootFrame;
                    var hoveredFrame = MainFrame.GetHoveredFrame(rootFrame, (Point)currentEventArgs.Position);
                    currentEventArgs.ActionType = ControlActionArgs.ClickActionType.ActionDown;
                    if (MainFrame.IsChildOf(rootFrame, hoveredFrame, this))
                    {
                        this.InnerActionDown(currentEventArgs);
                        oldPos = currentEventArgs.Position;
                    }
                    else
                    {
                        InnerActionOut(currentEventArgs);
                        currentEventArgs.IsClick = false;
                        currentEventArgs.IsAltClick = false;
                        currentEventArgs.IsDoubleClick = false;
                    }
                }

                if (args.IsEventEnd)
                {
                    if (base.Ghost || !Active) return;
                    var rootFrame = ApplicationInterface.Instance.rootFrame;
                    var hoveredFrame = MainFrame.GetHoveredFrame(rootFrame, (Point)currentEventArgs.Position);
                    currentEventArgs.ActionType = ControlActionArgs.ClickActionType.ActionUp;
                    if (MainFrame.IsChildOf(rootFrame, hoveredFrame, this) && Selected)
                    {
                        if (oldPos != null && (currentEventArgs.Position - oldPos.Value).Length() < 2) this.InnerActionClick(currentEventArgs);
                        this.InnerActionUp(currentEventArgs);
                    }
                    else
                    {
                        this.InnerActionOut(currentEventArgs);
                    }
                    oldPos = null;
                    currentEventArgs.IsClick = false;
                    currentEventArgs.IsAltClick = false;
                    currentEventArgs.IsDoubleClick = false;
                    currentEventArgs.IsTouch= false;
                }
                if (this.Active && oldPos != null && (currentEventArgs.Position - oldPos.Value).Length() > 2)
                    this.InnerActionDrag(currentEventArgs);
            };

        }

        public bool SuppressActions = false;



        public new bool Ghost
        {
            get { return !active; }
            set { active = !value; }
        }
        private bool active = true;

        public bool ActiveInHierarchy
        {
            get
            {
                Queue<ControllableFrame> q = new Queue<ControllableFrame>();
                q.Enqueue(this);
                while (q.Count != 0)
                {
                    var f = q.Dequeue();
                    if (!f.Active) return false;
                    if (f.Parent != null && f.Parent is ControllableFrame) q.Enqueue((ControllableFrame)f.Parent);
                }
                return true;
            }
        }

        public bool Active
        {
            get { return active && Visible; }
            set
            {
                active = value;
                //this.OverallColor = new Color(150, 150, 150, 1);
                Queue<ControllableFrame> q = new Queue<ControllableFrame>();
                q.Enqueue(this);
                while (q.Any())
                {
                    var frame = q.Dequeue();
                    frame.UpdateColor(value);
                    foreach (var c in frame.Children)
                    {
                        if (c is ControllableFrame) q.Enqueue((ControllableFrame)c);
                    }
                }
            }
        }


        public Color? ActiveForeColor = UIConfig.ActiveTextColor,
            InactiveForeColor = UIConfig.InactiveTextColor,
            ActiveBackColor = null,
            InactiveBackColor = null,
            ActiveBorderColor = UIConfig.BorderColor,
            InactiveBorderColor = UIConfig.BorderColor,
            ActiveImageColor = Color.White,
            InactiveImageColor = UIConfig.InactiveTextColor;

        public Texture ActiveImage, InactiveImage;

        override public Color BackColor
        {
            get => base.BackColor;
            set
            {
                base.BackColor = value;
                if (Active) ActiveBackColor = value;
                else InactiveBackColor = value;
            }
        }

        override public Color BorderColor {
            get => base.BorderColor;
            set
            {
                base.BorderColor = value;
                if (Active) ActiveBorderColor = value;
                else InactiveBorderColor = value;
            }
        }

        override public Color ForeColor {
            get => base.ForeColor;
            set
            {
                base.ForeColor = value;
                if (Active) ActiveForeColor = value;
                else InactiveForeColor = value;
            }
        }

        public virtual void UpdateColor(bool active)
        {
            if (this.Active && active) {
                if (ActiveForeColor != null) base.ForeColor = ActiveForeColor.Value;
                if (ActiveBackColor != null) base.BackColor = ActiveBackColor.Value;
                if (ActiveBorderColor != null) base.BorderColor = ActiveBorderColor.Value;
                if (ActiveImageColor != null) base.ImageColor = ActiveImageColor.Value;
                if (ActiveImage != null) base.Image = ActiveImage;
            } else {
                if (InactiveForeColor != null) base.ForeColor = InactiveForeColor.Value;
                if (InactiveBackColor != null) base.BackColor = InactiveBackColor.Value;
                if (InactiveBorderColor != null) base.BorderColor = InactiveBorderColor.Value;
                if (InactiveImageColor != null) base.ImageColor = InactiveImageColor.Value;
                if (InactiveImage != null) base.Image = InactiveImage;
            }
        }

        private bool selected = false;
        public virtual bool Selected {get { return selected && Visible; } set { selected = value; } }

        public class ControlActionArgs : EventArgs
        {
            public bool IsTouch;

            public Keys Key = Keys.None;
            public int X  => Position.X;

            public int Y => Position.Y;
            public Point Position = Point.Zero;

            public int DX {get { return (int)MoveDelta.X; } }
            public int DY { get { return (int)MoveDelta.Y; } }
            public Vector2 MoveDelta = Vector2.Zero;

            public int Wheel = 0;
            public float ScaleDelta = 0;
            public float RotationDelta = 0;

            public bool IsClick;
            public bool IsDoubleClick;
            public bool IsAltClick;

            public int FingerCount = 0;

            [Flags]
            public enum ClickActionType
            {
                ActionNone = 0,
                ActionClick = 1,
                ActionUp = 2,
                ActionDown = 4,
            }

            public ClickActionType ActionType;
        }

        public delegate void MouseAction(ControlActionArgs args, ref bool flag);

        public MouseAction ActionClick, ActionDrag, ActionUp, ActionDown, ActionOut, ActionLost;
        public Action<GameTime> ActionUpdate;
        public Action<GameTime, SpriteLayer, int> ActionDraw;
        Vector2? oldPos = null;

        bool InnerActionDown(ControlActionArgs args)
        {
            if (!Active || !Visible) return false;
            bool flag = false;
            Selected = true;
            foreach (var frame in Children.Reverse())
            {
                if (flag) return flag;
                if (frame is ControllableFrame)
                {
                    var sFrame = (ControllableFrame)frame;
                    if (!sFrame.Active) continue;
                    if (frame.GlobalRectangle.Contains(args.Position) && sFrame.Active && sFrame.Visible)
                    {

                        flag |= sFrame.InnerActionDown(args) || sFrame.SuppressActions;
                    }
                    else
                    {
                        flag |= sFrame.InnerActionOut(args);
                    }
                }
            }

            if (!flag)
            {
                ActionDown?.Invoke(args, ref flag);
            }
            return flag;
        }
        bool InnerActionUp(ControlActionArgs args)
        {
            if (!Active || !Visible || !Selected) return false;
            bool flag = false;
            bool wasSelected = Selected;
            Selected = false;
            foreach (var frame in Children.Reverse())
            {
                if (flag) return flag;
                if (frame is ControllableFrame)
                {
                    var sFrame = (ControllableFrame)frame;
                    if (!sFrame.Active) continue;
                    if (frame.GlobalRectangle.Contains(args.Position))
                    {
                        if (sFrame.Selected) flag |= sFrame.InnerActionUp(args) || sFrame.SuppressActions;
                    }
                    else
                    {
                        flag |= sFrame.InnerActionOut(args);
                    }
                }
            }
            if (!flag && wasSelected)
            {
                ActionUp?.Invoke(args, ref flag);
                ActionLost?.Invoke(args, ref flag);
            }
            return flag;
        }

        bool InnerActionClick(ControlActionArgs args)
        {
            if (!Active || !Visible || !Selected) return false;
            bool flag = false;
            foreach (var frame in Children.Reverse())
            {
                if (flag) return flag;
                if (frame is ControllableFrame)
                {
                    var sFrame = (ControllableFrame)frame;
                    if (frame.GlobalRectangle.Contains(args.Position) && sFrame.Selected)
                    {

                        flag |= sFrame.InnerActionClick(args) || sFrame.SuppressActions;
                    }
                    else
                    {
                        flag |= sFrame.InnerActionOut(args);
                    }
                }
            }
            if (!flag)
            {
                ActionClick?.Invoke(args, ref flag);
            }
            return flag;
        }
        bool InnerActionDrag(ControlActionArgs args)
        {
            bool flag = false;
            if (!Active || !Visible || !Selected) return false;
            foreach (var frame in Children.Reverse())
            {
                if (flag) return flag;
                if (frame is ControllableFrame)
                {
                    var sFrame = (ControllableFrame)frame;
                    if (sFrame.Selected)
                        flag |= sFrame.InnerActionDrag(args) || sFrame.SuppressActions;
                }
            }
            if (!flag) ActionDrag?.Invoke(args, ref flag);
            return flag;
        }

        bool InnerActionOut(ControlActionArgs args)
        {
            bool flag = false;
            if (!Active || !Visible) return false;
            oldPos = null;
            foreach (var frame in Children.Reverse())
            {
                if (flag)
                {
                    Selected = false;
                    return flag;
                }
                if (frame is ControllableFrame)
                {
                    var sFrame = (ControllableFrame)frame;
                    if (sFrame.Active && sFrame.Visible && sFrame.Selected)
                    {
                        flag |= sFrame.InnerActionOut(args);
                    }
                }
            }

            if (!flag)
            {
                ActionOut?.Invoke(args, ref flag);
                if (Selected) ActionLost?.Invoke(args, ref flag);
            }


            Selected = false;
            return flag;
        }

        private KeyUpEventHandler KeyUpAction;
        private KeyDownEventHandler KeyDownAction;
        private MouseMoveHandlerDelegate MoueMoveAction;
        private TouchTapEventHandler TouchTapAction, TouchManipulateAction, TouchSecondaryTapAction, TouchHoldAction, TouchDoubleTapAction;

        public void Clean()
        {
            Game.Keyboard.KeyUp -= KeyUpAction;
            Game.Keyboard.KeyDown -= KeyDownAction;
            Game.Mouse.Move -= MoueMoveAction;
            Game.Touch.Tap -= TouchTapAction;
            Game.Touch.DoubleTap -= TouchDoubleTapAction;
            Game.Touch.SecondaryTap -= TouchSecondaryTapAction;
            Game.Touch.Hold -= TouchHoldAction;
            Game.Touch.Manipulate -= TouchManipulateAction;
            foreach (var child in Children)
            {
                if (child is ControllableFrame) ((ControllableFrame)child).Clean();
            }
        }

        protected bool firstUpdate = true;
        protected override void Update(GameTime gameTime)
        {
            if (firstUpdate)
            {
                firstUpdate = false;
                initialize();

            }

            base.Update(gameTime);
            ActionUpdate?.Invoke(gameTime);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex)
        {
            base.DrawFrame(gameTime, spriteLayer, clipRectIndex);
            if (Visible) ActionDraw?.Invoke(gameTime, spriteLayer, clipRectIndex);
        }

        public Action OnAnchorsUpdate;

        public override void UpdateAnchors(int oldW, int oldH, int newW, int newH)
        {
            base.UpdateAnchors(oldW, oldH, newW, newH);
            OnAnchorsUpdate?.Invoke();
        }
    }
}
