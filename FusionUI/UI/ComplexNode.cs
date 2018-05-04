using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using FusionUI.UI.Elements;
using FusionUI.UI.Elements.DropDown;

namespace FusionUI.UI
{
    public class ComplexNode : TreeNode
    {
        private FrameProcessor ui;

        public Checkbox Checkbox;
        public DropDownSelector<DropDownSelectorTextRow> DropDown;
        public ScalableFrame Button;
        

        public ComplexNode(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            this.ui = ui;
            TextAlignment = Alignment.MiddleLeft;
            UnitTextOffsetX = UIConfig.UnitSettingsPanelSizeCheckbox*2;
            UnitImageOffsetX = this.UnitWidth - UIConfig.UnitSettingsPanelSizeCheckbox;
            Border = 1;
        }

        public void initDropDownNode(List<string> listValues, string valueDrop, bool valueCheckbox, Action<string> selectAction)
        {            
            Checkbox = new Checkbox(ui, UIConfig.UnitSettingsPanelSizeCheckbox/2, this.UnitHeight / 2 - UIConfig.UnitSettingsPanelSizeCheckbox / 2, UIConfig.UnitSettingsPanelSizeCheckbox, UIConfig.UnitSettingsPanelSizeCheckbox, "", Color.Zero)
            {
                IsChecked = valueCheckbox,
            };
            Add(Checkbox);
            if (listValues != null)
            {
                DropDown = new DropDownSelector<DropDownSelectorTextRow>(ui, (int)(UnitWidth - UIConfig.UnitSettingsPanelWidthDropDown - UIConfig.UnitSettingsPanelSizeCheckbox*2), this.UnitHeight/2 - UIConfig.UnitSettingsPanelHeightDropDown/2, UIConfig.UnitSettingsPanelWidthDropDown,
                    UIConfig.UnitSettingsPanelHeightDropDown, UIConfig.ButtonColor, listValues, selectAction, UIConfig.BorderColor)
                {                    
                    Border = 1
                };
                Add(DropDown);
            }
        }

        public void initButtonNode(string buttonName, bool valueCheckbox)
        {
            Checkbox = new Checkbox(ui, 0, 0, UIConfig.UnitSettingsPanelSizeCheckbox, UIConfig.UnitSettingsPanelSizeCheckbox, "", Color.Zero)
            {
                IsChecked = valueCheckbox,
            };
            Add(Checkbox);
            if (buttonName != null)
            {
                Button = new ScalableFrame(ui, UnitWidth - UIConfig.UnitSettingsPanelHeightDropDown * 2, 0, UIConfig.UnitSettingsPanelHeightDropDown,
                    UIConfig.UnitSettingsPanelHeightDropDown, buttonName, Color.Zero)
                {
                    Image = ui.Game.Content.Load<Fusion.Engine.Graphics.DiscTexture>("ui-new/fv-icons_ensembles"),
                    ForeColor = UIConfig.ActiveTextColor,
                };
                Add(Button);
                bool clicked = false;
                Color activeColor = new Color(0, 120, 215, 205);
                Button.ActionClick += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
                        {
                            clicked = !clicked;
                            if (clicked)
                            {
                                Button.BackColor = activeColor;
                            }
                            else
                            {
                                Button.BackColor = Color.Zero;
                            }
                            flag |= true;
                        };
            }
        }

        public void initSimpleButton(int xUnit, int yUnit, string image, Action action)
        {
            var Button = new Button(ui, xUnit, yUnit, UIConfig.UnitSettingsPanelHeightDropDown,
                UIConfig.UnitSettingsPanelHeightDropDown, "", UIConfig.ActiveColor, UIConfig.InactiveColor)
            {
                Image = ui.Game.Content.Load<Fusion.Engine.Graphics.DiscTexture>(image),
                ForeColor = UIConfig.ActiveTextColor,
            };
            Button.ButtonAction += b => {
                action();
            };
            Add(Button);
        }
    }
}
