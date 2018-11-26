using Fusion.Engine.Frames;
using System;
using FusionUI.UI.Elements;

namespace FusionUI.UI.Factories
{
    public class PopupFactory
    {
        public static FullScreenFrame<Window> NotificationPopupWindow(string label, string text, string okText, Action okAction, bool blockEverythingElse = true)
        {
            var rootFrame = ApplicationInterface.Instance.rootFrame;
            var scaleMultiplier = ScalableFrame.ScaleMultiplier;
            return NotificationPopupWindow((rootFrame.Width / scaleMultiplier - UIConfig.UnitPopupWindowWidth) / 2,
                100, label, text, okText, okAction, blockEverythingElse);
        }

        public static FullScreenFrame<Window> NotificationPopupWindow(float x, float y, string label, string text, string buttonText, Action buttonAction, bool blockEverythingElse = true)
        {
            var holder = new FullScreenFrame<Window>()
            {
                ZOrder = 10000,
            };
            holder.SuppressActions = blockEverythingElse;
            Window popupWindow = new Window(x, y, UIConfig.UnitPopupWindowWidth, 0, "", UIConfig.PopupColor, false);

            var caption = TextBlockFactory.TextBlockHolder(UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow,
                UIConfig.FontSubtitle, UIConfig.UnitPopupWindowCaptionMinHeight, label);

            var tb = TextBlockFactory.TextBlockHolder(UIConfig.UnitPopupWindowOffsetX, 0, popupWindow,
                UIConfig.FontBody, UIConfig.UnitPopupWindowTextMinHeight, text);
            popupWindow.Add(caption);
            popupWindow.Add(tb);

            popupWindow.Add(ButtonFactory.CenterButtonHolder(0, 0, popupWindow, UIConfig.UnitPopupWindowButtonWidth, buttonText, buttonAction, out Button button));
            button.ButtonAction += b => holder.Visible = false;
            holder.Add(popupWindow);
            holder.Item = popupWindow;
            return holder;
        }

        public static FullScreenFrame<Window> ConfirmationPopupWindow(string label, string text, string okText, string cancelText, Action okAction, Action cancelAction, bool blockEverythingElse = true)
        {
            var rootFrame = ApplicationInterface.Instance.rootFrame;
            var scaleMultiplier = ScalableFrame.ScaleMultiplier;
            return ConfirmationPopupWindow((rootFrame.Width / scaleMultiplier - UIConfig.UnitPopupWindowWidth) / 2,
                100, label, text, okText, cancelText, okAction, cancelAction, blockEverythingElse);
        }

        public static FullScreenFrame<Window> ConfirmationPopupWindow(float x, float y, string label, string text, string okText, string cancelText, Action okAction, Action cancelAction, bool blockEverythingElse = true)
        {
            var holder = new FullScreenFrame<Window>()
            {
                ZOrder = 10000,
            };
            holder.SuppressActions = blockEverythingElse;
            var popupWindow = new Window(x, y, UIConfig.UnitPopupWindowWidth, 0, "", UIConfig.PopupColor, false);

            var caption = TextBlockFactory.TextBlockHolder(UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow,
                UIConfig.FontSubtitle, UIConfig.UnitPopupWindowCaptionMinHeight, label);

            var tb = TextBlockFactory.TextBlockHolder(UIConfig.UnitPopupWindowOffsetX, 0, popupWindow,
                UIConfig.FontBody, UIConfig.UnitPopupWindowTextMinHeight, text);

            popupWindow.Add(caption);
            popupWindow.Add(tb);
            Button okButton, cancelButton;
            popupWindow.Add(ButtonFactory.PopupOKCancelButtonsHolder(UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow, 1, okAction, cancelAction, out okButton, out cancelButton));
            okButton.ButtonAction += b => holder.Visible = false;
            cancelButton.ButtonAction += b => holder.Visible = false;
            holder.Add(popupWindow);
            holder.Item = popupWindow;
            return holder;
        }

