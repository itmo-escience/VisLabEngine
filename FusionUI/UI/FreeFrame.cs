using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;

namespace FusionUI.UI
{
    public class FreeFrame : ScalableFrame {

        public bool DisableFree = false;
        public bool CanChangeSize = false;
        protected bool IsDrag = false, IsMoving = false;
        protected bool IsRightDrag = false;
        protected Vector2? PrevPosition = null;

        public bool ClampPos = true;

        public float TopPos => ApplicationInterface.Instance.rootFrame.UnitHeight - UIConfig.UnitTopmostWindowPosition;

        private ScalableFrame selectedFrame;

        public Action<FreeFrame> ActionMove;

        public FreeFrame(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            InitAllEvent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (ClampPos && Parent is MainFrame) UnitY = Math.Max(UnitY, TopPos);
        }

        void InitAllEvent()
        {
            // moving slider
            Vector2 startPos;


            ActionDrag += (ControlActionArgs args, ref bool flag) =>
            {
                if (!(args.IsClick || args.IsTouch)) return;
                if (DisableFree)
                    return;
                IsDrag = true;
                if (PrevPosition.HasValue)
                {
                    X += args.DX;//(int) (args.X - PrevPosition.Value.X);
                    Y += args.DY;//(int) (args.Y - PrevPosition.Value.Y);
                    if (ClampPos) UnitY = Math.Max(UnitY, TopPos);
                    ActionMove?.Invoke(this);
                }
                PrevPosition = args.Position;
            };

            ActionDown += (ControlActionArgs args, ref bool flag) =>
            {
                if (args.IsClick || args.IsTouch)
                {
                    var HoveredFrame = MainFrame.GetHoveredFrame(ApplicationInterface.Instance.rootFrame, args.Position);
                    //if (HoveredFrame is Scroll || HoveredFrame is Slider) return;
                    if (!this.Ghost && this.Visible &&
                        MainFrame.IsChildOf(ApplicationInterface.Instance.rootFrame, HoveredFrame, this))
                    {
                        IsMoving = true;
                        selectedFrame = this;
                        startPos = args.Position;
                        //IsDrag = true;
                    }
                }
            };

            ActionLost += (ControlActionArgs args, ref bool flag) =>
            {
                if (args.IsClick)
                {
                    IsMoving = false;
                    PrevPosition = null;
                    //        IsDrag = false;
                    selectedFrame = null;
                }
            };
        }
    }
}
