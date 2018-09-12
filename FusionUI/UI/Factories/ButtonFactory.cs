using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using FusionUI.UI.Elements;

namespace FusionUI.UI.Factories
{
    public class ButtonFactory
    {
        public static UIContainer<Button, Button> PopupOKCancelButtonsHolder(FrameProcessor ui, float OffsetX, float OffsetY, ScalableFrame parent, float distance, Action confirmAction, Action cancelAction, out Button OkButton, out Button cancelButton, string okText = "Ok", string cancelText = "Cancel")
        {
            UIContainer<Button, Button> holder = new UIContainer<Button, Button>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, UIConfig.UnitFilterWindowConfirmationButtonRowHeight + OffsetY, "", Color.Zero);

            OkButton = new Button(ui, OffsetX,
                OffsetY, holder.UnitWidth / 2 - OffsetX - distance/2,
                UIConfig.UnitFilterWindowElementHeight, okText, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, confirmAction, UIConfig.ActiveTextColor, UIConfig.ActiveTextColor)
            {
                TextAlignment = Alignment.MiddleCenter,
                ForeColor = UIConfig.ActiveTextColor,
            };
            cancelButton = new Button(ui,
                holder.UnitWidth / 2 + distance/2,
                OffsetY, holder.UnitWidth / 2 - OffsetX - distance/2,
                UIConfig.UnitFilterWindowElementHeight, cancelText, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, cancelAction, UIConfig.ActiveTextColor, UIConfig.ActiveTextColor)
            {
                TextAlignment = Alignment.MiddleCenter,
                ForeColor = UIConfig.ActiveTextColor,
            };

            holder.Item1 = OkButton;
            holder.Item2 = cancelButton;
            holder.Add(OkButton);
            holder.Add(cancelButton);
            return holder;
        }

        public static UIContainer<Button, Button, Button> PopupThreeButtonsHolder(FrameProcessor ui, float OffsetX, float OffsetY, ScalableFrame parent, float distance, Action button1Action, Action button2Action, Action button3Action, out Button button1, out Button button2, out Button button3, string button1Text = "Yes", string button2Text = "No", string button3Text = "Cancel")
        {
            UIContainer<Button, Button, Button> holder = new UIContainer<Button, Button, Button>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, UIConfig.UnitFilterWindowConfirmationButtonRowHeight + OffsetY, "", Color.Zero);

            float bw = (holder.UnitWidth - 2 * OffsetX - distance * 2) / 3;
            button1 = new Button(ui, OffsetX,
                OffsetY, bw,
                UIConfig.UnitFilterWindowElementHeight, button1Text, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, button1Action)
            {
                TextAlignment = Alignment.MiddleCenter,
                ForeColor = UIConfig.ActiveTextColor,
            };
            button2 = new Button(ui,
                OffsetX + bw + distance,
                OffsetY, bw,
                UIConfig.UnitFilterWindowElementHeight, button2Text, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, button2Action)
            {
                TextAlignment = Alignment.MiddleCenter,
                ForeColor = UIConfig.ActiveTextColor,
            };
            button3 = new Button(ui,
                OffsetX + 2 * bw + 2 * distance,
                OffsetY, bw,
                UIConfig.UnitFilterWindowElementHeight, button3Text, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, button3Action)
            {
                TextAlignment = Alignment.MiddleCenter,
                ForeColor = UIConfig.ActiveTextColor,
            };

            holder.Item1 = button1;
            holder.Item2 = button2;
            holder.Item3 = button3;
            holder.Add(button1);
            holder.Add(button2);
            holder.Add(button3);
            return holder;
        }

        public static UIContainer<Button> RightAlignButtonHolder(FrameProcessor ui, float OffsetX, float OffsetY, ScalableFrame parent, float buttonWidth, string label, Action action, out Button button)
        {
            UIContainer<Button> holder = new UIContainer<Button>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, UIConfig.UnitFilterWindowButtonRowHeight + OffsetY, "", Color.Zero)
            {                
            };

            button = new Button(ui,
                holder.UnitWidth - OffsetX - buttonWidth,
                OffsetY, buttonWidth,
                UIConfig.UnitFilterWindowElementHeight, label, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, action, UIConfig.ActiveTextColor, UIConfig.ActiveTextColor)
            {
                TextAlignment = Alignment.MiddleCenter,
                ForeColor = UIConfig.ActiveTextColor,
                
            };

            holder.Item = button;
            holder.Add(button);            
            return holder;
        }

        public static UIContainer<Button> CenterButtonHolder(FrameProcessor ui, float OffsetX, float OffsetY, ScalableFrame parent, float buttonWidth, string label, Action action, out Button button)
        {
            UIContainer<Button> holder = new UIContainer<Button>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, UIConfig.UnitFilterWindowConfirmationButtonRowHeight, "", Color.Zero)
            {
            };

            button = new Button(ui,
                holder.UnitWidth / 2 - buttonWidth / 2,
                OffsetY, buttonWidth,
                UIConfig.UnitFilterWindowElementHeight, label, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, action, UIConfig.ActiveTextColor, UIConfig.ActiveTextColor)
            {
                TextAlignment = Alignment.MiddleCenter,
                ForeColor = UIConfig.ActiveTextColor,
            };

            holder.Item = button;
            holder.Add(button);
            return holder;
        }

        public static UIContainer<Button, RichTextBlock, RichTextBlock> СoordinatesButtonHolder(FrameProcessor ui,
            float OffsetX, float OffsetY, ScalableFrame parent,
            float distance, Action button1Action, out Button buttonXY, out RichTextBlock textBlockX,
            out RichTextBlock textBlockY, string buttonText = "coordinates", string richTextX= "0",
            string richTextY = "0")
        {
            UIContainer<Button, RichTextBlock, RichTextBlock> holder = new UIContainer<Button, RichTextBlock, RichTextBlock>(ui, parent.UnitPaddingLeft, 0, 
                parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, 
                UIConfig.UnitFilterWindowConfirmationButtonRowHeight + OffsetY, 
                "", Color.Zero);
            float bw = (holder.UnitWidth - 2 * OffsetX - distance * 2) / 3;
            //buttonXY = new Button(ui,
            //    OffsetX, OffsetY,
            //    bw, UIConfig.UnitFilterWindowElementHeight,
            //    buttonText, UIConfig.ButtonColor, UIConfig.ActiveColor, 10, button1Action, Color.White, Color.White)
            //{
            //    TextAlignment = Alignment.MiddleCenter,
            //    ForeColor = Color.White,
            //};
            buttonXY = new Button(ui,
                OffsetX, OffsetY,
                bw, UIConfig.UnitFilterWindowElementHeight,
                buttonText, UIConfig.ActiveColor, UIConfig.ButtonColor, null, null, null, false, 0)
            {
                TextAlignment = Alignment.MiddleCenter
            };
            textBlockX = new RichTextBlock(ui,
                OffsetX + bw + distance, OffsetY, 
                bw, UIConfig.UnitFilterWindowElementHeight,
                richTextX, Color.Zero, UIConfig.FontBody, 10);
            textBlockY = new RichTextBlock(ui,
                OffsetX + 2 * bw + 2 * distance, OffsetY,
                bw, UIConfig.UnitFilterWindowElementHeight,
                richTextY, Color.Zero, UIConfig.FontBody, 10);
            holder.Item1 = buttonXY;
            holder.Item2 = textBlockX;
            holder.Item3 = textBlockY;
            holder.Add(buttonXY);
            holder.Add(textBlockX);
            holder.Add(textBlockY);
            return holder;
        }

    }
}
