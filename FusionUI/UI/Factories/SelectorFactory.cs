using System;
using System.Collections.Generic;
using Fusion.Engine.Frames;
using Fusion.Core.Mathematics;
using FusionUI.UI.Elements.DropDown;

namespace FusionUI.UI.Factories
{
    public static class SelectorFactory
    {
        public static UIContainer<DropDownSelector<TR>> HorizontalSelectorHolder<TR>(FrameProcessor ui, float OffsetX, float OffsetY, ScalableFrame parent, string label, List<string> elements, Action<string> selectAction, out DropDownSelector<TR> selector) where TR:DropDownSelectorRow, new()
        {
            UIContainer<DropDownSelector<TR>> holder = new UIContainer<DropDownSelector<TR>>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, UIConfig.UnitFilterWindowSelectorRowHeight, "", Color.Zero);
            ScalableFrame labelFrame = new ScalableFrame(ui, OffsetX, OffsetY, UIConfig.UnitFilterWindowLabelWidth, UIConfig.UnitFilterWindowElementHeight, label, Color.Zero)
            {
                TextAlignment = Alignment.MiddleLeft,
            };

            selector = new DropDownSelector<TR>(ui, OffsetX + UIConfig.UnitFilterWindowLabelWidth, OffsetY, holder.UnitWidth - OffsetX * 2 - UIConfig.UnitFilterWindowLabelWidth, UIConfig.UnitFilterWindowElementHeight, UIConfig.InactiveColor, elements, selectAction, UIConfig.BorderColor)
            {
                Border = 2,
                BorderColor = UIConfig.BorderColor,
                TextAlignment = Alignment.MiddleLeft,
                Capacity = 7,
                ForeColor = UIConfig.ActiveTextColor,
            };
            holder.Item = selector;
            holder.Add(labelFrame);
            holder.Add(selector);
            return holder;
        }

        public static UIContainer<DropDownSelector<TR>> VerticalSelectorHolder<TR>(FrameProcessor ui, float OffsetX, float OffsetY, ScalableFrame parent, string label, List<string> elements, Action<string> selectAction, out DropDownSelector<TR> selector, UIConfig.FontHolder? font = null) where TR : DropDownSelectorRow, new()
        {
            font = font ?? UIConfig.FontBase;
            UIContainer<DropDownSelector<TR>> holder = new UIContainer<DropDownSelector<TR>>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, UIConfig.UnitFilterWindowSelectorRowHeight * ((label != "") ?  2 : 1), "", Color.Zero);
            ScalableFrame labelFrame = new ScalableFrame(ui, OffsetX, OffsetY, UIConfig.UnitFilterWindowLabelWidth, UIConfig.UnitFilterWindowElementHeight, label, Color.Zero)
            {
                TextAlignment = Alignment.MiddleLeft,
            };            
            selector = new DropDownSelector<TR>(ui, OffsetX, OffsetY + (label != "" ? labelFrame.UnitHeight : 0), holder.UnitWidth - OffsetX * 2, UIConfig.UnitFilterWindowElementHeight, UIConfig.InactiveColor, elements, selectAction, UIConfig.BorderColor, font:font)
            {
                Border = 2,
                BorderColor = UIConfig.BorderColor,
                TextAlignment = Alignment.MiddleLeft,
                Capacity = 7,
                ForeColor = UIConfig.ActiveTextColor,
            };
            holder.Item = selector;
            holder.Add(labelFrame);
            holder.Add(selector);
            return holder;
        }
    }
}
