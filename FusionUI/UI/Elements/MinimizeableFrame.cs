using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI.Elements
{
    public class MinimizeableFrame : LayoutFrame
    {
        public MinimizeableFrame(FrameProcessor ui, float x, float y, float w, float h, Color backColor, string text, LayoutType layoutType = LayoutType.Vertical) : base(ui, x, y, w, h, backColor, layoutType)
        {
            GenerateTopNode(text);
        }

        private ScalableFrame TopNode;
        private bool isOpen = true;

        public bool IsOpen
        {
            get => isOpen;
            set
            {
                isOpen = value;
                foreach (var node in Nodes)
                {
                    if (node != TopNode) node.Visible = isOpen;
                }

                TopNode.Image = isOpen
                    ? Game.Content.Load<DiscTexture>("ui-new/fv-icons_close-list")
                    : Game.Content.Load<DiscTexture>("ui-new/fv-icons_open-list");
            }
        }
        public void GenerateTopNode(string text)
        {
            TopNode = new ScalableFrame(ui, 0, 0, UnitWidth - UnitPaddingLeft - UnitPaddingRight, UIConfig.UnitHatHeight, text, UIConfig.ButtonColor)
            {
                Image = Game.Content.Load<DiscTexture>("ui-new/fv-icons_open-list"),
                //UnitPadding = 2,
                UnitTextOffsetX = 2,
                UnitTextOffsetY = 2,
                UnitImageOffsetX = (UnitWidth - UnitPaddingLeft - UnitPaddingRight) / 2 - UIConfig.UnitHatHeight/ 2, 
                ImageMode = FrameImageMode.Fitted,
            };
            

            TopNode.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (args.IsClick) IsOpen = !IsOpen;
            };
            Add(TopNode);
        }
    }
}
