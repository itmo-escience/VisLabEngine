using System;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI.Elements
{
    class SetterValueFrame : ScalableFrame{

		protected SetterValueFrame()
		{
		}
		private FrameProcessor ui;
        public string nameLabel;

        public int sizeButton = 30;
        public int widthValueFrame = 100;

        public float step = 1;
		[XmlIgnore]
		public Action<float> OnChange;

        private float _currentValue;
        public float CurrentValue {
            get { return _currentValue; }
            set {
                _currentValue = value;
                ChangeTextValue();
                OnChange?.Invoke(value);
            }
        }

        private ScalableFrame valueFrame;

        private void ChangeTextValue()
        {
            valueFrame.Text = CurrentValue.ToString();
        }

        public SetterValueFrame(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            this.ui = ui;
            init();
        }

        private void init()
        {
            var labelText = new ScalableFrame(ui, 0, 0, 50, this.Height, nameLabel, Color.Zero)
            {
                Ghost = true
            };
            this.Add(labelText);

            var buttonDown = new ScalableFrame(ui, Width - 2*sizeButton - widthValueFrame, 0, sizeButton, sizeButton, "", Color.Zero)
            {
                Border = 1,
                Image = ui.Game.Content.Load<DiscTexture>(@"UI\checkbox_active")
            };
            buttonDown.ActionClick += (ControlActionArgs args, ref bool flag) => {
                if (!args.IsClick) return;
                CurrentValue -= step;
            };
            this.Add(buttonDown);

            var buttonUp = new ScalableFrame(ui, Width-sizeButton, 0, sizeButton, sizeButton, "", Color.Zero)
            {
                Border = 1,
                Image = ui.Game.Content.Load<DiscTexture>(@"UI\checkbox_active")
            };
            buttonUp.ActionClick += (ControlActionArgs args, ref bool flag) => {
                if (!args.IsClick) return;
                CurrentValue += step;
            };
            this.Add(buttonUp);

            valueFrame = new ScalableFrame(ui, Width - sizeButton - widthValueFrame, 0, widthValueFrame, this.Height, "", Color.Zero)
            {
                TextAlignment = Alignment.MiddleCenter,
                Border = 1,
                Ghost = true
            };
            this.Add(valueFrame);
        }
    }
}
