using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using FusionUI.UI.Elements;

namespace FusionUI.UI.Factories
{
    public class ContextMenuFactory
    {
        public static ScalableFrame ContextMenu(float x, float y, float width, float buttonHeight,
            List<Tuple<string, Action>> buttonData, out List<Button> buttons)
        {
            var holder = new ScalableFrame(x, y, width, buttonHeight * buttonData.Count, "", UIConfig.PopupColor)
            {
                ZOrder =  1000,
                Selected = true,
            };
            holder.ActionUpdate += time => { holder.ZOrder = 1000; };
            holder.ActionOut += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
            {
                if (!holder.Selected)
                {
                    holder.Parent.Remove(holder);
                    holder.Clean();
                    holder.Clear(holder);
                }
            };
            int i = 0;
            buttons = new List<Button>();
            foreach (var bd in buttonData)
            {
                var button = new Button(0, buttonHeight * i, width, buttonHeight, bd.Item1, Color.Zero, UIConfig.ActiveColor, 200, bd.Item2)
                {
                    Border = 1,
                    TextAlignment = Alignment.MiddleLeft,
                    UnitTextOffsetX = UIConfig.UnitTabTextOffsetX,
                    ForeColor = UIConfig.ActiveTextColor,
                };
                button.ButtonAction += b =>
                {
                    holder.Parent.Remove(holder);
                    holder.Clean();
                    holder.Clear(holder);
                };
                holder.Add(button);
                buttons.Add(button);
                i++;
            }
            return holder;
        }

        public static ScalableFrame ContextMenu(FrameProcessor ui, float x, float y, float width, float buttonHeight,
            List<Tuple<string, Action>> buttonData, out List<Button> buttons)
        {
            return ContextMenu(x, y, width, buttonHeight, buttonData, out buttons);
        }
    }
}
