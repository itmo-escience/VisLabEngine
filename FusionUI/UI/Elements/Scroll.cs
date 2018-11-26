using System;
using System.Xml.Serialization;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements
{
    public class Scroll : ScalableFrame
    {
		protected Scroll()
		{
		}
		public bool IsFixedX = false;
        public bool IsFixedY = false;
        public bool IsDrag = false;
        public Vector2? PrevPosition = null;
		/// <summary>
		/// Deprecated
		/// </summary>
		[XmlIgnore]
		public Action<float, float> actionForMove;
		/// <summary>
		/// Deprecated
		/// </summary>
		[XmlIgnore]
		public Action<float, float> actionClickUp;
		[XmlIgnore]
		public Action<float, float> actionForMoveRelative;
		[XmlIgnore]
		public Action<float, float> actionClickUpRelative
            ;

        public int extendedSize = 0;

        [Obsolete("Please use constructor without FrameProcessor")]
        public Scroll(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : this(x, y, w, h, text, backColor) { }

        public Scroll(float x, float y, float w, float h, string text, Color backColor) : base(x, y, w, h, text, backColor)
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

        public float XVal, YVal;

        public void SetFromRelativeX(float x)
        {
            XVal = (this.Parent.PaddingLeft - extendedSize / 2 +
                    x * (this.parent.GetPaddedRectangle().Width - extendedSize - Width));
            X = (int)XVal;
            UpdatePosition();
        }

        public void SetFromRelativeY(float y)
        {
            var parent = this.parent.GetPaddedRectangle();
            YVal = (0 + y * parent.Height);
            Y = (int)YVal;
            UpdatePosition();
        }

        public float GetRelativeX()
        {
            return MathUtil.Clamp((float)(XVal + extendedSize/2 - this.parent.PaddingLeft) /
                   (this.parent.GetPaddedRectangle().Width - extendedSize - Width), 0, 1);
        }

        public float GetRelativeY()
        {
            return MathUtil.Clamp((float)(YVal + this.parent.PaddingTop) /
                   (this.parent.GetPaddedRectangle().Height - extendedSize), 0, 1);
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
                var newX = XVal + (int)p.X - (int)PrevPosition.Value.X - this.Parent.PaddingLeft;
//                Log.Message($"{newX};{this.X}");
                XVal = this.Parent.PaddingLeft + MathUtil.Clamp(newX, 0 - extendedSize, parent.Width - scrollRectangle.Width + extendedSize);
                this.X = (int) XVal;
            }
            //&& parent.X < scrollRectangle.X + (int)p.X - (int)PrevPosition.Value.X && parent.Width + parent.X > scrollRectangle.Width + scrollRectangle.X + (int)p.X - (int)PrevPosition.Value.X)
                //this.X += (int)p.X - (int)PrevPosition.Value.X;
            if (!IsFixedY)
            {
                var newY = YVal + (int)p.Y - (int)PrevPosition.Value.Y - this.Parent.PaddingTop;

                YVal = this.Parent.PaddingTop + MathUtil.Clamp(newY, 0, parent.Height - scrollRectangle.Height);
                this.Y = (int)YVal;
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