        public static FullScreenFrame<Window> RenamePopupWindow(string label, string text, string okText, string cancelText, Action<string> okAction, Action cancelAction, bool blockEverythingElse = true)
        {
            var rootFrame = ApplicationInterface.Instance.rootFrame;
            var scaleMultiplier = ScalableFrame.ScaleMultiplier;
            return RenamePopupWindow((rootFrame.Width / scaleMultiplier - UIConfig.UnitPopupWindowWidth) / 2,
                100, label, text, okText, cancelText, okAction, cancelAction, blockEverythingElse);
        }

        public static FullScreenFrame<Window> RenamePopupWindow(float x, float y, string label, string text, string okText, string cancelText, Action<string> okAction, Action cancelAction, bool blockEverythingElse = true)
        {
            var holder = new FullScreenFrame<Window>()
            {
                ZOrder = 10000,
            };
            holder.SuppressActions = blockEverythingElse;
            Window popupWindow = new Window(x, y, UIConfig.UnitPopupWindowWidth, 0, "", UIConfig.PopupColor, false);

            var caption = TextBlockFactory.TextBlockHolder(UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow,
                UIConfig.FontSubtitle, UIConfig.UnitPopupWindowCaptionMinHeight, label);

            var RenameBox = EditboxFactory.EditboxHolder(UIConfig.UnitPopupWindowOffsetX, 0, popupWindow, text,
                out var box, out var tb);

            popupWindow.Add(caption);
            popupWindow.Add(RenameBox);
            Button okButton, cancelButton;
            popupWindow.Add(ButtonFactory.PopupOKCancelButtonsHolder(UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow, 1, () => okAction(box.Text), cancelAction, out okButton, out cancelButton));
            okButton.ButtonAction += b => holder.Visible = false;
            cancelButton.ButtonAction += b => holder.Visible = false;
            holder.Add(popupWindow);
            holder.Item = popupWindow;
            return holder;
        }

        public static FullScreenFrame<Window> ThreeButtonPopupWindow(string label, string text, Action Action1, Action Action2, Action Action3, string button1Text = "Yes", string button2Text = "No", string button3Text = "Cancel", bool blockEverythingElse = true)
        {
            var rootFrame = ApplicationInterface.Instance.rootFrame;
            var ScaleMultiplier = ScalableFrame.ScaleMultiplier;
            return ThreeButtonPopupWindow((rootFrame.Width / ScaleMultiplier - UIConfig.UnitPopupWindowWidth) / 2,
                100, label, text, Action1, Action2, Action3, button1Text, button2Text, button3Text, blockEverythingElse);
        }

        public static FullScreenFrame<Window> ThreeButtonPopupWindow(float x, float y, string label, string text, Action action1, Action action2, Action action3, string button1Text = "Yes", string button2Text = "No", string button3Text = "Cancel", bool blockEverythingElse = true)
        {
            var holder = new FullScreenFrame<Window>()
            {
                ZOrder = 10000,
            };
            holder.SuppressActions = blockEverythingElse;
            Window popupWindow = new Window(x, y, UIConfig.UnitPopupWindowWidth, 0, "", UIConfig.PopupColor, false);

            var caption = TextBlockFactory.TextBlockHolder(UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow,
                UIConfig.FontSubtitle, UIConfig.UnitPopupWindowCaptionMinHeight, label);

            var tb = TextBlockFactory.TextBlockHolder(UIConfig.UnitPopupWindowOffsetX, 0, popupWindow,
                UIConfig.FontBody, UIConfig.UnitPopupWindowTextMinHeight, text);

            popupWindow.Add(caption);
            popupWindow.Add(tb);
            Button yesButton, noButton, cancelButton;
            popupWindow.Add(ButtonFactory .PopupThreeButtonsHolder(UIConfig.UnitPopupWindowOffsetX, UIConfig.UnitPopupWindowOffsetY, popupWindow, 1, action1, action2, action3, out yesButton, out noButton, out cancelButton, button1Text, button2Text, button3Text));
            yesButton.ButtonAction += b => holder.Visible = false;
            noButton.ButtonAction += b => holder.Visible = false;
            cancelButton.ButtonAction += b => holder.Visible = false;
            holder.Add(popupWindow);
            holder.Item = popupWindow;
            return holder;
        }

