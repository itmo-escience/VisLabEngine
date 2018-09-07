using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Elements;
using FusionUI.UI.Factories;

namespace FusionUI.UI
{
    public class Window : FreeFrame
    {

        public void UpdateHolder()
        {
            holder.UpdateLayout();
        }

        protected LayoutFrame holder;

        public float UnitSeparateOffset
        {
            get => holder.UnitSeparateOffset;
            set { holder.UnitSeparateOffset = value; }
        }

        public Color HolderColor { get { return holder.BackColor; } set { holder.BackColor = value; } }
        public Color HatColor { get { return HatPanel.BackColor; } set { HatPanel.BackColor = value; } }
        public Color BasementColor { get { return BasementPanel.BackColor; } set { BasementPanel.BackColor = value; } }
        public Bool FixedSize;
        public ScalableFrame HatPanel, BasementPanel;

        public override float UnitPaddingLeft { get { return holder.UnitPaddingLeft; } set { base.UnitPaddingLeft = 0;
            holder.UnitPaddingLeft = value;
        } }
        public override float UnitPaddingRight { get { return holder.UnitPaddingRight; } set { base.UnitPaddingRight = 0;
            holder.UnitPaddingRight = value;
        } }
        public override float UnitPaddingTop { get { return holder.UnitPaddingTop; } set { base.UnitPaddingTop = 0;
            holder.UnitPaddingTop = value;
        } }
        public override float UnitPaddingBottom { get { return holder.UnitPaddingBottom; } set { base.UnitPaddingBottom = 0;
            holder.UnitPaddingBottom = value;
        } }

        protected virtual void HolderOnResize(object sender, ResizeEventArgs args)
        {
            if (!FixedSize)
            {    

                
            }
            BasementPanel.UnitY = holder.UnitY + holder.UnitHeight;
            holder.UpdateLayout();
        }

        public readonly bool DrawHat;
        public Action ActionCross;

        public Window(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor,
            bool drawHat = true, bool drawCross = true, bool drawHelp = false, string helpText = "", bool fixedSize = false)
            : base(ui, x, y, w, h, text, backColor)
        {
            DrawHat = drawHat;
            FixedSize = fixedSize;
            ((Frame) this).Ghost = false;            
            AutoHeight = !FixedSize;
            ForeColor = Color.Zero;
            Name = text;
            if (drawHat)
            {
                HatPanel = new ScalableFrame(ui, 0, 0, (int) UnitWidth, UIConfig.UnitHatHeight, Name, UIConfig.HatColor)
                {
                    TextAlignment = Alignment.MiddleLeft,
                    UnitTextOffsetX = UIConfig.UnitHatTextOffset,
                    FontHolder = UIConfig.FontCaptionAlt,
                };
                if (drawCross)
                {
                    var cross = new Button(ui, UnitWidth - UIConfig.UnitHatCrossSize - UIConfig.UnitHatTextOffset, 0,
                        UIConfig.UnitHatCrossSize, UIConfig.UnitHatHeight, "", Color.Zero, Color.Zero, 0,
                        () =>
                        {
                            this.Visible = false;
                            ActionCross?.Invoke();
                        })
                    {
                        Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_close-window"),
                        ImageMode = FrameImageMode.Cropped,
                    };
                    HatPanel.Add(cross);
                }

                if (drawHelp)
                {
                    ScalableFrame helpPopup = PopupFactory.NotificationPopupWindow(ui,
                        (ui.RootFrame.Width / ScaleMultiplier - UIConfig.UnitPopupWindowWidth) / 2, 100, "Help",
                        helpText, "Got it",
                        () => { });
                    var help = new Button(ui, UnitWidth - 2 * UIConfig.UnitHatCrossSize - UIConfig.UnitHatTextOffset, 0,
                        UIConfig.UnitHatCrossSize, UIConfig.UnitHatHeight, "", Color.Zero, Color.Zero, 0,
                        () => helpPopup.Visible = true)
                    {
                        Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_help-window"),
                        ImageMode = FrameImageMode.Cropped,
                    };
                    HatPanel.Add(help);
                }

                base.Add(HatPanel);
            }
            holder = new LayoutFrame(ui, 0, drawHat ? UIConfig.UnitHatHeight : 0, (int)UnitWidth, 0, Color.Zero)
            {
                AutoHeight = true,
                Anchor = FrameAnchor.All,
            };
            base.Add(holder);
            BasementPanel = new ScalableFrame(ui, 0, (int)(holder.UnitY + holder.UnitHeight), (int)UnitWidth, 0, "", UIConfig.SettingsColor);
            base.Add(BasementPanel);

            holder.Resize += HolderOnResize;
            UpdateResize();
            SuppressActions = true;
        }

        public IEnumerable<Frame> Children { get { return holder.Children; } }

        public override void Add(Frame frame)
        {
            holder.Add(frame);            
        }

        public void AddBase(Frame frame)
        {
            base.Add(frame);
        }

        public void Remove(Frame frame)
        {            
            holder.Remove(frame);
        }

        public void RemoveBase(Frame frame)
        {
            base.Remove(frame);
        }


        public override void Clear(Frame frame)
        {
            holder.Clear(frame);
            base.Clear(frame);
        }

	    public void HideWindow()
	    {
			Visible = false;
			Ghost	= true;
		}

	    public void ShowWindow()
	    {
			Visible = true;
			Ghost = false;
		}
    }
}
