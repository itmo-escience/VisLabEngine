using System.Collections.Generic;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Color = Fusion.Core.Mathematics.Color;

namespace FusionUI.UI
{
    public class UIConfig
    {
        public static string WindowName { get; set; } = "FloodVision 3";

        public static float UnitTopmostWindowPosition = 270f;

        public static float UnitHatTextOffset = 3;
        public static float UnitHatHeight = 8;
        public static float UnitHatCrossSize = 8;

        public static float UnitMenuButtonWidth = 12;

        public static float UnitMenuButtonHeight = 12;
        //public static float UnitMenuSettingsButtonWidth = 17;
        //public static float UnitMenuSettingsButtonHeight = 15;
        //public static float UnitMenuOffsetX = 12;
        //public static float UnitMenuOffsetY = 0;
        //public static float UnitMenuWidth = 80;

        //public static float UnitConfigRowHeight = 12;
        //public static float UnitConfigSelectorWidth = 14;
        //public static float UnitConfigSliderWidth = 60;

        public static float UnitSettingsPanelOffsetX = 50;
        public static float UnitSettingsPanelOffsetY = 50;
        public static float UnitSettingsPanelWidth = 80;
        public static float UnitSettingsPanelHeight = 100;
        public static float UnitSettingsLabelHeight = 6;
        public static float UnitSettingsButtonHeight = 8;
        public static float UnitSettingsPanelSizeCheckbox = 12;
        public static float UnitSettingsPanelHeightDropDown = 12;
        public static float UnitSettingsPanelWidthDropDown = 25;

        //public static float UnitConfigPanelOffsetX = 50;
        //public static float UnitConfigPanelOffsetY = 50;
        public static float UnitConfigPanelWidth = 80;

        public static float UnitConfigPanelHeight = 100;

        //public static float UnitPlotPointSelectorWidth = 12;
        //public static float UnitPlotHorizontalOffset = 10;
        public static float UnitPlotVerticalOffset = 6;

        //public static float UnitPlotAngleDistance = 16;
        //public static float UnitPlotWindowSizeX = 150;
        //public static float UnitPlotWindowSizeY = 104;
        public static float UnitPlotLineWidth = 1.25f;

        //public static float UnitPlotTreePanelWidth = 64;
        public static float UnitPlotNumbersHeight = 4;

        //public static float PlotWindow1DPlotPart = 0.60f;
        //public static float PlotWindowXOffset = 12;
        //public static float PlotWindowSeparatorWidth = 8;
        public static int PlotWindowMinPlotWidth = 32;

        public static int PlotWindowMinPlotHeight = 24;
        public static int PlotWindowMinHeight = 120;


        public static float UnitDefaultTapWidth = 25;
        public static float UnitDefaultTapHeight = 8;

        public static float UnitTimelineHeight = 12;
        public static float UnitTimelineWidth = 198;
        public static float UnitTimelineWindowHeight = 26;
        public static float UnitTimelineWindowWidth = 200;
        public static float UnitTimelinePanelOffsetX = 25;
        public static float UnitTimelineOffsetX = 6;
        public static float UnitTimelineLabelWidth = 25;
        public static float UnitTimelineLabelOffset = 8;
        public static float UnitTimelineTickness = 0.5f;

        public static float UnitPaletteWidth = 24;
        public static float UnitPaletteHeight = 52;
        public static float UnitPaletteLabelHeight = 8;
        public static float UnitPaletteNumbersHeight = 10;
        public static float UnitPaletteScrollerHeight = 6;
        public static float UnitPaletteButtonHeight = 7;
        public static float UnitPaletteButtonWidth = 11;
        public static float UnitPaletteScrollWidth = 12;
        public static float UnitPaletteSeparatorWidth = 8;
        public static float UnitPaletteSeparatorHeight = 0.25f;

        public static float UnitFilterWindowWidth = 117;
        public static float UnitFilterWindowSelectorRowHeight = 11;
        public static float UnitFilterWindowButtonRowHeight = 11;
        public static float UnitFilterWindowElementHeight = 8;
        public static float UnitFilterWindowElementOffsetX = 6;
        public static float UnitFilterWindowElementOffsetY = 3;
        public static float UnitFilterWindowLabelWidth = 31;
        public static float UnitFilterWindowCaptionHeight = 18;
        public static float UnitFilterWindowCaptionButtonWidth = 18;
        public static float UnitFilterWindowSingleButtonWidth = 30;
        public static float UnitFilterWindowConfirmationButtonWidth = 52;
        public static float UnitFilterWindowConfirmationButtonRowHeight = 14;

        public static float UnitSelectorArrowButtonWidth = 8;
        public static float UnitSelectorHeight = 8;
        public static float UnitSelectorRowHeight = 11;
        public static float UnitSelectorRowOffset = 2;

