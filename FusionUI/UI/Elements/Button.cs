using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Elements.TextFormatting;
using static FusionUI.UI.UIConfig;

namespace FusionUI.UI.Elements
{
    public class Button : ScalableFrame
    {

        public Color ActiveColor, InactiveColor;
        public Color ActiveFColor = UIConfig.ActiveTextColor, InactiveFColor = UIConfig.ActiveTextColor;
        public Texture ActiveImage, PassiveImage;
        public Action UpdateAction;
        public Action<bool> ButtonAction;

        private bool toggleState;

        public bool IsToggled => toggleState;

        public Button(FrameProcessor ui, float x, float y, float w, float h, string text, Color activeColor, Color inactiveColor, Texture activeImage = null, Texture passiveImage = null, Action<bool> action = null, bool active = false, int timeTransition=0, Color? activeFColor = null, Color? inactiveFColor = null) : base(ui, x, y, w, h, text, inactiveColor)
        {
            ActiveColor = activeColor;
            InactiveColor = inactiveColor;
            ActiveFColor = activeFColor ?? ForeColor;
            InactiveFColor = inactiveFColor ?? ForeColor;
            ActiveImage = activeImage;
            PassiveImage = passiveImage;
            initToggableButton(active, timeTransition);
            ButtonAction += action;
            ImageMode = FrameImageMode.Fitted;
        }

        void initToggableButton(bool active, int transitionTime = 0)
        {
            toggleState = active;

            if (transitionTime != 0)
            {
                this.ActionDown += (ControlActionArgs args, ref bool flag) =>
                {
                    if (!args.IsClick) return;
                    this.BackColor = ActiveColor;
                    this.ForeColor = ActiveFColor;
                    this.ImageColor = ActiveFColor;
                    flag = true;
                };

                this.ActionOut += (ControlActionArgs args, ref bool flag) =>
                {
                    this.BackColor = InactiveColor;
                    this.ForeColor = InactiveFColor;
                    this.ImageColor = InactiveFColor;
                };

                this.ActionDrag += (ControlActionArgs args, ref bool flag) => flag = true;

                this.ActionUp += (ControlActionArgs args, ref bool flag) =>
                {
                    this.BackColor = InactiveColor;
                    this.ForeColor = InactiveFColor;
                    this.ImageColor = InactiveFColor;
                    flag = true;
                };
            }
            
            this.ActionClick += (ControlActionArgs args, ref bool flag) => {
                if (!args.IsClick) return;
                ToggleOnOff(toggleState);
                ButtonAction?.Invoke(toggleState);                
                if (transitionTime != 0)
                {                    
                    this.BackColor = ActiveColor;
                    this.ForeColor = ActiveFColor;
                    this.ImageColor = ActiveFColor;
                    this.RunTransition("BackColor", InactiveColor, 0, transitionTime);
                    this.RunTransition("ForeColor", InactiveFColor, 0, transitionTime);
                    this.RunTransition("ImageColor", InactiveFColor, 0, transitionTime);
                }
            };
            ToggleOnOff(!toggleState);            
        }

        public void ToggleOn(bool invoke = true)
        {
            this.BackColor = ActiveColor;
            this.ForeColor = ActiveFColor;
            this.ImageColor = ActiveFColor;
            toggleState = true;
            if (ActiveImage != null) this.Image = ActiveImage;
            if (invoke)
            {
                ButtonAction?.Invoke(toggleState);
            }

        }

        public void ToggleOff(bool invoke = true)
        {
            this.BackColor = InactiveColor;
            this.ForeColor = InactiveFColor;
            this.ImageColor = InactiveFColor;
            toggleState = false;
            if (PassiveImage != null) this.Image = PassiveImage;
            if (invoke)
            {
                ButtonAction?.Invoke(toggleState);
            }
        }

