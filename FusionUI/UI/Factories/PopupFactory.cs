using Fusion.Engine.Frames;
using System;
using FusionUI.UI.Elements;

namespace FusionUI.UI.Factories
{
    public class PopupFactory
    {
        public static FullScreenFrame<Window> NotificationPopupWindow(FrameProcessor ui, string label, string text, string okText, Action okAction, bool blockEverythingElse = true)
        {
            var RootFrame = ui.RootFrame;
            var ScaleMultiplier = ScalableFrame.ScaleMultiplier;
            return NotificationPopupWindow(ui, (RootFrame.Width / ScaleMultiplier - UIConfig.UnitPopupWindowWidth) / 2,
                100, label, text, okText, okAction, blockEverythingElse);
        }

        public static FullScreenFrame<Window> NotificationPopupWindow(FrameProcessor ui, float x, float y, string label, string text, string buttonText, Action buttonAction, bool blockEverythingElse = true)
        {
            var RootFrame = ApplicationInterface.Instance.rootFrame;
            var holder = new FullScreenFrame<Window>(ui)
            {
                ZOrder = 10000,
            };
            holder.SuppressActions = blockEverythingElse;
            Window popupWindow = new Window(ui, x, y, UIConfig.UnitPopupWindowWidth, 0, "", UIConfig.PopupColor, false);

            var caption = TextBlockFactory.TextBlockHolder(ui, UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow,
                UIConfig.FontSubtitle, UIConfig.UnitPopupWindowCaptionMinHeight, label);

            var tb = TextBlockFactory.TextBlockHolder(ui, UIConfig.UnitPopupWindowOffsetX, 0, popupWindow,
                UIConfig.FontBody, UIConfig.UnitPopupWindowTextMinHeight, text);
            popupWindow.Add(caption);
            popupWindow.Add(tb);
            
            Button button;
            popupWindow.Add(ButtonFactory.CenterButtonHolder(ui, 0, 0, popupWindow, UIConfig.UnitPopupWindowButtonWidth, buttonText, buttonAction, out button));
            button.ButtonAction += b => holder.Visible = false;
            holder.Add(popupWindow);
            holder.Item = popupWindow;
            return holder;
        }

        public static FullScreenFrame<Window> ConfirmationPopupWindow(FrameProcessor ui, string label, string text, string okText, string cancelText, Action okAction, Action cancelAction, bool blockEverythingElse = true)
        {
            var RootFrame = ui.RootFrame;
            var ScaleMultiplier = ScalableFrame.ScaleMultiplier;
            return ConfirmationPopupWindow(ui, (RootFrame.Width / ScaleMultiplier - UIConfig.UnitPopupWindowWidth) / 2,
                100, label, text, okText, cancelText, okAction, cancelAction, blockEverythingElse);
        }

        public static FullScreenFrame<Window> ConfirmationPopupWindow(FrameProcessor ui, float x, float y, string label, string text, string okText, string cancelText, Action okAction, Action cancelAction, bool blockEverythingElse = true)
        {
            var RootFrame = ApplicationInterface.Instance.rootFrame;
            var holder = new FullScreenFrame<Window>(ui)
            {
                ZOrder = 10000,                
            };
            holder.SuppressActions = blockEverythingElse;
            Window popupWindow = new Window(ui, x, y, UIConfig.UnitPopupWindowWidth, 0, "", UIConfig.PopupColor, false);

            var caption = TextBlockFactory.TextBlockHolder(ui, UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow,
                UIConfig.FontSubtitle, UIConfig.UnitPopupWindowCaptionMinHeight, label);                

            var tb = TextBlockFactory.TextBlockHolder(ui, UIConfig.UnitPopupWindowOffsetX, 0, popupWindow,
                UIConfig.FontBody, UIConfig.UnitPopupWindowTextMinHeight, text);

            popupWindow.Add(caption);
            popupWindow.Add(tb);
            Button okButton, cancelButton;
            popupWindow.Add(ButtonFactory.PopupOKCancelButtonsHolder(ui, UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow, 1, okAction, cancelAction, out okButton, out cancelButton));
            okButton.ButtonAction += b => holder.Visible = false;
            cancelButton.ButtonAction += b => holder.Visible = false;
            holder.Add(popupWindow);
            holder.Item = popupWindow;
            return holder;
        }

