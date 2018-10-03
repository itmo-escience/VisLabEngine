using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI
{
    public class RichTextBlock : ScalableFrame
    {
		protected RichTextBlock()
		{
		}
		public float UnitOffsetLine;

        /// <summary>
        /// Offset between lines
        /// </summary>
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
        public override string Tooltip { get; set; } = "";

        public RichTextBlock(
            FrameProcessor ui, 
            float x, float y, 
            float w, float h, 
            string text, Color backColor, UIConfig.FontHolder font, 
            float offsetLine, float minHeight = 0, 
            bool isShortText = false, int maxWidth = 200000 // Wider than any screen
        ) : base(ui, x, y, w, h, text, backColor)
        {            
            IsShortText    = isShortText;
            UnitOffsetLine = offsetLine;
            FontHolder     = font;
            MinUnitHeight  = minHeight;

            MaxWidth = maxWidth;
        }

        private bool _isInitialized = false;

        public new string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                _isInitialized = false;
            }
        }

        public virtual void init()
        {
            BaseHeight = Height;
            
            splitByString();

            var textOffset = _linesToDraw.Count * (Font.LineHeight + OffsetLine);
            Height = textOffset + Font.CapHeight;

            Height = Math.Min(Height, MinHeight);
            if (IsShortText)
            {
                Height = BaseHeight;
            }
            Height = !IsShortText ? textOffset : BaseHeight;
        }              

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!_isInitialized)
            {
                init();
                _isInitialized = true;
            }
        }

        private struct TextLine
        {
            public int Width;
            public string Text;
        }
        private List<TextLine> _linesToDraw;

        /// <summary>
        /// Last drawn line
        /// </summary>
        protected string LastText = "";

        protected virtual void splitByString()
        {
            if (Text == null)
            //if (LastLine == Text && _linesToDraw != null || Text == null)
            {
                // Nothing changed since last time or nothing to split
                return;
            }

            LastText = Text;
            _linesToDraw = new List<TextLine>();

            var words = Text.Replace("\n", " \n ").Replace("#", " #").Split(' ');
            var currentLine = "";
            var currentLineWidth = 0;
            var maxAvailableWidth = Math.Min(MaxWidth, Width);

            var spaceWidth = FontHolder[ApplicationInterface.uiScale].MeasureString(" ").Width;
            var remainingWords = new Queue<string>(words);

            while(remainingWords.Count > 0)
            {
                var word = remainingWords.Peek();

                // Forced line break found
                if (word.Equals("\n"))
                {
                    _linesToDraw.Add(new TextLine { Text = currentLine, Width = currentLineWidth });                    

                    currentLineWidth = 0;
                    currentLine = "";

                    // remove first of remaining words
                    remainingWords.Dequeue();
                    continue;
                }

                var wordWidth = FontHolder[ApplicationInterface.uiScale].MeasureString(word).Width;
                // There is enough space for this word or current word is longer than availableWidth
                if (wordWidth + currentLineWidth <= maxAvailableWidth || currentLineWidth == 0 && wordWidth > maxAvailableWidth)
                {
                    currentLine += word + " ";
                    currentLineWidth += wordWidth + spaceWidth;

                    // remove first of remaining words
                    remainingWords.Dequeue();
                }
                else // Need to start new line
                {
                    _linesToDraw.Add(new TextLine { Text = currentLine, Width = currentLineWidth });
                    currentLine = "";
                    currentLineWidth = 0;
                }                
            }
            _linesToDraw.Add(new TextLine { Text = currentLine, Width = currentLineWidth });

            MaxLineWidth = _linesToDraw.Max(l => l.Width);

            if (EnableClip)
            {
                X += (Width - MaxLineWidth)/2;
                Width = MaxLineWidth;                
            }
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            splitByString();

            var textOffset = 0;
            foreach (var line in _linesToDraw)
            {
                // Line with only spaces
                if (IsShortText && line.Text.All(c => c == ' '))
                    continue;

                if (IsShortText && textOffset > BaseHeight - OffsetLine * 2 - Font.CapHeight * 2)
                {
                    // TODO: WTF is that
                    Font.DrawString(sb, 
                        line.Text.Length > 3 ? line.Text.Remove(line.Text.Length - 3) + "..." : "...", 
                        GlobalRectangle.X, GlobalRectangle.Y + textOffset,
                        ForeColor, 0, 0, false);
                    break;
                }

                // TODO: WTF is that
                var offsetX = TextAlignment == Alignment.MiddleCenter
                    ? (this.GlobalRectangle.Width - line.Width) / 2
                    : 0;

                Font.DrawString(sb, line.Text,
                    GlobalRectangle.X + offsetX, GlobalRectangle.Y + textOffset,
                    ForeColor, clipRectIndex, 
                    0, false
                );

                textOffset += Font.LineHeight + OffsetLine;
            }
            Height = Math.Max(!IsShortText ? textOffset : BaseHeight, MinHeight);
        }
    }
}
