using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements.DropDown
{
    public class DropDownSelectorTextRow :DropDownSelectorRow
    {
        private string value;


        public override string Value {
            get { return value; }
            set
            {
                this.value = value;
                this.Text = value;
            }
        }

        public override void Initialize(float x, float y, float w, float h, string text, Color backColor)
        {
            base.Initialize(x, y, w, h, text, backColor);
            UnitTextOffsetX = 2;
            TextAlignment = Alignment.MiddleLeft;
        }
    }
}
