using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements
{
    public abstract class Progressbar : ScalableFrame
    {
        public Progressbar(FrameProcessor ui, float x, float y, float w, float h) : base(ui, x, y, w, h, "", Color.Zero)
        {
        }
    }

    public class MeasuredProgressbar : Progressbar
    {
        private float value;

        public float Value
        {
            get { return value; }
            set
            {
                this.value = value;
                progress.UnitWidth = value * UnitWidth;
                ValueUpdate?.Invoke(value);
            }
        }


		public Color ProgressColor {
			get => progress.BackColor;
			set => progress.BackColor = value;
		}


        private ScalableFrame back, progress;

        public MeasuredProgressbar(FrameProcessor ui, float x, float y, float w, float h) : base(ui, x, y, w, h)
        {
            back		= new ScalableFrame(ui, 0, 0, w, h, "", UIConfig.ButtonColor);
            progress	= new ScalableFrame(ui, 0, 0, 0, h, "", UIConfig.ActiveColor);
            Add(back);
            Add(progress);
        }

        public Action<float> ValueUpdate;
    }

    public class UnmeasuredProgressbar : Progressbar
    {
        

        private ScalableFrame back, progress;
        private float speed;
        public UnmeasuredProgressbar(FrameProcessor ui, float x, float y, float w, float h, float part, float speed) : base(ui, x, y, w, h)
        {
            this.speed = speed;
            back = new ScalableFrame(ui, 0, 0, w, h, "", UIConfig.ButtonColor);
            progress = new ScalableFrame(ui, 0, 0, w * part, h, "", UIConfig.ActiveColor);
            Add(back);
            Add(progress);
        }

        protected override void Update(GameTime gameTime)
        {
            
            base.Update(gameTime);
            if (Active)
            {
                float dx = (float) gameTime.Elapsed.TotalSeconds * speed;
                float w = UnitWidth + progress.UnitWidth;
                progress.UnitX = (progress.UnitX + dx*w + progress.UnitWidth) %w - progress.UnitWidth;
            }
        }
    }
}
