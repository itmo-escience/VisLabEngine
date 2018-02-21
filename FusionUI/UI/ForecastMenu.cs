using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Elements;
using FusionUI.UI.Factories;

namespace FusionUI.UI
{
    public class ForecastMenu : Window
    {

        private FrameProcessor ui;
     

        private int textOffset = 12;

        private List<Frame> listButton;        

        private DiscTexture iconRadiobuttonOn, iconRadiobuttonOff, iconCheckboxOn, iconCheckboxOff;

        public ForecastMenu(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            this.ui = ui;
            listButton = new List<Frame>();
            iconCheckboxOn = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_checkbox-on");
            iconCheckboxOff = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_checkbox-off");
            iconRadiobuttonOn = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_radio-on");
            iconRadiobuttonOff = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_radio-off");
        }

        public void init(List<string> listNameForecast, Action<string> actionForecast, List<bool> isPartofSelector = null, int activeIndex = 0)
        {
            var i = 0;

            foreach (var child in new List<Frame>(Children))
            {
                this.Remove(child);
            }

            listButton = new List<Frame>();
            for (i = 0; i < listNameForecast.Count; i++)
            {
                if (isPartofSelector != null)
                {
                    CreateAndAddButton(listNameForecast[i], actionForecast, i == activeIndex, isPartofSelector[i]);
                } else
                {
                    CreateAndAddButton(listNameForecast[i], actionForecast, i == activeIndex);
                }
            }
        }

        public ScalableFrame CreateAndAddButton(string text, Action<string> action, bool selected, bool selector = true)
        {
            var button = new ScalableFrame(ui, 0, 0, Width, UIConfig.UnitSettingsButtonHeight, text, Color.Zero)
            {
                UnitTextOffsetX = textOffset,
                UnitImageOffsetX = -UnitWidth / 2 + 6,
                TextAlignment = Alignment.MiddleLeft,
                FontHolder = UIConfig.FontBase,
                BackColor = selected ? UIConfig.ActiveColor : Color.Zero,
            };
            if (selector)
            {
                button.Image = selected ? iconRadiobuttonOn : iconRadiobuttonOff;
                button.ActionClick += (ControlActionArgs args, ref bool flag) =>
                {
                    if (!args.IsClick) return;
                    action(text);
                    {
                        foreach (var b in listButton)
                        {
                            b.BackColor = Color.Zero;
                            b.Image = iconRadiobuttonOff;
                        }
                        button.BackColor = UIConfig.ActiveColor;
                        button.Image = iconRadiobuttonOn;
                    }
                };
            }
            else
            {
                button.Image = selected ? iconCheckboxOn : iconCheckboxOff;
                bool active = false;
                button.ActionClick += (ControlActionArgs args, ref bool flag) =>
                {
                    if (!args.IsClick) return;
                    action(text);
                    if (active)
                    {
                        button.BackColor = Color.Zero;
                        button.Image = iconCheckboxOff;
                    }
                    else
                    {
                        button.BackColor = UIConfig.ActiveColor;
                        button.Image = iconCheckboxOn;
                    }
                    active = !active;
                };
            }
                       
            if (selector)
                listButton.Add(button);
            this.Add(button);
            return button;
        }

        public ScalableFrame CreateAndAddClickButton(string text, Action action)
        {
            var button = new Button(ui, 0, 0, Width, UIConfig.UnitSettingsButtonHeight, text, Color.Zero, UIConfig.ActiveColor, 200, action)
            {
                UnitTextOffsetX = textOffset,
                UnitImageOffsetX = -UnitWidth / 2 + 6,
                TextAlignment = Alignment.MiddleLeft,
                FontHolder = UIConfig.FontBase,                
            };                                
            
            this.Add(button);
            return button;
        }

        public void AddScaleSettings()
        {
            var scaleSettings = new ScalableFrame(ui, 0, 0, UIConfig.UnitConfigPanelWidth, UIConfig.UnitSettingsLabelHeight + UIConfig.UnitSettingsButtonHeight, "", UIConfig.InactiveColor);
            float[] scales = new[] { 0.75f, 1.0f, 1.25f, 1.5f, 2.0f };
            int w = (int)(1.0f * UIConfig.UnitConfigPanelWidth / scales.Length);
            ScalableFrame[] rButtons = new ScalableFrame[scales.Length];
            for (int i = 0; i < scales.Length; i++)
            {
                var holder = new ScalableFrame(ui, i * w, 0, w, UIConfig.UnitSettingsLabelHeight + UIConfig.UnitSettingsButtonHeight, "", UIConfig.InactiveColor);
                float scale = scales[i];
                var label       = new ScalableFrame(ui, 0, 0, w, UIConfig.UnitSettingsLabelHeight, $"{(scales[i]):0.00%}", Color.Zero)
                {
                    TextAlignment = Alignment.MiddleCenter,
                };
                var radioButton = new ScalableFrame(ui, 0, UIConfig.UnitSettingsLabelHeight, w, UIConfig.UnitSettingsButtonHeight, "", Color.Zero)
                {
                    Image = scales[i] == ApplicationInterface.uiScale ? iconRadiobuttonOn : iconRadiobuttonOff,
                };
                holder.Add(label);
                holder.Add(radioButton);
                rButtons[i] = radioButton;
                scaleSettings.Add(holder);
            }

            for (int i = 0; i < scales.Length; i++)
            {
                int t = i;
                rButtons[t].ActionClick += (ControlActionArgs args, ref bool flag) =>
                {
                    if (!args.IsClick) return;
                    foreach (var button in rButtons)
                    {
                        button.Image = iconRadiobuttonOff;
                    }
                    rButtons[t].Image = iconRadiobuttonOn;
                    ApplicationInterface.uiScale = scales[t];
                    ApplicationInterface.Instance.UpdateScale();
                    ApplicationInterface.Instance.rootFrame.UpdateResize();
                };
            }
            this.Add(scaleSettings);
        }

        public void AddBottomSlider()
        {
            Slider s;
            Add(SliderFactory.SliderVerticalHolder(ui, 0, 0, this, "Max window pos", f => UIConfig.UnitTopmostWindowPosition = f * ApplicationInterface.Instance.rootFrame.UnitHeight, 0, 1, 270.0f / ApplicationInterface.Instance.rootFrame.UnitHeight, out s, false));
            s.ActionUpdate = time =>
            {
                s.CurrentValue = (UIConfig.UnitTopmostWindowPosition / ApplicationInterface.Instance.rootFrame.UnitHeight);
            };
        }
    }
}
