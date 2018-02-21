using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI
{
    public enum LayoutType
    {
        Derived,
        Horizontal,
        Vertical,
    }

    public class LayoutFrame : FreeFrame
    {

        protected LayoutType type;

        private float max;

        private List<ScalableFrame> nodes = new List<ScalableFrame>();

        public bool UpdateParent = false;

        public bool ControlChildrenSize = true;

        public LayoutFrame(FrameProcessor ui, float x, float y, float w, float h, Color backColor, LayoutType layoutType = LayoutType.Vertical)
            : base(ui, x, y, w, h, "", backColor)
        {
            type = layoutType;
            if (layoutType == LayoutType.Vertical) AutoHeight = true;
            if (layoutType == LayoutType.Horizontal) AutoWidth = true;
            max = 0;
            DisableFree = true;
        }

        public override float UnitPaddingLeft
        {
            get { return base.UnitPaddingLeft; }
            set
            {
                base.UnitPaddingLeft = value;
                UpdateLayout();
            }
        }
        public override float UnitPaddingRight
        {
            get { return base.UnitPaddingRight; }
            set
            {
                base.UnitPaddingRight = value;
                UpdateLayout();
            }
        }
        public override float UnitPaddingTop
        {
            get { return base.UnitPaddingTop; }
            set
            {
                base.UnitPaddingTop = value;
                UpdateLayout();
            }
        }
        public override float UnitPaddingBottom
        {
            get { return base.UnitPaddingBottom; }
            set
            {
                base.UnitPaddingBottom = value;
                UpdateLayout();
            }
        }

        public new Frame Add(Frame frame)
        {
            if (frame is ScalableFrame)
                Add((ScalableFrame) frame);
            return frame;
        }

        public new void Remove(Frame frame)
        {
            if (frame is ScalableFrame)
                Remove((ScalableFrame)frame);
        }

        public new ScalableFrame Add(ScalableFrame frame)
        {
            base.Add(frame);
            nodes.Add(frame);
            UpdateResize();
            UpdateLayout();
            //switch (type)
            //{
            //    case LayoutType.Vertical:
            //        frame.UnitY = max;
            //        frame.UnitX = UnitPaddingLeft;
            //        frame.UnitWidth = UnitWidth - UnitPaddingLeft - UnitPaddingRight;
            //        max = max + frame.UnitHeight;                    
            //        UnitHeight = max;
            //        Parent.Height = this.Y + this.Height;
            //        nodes.Add(frame);
            //        base.Add(frame);
            //        UpdateResize();
            //        //Parent.UpdateResize();
            //        break;
            //    case LayoutType.Horizontal:
            //        frame.UnitX = max;
            //        frame.UnitY = UnitPaddingTop;
            //        frame.UnitHeight = UnitHeight - UnitPaddingTop - UnitPaddingBottom;
            //        max = max + frame.UnitWidth;                    
            //        UnitWidth = max;
            //        Parent.Width = this.X + this.Width;
            //        nodes.Add(frame);
            //        base.Add(frame);
            //        UpdateResize();
            //        //Parent.UpdateResize();
            //        break;

            //}
            return frame;
        }

        public new void Remove(ScalableFrame frame)
        {
            base.Remove(frame);
            nodes.Remove(frame);
            UpdateResize();
            UpdateLayout();
        }

        public float UnitSeparateOffset = 0;
        public void UpdateLayout()
        {            
            switch (type)
            {
                case LayoutType.Vertical:
                    //if (!ControlChildrenSize && this.Children.Count > 0)
                    //    this.Width = this.Children.Max(a => a.Width) + (int)((UnitPaddingLeft + UnitPaddingRight) * ScaleMultiplier);
                    max = UnitPaddingTop;
                    int lastH = this.Height;
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var frame = nodes[i];
                        if (!frame.Visible) continue;
                        frame.UnitY = max;
                        frame.UnitX = UnitPaddingLeft;
                        if (ControlChildrenSize) frame.UnitWidth = UnitWidth - UnitPaddingLeft - UnitPaddingRight;
                        else UnitWidth = Math.Max(UnitHeight, UnitPaddingLeft + frame.UnitWidth+ UnitPaddingRight);
                        max = max + frame.UnitHeight + UnitSeparateOffset;                        
                    }
                    UnitHeight = max;
                    UnitHeight += UnitPaddingBottom;
                    if (UpdateParent && Parent != null)
                    {
                        if (!(Parent is LayoutFrame)) Parent.Height += this.Height - lastH;
                        Parent.UpdateResize();
                        (Parent as LayoutFrame)?.UpdateLayout();
                    }
                    break;
                case LayoutType.Horizontal:
                    //if (!ControlChildrenSize && this.Children.Count > 0) this.Height = (this.Children.Count > 0 ? this.Children.Max(a => a.Height) : 0) + (int)((UnitPaddingTop + UnitPaddingBottom)*ScaleMultiplier);
                    max = UnitPaddingLeft;
                    int lastW = this.Width;
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var frame = nodes[i];
                        if (!frame.Visible) continue;
                        frame.UnitX = max;
                        frame.UnitY = UnitPaddingTop;
                        if (ControlChildrenSize) frame.UnitHeight = UnitHeight - UnitPaddingTop - UnitPaddingBottom;
                        else UnitHeight = Math.Max(UnitHeight, UnitPaddingTop + frame.UnitHeight + UnitPaddingBottom);
                        max = max + frame.UnitWidth + UnitSeparateOffset;                                              
                    }
                    UnitWidth = max;
                    UnitWidth += UnitPaddingRight;
                    if (UpdateParent && Parent != null)
                    {
                        if (!(Parent is LayoutFrame)) Parent.Width += this.Width - lastW;
                        Parent.UpdateResize();                        
                        (Parent as LayoutFrame)?.UpdateLayout();
                    }
                    break;
            }            
        }

        private int oldW = -1, oldH = -1;

        public override void UpdateScale(float scale)
        {
            base.UpdateScale(scale);
            UpdateLayout();
        }

        public override void UpdateResize(bool updateChildren = true)
        {

            if (Width != oldW || Height != oldH)
            {
                UpdateLayout();
                oldW = Width;
                oldH = Height;
            }
            else if (ControlChildrenSize)
            {
                UpdateLayout();
            }
            base.UpdateResize();
        }
    }
}
