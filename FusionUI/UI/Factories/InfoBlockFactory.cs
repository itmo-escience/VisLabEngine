using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Elements;

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
                ForeColor = textColor ?? UIConfig.ActiveTextColor,
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
                //ForeColor = textColor ?? UIConfig.ActiveTextColor,
                TextAlignment = Alignment.MiddleCenter,                
            };
            holder.Item = valueLabel;

            holder.Add(label);
            holder.Add(label2);

            holder.Add(valueLabel);

            return holder;


        }

        public static UIContainer<ScalableFrame> CompareBlockHolder(FrameProcessor ui, float OffsetX,
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
                ForeColor = textColor ?? UIConfig.ActiveTextColor,
                TextAlignment = Alignment.MiddleLeft,
            };
            ScalableFrame valueLabel = new ScalableFrame(ui, OffsetX, 0, holder.UnitWidth - valueBlockWidth - 2 * OffsetX,
                UIConfig.UnitSettingsLabelHeight, text2, backColor ?? Color.Zero)
            {
                FontHolder = font,
                //ForeColor = textColor ?? UIConfig.ActiveTextColor,
                TextAlignment = Alignment.MiddleCenter,
            };
            ScalableFrame valueLabel2 = new ScalableFrame(ui, holder.UnitWidth - valueBlockWidth - OffsetX, 0, valueBlockWidth,
                UIConfig.UnitSettingsLabelHeight, value, color)
            {
                FontHolder = font,
                //ForeColor = textColor ?? UIConfig.ActiveTextColor,
                TextAlignment = Alignment.MiddleCenter,
            };
            holder.Item = valueLabel;

            holder.Add(label);
            holder.Add(valueLabel);

            holder.Add(valueLabel2);

            return holder;


        }

        public static UIContainer<ScalableFrame> CaptionHolder(FrameProcessor ui, float OffsetX,
    float OffsetY, ScalableFrame parent, String text, Color? textColor = null, UIConfig.FontHolder? UsedFont = null, bool cross = true, Action crossAction = null)
        {
            var color = textColor ?? UIConfig.ActiveTextColor;
            var font = UsedFont ?? UIConfig.FontHeader;
            UIContainer<ScalableFrame> holder = new UIContainer<ScalableFrame>(ui, parent.UnitPaddingLeft, 0,
                parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight,
                17, "", Color.Zero);

            ScalableFrame label = new ScalableFrame(ui, OffsetX, 0, holder.UnitWidth,
                holder.UnitHeight, text, Color.Zero)
            {
                FontHolder = font,
                ForeColor = textColor ?? UIConfig.ActiveTextColor,
                TextAlignment = Alignment.BaselineLeft,
                UnitTextOffsetY = 11,
            };
            

            holder.Add(label);


            if (cross)
            {
                var crossButton = new Button(ui, holder.UnitWidth - 17, 0, 17, 17, "", Color.Zero, UIConfig.ActiveColor, 200, crossAction)
                {
                    Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\southpark_cross_big"),
                    ImageColor = Color.Black,
                }; 
                holder.Add(crossButton);
            }

            return holder;


        }
    }
}