        #region Obsoloetes
        [Obsolete("Please use factory without FrameProcessor")]
        public static FullScreenFrame<Window> NotificationPopupWindow(FrameProcessor ui, string label, string text,
            string okText, Action okAction, bool blockEverythingElse = true)
        {
            return NotificationPopupWindow(label, text, okText, okAction, blockEverythingElse);
        }

        [Obsolete("Please use factory without FrameProcessor")]
        public static FullScreenFrame<Window> NotificationPopupWindow(FrameProcessor ui, float x, float y, string label,
            string text, string buttonText, Action buttonAction, bool blockEverythingElse = true)
        {
            return NotificationPopupWindow(x, y, label, text, buttonText, buttonAction, blockEverythingElse);
        }

        [Obsolete("Please use factory without FrameProcessor")]
        public static FullScreenFrame<Window> ConfirmationPopupWindow(FrameProcessor ui, string label, string text,
            string okText, string cancelText, Action okAction, Action cancelAction, bool blockEverythingElse = true)
        {
            return ConfirmationPopupWindow(label, text, okText, cancelText, okAction, cancelAction,
                blockEverythingElse);
        }

        [Obsolete("Please use factory without FrameProcessor")]
        public static FullScreenFrame<Window> ConfirmationPopupWindow(FrameProcessor ui, float x, float y, string label,
            string text, string okText, string cancelText, Action okAction, Action cancelAction,
            bool blockEverythingElse = true)
        {
            return ConfirmationPopupWindow(x, y, label, text, okText, cancelText, okAction, cancelAction,
                blockEverythingElse);
        }

        [Obsolete("Please use factory without FrameProcessor")]
        public static FullScreenFrame<Window> RenamePopupWindow(FrameProcessor ui, string label, string text,
            string okText, string cancelText, Action<string> okAction, Action cancelAction,
            bool blockEverythingElse = true)
        {
            return RenamePopupWindow(label, text, okText, cancelText, okAction, cancelAction, blockEverythingElse);
        }

        [Obsolete("Please use factory without FrameProcessor")]
        public static FullScreenFrame<Window> RenamePopupWindow(FrameProcessor ui, float x, float y, string label,
            string text, string okText, string cancelText, Action<string> okAction, Action cancelAction,
            bool blockEverythingElse = true)
        {
            return RenamePopupWindow(x, y, label, text, okText, cancelText, okAction, cancelAction,
                blockEverythingElse);
        }

        [Obsolete("Please use factory without FrameProcessor")]
        public static FullScreenFrame<Window> ThreeButtonPopupWindow(FrameProcessor ui, string label, string text,
            Action Action1, Action Action2, Action Action3, string button1Text = "Yes", string button2Text = "No",
            string button3Text = "Cancel", bool blockEverythingElse = true)
        {
            return ThreeButtonPopupWindow(label, text, Action1, Action2, Action3, button1Text, button2Text, button3Text,
                blockEverythingElse);
        }

        [Obsolete("Please use factory without FrameProcessor")]
        public static FullScreenFrame<Window> ThreeButtonPopupWindow(FrameProcessor ui, float x, float y, string label,
            string text, Action action1, Action action2, Action action3, string button1Text = "Yes",
            string button2Text = "No", string button3Text = "Cancel", bool blockEverythingElse = true)
        {
            return ThreeButtonPopupWindow(x, y, label, text, action1, action2, action3, button1Text, button2Text,
                button3Text, blockEverythingElse);
        }
        #endregion
    }
}
