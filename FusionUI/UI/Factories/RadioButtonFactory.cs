using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Elements;

namespace FusionUI.UI.Factories
{
    public class RadioButtonFactory
    {

        public static RadioButtonElement RadioButtonHolder(FrameProcessor ui, ScalableFrame parent, float offsetY, string label, bool selected = false, float offsetX = 0)
        {
            RadioButtonElement holder = new RadioButtonElement(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight,
                offsetY + UIConfig.UnitRadioButtonCheckHeight + 0, "", Color.Zero);

            holder.Check = new RadioButton(ui, offsetX, offsetY, holder.UnitWidth,
                UIConfig.UnitRadioButtonCheckHeight, "", Color.Zero)
            {
                Image = selected? ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_radio-sel") : ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_radio-unsel"),      
                UnitImageOffsetX = -(holder.UnitWidth - UIConfig.UnitRadioButtonCheckWidth)/2,
                ImageMode = FrameImageMode.Fitted,
                Checked          = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_radio-sel"),
                None = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_radio-unsel"),
                IsChecked = selected,                                               
            };
            holder.Label = new ScalableFrame(ui,
                offsetX + UIConfig.UnitRadioButtonCheckWidth + UIConfig.UnitRadioButtonLabelOffsetX, offsetY,
                holder.Width + UIConfig.UnitRadioButtonCheckWidth + UIConfig.UnitRadioButtonLabelOffsetX,
                UIConfig.UnitRadioButtonCheckHeight, label, Color.Zero)
            {
                TextAlignment = Alignment.BaselineLeft,
                UnitTextOffsetY = 5,
                FontHolder = UIConfig.FontBody,
                ForeColor = UIConfig.ActiveTextColor,
            };
            holder.Add(holder.Check);
            holder.Add(holder.Label);
            return holder;
        }

        public static UIContainer<RadioButtonGroup> RadioButtonGroupHolder(FrameProcessor ui, float OffsetX, float OffsetY, ScalableFrame parent, string label, out RadioButtonGroup group, float unitHeightHolder = 0)
        {
            UIContainer<RadioButtonGroup> holder = new UIContainer<RadioButtonGroup>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, unitHeightHolder + OffsetY, "", Color.Zero)
            {
            };
            group = new RadioButtonGroup(ui, OffsetX, OffsetY,
                holder.UnitWidth - OffsetX, 0, Color.Zero);
            holder.Add(group);
            if (!string.IsNullOrEmpty(label))
            {
                group.UnitY += UIConfig.UnitRadioButtonGroupCaptionHeight;
                holder.UnitHeight += UIConfig.UnitRadioButtonGroupCaptionHeight;
                ScalableFrame labelFrame = new ScalableFrame(ui, OffsetX, OffsetY, holder.UnitWidth - OffsetX, UIConfig.UnitRadioButtonGroupCaptionHeight, label, Color.Zero)
                {
                    TextAlignment = Alignment.BaselineLeft,
                    UnitTextOffsetY = 4,
                    FontHolder = UIConfig.FontBody,
                    ForeColor = UIConfig.ActiveTextColor,
                };
                holder.Add(labelFrame);
            }
            holder.Item = group;
            return holder;
        }
    }
}