        public void ToggleOnOff(bool currentState, bool invoke = false)
        {
            if (currentState)
            {
                ToggleOff(invoke);
            }
            else 
            {
                ToggleOn(invoke);
            }
        }

        public Button(FrameProcessor ui, float x, float y, float w, float h, string text, Color mainColor, Color transitionColor, int transitionTime, Action action = null, Color? activeFColor = null, Color? inActiveFColor = null) : base(ui, x, y, w, h, text, mainColor)
        {
            ActiveColor = transitionColor;
            InactiveColor = mainColor;
            ActiveFColor = activeFColor ?? ActiveFColor;
            InactiveFColor = inActiveFColor ?? InactiveFColor;
            ActiveImageColor = activeFColor ?? ActiveImageColor;
            InactiveImageColor = inActiveFColor ?? InactiveImageColor;
            initClickableButton(transitionTime);
            if (action != null) ButtonAction += b => action();
            ImageMode = FrameImageMode.Fitted;
        }

        void initClickableButton(int transitionTime)
        {
            this.ActionDown += (ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsClick) return;
                this.BackColor = ActiveColor;
                this.ForeColor = ActiveFColor;
                this.ImageColor = ActiveFColor;
                flag = true;
            };

            this.ActionOut += (ControlActionArgs args, ref bool flag) =>
            {
                this.BackColor = InactiveColor;
                this.ForeColor = InactiveFColor;
                this.ImageColor = InactiveFColor;
            };

            this.ActionDrag += (ControlActionArgs args, ref bool flag) => flag = true;

            this.ActionUp += (ControlActionArgs args, ref bool flag) =>
            {
                this.BackColor = InactiveColor;
                this.ForeColor = InactiveFColor;
                this.ImageColor = InactiveFColor;
                flag = true;
            };

            this.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsClick) return;
                this.BackColor = ActiveColor;
                this.ForeColor = ActiveFColor;
                this.ImageColor = ActiveFColor;
                this.RunTransition("BackColor", InactiveColor, 0, transitionTime);                
                ButtonAction?.Invoke(true);
            };
            this.BackColor = InactiveColor;
            this.ForeColor = InactiveFColor;
            this.ImageColor = InactiveFColor;
        }

        protected override void Update(GameTime gameTime)
        {
            UpdateAction?.Invoke();
            base.Update(gameTime);
        }
    }

    public class FormatTexstButton : Button
    {
        private FormatTextBlock block;

        protected FormatTextBlock Block => block ?? InitTextBlock();

        private FormatTextBlock InitTextBlock()
        {
            block = new FormatTextBlock(ui, 0, 0, UnitWidth, UnitHeight, Text, Color.Zero, FontHolder, minHeight:UnitHeight)
            {
                TextAlignment = Alignment.MiddleCenter,
            };
            Add(block);
            Text = "";
            return block;
        }

        public override FontHolder FontHolder
        {
            get => base.FontHolder;
            set
            {
                base.FontHolder = value;
                Block.FontHolder = value;
            }
        }

        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                Block.Text = TryGetText(value);
            }
        }

        public FormatTexstButton(FrameProcessor ui, float x, float y, float w, float h, string text, Color activeColor, Color inactiveColor, Texture activeImage = null, Texture passiveImage = null, Action<bool> action = null, bool active = false, int timeTransition = 0, Color? activeFColor = null, Color? inactiveFColor = null) : base(ui, x, y, w, h, text, activeColor, inactiveColor, activeImage, passiveImage, action, active, timeTransition, activeFColor, inactiveFColor)
        {
            InitTextBlock();
        }

        public FormatTexstButton(FrameProcessor ui, float x, float y, float w, float h, string text, Color mainColor, Color transitionColor, int transitionTime, Action action = null, Color? activeFColor = null, Color? inActiveFColor = null) : base(ui, x, y, w, h, text, mainColor, transitionColor, transitionTime, action, activeFColor, inActiveFColor)
        {
            InitTextBlock();
        }
    }
}
