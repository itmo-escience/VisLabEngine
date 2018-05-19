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
using FusionUI.UI.Elements.TextFormatting;

namespace FusionUI.UI.Factories
{
    public class InfoBlockFactory
    {
        public static UIContainer<ScalableFrame> InfoBlockHolder(FrameProcessor ui, float OffsetX,
            float OffsetY, ScalableFrame parent, float valueBlockWidth = 16,
            string text1 = "Text1", string text2 = "Text2", string value = "0.00", Color? backColor = null,
            Color? textColor = null, Color? valueBackColor = null, UIConfig.FontHolder? UsedFont = null)
        {
            var color = valueBackColor ?? UIConfig.PopupColor;
            var font = UsedFont ?? UIConfig.FontBase;
            UIContainer<ScalableFrame> holder = new UIContainer<ScalableFrame>(ui, parent.UnitPaddingLeft, 0,
                parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight,
                UIConfig.UnitSettingsLabelHeight + OffsetY, "", Color.Zero);

            FormatTextBlock label = new FormatTextBlock(ui, OffsetX, 0, holder.UnitWidth - valueBlockWidth * (text2 == "" ? 1 : 2) - 3 * OffsetX,
                UIConfig.UnitSettingsLabelHeight, text1, backColor ?? Color.Zero, font, 0)
            {
                FontHolder = font,
                ForeColor = textColor ?? UIConfig.ActiveTextColor,
                TextAlignment = Alignment.MiddleLeft,
            };
            FormatTextBlock label2 = new FormatTextBlock(ui, OffsetX + label.UnitWidth, 0, valueBlockWidth,
                UIConfig.UnitSettingsLabelHeight, text2, backColor ?? Color.Zero, font, 0)
            {
                FontHolder = font,
                TextAlignment = Alignment.MiddleRight,
                DefaultAlignment = "right",
                UnitTextOffsetX = -OffsetX,
                ForeColor = Color.Gray,                
            };
            FormatTextBlock valueLabel = new FormatTextBlock(ui, holder.UnitWidth - valueBlockWidth - OffsetX, 0,
                valueBlockWidth,
                UIConfig.UnitSettingsLabelHeight, value, color, font, 0)
            {
                FontHolder = font,
                //ForeColor = textColor ?? UIConfig.ActiveTextColor,
                TextAlignment = Alignment.MiddleCenter,
                DefaultAlignment = "center",
            };
            holder.Item = valueLabel;

            holder.Add(label);
            holder.Add(label2);

            holder.Add(valueLabel);
            holder.Height = holder.Children.Max(a => a.Height);
            foreach (var holderChild in holder.Children)
            {
                holderChild.Height = holder.Height;
                if (holderChild is RichTextBlock) ((RichTextBlock) holderChild).MinHeight = holder.Height;
            }
            return holder;


        }

        public static UIContainer<ScalableFrame, ScalableFrame> InfoBlockHolder5(FrameProcessor ui, float OffsetX,
            float OffsetY, ScalableFrame parent,
            List<Tuple<string, float>> vals, float InnerOffset = 1, float blockHeight = 9, bool useOldStyle = false, bool fixSizes = false, Color? backColor = null,
            Color? textColor = null, Color? valueBackColor = null, UIConfig.FontHolder? UsedFont = null)
        {
            var color = valueBackColor ?? UIConfig.PopupColor;
            var font = UsedFont ?? UIConfig.FontBase;
            UIContainer<ScalableFrame, ScalableFrame> holder = new UIContainer<ScalableFrame, ScalableFrame>(ui, parent.UnitPaddingLeft, 0,
                parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight,
                blockHeight + 2 * OffsetY, "", Color.Zero);


            float m = fixSizes ? (holder.UnitWidth - holder.UnitPaddingRight - holder.UnitPaddingLeft) / (vals.Sum(a => a.Item2) + (vals.Count - 1) * InnerOffset) : 1;
            float w = 0;
            for (int i = 0; i < vals.Count; i++)
            {
                if (i == 2 && useOldStyle)
                {

                    FormatTextBlock label = new FormatTextBlock(ui, OffsetX + w, OffsetY,
                        vals[i].Item2 * m,
                        blockHeight, vals[i].Item1, Color.Zero, font, 0)
                    {
                        FontHolder = font,
                        ForeColor = UIConfig.InactiveTextColor,
                        TextAlignment = i == 0 ? Alignment.MiddleLeft : Alignment.MiddleCenter,
                        DefaultAlignment = i == 0 ? "left" : "center"
                    };                    
                    holder.Add(label);
                    if (i == 2) holder.Item1 = label;
                    if (i == 3) holder.Item2 = label;
                }
                else
                {
                    FormatTextBlock label = new FormatTextBlock(ui, OffsetX + w, 0,
                        vals[i].Item2 * m,
                        blockHeight, vals[i].Item1, (i > 1 && backColor != null) ? backColor.Value : Color.Zero, font, 0)
                    {
                        FontHolder = font,
                        ForeColor = textColor ?? UIConfig.ActiveTextColor,
                        TextAlignment = i == 0? Alignment.MiddleLeft : Alignment.MiddleCenter,
                        DefaultAlignment = i == 0 ? "left" : "center"

                    };
                    holder.Add(label);
                    if (i == 2) holder.Item1 = label;
                    if (i == 3) holder.Item2 = label;
                }
                w += vals[i].Item2 * m + InnerOffset;
            }

            holder.Height = holder.Children.Max(a => a.Height) + (int)(OffsetY * 2 * ApplicationInterface.ScaleMod);
            foreach (var holderChild in holder.Children)
            {
                holderChild.Height = holder.Height - (int)(OffsetY * 2 * ApplicationInterface.ScaleMod);
                if (holderChild is RichTextBlock) ((RichTextBlock)holderChild).MinHeight = holder.Height - (int)(OffsetY * 2 * ApplicationInterface.ScaleMod);
            }
            return holder;


        }




