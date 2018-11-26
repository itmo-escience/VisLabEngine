using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.Graph;
using FusionUI.UI.Elements;
using FusionUI.UI.Elements.TextFormatting;

namespace FusionUI.UI.Factories
{
    public class SliderFactory
    {
        public static UIContainer<Slider> SliderVerticalHolder(float OffsetX, float OffsetY,
            ScalableFrame parent, string label,
            Action<float> changeAction, float min, float max, float initValue, out Slider slider, bool minMaxSelector = false, string imageSlider = null, bool IsVertical=false, bool percent = false)
        {
            UIContainer<Slider> holder = new UIContainer<Slider>(parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, UIConfig.UnitSliderLabelHeight + UIConfig.UnitSliderObjectHeight + OffsetY, "", Color.Zero)
            {
            };

            ScalableFrame labelFrame = new ScalableFrame(OffsetX, OffsetY, holder.UnitWidth, UIConfig.UnitCheckboxLabelHeight, label, Color.Zero)
            {
                TextAlignment = Alignment.BaselineLeft,
                UnitTextOffsetY = 4,
                FontHolder = UIConfig.FontBody,
            };

            ScalableFrame valueFrame = new ScalableFrame(OffsetX, OffsetY, holder.UnitWidth - 2 * OffsetX,
                UIConfig.UnitCheckboxLabelHeight, percent ? $"{initValue * 100:0.##}%" : $"{initValue:0.##}", Color.Zero)
            {
                TextAlignment = Alignment.BaselineRight,
                UnitTextOffsetY = 4,
                FontHolder = UIConfig.FontBody,
            };

            slider = new Slider(OffsetX, OffsetY + UIConfig.UnitSliderLabelHeight, holder.UnitWidth - 2 * OffsetX, UIConfig.UnitSliderObjectHeight, "", Color.Zero)
            {
                backColorForSlider = ColorConstant.BackColorForSlider,
                ForeColor = ColorConstant.ForeColor,
                HoverColor = ColorConstant.HoverColor,
                HoverForeColor = ColorConstant.HoverForeColor,
                MinValue = min,
                MaxValue = max,
                CurrentValue = initValue,
                UnitSliderWidth = 0.5f,
                UnitWidthControlElement = 4,
                UnitHeightControlElement = UIConfig.UnitSliderObjectHeight,
                Image = Game.Instance.Content.Load<DiscTexture>(imageSlider ?? @"UI-new\fv-icons_slider"),
                IsVertical           = IsVertical
            };

            //slider.OnChange += f => { valueFrame.Text = $"{f:0.##}"; };
            if (percent)
            {
                slider.OnChange += f => { valueFrame.Text = $"{f * 100:0.##}%"; };
            }
            else
            {
                slider.OnChange += f => { valueFrame.Text = $"{f:0.##}"; };
            }
            slider.OnChange += changeAction;
            holder.Item = slider;
            holder.Add(labelFrame);
            holder.Add(valueFrame);
            holder.Add(slider);


            if (minMaxSelector)
            {
                holder.UnitHeight += OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ElementOffset;
                float EditboxWidth = 25;
                Editbox minEdit = new Editbox(OffsetX,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight, EditboxWidth,
                    UIConfig.UnitPalette2LabelHeight, $"{slider.MinValue}", Color.Black)
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                    BorderActive = UIConfig.ActiveColor,
                    HoverColor = Color.White,
                    PaddingLeft = (int)(2 * ApplicationInterface.ScaleMod),
                    //PaddingBottom = (int) (2*AppInterface.ScaleMod),
                    TextAlignment = Alignment.MiddleLeft
                };
                minEdit.ActionOut += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
                {
                    if (args.IsClick) minEdit.SetActiveStatus(false);
                };
                Editbox maxEdit = new Editbox(holder.UnitWidth - OffsetX - EditboxWidth,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight, EditboxWidth,
                    UIConfig.UnitPalette2LabelHeight, $"{slider.MaxValue}", Color.Black)
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                    BorderActive = UIConfig.ActiveColor,
                    HoverColor = Color.White,
                    PaddingLeft = (int)(2 * ApplicationInterface.ScaleMod),
                    //PaddingBottom = (int)(2 * AppInterface.ScaleMod),
                    TextAlignment = Alignment.MiddleLeft
                };
                maxEdit.ActionOut += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
                {
                    if (args.IsClick) maxEdit.SetActiveStatus(false);
                };
                var s = slider;
                Button setMinMaxButton = new Button(OffsetX + EditboxWidth,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight,
                    holder.UnitWidth - 2 * (OffsetX + EditboxWidth), UIConfig.UnitPalette2LabelHeight, "Set",
                    UIConfig.ButtonColor, UIConfig.ActiveColor, 200,
                    () =>
                    {
                        float newMin = float.Parse(minEdit.Text), newMax = float.Parse(maxEdit.Text);
                        s.MinValue = newMin;
                        s.MaxValue = newMax;
                        s.CurrentValue = MathUtil.Clamp(s.MinValue, s.MaxValue, s.CurrentValue);
                        s.OnChange.Invoke(s.CurrentValue);
                    })
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                };
                holder.Add(minEdit);
                holder.Add(maxEdit);
                holder.Add(setMinMaxButton);
            }


            return holder;
        }

        public static UIContainer<Slider> SliderVerticalHolderNew(float OffsetX, float OffsetY, float width, float height,
            ScalableFrame parent, string label,
            Action<float> changeAction, float min, float max, float initValue, out Slider slider,
            bool minMaxSelector = false, string imageSlider = null,
            bool IsVertical = false, bool showValue = true, Alignment labelAlignment = Alignment.TopCenter, bool percent = false, Color? labelBackColor = null)
        {
            var sizeLabel = UIConfig.FontBody[1].MeasureString(ScalableFrame.TryGetText(label));

            var holder = new UIContainer<Slider>(parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, sizeLabel.Height / ApplicationInterface.gridUnitDefault + height + OffsetY, "", Color.Zero)
            {
            };


            ScalableFrame labelFrame = new FormatTextBlock(OffsetX, OffsetY, holder.UnitWidth, (sizeLabel.Height) /ApplicationInterface.gridUnitDefault, label, labelBackColor ?? Color.Zero, UIConfig.FontBody, 0)
            {
                TextAlignment = labelAlignment,
                //UnitTextOffsetY = 4,
                FontHolder = UIConfig.FontBody,
            };

            slider = new Slider(OffsetX, labelFrame.UnitHeight + OffsetY, holder.UnitWidth, height, "", Color.Zero)
            {
                backColorForSlider = ColorConstant.BackColorForSlider,
                ForeColor = ColorConstant.ForeColor,
                HoverColor = ColorConstant.HoverForeColor,
                HoverForeColor = ColorConstant.HoverForeColor,
                MinValue = min,
                MaxValue = max,
                CurrentValue = initValue,
                UnitSliderWidth = 0.5f,
                UnitWidthControlElement = 4,
                UnitHeightControlElement = UIConfig.UnitSliderObjectHeight,
                Image = Game.Instance.Content.Load<DiscTexture>(imageSlider ?? @"UI-new\fv-icons_slider"),
                IsVertical = IsVertical,
            };
            holder.UnitHeight = labelFrame.UnitHeight + slider.UnitHeight;
            if (showValue)
            {
                ScalableFrame valueFrame = new ScalableFrame(OffsetX, OffsetY, holder.UnitWidth - 2 * OffsetX,
                    height, percent ? $"{initValue * 100:0.##}%" : $"{initValue:0.##}", Color.Zero)
                {
                    TextAlignment = Alignment.BaselineRight,
                    UnitTextOffsetY = 4,
                    FontHolder = UIConfig.FontBody,
                };
                if (percent)
                {
                    slider.OnChange += f => { valueFrame.Text = $"{f * 100:0.##}%"; };
                }
                else
                {
                    slider.OnChange += f => { valueFrame.Text = $"{f:0.##}"; };
                }

                holder.Add(valueFrame);
            }

            slider.OnChange += changeAction;
            holder.Add(labelFrame);
            holder.Add(slider);
            holder.Item = slider;

            if (minMaxSelector)
            {
                holder.UnitHeight += OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ElementOffset;
                float EditboxWidth = 25;
                Editbox minEdit = new Editbox(OffsetX,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight, EditboxWidth,
                    UIConfig.UnitPalette2LabelHeight, $"{slider.MinValue}", Color.Black)
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                    BorderActive = UIConfig.ActiveColor,
                    HoverColor = Color.White,
                    PaddingLeft = (int)(2 * ApplicationInterface.ScaleMod),
                    //PaddingBottom = (int) (2*AppInterface.ScaleMod),
                    TextAlignment = Alignment.MiddleLeft
                };
                minEdit.ActionOut += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
                {
                    if (args.IsClick) minEdit.SetActiveStatus(false);
                };
                Editbox maxEdit = new Editbox(holder.UnitWidth - OffsetX - EditboxWidth,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight, EditboxWidth,
                    UIConfig.UnitPalette2LabelHeight, $"{slider.MaxValue}", Color.Black)
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                    BorderActive = UIConfig.ActiveColor,
                    HoverColor = Color.White,
                    PaddingLeft = (int)(2 * ApplicationInterface.ScaleMod),
                    //PaddingBottom = (int)(2 * AppInterface.ScaleMod),
                    TextAlignment = Alignment.MiddleLeft
                };
                maxEdit.ActionOut += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
                {
                    if (args.IsClick) maxEdit.SetActiveStatus(false);
                };
                var s = slider;
                Button setMinMaxButton = new Button(OffsetX + EditboxWidth,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight,
                    holder.UnitWidth - 2 * (OffsetX + EditboxWidth), UIConfig.UnitPalette2LabelHeight, "Set",
                    UIConfig.ButtonColor, UIConfig.ActiveColor, 200,
                    () =>
                    {
                        float newMin = float.Parse(minEdit.Text), newMax = float.Parse(maxEdit.Text);
                        s.MinValue = newMin;
                        s.MaxValue = newMax;
                        s.CurrentValue = MathUtil.Clamp(s.MinValue, s.MaxValue, s.CurrentValue);
                        s.OnChange.Invoke(s.CurrentValue);
                    })
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                };
                holder.Add(minEdit);
                holder.Add(maxEdit);
                holder.Add(setMinMaxButton);
            }


            return holder;
        }

        public static ScalableFrame SliderHorizontalHolderNew(float OffsetX, float OffsetY,
            float width, float height,
            ScalableFrame parent, string label, float labelWidth,
            Action<float> changeAction, float min, float max, float initValue, out Slider slider,
            bool minMaxSelector = false, string imageSlider = null,
            bool IsVertical = false, bool showValue = true, float valueWidth = 0,
            Alignment labelAlignment = Alignment.MiddleCenter, Alignment valueAlignment = Alignment.MiddleCenter,
            UIConfig.FontHolder? labelFont = null, UIConfig.FontHolder? valueFont = null, bool percent = false)
        {
            var sizeLabel = UIConfig.FontBody[2].MeasureString(ScalableFrame.TryGetText(label));
            var labelFontHolder = labelFont ?? UIConfig.FontBody;
            var valueFontHolder = valueFont ?? UIConfig.FontBody;
            ScalableFrame holder = new ScalableFrame(parent.UnitPaddingLeft, 0,
                parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight,
                height + OffsetY, "", Color.Zero);


            ScalableFrame labelFrame = new FormatTextBlock(OffsetX, OffsetY, labelWidth,
                height, label, Color.Zero, labelFontHolder, 0, minHeight: height)
            {
                TextAlignment = labelAlignment,
                //UnitTextOffsetY = 4,
                FontHolder = labelFontHolder,
                IsShortText = false,
            };

            slider = new Slider(OffsetX - 2 + labelWidth, OffsetY,
                holder.UnitWidth - labelWidth - (showValue ? valueWidth : 0), height, "", Color.Zero)
            {
                backColorForSlider = ColorConstant.BackColorForSlider,
                ForeColor = ColorConstant.ForeColor,
                HoverColor = ColorConstant.HoverForeColor,
                HoverForeColor = ColorConstant.HoverForeColor,
                MinValue = min,
                MaxValue = max,
                CurrentValue = initValue,
                UnitSliderWidth = 0.5f,
                UnitWidthControlElement = 4,
                UnitHeightControlElement = UIConfig.UnitSliderObjectHeight,
                Image = Game.Instance.Content.Load<DiscTexture>(imageSlider ?? @"UI-new\fv-icons_slider"),
                IsVertical = IsVertical
            };

            if (showValue)
            {
                ScalableFrame valueFrame = new ScalableFrame(holder.UnitWidth - valueWidth, OffsetY, valueWidth,
                    height, percent ? $"{initValue * 100:0.##}%" : $"{initValue:0.##}", Color.Zero)
                {
                    TextAlignment = valueAlignment,
                    FontHolder = valueFontHolder,
                };
                if (percent)
                {
                    slider.OnChange += f => { valueFrame.Text = $"{f * 100:0.##}%"; };
                }
                else
                {
                    slider.OnChange += f => { valueFrame.Text = $"{f:0.##}"; };
                }
                //slider.OnChange += f => { valueFrame.Text = $"{f:0.##}"; };
                holder.Add(valueFrame);
            }

            slider.OnChange += changeAction;
            holder.Add(labelFrame);
            holder.Add(slider);


            if (minMaxSelector)
            {
                holder.UnitHeight += OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ElementOffset;
                float EditboxWidth = 25;
                Editbox minEdit = new Editbox(OffsetX,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight, EditboxWidth,
                    UIConfig.UnitPalette2LabelHeight, $"{slider.MinValue}", Color.Black)
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                    BorderActive = UIConfig.ActiveColor,
                    HoverColor = Color.White,
                    PaddingLeft = (int) (2 * ApplicationInterface.ScaleMod),
                    //PaddingBottom = (int) (2*AppInterface.ScaleMod),
                    TextAlignment = Alignment.MiddleLeft
                };
                minEdit.ActionOut += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
                {
                    if (args.IsClick) minEdit.SetActiveStatus(false);
                };
                Editbox maxEdit = new Editbox(holder.UnitWidth - OffsetX - EditboxWidth,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight, EditboxWidth,
                    UIConfig.UnitPalette2LabelHeight, $"{slider.MaxValue}", Color.Black)
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                    BorderActive = UIConfig.ActiveColor,
                    HoverColor = Color.White,
                    PaddingLeft = (int) (2 * ApplicationInterface.ScaleMod),
                    //PaddingBottom = (int)(2 * AppInterface.ScaleMod),
                    TextAlignment = Alignment.MiddleLeft
                };
                maxEdit.ActionOut += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
                {
                    if (args.IsClick) maxEdit.SetActiveStatus(false);
                };
                var s = slider;
                Button setMinMaxButton = new Button(OffsetX + EditboxWidth,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight,
                    holder.UnitWidth - 2 * (OffsetX + EditboxWidth), UIConfig.UnitPalette2LabelHeight, "Set",
                    UIConfig.ButtonColor, UIConfig.ActiveColor, 200,
                    () =>
                    {
                        float newMin = float.Parse(minEdit.Text), newMax = float.Parse(maxEdit.Text);
                        s.MinValue = newMin;
                        s.MaxValue = newMax;
                        s.CurrentValue = MathUtil.Clamp(s.MinValue, s.MaxValue, s.CurrentValue);
                        s.OnChange.Invoke(s.CurrentValue);
                    })
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                };
                holder.Add(minEdit);
                holder.Add(maxEdit);
                holder.Add(setMinMaxButton);
            }


            return holder;
        }

        #region Obsoletes
        public static UIContainer<Slider> SliderVerticalHolder(FrameProcessor ui, float OffsetX, float OffsetY,
            ScalableFrame parent, string label,
            Action<float> changeAction, float min, float max, float initValue, out Slider slider, bool minMaxSelector = false, string imageSlider = null, bool IsVertical = false, bool percent = false)
        {
            return SliderVerticalHolder(OffsetX, OffsetY, parent, label, changeAction, min, max, initValue, out slider,
                minMaxSelector, imageSlider, IsVertical, percent);
        }

        public static UIContainer<Slider> SliderVerticalHolderNew(FrameProcessor ui, float OffsetX, float OffsetY, float width, float height,
            ScalableFrame parent, string label,
            Action<float> changeAction, float min, float max, float initValue, out Slider slider,
            bool minMaxSelector = false, string imageSlider = null,
            bool IsVertical = false, bool showValue = true, Alignment labelAlignment = Alignment.TopCenter, bool percent = false, Color? labelBackColor = null)
        {
            return SliderVerticalHolderNew(OffsetX, OffsetY, width, height, parent, label, changeAction, min, max,
                initValue, out slider, minMaxSelector, imageSlider, IsVertical, showValue, labelAlignment, percent,
                labelBackColor);
        }

        public static ScalableFrame SliderHorizontalHolderNew(FrameProcessor ui, float OffsetX, float OffsetY,
            float width, float height,
            ScalableFrame parent, string label, float labelWidth,
            Action<float> changeAction, float min, float max, float initValue, out Slider slider,
            bool minMaxSelector = false, string imageSlider = null,
            bool IsVertical = false, bool showValue = true, float valueWidth = 0,
            Alignment labelAlignment = Alignment.MiddleCenter, Alignment valueAlignment = Alignment.MiddleCenter,
            UIConfig.FontHolder? labelFont = null, UIConfig.FontHolder? valueFont = null, bool percent = false)
        {
            return SliderHorizontalHolderNew(OffsetX, OffsetY, width, height, parent, label, labelWidth,
                 changeAction, min, max, initValue, out slider, minMaxSelector, imageSlider, IsVertical, showValue, valueWidth,
                 labelAlignment, valueAlignment, labelFont, valueFont, percent);
        }
        #endregion
    }
}