        public static Color ActiveColor = new Color(0, 120, 215, 205);
        public static Color InactiveColor = new Color(0, 0, 0, 155);
        public static Color ActiveTextColor = new Color(255, 255, 255, 255);
        public static Color InactiveTextColor = new Color(255, 255, 255, 150);
        public static Color HighlightTextColor = new Color(255, 255, 255, 255);
        public static Color HatColor = new Color(0, 0, 0, 255);
        public static Color SettingsColor = new Color(30, 30, 30, 255);
        public static Color ButtonColor = new Color(255, 255, 255, 50);
        public static Color BorderColor = new Color(255, 255, 255, 100);
        public static Color PopupColor = new Color(40, 40, 40, 255);
        public static Color TooltipColor = new Color(40, 40, 40, 255);
        public static Color BackColor = new Color(0, 0, 0, 220);
        public static Color ConfigColor = new Color(0, 0, 0, 190);
        public static Color NodeColor = new Color(0, 0, 0, 190);
        public static Color BackColorLayer = new Color(255, 255, 255, 30);

        public static Color TimeLineColor1 = new Color(0, 120, 215, 205);
        public static Color TimeLineColor2 = new Color(255, 255, 255, 205);

        public static Color TimeLineIconColor = new Color(255, 255, 255, 255);


        public static float UnitCheckboxLabelHeight = 6;
        public static float UnitCheckboxWidth = 12;
        public static float UnitSwitcherWidth = 11;
        public static float UnitSwitcherHeight = 8;
        public static float UnitCheckboxHeight = 10;
        public static float UnitCheckboxValueOffset = 3;

        public static float UnitSliderLabelHeight = 5;
        public static float UnitSliderObjectHeight = 11;

        public static float UnitColorPickerSampleWidth = 8;
        public static float UnitColorPickerSampleOffset = 4;

        public static float UnitRadioButtonCheckHeight = 8;
        public static float UnitRadioButtonCheckWidth = 5;
        public static float UnitRadioButtonLabelOffsetX = 2;
        public static float UnitRadioButtonGroupCaptionHeight = 5;
        public static float UnitRadioButtonGroupElementOffset = 3;

        public static float UnitEditboxLabelHeight = 5;
        public static float UnitEditboxElementOffset = 2;
        public static float UnitEditboxElementHeight = 8;
        public static float UnitEditboxIconWidth = 7;
        public static float UnitEditboxIconOffset = 0.5f;

        public static float UnitProgressbarTickness = 1;
        public static float UnitProgressbarLabelHeight = 5;
        public static float UnitProgressbarElementOffset = 2;

        public static float UnitPalette2LabelHeight = 5;
        public static float UnitPalette2ElementOffset = 3;
        public static float UnitPalette2ElementHeight = 6;
        public static float UnitPalette2ButtonWidth = 12;
        public static float UnitPalette2ButtonHeight = 12;
        public static float UnitPalette2ScrollWidth = 4;
        public static float UnitPalette2ScrollHeight = 12;

        //        public static float UnitScenarioConfigOffsetX = 24;
        //        public static float UnitScenarioConfigOffsetY = 24;
        public static float UnitScenarioConfigWidth = 210; 
        public static float UnitScenarioConfigHeight = 270; 
        public static float UnitScenarioConfigOffsetX = 6;
        public static float UnitScenarioConfigInnerOffsetX = 2;
        public static float UnitScenarioConfigOffsetY = 3;
        public static float UnitScenarioConfigTextOffsetX = 3;
        public static float UnitScenarioConfigTitelWidth = 162;
        public static float UnitScenarioConfigTitelHeight = 12;
        public static float UnitScenarioConfigTitelButtonSize = 12;
        public static float UnitScenarioConfigLayersButtonWidth = 30;
        public static float UnitScenarioConfigLayersButtonHeight = 8;
        public static float UnitScenarioConfigLayersWidth = 198;
        public static float UnitScenarioConfigLayersHeight = 13;
        public static float UnitScenarioConfigCheckboxSize = 6;
        public static float UnitScenarioConfigMetadataWidth = 69;
        public static float UnitScenarioConfigMetadataHeight = 30;
        public static float UnitScenarioConfigDrawPropWidth = 129;
        public static float UnitScenarioConfigDrawPropHeight = 30;
        public static float UnitScenarioConfigProgressWidth = 1;
        public static float UnitScenarioConfigTimelineHeight = 33;

        public static float UnitScenarioAdderWidth = 91;
        public static float UnitScenarioAdderHeight = 116;
        public static float UnitScenarioAdderOffsetX = 2;

        public static float UnitBetweenButtonOffsetX = 1;

        public static float UnitScenarioAdderRadioButtonHeight = 8;
        public static float UnitScenarioAdderBigTitle = 15;
        public static float UnitScenarioAdderSmallTitle = 3;
        public static float UnitScenarioAdderEditboxHeight = 8;

