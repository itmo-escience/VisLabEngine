using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Factories
{
    public class InfoBlockFactory
    {
        public static UIContainer<ScalableFrame> InfoBlockHolder(FrameProcessor ui, float OffsetX,
            float OffsetY, ScalableFrame parent, float valueBlockWidth = 16, 
            string text1 = "Text1", string text2 = "Text2", string value = "0.00", Color? backColor = null, Color? textColor = null, Color? valueBackColor = null, UIConfig.FontHolder? UsedFont = null)
        {
            var color = valueBackColor ?? UIConfig.PopupColor;            
            var font = UsedFont ?? UIConfig.FontBase;
            UIContainer<ScalableFrame> holder = new UIContainer<ScalableFrame>(ui, parent.UnitPaddingLeft, 0,
                parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight,
                UIConfig.UnitSettingsLabelHeight + OffsetY, "", Color.Zero);

            ScalableFrame label = new ScalableFrame(ui, OffsetX, 0, holder.UnitWidth - valueBlockWidth - 2 * OffsetX,
                UIConfig.UnitSettingsLabelHeight, text1, backColor ?? Color.Zero)
            {
                FontHolder = font,
                ForeColor = textColor ?? Color.White,
                TextAlignment = Alignment.MiddleLeft,                
            };
            ScalableFrame label2 = new ScalableFrame(ui, OffsetX, 0, holder.UnitWidth - valueBlockWidth - 2 * OffsetX,
                UIConfig.UnitSettingsLabelHeight, text2, backColor ?? Color.Zero)
            {
                FontHolder = font,
                TextAlignment = Alignment.MiddleRight,
                UnitTextOffsetX = -OffsetX,
                ForeColor = Color.Gray,
            };
            ScalableFrame valueLabel = new ScalableFrame(ui, holder.UnitWidth - valueBlockWidth - OffsetX, 0, valueBlockWidth,
                UIConfig.UnitSettingsLabelHeight, value, color)
            {
                FontHolder = font,
                //ForeColor = textColor ?? Color.White,
                TextAlignment = Alignment.MiddleCenter,                
            };
            holder.Item = valueLabel;

            holder.Add(label);
            holder.Add(label2);

            holder.Add(valueLabel);

            return holder;


        }
    }
}
