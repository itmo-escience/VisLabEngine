﻿using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Elements;

namespace FusionUI.UI.Factories
{
    public class EditboxFactory
    {

        public static UIContainer<Editbox> EditboxHolder(FrameProcessor ui, float OffsetX, float OffsetY, ScalableFrame parent,
            string label, out Editbox box, out ScalableFrame labelFrame, string initValue = "")
        {
            UIContainer<Editbox> holder = new UIContainer<Editbox>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, UIConfig.UnitEditboxElementHeight + UIConfig.UnitEditboxElementOffset + UIConfig.UnitEditboxLabelHeight + OffsetY, "", Color.Zero)
            {                
            };

            labelFrame = new ScalableFrame(ui, OffsetX, OffsetY, holder.UnitWidth - 2 * OffsetX, UIConfig.UnitEditboxLabelHeight, label, Color.Zero)
            {
                TextAlignment = Alignment.BaselineLeft,
                UnitTextOffsetY = 4,
                FontHolder = UIConfig.FontBody,
            };
            var inBox = new Editbox(ui, OffsetX, OffsetY + UIConfig.UnitEditboxElementOffset + UIConfig.UnitEditboxLabelHeight,
                holder.UnitWidth - 2*OffsetX, UIConfig.UnitEditboxElementHeight, initValue, Color.Zero)
            {
                Border = 2,
                BorderColor = UIConfig.BorderColor,
                BorderActive = UIConfig.ActiveColor,
                HoverColor = Color.White,
                PaddingLeft = (int)(2 * ApplicationInterface.ScaleMod),
                PaddingBottom = (int)(2 * ApplicationInterface.ScaleMod),                
                TextAlignment = Alignment.MiddleLeft
            };
            box = inBox;
            var cross = new Button(ui, holder.UnitWidth - OffsetX - UIConfig.UnitEditboxIconWidth, OffsetY + UIConfig.UnitEditboxElementOffset + UIConfig.UnitEditboxLabelHeight + UIConfig.UnitEditboxIconOffset, UIConfig.UnitEditboxIconWidth, UIConfig.UnitEditboxIconWidth, "", Color.Zero, Color.Zero, 0, () => {})
            {
                Image = Game.Instance.Content.Load<DiscTexture>(@"UI-new\fv-icons_clear-text-box"),
                ImageColor = Color.Black,
                ActiveImageColor = Color.Black,     
                InactiveImageColor  = Color.Black,
                Name = "Cross",         
            };
            cross.ButtonAction += b => { inBox.SetActiveStatus(false); };
            cross.UpdateAction += () =>
            {
                cross.Visible = inBox.IsActive;
                cross.ZOrder = inBox.ZOrder + 1;
            };
            holder.Item = box;
            holder.Add(labelFrame);            
            holder.Add(box);
            holder.Add(cross);

            return holder;
        }
    }
}
