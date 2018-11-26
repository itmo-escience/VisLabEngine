using System;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements
{
    public class Tooltip : ScalableFrame
    {
		protected Tooltip()
		{
		}
		public Tooltip(FrameProcessor ui) : base(ui)
        {
            this.Ghost = true;
            ((Frame) this).Ghost = true;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var root = ApplicationInterface.Instance.rootFrame;
            var hover = ((ControllableFrame) MainFrame.GetHoveredFrame(root, Game.Instance.Mouse.Position));
            if (hover == null) return;
            var s = hover.Tooltip;
            if (!String.IsNullOrWhiteSpace(s))
            {
                this.Visible = true;
                this.X = Game.Instance.Mouse.Position.X + 5;
                this.Y = Game.Instance.Mouse.Position.Y + 5;

                this.Font = hover.Font;
                var srect = Font.MeasureString(s);
                this.Width = srect.Width;
                this.Height = srect.Height;

                this.Text = s;
                this.ZOrder = 1000;
            }
            else
            {
                this.Visible = false;
            }

        }
    }
}
