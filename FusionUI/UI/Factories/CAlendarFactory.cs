using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using FusionUI.UI.Elements;

namespace FusionUI.UI.Factories
{
    public static class CalendarFactory
    {
        public static UIContainer<TimeSelector> HorizontalCalendarHolder(FrameProcessor ui, float OffsetX, float OffsetY, ScalableFrame parent, 
            string label, Action<DateTime> selectAction, 
            DateTime initialDate, DateTime startDate, DateTime endDate, out TimeSelector TimeSelector)
        {
            UIContainer<TimeSelector> holder = new UIContainer<TimeSelector>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight,
                UIConfig.UnitFilterWindowSelectorRowHeight, "",
                Color.Zero);
            ScalableFrame labelFrame = new ScalableFrame(ui, OffsetX,
                OffsetY, UIConfig.UnitFilterWindowLabelWidth,
                UIConfig.UnitFilterWindowElementHeight, label, Color.Zero)
            {
                TextAlignment = Alignment.MiddleLeft,
            };
            float offset = OffsetX + UIConfig.UnitFilterWindowLabelWidth;
            float width = (holder.UnitWidth - OffsetX * 2 -
                           UIConfig.UnitFilterWindowLabelWidth);

            TimeSelector = new TimeSelector(ui, offset, OffsetY, width,
                UIConfig.UnitFilterWindowElementHeight, UIConfig.InactiveColor, selectAction, UIConfig.BorderColor,
                initialDate, startDate, endDate);
            holder.Item = TimeSelector;       
            holder.Add(labelFrame);
            holder.Add(TimeSelector);
            return holder;
        }
    }
}
