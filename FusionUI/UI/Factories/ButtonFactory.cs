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
                UIConfig.UnitFilterWindowElementHeight, okText, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, confirmAction)
            {
                TextAlignment = Alignment.MiddleCenter,
            };
            cancelButton = new Button(ui,
                holder.UnitWidth / 2 + distance/2,
                OffsetY, holder.UnitWidth / 2 - OffsetX - distance/2,
                UIConfig.UnitFilterWindowElementHeight, cancelText, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, cancelAction)
            {
                TextAlignment = Alignment.MiddleCenter,
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
            };
            button2 = new Button(ui,
                OffsetX + bw + distance,
                OffsetY, bw,
                UIConfig.UnitFilterWindowElementHeight, button2Text, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, button2Action)
            {
                TextAlignment = Alignment.MiddleCenter,
            };
            button3 = new Button(ui,
                OffsetX + 2 * bw + 2 * distance,
                OffsetY, bw,
                UIConfig.UnitFilterWindowElementHeight, button3Text, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, button3Action)
            {
                TextAlignment = Alignment.MiddleCenter,
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
                UIConfig.UnitFilterWindowElementHeight, label, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, action)
            {
                TextAlignment = Alignment.MiddleCenter,
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
                UIConfig.UnitFilterWindowElementHeight, label, UIConfig.ButtonColor, UIConfig.ActiveColor, 200, action)
            {
                TextAlignment = Alignment.MiddleCenter,
            };

            holder.Item = button;
            holder.Add(button);
            return holder;
        }

    }
}
