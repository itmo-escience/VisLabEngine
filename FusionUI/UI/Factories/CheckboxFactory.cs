using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Elements.TextFormatting;

namespace FusionUI.UI.Factories
{
    public class CheckboxFactory
    {
        public static UIContainer<Checkbox> CheckboxVerticalHolder(FrameProcessor ui, float OffsetX, float OffsetY, ScalableFrame parent, string label,
            Action<bool> switchAction, bool active, out Checkbox checkbox, string imageOn = null, string imageOff = null)
        {
            var labelHeight = UIConfig.UnitCheckboxLabelHeight * label.Split('\n').Length;
            UIContainer<Checkbox> holder = new UIContainer<Checkbox>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, UIConfig.UnitCheckboxHeight + labelHeight + OffsetY, "", Color.Zero);

            ScalableFrame labelFrame = new ScalableFrame(ui, OffsetX, OffsetY, holder.UnitWidth, labelHeight, label, Color.Zero)
            {
                TextAlignment = Alignment.BaselineLeft,
                UnitTextOffsetY = 4,
                FontHolder = UIConfig.FontBody,
                ForeColor = UIConfig.ActiveTextColor,
            };

            checkbox = new Checkbox(ui, OffsetX, OffsetY + labelHeight, holder.Width - OffsetX * 2, UIConfig.UnitCheckboxHeight, "", Color.Zero)
            {
                Checked = ui.Game.Content.Load<DiscTexture>(imageOn ?? @"UI-new\fv-icons_switcher-on"),
                None = ui.Game.Content.Load<DiscTexture>(imageOff ?? @"UI-new\fv-icons_switcher-off"),
                UnitImageOffsetX = -(holder.Width - OffsetX * 2 - UIConfig.UnitCheckboxWidth) / 2,
                ImageMode = FrameImageMode.Fitted,
                IsChecked = active,
                Name = label
            };

            ScalableFrame cbLabel = new ScalableFrame(ui, OffsetX + UIConfig.UnitCheckboxWidth + UIConfig.UnitCheckboxValueOffset, OffsetY + labelHeight,
                holder.Width - UIConfig.UnitCheckboxWidth + UIConfig.UnitCheckboxValueOffset - OffsetX, UIConfig.UnitCheckboxHeight, active ? "On" : "Off", Color.Zero)
            {
                TextAlignment = Alignment.BaselineLeft,
                UnitTextOffsetY = 6,
                FontHolder = UIConfig.FontBody,
                ForeColor = UIConfig.ActiveTextColor,
            };

            checkbox.Changed += (bool flag) =>
            {
                cbLabel.Text = flag ? "On" : "Off";
            };
            checkbox.Changed += switchAction;
            holder.Item = checkbox;
            holder.Add(checkbox);
            holder.Add(labelFrame);
            holder.Add(cbLabel);
            return holder;
        }

        public static UIContainer<Checkbox> CheckboxHorizontalHolder(FrameProcessor ui, float OffsetX, float OffsetY, float width, float height, ScalableFrame parent, string label,
            Action<bool> switchAction, bool active, out Checkbox checkbox, bool isSwitcher = true)
        {
            UIContainer<Checkbox> holder = new UIContainer<Checkbox>(ui, parent.UnitPaddingLeft, 0, width + OffsetX, height + OffsetY, "", Color.Zero);
            float UnitCheckboxWidth = isSwitcher ? UIConfig.UnitSwitcherWidth : UIConfig.UnitCheckboxWidth;
            float UnitCheckboxHeight = isSwitcher ? UIConfig.UnitSwitcherHeight : UIConfig.UnitCheckboxHeight;
            checkbox = new Checkbox(ui, OffsetX, OffsetY, width - 2 * OffsetX, height - 2 * OffsetY, "", Color.Zero)
            {
                Checked = isSwitcher ? ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_switcher-on")  : ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_checkbox-big-on"),
                None    = isSwitcher ? ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_switcher-off") : ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_checkbox-big-off"),
                //UnitImageOffsetX = -(holder.UnitWidth - OffsetX * 2 - UIConfig.UnitCheckboxWidth) / 2,
                //UnitImageOffsetY = holder.UnitHeight - UIConfig.UnitCheckboxHeight,
                ImageMode = FrameImageMode.Fitted,
                UnitVPadding = (height - 2 * OffsetY - UnitCheckboxHeight)/2,
                UnitPaddingRight = width - 2 * OffsetX - UnitCheckboxWidth - UIConfig.UnitCheckboxValueOffset,
                IsChecked = active,
                Name = label
            };

            FormatTextBlock cbLabel = new FormatTextBlock(ui, OffsetX + UnitCheckboxWidth + UIConfig.UnitCheckboxValueOffset, OffsetY,
                holder.Width - UnitCheckboxWidth + UIConfig.UnitCheckboxValueOffset - OffsetX, height - 2 * OffsetY, label, Color.Zero, UIConfig.FontBody, 0)
            {
                TextAlignment = Alignment.MiddleLeft,                
                FontHolder = UIConfig.FontBody,
                ForeColor = UIConfig.ActiveTextColor,
            };
            
            checkbox.Changed += switchAction;
            holder.Add(checkbox);
            holder.Add(cbLabel);
            holder.Item = checkbox;
            return holder;
        }        

    }
}
