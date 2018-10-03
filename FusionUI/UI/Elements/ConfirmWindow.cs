using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements
{
    class ConfirmWindow : Window {

		protected ConfirmWindow()
		{

		}

        public ScalableFrame textMessage;

        public Button OkButton;
        public Button CancelButton;

        private float leftOffsetX = UIConfig.UnitScenarioConfigOffsetX;
        private float leftOffsetY = 6;

        private float minSizeText = UIConfig.UnitPopupWindowTextMinHeight;

        public ConfirmWindow(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor, bool drawCross = true) : base(ui, x, y, w, h, text, backColor, drawCross)
        {
            Border = 1;
        }

        public void init(string message)
        {
            var zoneText = new ScalableFrame(ui, 0, 0, 0, 0, "", Color.Zero)
            {
                Border = 1,
//                AutoHeight = true
            };
            
            textMessage = new RichTextBlock(ui, leftOffsetX, 0, this.UnitWidth - 2 * leftOffsetX, minSizeText, message, Color.Zero, UIConfig.FontBase, 5)
            {
                Border = 1
            };
            zoneText.UnitHeight = textMessage.UnitHeight;
            zoneText.Add(textMessage);
            this.Add(zoneText);
            var zoneButton = new ScalableFrame(ui, 0, 0, this.UnitWidth, UIConfig.UnitScenarioConfigLayersButtonHeight + leftOffsetY * 2, "", Color.Zero )
            {
                Border = 1
            };
            this.Add(zoneButton);
            var widthButton = (UIConfig.UnitPopupWindowWidth - leftOffsetX*2 - UIConfig.UnitBetweenButtonOffsetX)/2;
            OkButton = new Button(ui, leftOffsetX, leftOffsetY, 
                widthButton, UIConfig.UnitScenarioConfigLayersButtonHeight, "OK", UIConfig.ButtonColor, UIConfig.ButtonColor)
            {
                TextAlignment = Alignment.MiddleCenter
            };
            zoneButton.Add(OkButton);

            CancelButton = new Button(ui, leftOffsetX + widthButton + UIConfig.UnitBetweenButtonOffsetX,
                leftOffsetY, widthButton, UIConfig.UnitScenarioConfigLayersButtonHeight, "Cancel", UIConfig.ButtonColor,
                UIConfig.ButtonColor)
            {
                TextAlignment = Alignment.MiddleCenter
            };
            zoneButton.Add(CancelButton);
        }
    }
}
