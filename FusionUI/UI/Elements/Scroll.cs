using System;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements
{
    public class Scroll : ScalableFrame
    {
        public bool IsFixedX = false;
        public bool IsFixedY = false;
        public bool IsDrag = false;
        public Vector2? PrevPosition = null;
        /// <summary>
        /// Deprecated
        /// </summary>
        public Action<float, float> actionForMove;
        /// <summary>
        /// Deprecated
        /// </summary>
        public Action<float, float> actionClickUp;

        public Action<float, float> actionForMoveRelative;
        public Action<float, float> actionClickUpRelative
            ;

        public int extendedSize = 0;
             
        public Scroll(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {            
            initAllEvent();
        }

        void initAllEvent()
        {
            // moving slider
            ActionDrag += Scroll_MouseMove;
            ActionDown += Scroll_MouseDown;
            ActionClick += Scroll_Click;
            ActionLost += Scroll_MouseUp;                        
        }

        void Scroll_MouseDown(ControlActionArgs args, ref bool flag)
        {
            IsDrag = true;
            flag |= true;
        }

        void Scroll_Click(ControlActionArgs args, ref bool flag)
        {
            PrevPosition = null;
            actionClickUp?.Invoke(this.GlobalRectangle.X, this.GlobalRectangle.Y);
            actionClickUpRelative?.Invoke(GetRelativeX(),
                GetRelativeY());
//            actionForMove?.Invoke(this.GlobalRectangle.X, this.GlobalRectangle.Y);
            flag |= true;
        }

        void Scroll_MouseUp(ControlActionArgs args, ref bool flag)
        {
            IsDrag = false;
            PrevPosition = null;            
        }        

        void Scroll_MouseMove(ControlActionArgs args, ref bool flag)
        {
            Log.Message($"{args.Position}");
            ChangeSliderPosition(args.Position);
            actionForMove?.Invoke(this.GlobalRectangle.X, this.GlobalRectangle.Y);
            actionForMoveRelative?.Invoke(GetRelativeX(),
                GetRelativeY());
            flag |= true;
        }

        public void SetFromRelative(float x, float y)
        {
            SetFromRelativeX(x);
            SetFromRelativeY(y);
        }

        public void SetFromRelativeX(float x)
        {
            var parent = this.parent.GetPaddedRectangle();
            X = (int)(this.Parent.PaddingLeft - extendedSize / 2 + x * (parent.Width - extendedSize));            
            UpdatePosition();
        }

        public void SetFromRelativeY(float y)
        {
            var parent = this.parent.GetPaddedRectangle();            
            Y = (int)(0 + y * parent.Height);
            UpdatePosition();
        }

        public float GetRelativeX()
        {
            return MathUtil.Clamp((float)(X + extendedSize/2 - this.parent.PaddingLeft) /
                   (this.parent.GetPaddedRectangle().Width - extendedSize), 0, 1);
        }

        public float GetRelativeY()
        {
            return MathUtil.Clamp((float)(this.GlobalRectangle.Y + this.parent.GetPaddedRectangle().Y) /
                   (this.parent.GetPaddedRectangle().Width - extendedSize), 0, 1);
        }

        public Vector2 GetRelative()
        {
            return new Vector2(GetRelativeX(), GetRelativeY());
        }

        public void UpdatePosition()
        {
            actionClickUp?.Invoke(this.GlobalRectangle.X, this.GlobalRectangle.Y);
            actionClickUpRelative?.Invoke(GetRelativeX(),
                GetRelativeY());
            actionForMove?.Invoke(this.GlobalRectangle.X, this.GlobalRectangle.Y);
            actionForMoveRelative?.Invoke(GetRelativeX(),
                GetRelativeY());

        }

        void ChangeSliderPosition(Vector2 p)
        {
            if (IsFixedX && IsFixedY)
                return;
            //var p = new Vector2
            //{
            //    X = this.Game.Mouse.Position.X,
            //    Y = this.Game.Mouse.Position.Y,
            //};
            if (PrevPosition == null)
            {
                PrevPosition = p;
            }
            var parent = this.Parent.GetPaddedRectangle(true);
            var scrollRectangle = this.GlobalRectangle;
            if (!IsFixedX)
            {                
                var newX = X + (int)p.X - (int)PrevPosition.Value.X - this.Parent.PaddingLeft;
//                Log.Message($"{newX};{this.X}");
                this.X = this.Parent.PaddingLeft + MathUtil.Clamp(newX, 0 - extendedSize, parent.Width - scrollRectangle.Width + extendedSize);
            }
            //&& parent.X < scrollRectangle.X + (int)p.X - (int)PrevPosition.Value.X && parent.Width + parent.X > scrollRectangle.Width + scrollRectangle.X + (int)p.X - (int)PrevPosition.Value.X)
                //this.X += (int)p.X - (int)PrevPosition.Value.X;
            if (!IsFixedY)
            {
                var newY = Y + (int)p.Y - (int)PrevPosition.Value.Y - this.Parent.PaddingTop;
                this.Y = this.Parent.PaddingTop + MathUtil.Clamp(newY, 0, parent.Height - scrollRectangle.Height);
            }
//            this.X = Math.Min(this.Width, this.X);
            //&& parent.Y < scrollRectangle.Y + (int)p.Y - (int)PrevPosition.Value.Y && parent.Height + parent.Y > scrollRectangle.Height + scrollRectangle.Y + (int)p.Y - (int)PrevPosition.Value.Y)
            //this.Y += (int)p.Y - (int)PrevPosition.Value.Y;            
            PrevPosition = p;
        }

        public override void UpdateScale(float scale)
        {
            base.UpdateScale(scale);
            actionForMove?.Invoke(this.GlobalRectangle.X, this.GlobalRectangle.Y);
        }
    }
}
