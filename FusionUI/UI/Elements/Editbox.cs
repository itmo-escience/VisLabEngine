using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;
using Keys = Fusion.Engine.Input.Keys;

namespace FusionUI.UI.Elements
{
    public class Editbox : ScalableFrame
    {

		protected Editbox()
		{
		}
		public string Label;

        private bool isFixWidth = true;
        private bool isHovered = false;
        private bool isActive = false;
        public bool IsActive => isActive && ActiveInHierarchy;
        public int FontSize = 3;

        public bool HidePassword = false;
        public string AltText = null;


        public Color HoverColor;
        public Color BorderActive;

        private int PositionSubstr = 0;
        const float carretBlinkTime = 0.4f;
        private bool inBound = true;


        public Editbox(FrameProcessor ui) : base(ui)
        {
            Text = "";
            init();
        }

        public Editbox(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public void init()
        {
            this.Game.GameInterface.Game.Keyboard.FormKeyPress += EditBox_KeyPress;
            this.Game.GameInterface.Game.Keyboard.KeyDown += EditBox_KeyDown;

			this.StatusChanged += (s, e) =>
            {
                isHovered = e.Status == FrameStatus.Hovered;
            };

            this.ActionClick += (ControlActionArgs args, ref bool flag) => 
            {
                if (args.IsClick)
                {
                    if (!isActive)
                    {
                        SetActiveStatus(true);
                    }
                    else
                    {
                        var pos = args.X - GlobalRectangle.X;
                        if (pos > Font.MeasureString(Text).Width) carretPos = Text.Length;
                        else
                        {
                            int i = Text.Length / 2;
                            int l = 0;
                            int r = Text.Length;
                            while (r - l > 1)
                            {
                                var w = Font.MeasureString(Text.Substring(0, i)).Width;
                                if (w < pos)
                                {
                                    l = i;
                                    i = (r + i) / 2;
                                    r = r;
                                }
                                else
                                {
                                    l = l;
                                    r = i;
                                    i = (l + i) / 2;
                                    
                                }
                                if (r - l <= 1)
                                {
                                    carretPos = i;
                                    break;                                    
                                }
                            }
                        }
                    }

                }
            };
        }

        public void SetActiveStatus(bool forceValue)
        {
            if (isActive && !forceValue)
                OnPropertyChanged("Text");
            if (forceValue) Selected = true;
            isActive = forceValue;            
        }

        private void changeActiveStatus()
        {
            isActive = !isActive;
            if (isActive) Selected = true;
            if (!isActive)
                OnPropertyChanged("Text");
        }


        /// <summary>
        /// Key-down event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EditBox_KeyDown(object sender, Fusion.Engine.Input.KeyEventArgs e)
        {
            if (!IsActive)
                return;
            counter = carretBlinkTime / 2;
            if (e.Key == Keys.Left)
            {
                carretPos = carretPos - 1;
            }
            if (e.Key == Keys.Right)
            {
                carretPos = carretPos + 1;
            }
            if (e.Key == Keys.Home)
            {
                carretPos = 0;
            }
            if (e.Key == Keys.End)
            {
                carretPos = int.MaxValue;
            }
            if (e.Key == Keys.Delete && carretPos < Text.Length)
            {
                Text = Text.Remove(carretPos, 1);
            }

			if (e.Key == Keys.V && this.Game.GameInterface.Game.Keyboard.IsKeyDown(Keys.LeftControl)) {
				IDataObject iData = System.Windows.Forms.Clipboard.GetDataObject();

				if (iData.GetDataPresent(DataFormats.Text) || iData.GetDataPresent(DataFormats.Text)) {
					string text = (string)iData.GetData(DataFormats.Text);
					Text = Text.Insert(carretPos, text);
					carretPos = carretPos + text.Length;
				}
			}

        }


        /// <summary>
        /// Key-press event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EditBox_KeyPress(object sender, KeyPressArgs e)
        {
            if (!IsActive || Game.GameInterface.Game.Keyboard.IsKeyDown(Keys.LeftControl))
                return;
            counter = carretBlinkTime / 2;

            if (e.KeyChar == '\b')
            {
                if (carretPos > 0)
                {
                    Text = Text.Remove(carretPos - 1, 1);
                    carretPos = carretPos - 1;
                }
                return;
            }

            if (e.KeyChar == '\t' || e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                return;
            }

            Text = Text.Insert(carretPos, new string(e.KeyChar, 1));

            carretPos++;
        }




        int _carretPos;

        int carretPos
        {
            get
            {
                return _carretPos;
            }
            set
            {
                _carretPos = MathUtil.Clamp(value, 0, Text.Length);
            }
        }


        bool showCarret;
        float counter = 0;


        /// <summary>
        /// Update
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            //if(!this.Equals(this.ui.TargetFrame) && isActive)
            //    changeActiveStatus();
            if (!isFixWidth)
            {
                var r = this.Font.MeasureString(Text);

                var w = r.Width + 2 * PaddingLeft;
                var h = r.Height + 2 * PaddingBottom;

                Width = Math.Max(w, Width);
                Height = Math.Max(h, Height);
            }


            //
            //	Handle carret blink
            //
            counter -= gameTime.ElapsedSec;

            if (counter < 0)
            {
                counter = carretBlinkTime;
            }
            showCarret = counter <= carretBlinkTime / 2;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentOpacity"></param>
        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            bool showAltText = false;

            var value = Text.Substring(PositionSubstr);

            if (HidePassword)
            {
                value = new string((char)0x2022, value.Length);
            }

            if (!string.IsNullOrEmpty(AltText) && string.IsNullOrEmpty(Text) && !IsActive)
            {
                value = AltText;
                showAltText = true;
            }
            carretPos = Math.Min(carretPos, value.Length);
            var sizeText = this.Font.MeasureString(value);
            int carretOffs = this.Font.MeasureString(value.Substring(0, carretPos)).Width;

            var gr = GlobalRectangle;
            int labelHeight = 0;
            if (!string.IsNullOrEmpty(Label))
            {
                labelHeight = this.Font.CapHeight * 3 / 2;
                this.Font.DrawString(sb, Label, gr.X + PaddingLeft, gr.Y, Color.White, clipRectIndex, 0, false);
            }


            var texWhite = this.Game.RenderSystem.WhiteTexture;


            if (IsActive)
            {
                sb.Draw(texWhite, gr.X, gr.Y + labelHeight, gr.Width, gr.Height - labelHeight, HoverColor, clipRectIndex);
                if (showCarret)
                {
                    sb.Draw(texWhite, gr.X + PaddingLeft + carretOffs, gr.Y + PaddingBottom + labelHeight, 2, sizeText.Height, Color.Black, clipRectIndex);
                }
            }
            
            this.Font.DrawString(sb, value, gr.X + PaddingLeft, gr.Y + PaddingBottom + labelHeight, showAltText ? Color.Gray : IsActive ? Color.Black : Color.White, clipRectIndex, 0, false);

            sb.Draw(texWhite, (float)gr.X, (float)gr.Y + labelHeight, (float)gr.Width, (float)BorderTop, BorderActive, clipRectIndex);
            sb.Draw(texWhite, (float)gr.X, (float)(gr.Y + gr.Height - BorderBottom), (float)gr.Width, (float)BorderBottom, BorderActive, clipRectIndex);
            sb.Draw(texWhite, (float)gr.X, (float)(gr.Y + BorderTop + labelHeight), (float)BorderLeft, (float)(gr.Height - labelHeight - BorderTop - BorderBottom), BorderActive, clipRectIndex);
            sb.Draw(texWhite, (float)(gr.X + gr.Width - BorderRight), (float)(gr.Y + BorderTop + labelHeight), (float)BorderRight, (float)(gr.Height - labelHeight - BorderTop - BorderBottom), BorderActive, clipRectIndex);
        }

        private int getCountSub(string text)
        {
            var x = (int)((Font.MeasureString(text).Width - this.Width) / this.Font.SpaceWidth);
            return x;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
