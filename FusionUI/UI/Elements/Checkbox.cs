using System;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI
{
    public class Checkbox : ScalableFrame
    {
		[XmlIgnore]
		public Texture Checked;
		[XmlIgnore]
		public Texture None;

        private bool _isChecked;

		[XmlIgnore]
		public Action<bool> Changed;

        public bool IsChecked {
            get { return _isChecked; }
            set {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    Changed?.Invoke(value);
                }
                ChangeValue();
            }
        }

        public Checkbox()
        {
            Text = "";
            init();
            this.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsClick) return;
                Checkbox_Click();
                flag |= true;
            };
        }

        public Checkbox(float x, float y, float w, float h, string text, Color backColor) : base(x, y, w, h, text, backColor)
        {
            init();
            this.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsClick) return;
                Checkbox_Click();
                flag |= true;
            };
        }

        [Obsolete("Please use constructor without FrameProcessor")]
        public Checkbox(FrameProcessor ui) : this() { }

        [Obsolete("Please use constructor without FrameProcessor")]
        public Checkbox(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : this(x, y, w, h, text, backColor) { }

        void init()
        {
//            Checked = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_checkbox-on");
//            None = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_checkbox-off");
            IsChecked = false;
            this.Image = None;
            this.ImageMode = FrameImageMode.Fitted;
        }

        void Checkbox_Click()
        {
            IsChecked = !IsChecked;
        }

        public void ChangeValue()
        {
            Image = _isChecked ? Checked : None;
        }

    }
}
