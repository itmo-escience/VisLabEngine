using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;

namespace FusionUI.UI.Elements
{
    public class Slider : ScalableFrame
    {
        private FrameProcessor ui;

        public float MinValue = 0;
        public float MaxValue = 1;

        public bool IsVertical = false;
        public bool DrawPreset = true;

        public Action<float> OnChange;

        private List<float> _presetValues = null;

        public List<float> PresetValues
        {
            get { return _presetValues; }
            set
            {                
                _presetValues = value.ToList();
                _presetValues.Add(MinValue);
                _presetValues.Add(MaxValue);
                _presetValues.Sort();
            }
        }

        public bool RoundValues = false;

        public float PresetDistance = 0.05f;

        private float _currentValue;
        public float CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = MathUtil.Clamp((RoundValues ? (float)Math.Round(value) : value), MinValue, MaxValue);                
                OnChange?.Invoke(value);
            }
        }

        public void SetValueImplicit(float value)
        {
            _currentValue = MathUtil.Clamp(RoundValues ? (float)Math.Round(value) : value, MinValue, MaxValue);
        }

        public int SliderWidth => (int)(UnitSliderWidth * ScaleMultiplier);
        public float UnitWidthControlElement = 4, UnitHeightControlElement = 11, UnitSliderWidth = 0.5f;
        public int widthControlElement => (int)(UnitWidthControlElement * ScaleMultiplier);
        public int heightControlElement => (int)(UnitHeightControlElement * ScaleMultiplier);

        private bool IsDrag = false;
        private bool IsHovered = false;
        public Color backColorForSlider;
        public Color HoverColor { get; set; }
        public Color HoverForeColor { get; set; }

        public Slider(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            this.ui = ui;
            initAllEvent();
        }


        void initAllEvent()
        {
            // moving slider
            ActionDrag += Slider_MouseMove;
            //ActionDown += Slider_MouseDown;
            ActionClick += Slider_Click;
            MouseAction outAction = (ControlActionArgs args, ref bool flag) =>
            {
                if (PresetValues != null && !Selected)
                {
                    var v = _presetValues.BinarySearch(_currentValue);
                    if (v < 0) v = ~v;
                    var right = _presetValues[v];
                    var left = v > 0 ? _presetValues[v - 1] : right;
                    if (_currentValue - left > right - _currentValue && (right - _currentValue) / (MaxValue - MinValue) < PresetDistance) _currentValue = right;
                    else if ((_currentValue - left) / (MaxValue - MinValue) < PresetDistance) _currentValue = left;
                    CurrentValue = _currentValue;
                }
            };
            ActionOut += outAction;
            ActionLost += outAction;
            ActionUp += outAction;
            ActionClick += outAction;                       
            // hovered

            //Game.Touch.Manipulate += args =>
            //{
            //    if (this.GlobalRectangle.Contains(args.Position))
            //    {                    
            //        if (args.IsEventBegin)
            //        {
            //            Slider_TouchDown(this, args);
            //        }
            //        else if (args.IsEventEnd)
            //        {
            //            Slider_Touch(this, args);
            //        }
            //        else
            //        {
            //            Slider_TouchMove(this, args);
            //        }
            //    }                                
            //};
        }

        void Slider_MouseDown(ControlActionArgs args, ref bool flag)
        {
            var p = new Vector2
            {
                X = args.X - this.GlobalRectangle.X,
                Y = args.Y - this.GlobalRectangle.Y,
            };
            ChangeSliderPosition(p);
            flag |= true;
        }

        void Slider_Click(ControlActionArgs args, ref bool flag)
        {
            var p = new Vector2
            {
                X = args.X - this.GlobalRectangle.X,
                Y = args.Y - this.GlobalRectangle.Y,
            };
            ChangeSliderPosition(p);
            flag |= true;
        }

        void Slider_MouseMove(ControlActionArgs args, ref bool flag)
        {
            var p = new Vector2
            {
                X = args.Position.X - this.GlobalRectangle.X,
                Y = args.Position.Y - this.GlobalRectangle.Y,
            };
            ChangeSliderPosition(p);
            flag |= true;
        }

        bool Slider_TouchDown(object sender, TouchEventArgs e)
        {
            var p = new Vector2
            {
                X = e.Position.X - this.GlobalRectangle.X,
                Y = e.Position.Y - this.GlobalRectangle.Y,
            };
            ChangeSliderPosition(p);
            IsDrag = true;
            return true;
        }

        void Slider_Touch(object sender, TouchEventArgs e)
        {
            IsDrag = false;
        }       

        void Slider_TouchMove(object sender, TouchEventArgs e)
        {
            if (IsDrag)
            {
                var p = new Vector2
                {
                    X = e.Position.X - this.GlobalRectangle.X,
                    Y = e.Position.Y - this.GlobalRectangle.Y,
                };
                ChangeSliderPosition(p);
            }
        }

        void ChangeSliderPosition(Vector2 position)
        {
            if(!IsVertical)
                CurrentValue = ((position.X - this.PaddingLeft) / (float)(GlobalRectangle.Width - this.PaddingLeft - this.PaddingRight - widthControlElement)) * (MaxValue - MinValue) + MinValue;
            else
                CurrentValue = ((position.Y - this.PaddingTop) / (float)(GlobalRectangle.Height - this.PaddingTop - this.PaddingBottom - widthControlElement)) * (MaxValue - MinValue) + MinValue;
            CurrentValue = MathUtil.Clamp(CurrentValue, MinValue, MaxValue);

            var r = this.Font.MeasureString(Text);
            var w = r.Width + 2 * this.PaddingLeft + widthControlElement;
            var h = r.Height + 2 * this.PaddingBottom + SliderWidth;

            Width = Math.Max(w, Width);
            Height = Math.Max(h, Height);

            OnChange?.Invoke(CurrentValue);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            int x;
            int y;
            int w;
            int h;
            int vw;

            var whiteTex = Game.RenderSystem.WhiteTexture;
            if (!IsVertical)
            {
                x = GlobalRectangle.X + this.PaddingLeft + widthControlElement;
                y = GlobalRectangle.Y + (GlobalRectangle.Height - SliderWidth) / 2;
                w = GlobalRectangle.Width - this.PaddingLeft - this.PaddingRight - 2 * widthControlElement;
                h = SliderWidth;
                vw = (int)(w * ((CurrentValue - MinValue) / (MaxValue - MinValue)));
                sb.Draw(whiteTex, new Rectangle(x, y, w, h), /*IsHovered ? HoverColor :*/ backColorForSlider, clipRectIndex);
                sb.Draw(whiteTex, new Rectangle(x, y, vw, h),/* IsHovered ?*/ HoverForeColor /*: ForeColor*/, clipRectIndex);

                if (PresetValues != null && DrawPreset)
                {
                    for (int i = 0; i < PresetValues.Count; i++)
                    {                 
                        var pos = (PresetValues[i] - MinValue) / (MaxValue - MinValue) * w +
                                  x;
                        sb.Draw(Image ?? whiteTex, pos - ApplicationInterface.ScaleMod, y - ApplicationInterface.ScaleMod * 2, ApplicationInterface.ScaleMod * 2, h + ApplicationInterface.ScaleMod * 4, PresetValues[i] < CurrentValue ? HoverForeColor : backColorForSlider);
                    }
                }

                if (Image != null)
                {
                    sb.Draw(Image, new Rectangle(x + vw - widthControlElement / 2, y - heightControlElement / 2, widthControlElement, heightControlElement), HoverForeColor, clipRectIndex);
                }

                
            }
            else
            {
                x = GlobalRectangle.X + (GlobalRectangle.Width - SliderWidth) / 2;
                y = GlobalRectangle.Y + this.PaddingLeft + widthControlElement;
                w = SliderWidth;
                h = GlobalRectangle.Height - this.PaddingTop - this.PaddingBottom - 2 * widthControlElement;
                vw = (int)(h * ((CurrentValue - MinValue) / (MaxValue - MinValue)));
                sb.Draw(whiteTex, new Rectangle(x, y, w, h), /*IsHovered ? HoverColor :*/ backColorForSlider, clipRectIndex);
                sb.Draw(whiteTex, new Rectangle(x, y, w, vw),/* IsHovered ?*/ HoverForeColor /*: ForeColor*/, clipRectIndex);
                if (Image != null)
                {
                    sb.DrawFreeUV(Image,
                        new Vector2(x - heightControlElement / 2, y + vw - widthControlElement / 2),
                        new Vector2(x - heightControlElement / 2 + heightControlElement, y + vw - widthControlElement / 2),
                        new Vector2(x - heightControlElement / 2, y + vw - widthControlElement / 2 + widthControlElement),
                        new Vector2(x - heightControlElement / 2 + heightControlElement, y + vw - widthControlElement / 2 + widthControlElement),

                        HoverForeColor,
                        new Vector2(0, 0),
                        new Vector2(0, 1),
                        new Vector2(1, 0),
                        new Vector2(1, 1), clipRectIndex
                    );
                }
            }


        }
    }
}
