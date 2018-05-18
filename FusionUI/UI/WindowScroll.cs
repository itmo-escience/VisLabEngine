using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI
{
    public class WindowScroll : Window
    {
        protected override void HolderOnResize(object sender, ResizeEventArgs args)
        {
            //Log.Message("LoL");
        }

        private float unitPaddingTop, unitPaddingBottom, unitPaddingLeft, unitPaddingRight;
        public override float UnitPaddingLeft
        {
            get { return unitPaddingLeft; }
            set
            {
                unitPaddingLeft = value;
                UpdatePadding();
            }
        }
        public override float UnitPaddingRight
        {
            get { return unitPaddingRight; }
            set
            {
                unitPaddingRight = value;
                UpdatePadding();
            }
        }
        public override float UnitPaddingTop
        {
            get { return unitPaddingTop; }
            set
            {
               unitPaddingTop = value;
                UpdatePadding();
            }
        }
        public override float UnitPaddingBottom
        {
            get { return unitPaddingBottom; }
            set
            {
                unitPaddingBottom = value;
                UpdatePadding();
            }
        }

        public float HeightLimit;

        public override void UpdateAnchors(int oldW, int oldH, int newW, int newH)
        {
            if (newH != oldH)
            {
                HeightLimit += (newH - oldH) / ScaleMultiplier;
            }
            base.UpdateAnchors(oldW, oldH, newW, newH);   
            UpdateResize();
            //holder.UpdateAnchors(oldW, oldH, newW, newH);
        }

        public override void UpdateResize (bool UpdateChildren = true)
        {            
            base.UpdateResize(UpdateChildren);
            bool fr = firstResize;
            var ow = oldW;
            var oh = oldH;
            AutoResize(true);
            if (oldW != Width || oldH != Height ) {                
                if (!fr && UpdateChildren)
                {
                    holder.UpdateAnchors(ow, oh, Width, Height);
                }                                                
            }
        }

        public void UpdatePadding()
        {
            scrollHolder.UnitY = DrawHat ? UIConfig.UnitHatHeight : 0 + UnitPaddingTop;
            scrollHolder.UnitX = UnitPaddingLeft;
            scrollHolder.UnitWidth = UnitWidth - UnitPaddingLeft - UnitPaddingRight;
            scrollHolder.UnitHeight = HeightLimit - UnitPaddingTop - UnitPaddingBottom;
            holder.UnitWidth = scrollHolder.UnitWidth;
        }

        public float ScrollSize = 2;

        public bool AllowShrink = true;

        private ScalableFrame scrollHolder;
        public WindowScroll(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor,
            bool drawHat = true, bool drawCross = true)
            : base(ui, x, y, w, h, text, backColor, drawHat, drawCross)
        {
            ClampPos = false;
            AutoHeight = false;
            UnitWidth = w;
            UnitHeight = h;            
            UpdateResize();
            SuppressActions = true;
            HeightLimit = h - (HatPanel?.UnitHeight ?? 0) - (BasementPanel?.UnitHeight ?? 0) ;
            scrollHolder = new ScalableFrame(ui, holder.UnitX, holder.UnitY, holder.UnitWidth, HeightLimit - UnitPaddingTop - UnitPaddingBottom, "", Color.Zero)
            {   
                //Border = 1,
                //BorderColor = Color.Violet,
                Anchor = FrameAnchor.Left | FrameAnchor.Right | FrameAnchor.Top,
            };            
            if (BasementPanel != null)
            {
                BasementPanel.UnitY = h - BasementPanel.UnitHeight;
            }
            if (HatPanel != null) HatPanel.Border = 1;
            RemoveBase(holder);
            AddBase(scrollHolder);
            scrollHolder.Add(holder);
            holder.UnitY = 0;
            holder.UnitWidth = Math.Max(this.UnitWidth, this.UnitWidth - ScrollSize + holder.UnitPaddingRight);
            holder.ZOrder = -100;
            holder.ActionDrag += (ControlActionArgs args, ref bool flag) =>
            {
                if (args.IsClick && holder.Selected)
                {
                    holder.UnitY += args.MoveDelta.Y / ScaleMultiplier;
                    holder.UnitY = MathUtil.Clamp(holder.UnitY, Math.Min(MaxHeight - RealHeight, 0), 0);
                }
            };
            holder.MouseWheel += (sender, args) =>
            {
                if (MainFrame.IsChildOf(ui.RootFrame, MainFrame.GetHoveredFrame(ui.RootFrame, Game.Mouse.Position), holder))
                {
                    holder.UnitY += args.Wheel / ScaleMultiplier;
                    holder.UnitY = MathUtil.Clamp(holder.UnitY, Math.Min(MaxHeight - RealHeight, 0), 0);
                }
            };
            holder.UpdateParent = false;
            holder.ActionDraw += DrawScrollLine;

            ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (args.X > GlobalRectangle.Right - ScrollSize * ScaleMultiplier)
                {
                    float p = (float) (args.Y - GlobalRectangle.Top) / GlobalRectangle.Height;
                    holder.UnitY = p * (MaxHeight - RealHeight);
                    holder.UnitY = MathUtil.Clamp(holder.UnitY, Math.Min(MaxHeight - RealHeight, 0), 0);
                }
            };

            float clickPos = 0;
            ActionDown += (ControlActionArgs args, ref bool flag) =>
            {
                var Rectangle = new RectangleF(scrollHolder.GlobalRectangle.Left, scrollHolder.GlobalRectangle.Top,
                    scrollHolder.GlobalRectangle.Width,
                    scrollHolder.GlobalRectangle.Height);
                var relativeSize = MaxHeight / RealHeight;
                var relativePos = (1 - relativeSize) * (-ScrollDelta / (RealHeight - MaxHeight));

                var p = Rectangle.Top + relativePos * Rectangle.Height;
                var d = relativeSize * Rectangle.Height;
                clickPos = (args.Y - p) / d;
            };

            ActionDrag += (ControlActionArgs args, ref bool flag) =>
            {
                if (Selected && args.X > GlobalRectangle.Right - ScrollSize * ScaleMultiplier)
                {
                    var relativeSize = MaxHeight / RealHeight;
                    //var relativePos = (1 - relativeSize) * (-ScrollDelta / (RealHeight - MaxHeight));                    

                    float rp = (float) (args.Y - GlobalRectangle.Top) / GlobalRectangle.Height - clickPos * relativeSize;
                    var p = rp / (1 - relativeSize) * (RealHeight - MaxHeight);
                    //holder.UnitY = p * (MaxHeight - RealHeight);
                    holder.UnitY = -p;
                    holder.UnitY = MathUtil.Clamp(holder.UnitY, Math.Min(MaxHeight - RealHeight, 0), 0);
                }
            };
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (AllowShrink)
            {
                scrollHolder.UnitHeight = Math.Min(HeightLimit, holder.UnitHeight);
                UnitHeight = (HatPanel?.UnitHeight ?? 0) + (scrollHolder.UnitHeight) + (BasementPanel?.UnitHeight ?? 0);
            }                        
            holder.UnitY = MathUtil.Clamp(holder.UnitY, Math.Min(MaxHeight - RealHeight, 0), 0);
        }

        private float RealHeight => holder.UnitHeight;
        private float MaxHeight => scrollHolder.UnitHeight;
        private float ScrollDelta => holder.UnitY;
        protected virtual void DrawScrollLine(GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex)
        {
            var whiteTex = this.Game.RenderSystem.WhiteTexture;
            if (RealHeight > MaxHeight)
            {
                var Rectangle = new RectangleF(GlobalRectangle.Left, scrollHolder.GlobalRectangle.Top,
                    GlobalRectangle.Width,
                    scrollHolder.GlobalRectangle.Height);

                var relativeSize = MaxHeight / RealHeight;
                var relativePos = (1 - relativeSize) * (-ScrollDelta / (RealHeight - MaxHeight));
                spriteLayer.Draw(whiteTex, Rectangle.Right - (ScrollSize * ScaleMultiplier), Rectangle.Top + relativePos * Rectangle.Height, ScrollSize * ScaleMultiplier, relativeSize * Rectangle.Height, UIConfig.ActiveColor);
                //spriteLayer.Draw(whiteTex, Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height, Color.Green);
            }
        }
    }
}