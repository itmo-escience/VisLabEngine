using System;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI.Elements
{
    public class RadioButton : ScalableFrame
    {
		protected RadioButton()
		{
		}
		[XmlIgnore]
		public Texture Checked;
		[XmlIgnore]
		public Texture None;
        private bool _isChecked;
		[XmlIgnore]
		public Action<bool> Changed;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                ChangeValue();
                //if(_isChecked)
                    Changed?.Invoke(value);
            }
        }

        public RadioButton(FrameProcessor ui) : base(ui)
        {
            Text = "";
            init(ui);
            this.ActionClick += (ControlActionArgs args, ref bool flag) => 
            {
                if (!args.IsClick) return;
                Radio_Click();
                flag |= true;
            };
        }

        public RadioButton(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui);
            this.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsClick) return;
                Radio_Click();
                flag |= true;
            };
        }

        void init(FrameProcessor ui)
        {
            Checked = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_radio-on");
            None = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_radio-off");
            IsChecked = false;
            this.Image = None;
            this.ImageMode = FrameImageMode.Stretched;
        }

        void Radio_Click()
        {
            if (IsChecked != true)
                IsChecked = !IsChecked;
        }

        public void ChangeValue()
        {
            Image = _isChecked ? Checked : None;
        }
    }
}
