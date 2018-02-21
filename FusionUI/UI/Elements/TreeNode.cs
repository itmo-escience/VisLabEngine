using System;
using System.Collections.Generic;
using System.Drawing.Text;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Elements;
using FusionUI.UI.Factories;

namespace FusionUI.UI
{
    public class TreeNode : ScalableFrame
    {
        public bool IsExpand = false;

        public int OffsetChild
        {
            get { return (int)(UnitOffsetChild * ScaleMultiplier); }
            set { UnitOffsetChild = (float)value / ScaleMultiplier; }
        }

        public float UnitOffsetChild { get; set; }
        public Texture ExpandedPicture;
        public Texture CollapsedPicture;

        private float UnitHeightCollaps;
        private float UnitHeightExpand;

        private int HeightCollaps
        {
            get { return (int)(UnitHeightCollaps * ScaleMultiplier); }
            set { UnitHeightCollaps = value / ScaleMultiplier; }
        }

        private int HeightExpand
        {
            get { return (int)(UnitHeightExpand * ScaleMultiplier); }
            set { UnitHeightExpand = value / ScaleMultiplier; }            
        }

        public Color backColorMainNode;
        public List<Frame> listNode;

        public int UnitSizeExpandButton = 5;

        public TreeNode(FrameProcessor ui) : base(ui)
        {
            //            initDropDownNode(ui);
            listNode = new List<Frame>();
            ClippingMode = ClippingMode.ClipByFrame;
        }

        public TreeNode(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui);
        }

        private void init(FrameProcessor ui)
        {

            // for resize
            HeightCollaps = this.Height;
            this.HeightExpand = this.Height;
            listNode = new List<Frame>();
            // for expand/collaps
            if (Text != "")
            {
                this.ActionClick += (ControlActionArgs args, ref bool flag) => {                    
                    if (!args.IsClick || args.IsAltClick) return;
                    if (args.Position.Y - this.GlobalRectangle.Y > HeightCollaps) return;
                    if (listNode?.Count > 0)
                    {
                        IsExpand = !IsExpand;
                        ExpandNodes(IsExpand);
                    }
                    flag |= true;
                };
            }
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return;
            var xButton = this.GlobalRectangle.X + PaddingLeft + TextOffsetX / 2 - (int)(UnitSizeExpandButton * ScaleMultiplier) / 2 + ImageOffsetX;
            var yButton = this.GlobalRectangle.Y + this.HeightCollaps / 2 - (int)(UnitSizeExpandButton * ScaleMultiplier) / 2 + ImageOffsetY;

            var xText = this.GlobalRectangle.X + PaddingLeft + TextOffsetX;
            var yText = this.GlobalRectangle.Y + this.HeightCollaps / 2 + this.Font.CapHeight / 2;
            var whiteTexture = this.Game.RenderSystem.WhiteTexture;

            sb.Draw(whiteTexture,
                    new Rectangle(this.GlobalRectangle.X, this.GlobalRectangle.Y, this.Width, this.HeightCollaps),
                    backColorMainNode, clipRectIndex);
            

            var w = this.Width - (!(ExpandedPicture == null || CollapsedPicture == null) ? (int) (UnitSizeExpandButton * ScaleMultiplier) : 0) - PaddingLeft - TextOffsetX;
            var tw = Font.MeasureString(Text).Width;
            var dw = (float) w / tw;
            var text2 = (dw < 1) ? Text.Substring(0, (int) (dw * Text.Length) - 3).TrimEnd(' ') + "..." : Text;

            Font.DrawString(sb, text2, xText, yText, ForeColor, clipRectIndex, 0, true);                
            if (!(ExpandedPicture == null || CollapsedPicture == null))
                sb.Draw(IsExpand ? ExpandedPicture : CollapsedPicture,
                    new Rectangle(xButton, yButton, (int) (UnitSizeExpandButton * ScaleMultiplier),
                        (int) (UnitSizeExpandButton * ScaleMultiplier)), Color.White, clipRectIndex);            

        }        

