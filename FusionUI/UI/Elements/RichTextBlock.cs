using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI
{
    public class RichTextBlock : ScalableFrame
    {
        public float UnitOffsetLine;
        public int OffsetLine
        {
            get { return (int)(UnitOffsetLine * ScaleMultiplier); }
            set { UnitOffsetLine = value / ScaleMultiplier; }
        }

        public bool IsShortText = false;
        private float BaseUnitHeight = 0;

        public int BaseHeight
        {
            get { return (int) (BaseUnitHeight*ScaleMultiplier); }
            set { BaseUnitHeight = value/ScaleMultiplier; }
        }

        public float MinUnitHeight = 0;

        public int MinHeight
        {
            get { return (int)(MinUnitHeight * ScaleMultiplier); }
            set { MinUnitHeight = value / ScaleMultiplier; }
        }

        public int MaxWidth;

        public int MaxLineWidth;

        public bool EnableClip = false;

        private string tooltip = "";
        public override string Tooltip { get { return tooltip; } set { tooltip = value; } }

        public RichTextBlock(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor, UIConfig.FontHolder font, float offsetLine, float minHeight = 0, bool isShortText = false, int maxWidth = 0) : base(ui, x, y, w, h, text, backColor)
        {
            MaxWidth = maxWidth > Width ? maxWidth : Width;
            this.IsShortText = isShortText;
            this.UnitOffsetLine = offsetLine;
            this.FontHolder = font;
            this.MinUnitHeight = minHeight;
            
        }

        private bool isInit = false;

        public new string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                isInit = false;
            }
        }

        public virtual void init()
        {
            BaseHeight = this.Height;
            
            splitByString();
            var textOffset = strForDraw.Count * (this.Font.LineHeight + OffsetLine);
            //foreach (var str in strForDraw)
            //{
            //    textOffset += this.Font.CapHeight + OffsetLine;
            //}
            this.Height = textOffset + this.Font.CapHeight;

            this.Height = Math.Min(this.Height, this.MinHeight);
            if (IsShortText)
            {
                this.Height = BaseHeight;
            }
            this.Height = !IsShortText ? textOffset : BaseHeight;
        }

        protected new List<Tuple<Rectangle, string>> strForDraw;
        protected string lastString = "";

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!isInit)
            {
                init();
                isInit = true;
            }
        }

        protected virtual void splitByString()
        {
            if (lastString == Text && strForDraw != null || Text==null) return;
            MaxLineWidth = 0;
            lastString = Text;
            strForDraw = new List<Tuple<Rectangle, string>>();
            var words = Text.Replace("\n", " \n ").Replace("#", " #").Split(' ');
            bool haveMoreOneWord = false;
            var currentStr = "";
            var currentStrSize = 0;
            var currentStrSizeRect = Rectangle.Empty;

            foreach (var word in words)
            {
                if (word.Equals("\n"))
                {
                    strForDraw.Add(new Tuple<Rectangle, string>(currentStrSizeRect, currentStr));
                    MaxLineWidth = currentStrSizeRect.Width > MaxLineWidth ? currentStrSizeRect.Width : MaxLineWidth;
                    currentStrSize = 0;
                    currentStr = "";
                    currentStrSizeRect = Rectangle.Empty;
                    continue;
                }

                var sizeText = this.FontHolder[ApplicationInterface.uiScale].MeasureString(word);
                if (sizeText.Width + currentStrSize > MaxWidth)
                {
                    if (!haveMoreOneWord)
                    {
                        strForDraw.Add(new Tuple<Rectangle, string>(sizeText, word));
                        MaxLineWidth = sizeText.Width > MaxLineWidth ? sizeText.Width : MaxLineWidth;
                        currentStrSize = 0;
                        currentStr = "";
                        currentStrSizeRect = Rectangle.Empty;
                    }
                    else
                    {
                        strForDraw.Add(new Tuple<Rectangle, string>(currentStrSizeRect, currentStr));
                        MaxLineWidth = currentStrSizeRect.Width > MaxLineWidth ? currentStrSizeRect.Width : MaxLineWidth;
                        currentStr = word + " ";
                        currentStrSize = sizeText.Width;
                        currentStrSizeRect = Rectangle.Empty;
                        currentStrSizeRect.Width = currentStrSize;
                    }
                }
                else
                {
                    haveMoreOneWord = true;
                    currentStr += word + " ";
                    currentStrSize += sizeText.Width + this.Font.SpaceWidth;
                    currentStrSizeRect.Width = currentStrSize;
                }

            }
            strForDraw.Add(new Tuple<Rectangle, string>(currentStrSizeRect, currentStr));
            MaxLineWidth = currentStrSizeRect.Width > MaxLineWidth ? currentStrSizeRect.Width : MaxLineWidth;
            if (EnableClip)
            {
                this.X += (Width - MaxLineWidth)/2;
                this.Width = MaxLineWidth;
                
            }
        }

        protected virtual void DrawString(SpriteLayer spriteBatch, string text, float xPos, float yPos, Color color,
            int frameIndex = 0, float tracking = 0, bool useBaseLine = true, bool flip = false)
        {
            this.Font.DrawString(spriteBatch, text, xPos, yPos,
                color, frameIndex, tracking, useBaseLine, flip);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            splitByString();
            var textOffset = 0;
            foreach (var str in strForDraw)
            {
                if (IsShortText && str.Item2.Replace(" ", "").Equals(""))
                    continue;
                if (IsShortText && textOffset > BaseHeight - OffsetLine * 2 - this.Font.CapHeight * 2)
                {
                    this.Font.DrawString(sb, str.Item2.Length > 3 ? str.Item2.Remove(str.Item2.Length - 3) + "..." : "...", this.GlobalRectangle.X, this.GlobalRectangle.Y + textOffset,
                        ForeColor, 0, 0, false);
                    break;
                }
                var offsetX = TextAlignment == Alignment.MiddleCenter
                    ? (this.GlobalRectangle.Width - str.Item1.Width) / 2
                    : 0;
                DrawString(sb, str.Item2, this.GlobalRectangle.X + offsetX, this.GlobalRectangle.Y + textOffset,
                    ForeColor, clipRectIndex, 0, false);
                textOffset += this.Font.LineHeight + OffsetLine;
            }
            this.Height = !IsShortText ? textOffset : BaseHeight;
        }
    }
}
