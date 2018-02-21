using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.Graph;
using FusionUI.UI.Elements;

namespace FusionUI.UI.Factories
{
    public class ColorPickerFactory
    {
        public static UIContainer<Slider, Slider, Slider> ColorPickerHolder(FrameProcessor ui, float OffsetX, float OffsetY,
            ScalableFrame parent, string label,
            Action<Color> changeAction, Color initValue, out ScalableFrame colorHolder)
        {
            UIContainer<Slider, Slider, Slider> holder = new UIContainer<Slider, Slider, Slider>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, UIConfig.UnitSliderLabelHeight + UIConfig.UnitSliderObjectHeight*3 + OffsetY, "", Color.Zero);

            ScalableFrame labelFrame = new ScalableFrame(ui, OffsetX, OffsetY, holder.UnitWidth - UIConfig.UnitColorPickerSampleWidth - 2 * OffsetX, UIConfig.UnitCheckboxLabelHeight, label, Color.Zero)
            {
                TextAlignment = Alignment.BaselineLeft,
                UnitTextOffsetY = 4,
                FontHolder = UIConfig.FontBody,
            };            

            var redSlider = new Slider(ui, OffsetX, OffsetY + UIConfig.UnitSliderLabelHeight, holder.UnitWidth - UIConfig.UnitColorPickerSampleWidth - 2 * OffsetX - UIConfig.UnitColorPickerSampleOffset, UIConfig.UnitSliderObjectHeight, "", Color.Zero)
            {
                backColorForSlider = ColorConstant.BackColorForSlider,
                ForeColor = ColorConstant.ForeColor,
                HoverColor = ColorConstant.HoverColor,
                HoverForeColor = Color.Red,
                MinValue = 0,
                MaxValue = 1,
                CurrentValue = initValue.R*(1.0f / 255),
                UnitSliderWidth = 0.5f,
                UnitWidthControlElement = 4,
                UnitHeightControlElement = UIConfig.UnitSliderObjectHeight,
                Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_slider"),
            };

            var greenSlider = new Slider(ui, OffsetX, OffsetY + UIConfig.UnitSliderLabelHeight + UIConfig.UnitSliderObjectHeight, holder.UnitWidth - UIConfig.UnitColorPickerSampleWidth - 2 * OffsetX - UIConfig.UnitColorPickerSampleOffset, UIConfig.UnitSliderObjectHeight, "", Color.Zero)
            {
                backColorForSlider = ColorConstant.BackColorForSlider,
                ForeColor = ColorConstant.ForeColor,
                HoverColor = ColorConstant.HoverColor,
                HoverForeColor = Color.Green,
                MinValue = 0,
                MaxValue = 1,
                CurrentValue = initValue.G * (1.0f/255),
                UnitSliderWidth = 0.5f,
                UnitWidthControlElement = 4,
                UnitHeightControlElement = UIConfig.UnitSliderObjectHeight,
                Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_slider"),
            };
            var blueSlider = new Slider(ui, OffsetX, OffsetY + UIConfig.UnitSliderLabelHeight + UIConfig.UnitSliderObjectHeight * 2, holder.UnitWidth - UIConfig.UnitColorPickerSampleWidth - 2 * OffsetX - UIConfig.UnitColorPickerSampleOffset, UIConfig.UnitSliderObjectHeight, "", Color.Zero)
            {
                backColorForSlider = ColorConstant.BackColorForSlider,
                ForeColor = ColorConstant.ForeColor,
                HoverColor = ColorConstant.HoverColor,
                HoverForeColor = Color.Blue,
                MinValue = 0,
                MaxValue = 1,
                CurrentValue = initValue.B*(1.0f / 255),
                UnitSliderWidth = 0.5f,
                UnitWidthControlElement = 4,
                UnitHeightControlElement = UIConfig.UnitSliderObjectHeight,
                Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_slider"),
            };

            var sample = new ScalableFrame(ui, holder.UnitWidth - UIConfig.UnitColorPickerSampleWidth - 2 * OffsetX, OffsetY + UIConfig.UnitSliderLabelHeight, UIConfig.UnitColorPickerSampleWidth, UIConfig.UnitSliderObjectHeight*3, "", initValue);

            blueSlider.OnChange += f => {
                sample.BackColor = new Color(redSlider.CurrentValue, greenSlider.CurrentValue, blueSlider.CurrentValue);
                changeAction(sample.BackColor);
            };
            greenSlider.OnChange += f => {
                sample.BackColor = new Color(redSlider.CurrentValue, greenSlider.CurrentValue, blueSlider.CurrentValue);
                changeAction(sample.BackColor);
            };
            redSlider.OnChange += f => {
                sample.BackColor = new Color(redSlider.CurrentValue, greenSlider.CurrentValue, blueSlider.CurrentValue);
                changeAction(sample.BackColor);
            };
            holder.Item1 = redSlider;
            holder.Item2 = greenSlider;
            holder.Item3 = blueSlider;
            holder.Add(labelFrame);
            holder.Add(sample);
            holder.Add(redSlider);
            holder.Add(greenSlider);
            holder.Add(blueSlider);
            colorHolder = sample;
            return holder;
        }
    }
}