        public void addNode(Frame node)
        {
            initNode(node);
            listNode.Add(node);
            if (IsExpand)
            {
                this.Add(node);
                this.Height += node.Height;

                if (this.Parent is TreeNode)
                {
                    var treeNode = (TreeNode)this.Parent;
                    treeNode.ResizeTree();
                }
            }
            //                expandNodes(IsExpand);
            if (this.Width < node.Width + node.X)
                this.Width = node.Width + node.X;
        }

        public void removeNode(Frame node)
        {
            this.Remove(node);
            listNode.Remove(node);
            Height -= node.Height;
            HeightExpand -= node.Height;
            ResizeTree();
        }

        public void RemoveAllNode()
        {
            foreach (var node in listNode.ToArray())
            {
                this.Remove(node);
                listNode.Remove(node);
            }
            HeightExpand = HeightCollaps;
            ResizeTree();
        }

        private void initNode(Frame node)
        {
            node.X = PaddingLeft + OffsetChild;
            if (!IsExpand)
            {
                node.Y = this.HeightExpand;
            }
            else
            {
                node.Y = this.Height;                
            }
            this.HeightExpand += node.Height;
        }

        public void ExpandNodes(bool isExpand)
        {
            foreach (var node in listNode)
            {                
                if (isExpand)
                {
                    node.Y = this.Height;
                    this.Add(node);
                    this.Height += node.Height;
                }
                else
                {
                    if (!Children.Contains(node)) continue;
                    this.Remove(node);
                    this.Height -= node.Height;
                }
            }

            if (this.Parent is TreeNode)
            {
                var treeNode = (TreeNode)this.Parent;
                treeNode.ResizeTree();
            }
        }

        public override void UpdateResize(bool UpdateChildren = true)
        {
            base.UpdateResize(UpdateChildren);
            ResizeTree();
        }

        public void ResizeTree()
        {
            this.Height = HeightCollaps;
            foreach (var node in listNode)
            {
                initNode(node);
                this.Height += node.Height;
            }
            if (!IsExpand) this.Height = HeightCollaps;
            if (this.Parent is TreeNode)
            {
                var treeNode = (TreeNode)this.Parent;
                treeNode.ResizeTree();
            }
            
        }

        public override string Tooltip
        {
            get
            {
                if (Text != null && Font.MeasureString(Text).Width + TextOffsetX > Width - (ExpandedPicture?.Width ?? CollapsedPicture?.Width ?? 0))
                {
                    return Text;
                }
                return "";
            }
            set { }
        }
    }

    public class CheckboxNode : TreeNode
    {
        public Action<GameTime> UpdateFunc;
        public Checkbox Checkbox;
        public CheckboxNode(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor, Action<bool> checkboxAction, bool isChecked = false) : base(ui, x, y, w, h, text, backColor)
        {            
            this.Add(Checkbox = new Checkbox(ui, 0, 0, h, h, "", Color.Zero)
            {
                Checked = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_checkbox-big-on"),
                None = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_checkbox-big-off"),
                ClippingMode = ClippingMode.ClipByFrame
            });
            Checkbox.IsChecked = isChecked;
            Checkbox.Changed += checkboxAction;
            UnitTextOffsetX = Checkbox.UnitWidth;
        }

        

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateFunc?.Invoke(gameTime);
        }
    }

    public class RadiobuttonNode : TreeNode
    {
        public Action<GameTime> UpdateFunc;
        public RadioButton Checkbox;
        public RadiobuttonNode(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor, Action<bool> checkboxAction, bool selected) : base(ui, x, y, w, h, text, backColor)
        {
            this.Add(Checkbox = new RadioButton(ui, 0, 0, h, h, "", Color.Zero));

            Checkbox.Changed += (b) =>
            {
                if (b)
                {
                    if (parent == null) return;
                    foreach (var frame in parent.Children)
                    {
                        if (!(frame is RadiobuttonNode) || frame == this) continue;                                                
                        ((RadiobuttonNode)frame).Checkbox.IsChecked = false;
                    }
                }
                checkboxAction?.Invoke(b);
            };
            Checkbox.IsChecked = selected;

            UnitTextOffsetX = Checkbox.UnitWidth;
        }



        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateFunc?.Invoke(gameTime);
        }
    }
}