        public static float UnitPopupWindowCaptionMinHeight = 9;
        public static float UnitPopupWindowTextMinHeight = 9;
        public static float UnitPopupWindowWidth = 93;
        public static float UnitPopupWindowButtonWidth = 39;
        public static float UnitPopupWindowOffsetX = 6;
        public static float UnitPopupWindowOffsetY = 6;

        public static float UnitScenarioSwitchPanelWidth = 104;
        public static float UnitScenarioSwitchPanelCaptionHeight = 15;
        public static float UnitScenarioSwitchPanelCaptionOffset = 10;
        public static float UnitScenarioSwitchPanelHPadding = 6;
        public static float UnitScenarioSwitchPanelElementTextWidth = 38;
        public static float UnitScenarioSwitchPanelElementHeight = 17;
        public static float UnitScenarioSwitchPanelElementOffset = 3;
        public static float UnitScenarioSwitchPanelButtonSize = 12;

        public static float UnitTabTextOffsetX = 3;

        public static float UnitLegendWidth = 54;
        public static float UnitLegendElementHeight = 7;

        public struct FontHolder
        {
            //private SpriteFont font75, font100, font125, font150, font200;
            public string FontName;

            private int lastIndex;
            public SpriteFont Current => currentFont??this[1];
            private SpriteFont currentFont;
            public SpriteFont this[float scale]
            {
                get
                {
                    int index;
                    if (scale < 0.87)
                    {
                        index = 0;
                    }
                    else if (scale < 1.12)
                    {
                        index = 1;
                    }
                    else if (scale < 1.37)
                    {
                        index = 2;
                    }
                    else if (scale < 1.75)
                    {
                        index = 3;
                    }
                    else
                    {
                        index = 4;
                    }
                    if (index != lastIndex)
                    {
                        switch (index)
                        {
                            case 0:
                                currentFont = Game.Instance.Content.Load<SpriteFont>($"{FontName}-75");
                                break;
                            case 1:
                                currentFont = Game.Instance.Content.Load<SpriteFont>($"{FontName}");
                                break;
                            case 2:
                                currentFont = Game.Instance.Content.Load<SpriteFont>($"{FontName}-125");
                                break;
                            case 3:
                                currentFont = Game.Instance.Content.Load<SpriteFont>($"{FontName}-150");
                                break;
                            case 4:
                                currentFont = Game.Instance.Content.Load<SpriteFont>($"{FontName}-200");
                                break;
                            default:
                                currentFont = Game.Instance.Content.Load<SpriteFont>($"{FontName}");
                                break;
                        }
                    }
                    lastIndex = index;
                    return currentFont;
                }
            }

            public FontHolder(string name)
            {
                FontName = name;
                lastIndex = -1;
                currentFont = null;
            }
        }

        public static FontHolder FontHeader = new FontHolder(@"fonts\new\HeaderNew");
        public static FontHolder FontSubheader = new FontHolder(@"fonts\new\Subheader");
        public static FontHolder FontTitle = new FontHolder(@"fonts\new\Title");
        public static FontHolder FontSubtitle = new FontHolder(@"fonts\new\Subtitle");
        public static FontHolder FontBase = new FontHolder(@"fonts\new\Base");
        public static FontHolder FontBody = new FontHolder(@"fonts\new\Body");
        public static FontHolder FontCaption = new FontHolder(@"fonts\new\Caption");

        public static FontHolder FontSubtitleAlt = new FontHolder(@"fonts\new\SubtitleAlt");
        public static FontHolder FontBaseAlt = new FontHolder(@"fonts\new\BaseAlt");
        public static FontHolder FontCaptionAlt = new FontHolder(@"fonts\new\CaptionAlt");

		public static FontHolder MullerExtraBold = new FontHolder(@"fonts\Muller\MullerExtraBold");
		public static FontHolder MullerLight = new FontHolder(@"fonts\Muller\MullerLight");

		public static List<string> ListPalettes = new List<string>
        {
            "pallete.tga",
            "pallete_atmo_pressure.png",
            "IcePallete.png",
            "palette_fromPanoply.png",
        };

        public static string CustomPalettePath = @"Palettes\";

        public static List<string> ListBackgrounds = new List<string>
        {
            "Bing Satellite",
            "Bing Map",
            "Yandex Map",
            "OpenStreetMap",          
        };

        public static float UnitPlotScaleWidth = 10;
        public static float UnitPlotScaleHeight = 10;
        public static float UnitPlotLegendOffset = 2;
        public static float UnitPlotScaleLineWidth = 0.5f;

        public static float ScrollBarWidth = 1.0f;

        public static Color PlotScaleColor = new Color(1, 1, 1, 0.2f);
        public static Color PlotLineColor = new Color(1, 1, 1, 1f);
    }
}
