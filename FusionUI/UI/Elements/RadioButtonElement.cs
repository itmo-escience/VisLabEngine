using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements
{
    public class RadioButtonElement : ScalableFrame
    {
        public RadioButton Check;
        public ScalableFrame Label;
        public string Value => Label.DefaultText;

        public bool IsChecked
        {
            get { return Check.IsChecked; }
            set { Check.IsChecked = value; }
        }

        public RadioButtonElement(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
        }
    }
}