        public static UIContainer<ScalableFrame> CompareBlockHolder(FrameProcessor ui, float OffsetX,
            float OffsetY, ScalableFrame parent, float valueBlockWidth = 16,
            string text1 = "Text1", string text2 = "Text2", string value = "0.00", Color? backColor = null,
            Color? textColor = null, Color? valueBackColor = null, UIConfig.FontHolder? UsedFont = null)
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
            ScalableFrame valueLabel = new ScalableFrame(ui, OffsetX, 0,
                holder.UnitWidth - valueBlockWidth - 2 * OffsetX,
                UIConfig.UnitSettingsLabelHeight, text2, backColor ?? Color.Zero)
            {
                FontHolder = font,
                //ForeColor = textColor ?? UIConfig.ActiveTextColor,
                TextAlignment = Alignment.MiddleCenter,
            };
            ScalableFrame valueLabel2 = new ScalableFrame(ui, holder.UnitWidth - valueBlockWidth - OffsetX, 0,
                valueBlockWidth,
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
            float OffsetY, ScalableFrame parent, String text, Color? textColor = null,
            UIConfig.FontHolder? UsedFont = null, bool cross = true, Action crossAction = null)
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
                var crossButton = new Button(ui, holder.UnitWidth - 17, 0, 17, 17, "", Color.Zero, UIConfig.ActiveColor,
                    200, crossAction)
                {
                    Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\southpark_cross_big"),
                    ImageColor = Color.Black,
                };
                holder.Add(crossButton);
                holder.Item = crossButton;
            }

            return holder;


        }

        public static UIContainer<ScalableFrame, ScalableFrame> CaptionHolder(FrameProcessor ui, float OffsetX,
            float OffsetY, ScalableFrame parent, String text, string buttonImage, Color? textColor = null,
            UIConfig.FontHolder? UsedFont = null, bool cross = true, Action crossAction = null, Action ButtonAction = null)
        {
            var color = textColor ?? UIConfig.ActiveTextColor;
            var font = UsedFont ?? UIConfig.FontHeader;
            UIContainer<ScalableFrame, ScalableFrame> holder = new UIContainer<ScalableFrame, ScalableFrame>(ui, parent.UnitPaddingLeft, 0,
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
                var crossButton = new Button(ui, holder.UnitWidth - 17 - OffsetX, 0, 17, 17, "", Color.Zero, UIConfig.ActiveColor,
                    200, crossAction)
                {
                    Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\southpark_cross_big"),
                    ImageColor = Color.Black,
                    ActiveFColor = Color.White,
                    InactiveFColor = Color.Black,
                };
                holder.Add(crossButton);
                holder.Item2 = crossButton;
            }

            var actionButton = new Button(ui, holder.UnitWidth - 17 - OffsetX - (cross ? 20 : 0), 0, 17, 17, "", Color.Zero, UIConfig.ActiveColor,
                200, ButtonAction)
            {
                Image = ui.Game.Content.Load<DiscTexture>(buttonImage),
                ImageColor = Color.Black,
                ActiveFColor = Color.White,
                InactiveFColor = Color.Black,
            };
            holder.Add(actionButton);

            holder.Item1 = actionButton;
            return holder;


        }


    }
}
