using System;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI.Elements
{
    public class RadioButton : ScalableFrame
    {
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
                Changed?.Invoke(value);
            }
        }

        public RadioButton()
        {
            Text = "";
            init();
            this.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsClick) return;
                Radio_Click();
                flag |= true;
            };
        }


        public RadioButton(float x, float y, float w, float h, string text, Color backColor) : base(x, y, w, h, text, backColor)
        {
            init();
            this.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsClick) return;
                Radio_Click();
                flag |= true;
            };
        }

        [Obsolete("Please use constructor without FrameProcessor")]
        public RadioButton(FrameProcessor ui) : this() { }
        [Obsolete("Please use constructor without FrameProcessor")]
        public RadioButton(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor)
            : this(x, y, w, h, text, backColor) { }

        void init()
        {
            Checked = Game.Instance.Content.Load<DiscTexture>(@"UI-new\fv-icons_radio-on");
            None = Game.Instance.Content.Load<DiscTexture>(@"UI-new\fv-icons_radio-off");
            IsChecked = false;
            Image = None;
            ImageMode = FrameImageMode.Stretched;
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
