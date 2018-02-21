using System;
using Fusion.Core.Mathematics;

namespace FusionUI.UI.Elements.DropDown
{
    public abstract class DropDownSelectorRow : ScalableFrame
    {
        public DropDownSelectorRow() : base(ApplicationInterface.Instance.FrameProcessor, 0, 0, 0, 0, "", Color.Zero)
        {

        }      

        public virtual void Initialize(float x, float y, float w, float h, string text, Color backColor)
        {
            UnitX = x;
            UnitY = y;
            UnitWidth = w;
            UnitHeight = h;
            Value = text;
            BackColor = backColor;
        }

        public abstract String Value { get; set; }
    }
}
