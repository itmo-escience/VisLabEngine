using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Factories;

namespace FusionUI.UI.Elements
{
    public class WindowFixed : FreeFrame
    {

        public Color HatColor { get { return HatPanel.BackColor; } set { HatPanel.BackColor = value; } }

        public ScalableFrame HatPanel;

        public Button Cross;

        public WindowFixed(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor, bool drawCross = true, bool drawHelp = false, string helpText = "") : base(ui, x, y, w, h, text, backColor)
        {
            AutoHeight = true;
            ForeColor = Color.Zero;
            Name = text;
            HatPanel = new ScalableFrame(ui, 0, 0, (int)UnitWidth, UIConfig.UnitHatHeight, Name, UIConfig.HatColor)
            {
                TextAlignment = Alignment.MiddleLeft,
                UnitTextOffsetX = UIConfig.UnitHatTextOffset,
                FontHolder = UIConfig.FontCaptionAlt,
            };
            if (drawCross)
            {
                Cross = new Button(ui, UnitWidth - UIConfig.UnitHatCrossSize - UIConfig.UnitHatTextOffset, 0,
                    UIConfig.UnitHatCrossSize, UIConfig.UnitHatHeight, "", Color.Zero, Color.Zero, 0,
                    () => this.Visible = false)
                {
                    Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_close-window"),
                    ImageMode = FrameImageMode.Cropped,
                };
                HatPanel.Add(Cross);
            }

            if (drawHelp) {                 
                var help = new Button(ui, UnitWidth - 2 * UIConfig.UnitHatCrossSize - UIConfig.UnitHatTextOffset, 0,
                    UIConfig.UnitHatCrossSize, UIConfig.UnitHatHeight, "", Color.Zero, Color.Zero, 0,
                    () => {
                        FullScreenFrame<Window> helpPopup = null;
                        ui.RootFrame.Add(helpPopup = PopupFactory.NotificationPopupWindow(ui,
                            (ui.RootFrame.Width / ScaleMultiplier - UIConfig.UnitPopupWindowWidth) / 2, 100, "Help",
                            helpText, "Got it",
                            () =>
                            {
                                helpPopup.Clear(helpPopup);
                                helpPopup.Clean();
                                ui.RootFrame.Remove(helpPopup);
                            }));
                        helpPopup.Item.BackColor = UIConfig.ConfigColor;
                    })
                {
                    Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_help-window"),
                    ImageMode = FrameImageMode.Cropped,
                };
                HatPanel.Add(help);
            }

            ((Frame)this).Add(HatPanel);
            SuppressActions = true;
        }
    }
}
