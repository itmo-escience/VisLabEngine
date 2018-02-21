using Fusion.Engine.Frames;
using Fusion.Core.Mathematics;
using FusionUI.UI.Elements;

namespace FusionUI.UI.Factories
{
    public class ProgressbarFactory
    {
        public static UIContainer<MeasuredProgressbar> MeasurableProgressbarHolder(FrameProcessor ui, float OffsetX, float OffsetY,
            ScalableFrame parent,
            string label, out MeasuredProgressbar progressbar)
        {
            UIContainer<MeasuredProgressbar> holder = new UIContainer<MeasuredProgressbar>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight,
                UIConfig.UnitProgressbarTickness + UIConfig.UnitProgressbarElementOffset +
                UIConfig.UnitProgressbarLabelHeight + OffsetY, "", Color.Zero)
            {
            };

            var labelFrame = new ScalableFrame(ui, OffsetX, OffsetY, holder.UnitWidth - 2*OffsetX,
                UIConfig.UnitProgressbarLabelHeight, label, Color.Zero)
            {
                TextAlignment = Alignment.BaselineLeft,
                UnitTextOffsetY = 4,
                FontHolder = UIConfig.FontBody,
            };

            ScalableFrame valueFrame = new ScalableFrame(ui, OffsetX, OffsetY, holder.UnitWidth - 2*OffsetX,
                UIConfig.UnitProgressbarLabelHeight, "0%", Color.Zero)
            {
                TextAlignment = Alignment.BaselineRight,
                UnitTextOffsetY = 4,
                FontHolder = UIConfig.FontBody,
            };

            progressbar = new MeasuredProgressbar(ui, OffsetX,
                OffsetY + UIConfig.UnitProgressbarLabelHeight + UIConfig.UnitProgressbarElementOffset,
                holder.UnitWidth - 2*OffsetX, UIConfig.UnitProgressbarTickness);

            progressbar.ValueUpdate += f =>
            {
                valueFrame.Text = $"{f:0%}";
            };
            holder.Item = progressbar;
            holder.Add(labelFrame);
            holder.Add(valueFrame);
            holder.Add(progressbar);
            return holder;
        }

        public static UIContainer<UnmeasuredProgressbar> UnmeasurableProgressbarHolder(FrameProcessor ui, float OffsetX, float OffsetY,
            ScalableFrame parent,
            string label, out UnmeasuredProgressbar progressbar)
        {
            UIContainer<UnmeasuredProgressbar> holder = new UIContainer<UnmeasuredProgressbar>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight,
                UIConfig.UnitProgressbarTickness + UIConfig.UnitProgressbarElementOffset +
                UIConfig.UnitProgressbarLabelHeight + OffsetY, "", Color.Zero)
            {
            };

            var labelFrame = new ScalableFrame(ui, OffsetX, OffsetY, holder.UnitWidth - 2 * OffsetX,
                UIConfig.UnitProgressbarLabelHeight, label, Color.Zero)
            {
                TextAlignment = Alignment.BaselineLeft,
                UnitTextOffsetY = 4,
                FontHolder = UIConfig.FontBody,
            };

            progressbar = new UnmeasuredProgressbar(ui, OffsetX,
                OffsetY + UIConfig.UnitProgressbarLabelHeight + UIConfig.UnitProgressbarElementOffset,
                holder.UnitWidth - 2 * OffsetX, UIConfig.UnitProgressbarTickness, 0.15f, 0.25f);
            holder.Item = progressbar;
            holder.Add(labelFrame);
            holder.Add(progressbar);
            return holder;
        }
    }
}