        public static FullScreenFrame<Window> RenamePopupWindow(FrameProcessor ui, string label, string text, string okText, string cancelText, Action<string> okAction, Action cancelAction, bool blockEverythingElse = true)
        {
            var RootFrame = ui.RootFrame;
            var ScaleMultiplier = ScalableFrame.ScaleMultiplier;
            return RenamePopupWindow(ui, (RootFrame.Width / ScaleMultiplier - UIConfig.UnitPopupWindowWidth) / 2,
                100, label, text, okText, cancelText, okAction, cancelAction, blockEverythingElse);
        }

        public static FullScreenFrame<Window> RenamePopupWindow(FrameProcessor ui, float x, float y, string label, string text, string okText, string cancelText, Action<string> okAction, Action cancelAction, bool blockEverythingElse = true)
        {
            var RootFrame = ApplicationInterface.Instance.rootFrame;
            var holder = new FullScreenFrame<Window>(ui)
            {
                ZOrder = 10000,
            };
            holder.SuppressActions = blockEverythingElse;
            Window popupWindow = new Window(ui, x, y, UIConfig.UnitPopupWindowWidth, 0, "", UIConfig.PopupColor, false);

            var caption = TextBlockFactory.TextBlockHolder(ui, UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow,
                UIConfig.FontSubtitle, UIConfig.UnitPopupWindowCaptionMinHeight, label);

            //var tb = TextBlockFactory.TextBlockHolder(ui, UIConfig.UnitPopupWindowOffsetX, 0, popupWindow,
            //    UIConfig.FontBody, UIConfig.UnitPopupWindowTextMinHeight, text);
            ScalableFrame tb;
            Editbox box;
            var RenameBox = EditboxFactory.EditboxHolder(ui, UIConfig.UnitPopupWindowOffsetX, 0, popupWindow, text,
                out box, out tb);

            popupWindow.Add(caption);
            popupWindow.Add(RenameBox);
            Button okButton, cancelButton;
            popupWindow.Add(ButtonFactory.PopupOKCancelButtonsHolder(ui, UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow, 1, () => okAction(box.Text), cancelAction, out okButton, out cancelButton));
            okButton.ButtonAction += b => holder.Visible = false;
            cancelButton.ButtonAction += b => holder.Visible = false;
            holder.Add(popupWindow);
            holder.Item = popupWindow;
            return holder;
        }


        public static FullScreenFrame<Window> ThreeButtonPopupWindow(FrameProcessor ui, string label, string text, Action Action1, Action Action2, Action Action3, string button1Text = "Yes", string button2Text = "No", string button3Text = "Cancel", bool blockEverythingElse = true)
        {
            var RootFrame = ui.RootFrame;
            var ScaleMultiplier = ScalableFrame.ScaleMultiplier;
            return ThreeButtonPopupWindow(ui, (RootFrame.Width / ScaleMultiplier - UIConfig.UnitPopupWindowWidth) / 2,
                100, label, text, Action1, Action2, Action3, button1Text, button2Text, button3Text, blockEverythingElse);
        }

        public static FullScreenFrame<Window> ThreeButtonPopupWindow(FrameProcessor ui, float x, float y, string label, string text, Action action1, Action action2, Action action3, string button1Text = "Yes", string button2Text = "No", string button3Text = "Cancel", bool blockEverythingElse = true)
        {
            var RootFrame = ApplicationInterface.Instance.rootFrame;
            var holder = new FullScreenFrame<Window>(ui)
            {
                ZOrder = 10000,
            };
            holder.SuppressActions = blockEverythingElse;
            Window popupWindow = new Window(ui, x, y, UIConfig.UnitPopupWindowWidth, 0, "", UIConfig.PopupColor, false);

            var caption = TextBlockFactory.TextBlockHolder(ui, UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow,
                UIConfig.FontSubtitle, UIConfig.UnitPopupWindowCaptionMinHeight, label);

            var tb = TextBlockFactory.TextBlockHolder(ui, UIConfig.UnitPopupWindowOffsetX, 0, popupWindow,
                UIConfig.FontBody, UIConfig.UnitPopupWindowTextMinHeight, text);

            popupWindow.Add(caption);
            popupWindow.Add(tb);
            Button yesButton, noButton, cancelButton;
            popupWindow.Add(ButtonFactory.PopupThreeButtonsHolder(ui, UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow, 1, action1, action2, action3, out yesButton, out noButton, out cancelButton, button1Text, button2Text, button3Text));
            yesButton.ButtonAction += b => holder.Visible = false;
            noButton.ButtonAction += b => holder.Visible = false;
            cancelButton.ButtonAction += b => holder.Visible = false;
            holder.Add(popupWindow);
            holder.Item = popupWindow;
            return holder;
        }
    }
}
