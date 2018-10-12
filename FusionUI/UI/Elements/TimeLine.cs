using System;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI.Elements
{
    public class TimeLine : ScalableFrame {
		protected TimeLine()
		{
		}
		private FrameProcessor ui;

        public Scroll LordOfTime;
        public ScalableFrame timeLabelTime;
        public ScalableFrame timeLabelDate;

        public ScalableFrame lineLineBeforeLord;
        public ScalableFrame lineLineAfterLord;

        public bool IsUpdateWithMove = false;

        public int emptySizeImage = 0;
        //        private StripLine stripLine;        

        public bool isInited = false;

        public AbstractTimeManager TimeManager;

        public DateTime MinTime {
            get { return TimeManager?.StartTime ?? DateTime.MinValue; }
            set {
                if(TimeManager!=null)
                    TimeManager.StartTime = value;
            }
        }

        private DateTime _maxTime;
        public DateTime MaxTime
        {
            get { return TimeManager?.EndTime ?? DateTime.MaxValue; }
            set {
                if (TimeManager != null)
                    TimeManager.EndTime = value;
            }
        }
		[XmlIgnore]
		public DiscTexture textureLord;

        private DateTime _currentTime;
        public DateTime CurrentTime
        {
            get { return _currentTime; }

            set
            {
                _currentTime = value;
                if(!LordOfTime.IsDrag && !LordOfTime.Selected)
                    UpdatePostionLord();                
            }
        }

        public TimeLine(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor, DiscTexture lordTexture=null, int emptyWidth = 0)
            : base(ui, x, y, w, h, text, backColor)
        {
            this.emptySizeImage = emptyWidth;
            this.textureLord = lordTexture;
            UnitHPadding = UIConfig.UnitTimelineOffsetX;    
            Init(ui);            
        }

        public void initTimeValue(AbstractTimeManager timeManager)
        {
            TimeManager = timeManager;
            this.CurrentTime = timeManager.CurrentTime;
            isInited = true;
        }

        public void UpdatePostionLord()
        {
            if (isInited)
            {
                //LordOfTime.UnitX = (float) (TimeToXPositon(CurrentTime) - LordOfTime.Image.Width / 2 / ScaleMultiplier);
                ////                stripLine.UnitX = LordOfTime.UnitX;
                LordOfTime.SetFromRelativeX(TimeToXPositon(CurrentTime));
                UpdatedTimeElement();
            }    
        }

        public float TimeToXPositon(DateTime time)
        {
            return (float)((time - TimeManager.StartTime).TotalSeconds / (TimeManager.EndTime - TimeManager.StartTime).TotalSeconds);
        }

        public float TimeToXPositonTL(DateTime time)
        {
            return UnitPaddingLeft + (UnitWidth - UnitPaddingLeft - UnitPaddingRight) * TimeToXPositon(time);
        }

        private void UpdatedTimeElement()
        {
            timeLabelTime.UnitX = LordOfTime.UnitX + LordOfTime.UnitWidth / 2 - UIConfig.UnitTimelineLabelWidth / 2;
            timeLabelDate.UnitX = LordOfTime.UnitX + LordOfTime.UnitWidth / 2 - UIConfig.UnitTimelineLabelWidth / 2;
            lineLineBeforeLord.UnitWidth = LordOfTime.UnitX - UnitPaddingLeft + LordOfTime.UnitWidth / 2;
        }        

        public DateTime XPositionToDateTime(float x)
        {
            var valueSecond =  (TimeManager.EndTime - TimeManager.StartTime).TotalSeconds * x;            
            var date = TimeManager.StartTime.AddSeconds(valueSecond);
            return date;
        }

        private void Init(FrameProcessor frameProcessor)
        {            
            ui = frameProcessor;
            if (textureLord == null)
            {
                textureLord = ui.Game.Content.Load<DiscTexture>("ui/icons_lord-of-time-without-wand");
            }
            LordOfTime = new Scroll(ui, 0, 0, UnitHeight, UnitHeight, "", Color.Zero)
            {
                IsFixedY = true,
                Image = textureLord,
                ImageColor = UIConfig.TimeLineColor1,
                ImageMode = FrameImageMode.Centered, 
            };

            var font = ui.DefaultFont;
            var yLabel = 0;
            timeLabelTime = new ScalableFrame(ui, LordOfTime.UnitX + LordOfTime.UnitWidth/2 - UIConfig.UnitTimelineLabelWidth/2, yLabel > 0 ? yLabel : 1.5f, UIConfig.UnitTimelineLabelWidth, font.CapHeight / ScaleMultiplier, "Time", Color.Zero)
            {
                ForeColor = UIConfig.TimeLineColor1,
//                Border = 1,
//                BorderColor = new Color(110, 110, 110),
                TextAlignment = Alignment.MiddleCenter,
                FontHolder = UIConfig.FontCaption,                
                Ghost = true
            };

            var yLabelDate = this.UnitHeight - font.CapHeight / ScaleMultiplier;
            timeLabelDate = new ScalableFrame(ui, LordOfTime.UnitX + LordOfTime.UnitWidth / 2 - UIConfig.UnitTimelineLabelWidth / 2, yLabelDate, UIConfig.UnitTimelineLabelWidth, font.CapHeight / ScaleMultiplier, "Date", Color.Zero)
            {
                //                Border = 1,
                //                BorderColor = new Color(110, 110, 110),
                ForeColor = UIConfig.TimeLineColor1,
                TextAlignment = Alignment.MiddleCenter,
                FontHolder = UIConfig.FontCaption,
                Ghost = true
            };

            //            var widthStripLine = 3;
            //            var xStripStart = LordOfTime.UnitX + LordOfTime.UnitWidth / 2 - widthStripLine / 2;
            //            stripLine = new StripLine(ui, new Vector2(xStripStart, timeLabelTime.Y),
            //            new Vector2(xStripStart, LordOfTime.Y + textureLord.UnitHeight / 2))
            //                    {
            //                        widthLine = widthStripLine,
            //                        lineColor = Color.White,
            //                        lengthGap = 1,
            //                        lengthStrip = 2
            //                    };


            var thinknessLine = UIConfig.UnitTimelineTickness;
            lineLineBeforeLord = new ScalableFrame(ui, UnitPaddingLeft , this.UnitHeight/2 - thinknessLine/2, LordOfTime.UnitX + emptySizeImage / ScaleMultiplier, thinknessLine, "", UIConfig.TimeLineColor1)
            {
                //Anchor = FrameAnchor.Bottom | FrameAnchor.Left | FrameAnchor.Right,
                Ghost = true,
                ZOrder = 3,
            };

            lineLineAfterLord = new ScalableFrame(ui, UnitPaddingLeft, this.UnitHeight/2 - thinknessLine/2, UnitWidth - UnitPaddingLeft - UnitPaddingRight, thinknessLine, "", UIConfig.TimeLineColor2)
            {
                //Anchor = FrameAnchor.Bottom | FrameAnchor.Left | FrameAnchor.Right,
                Ghost = true,
                ZOrder = 2,
            };

            LordOfTime.SuppressActions = true;
            //LordOfTime.ActionDrag += (ControlActionArgs args, ref bool flag) => {
            //    var positionX = Math.Min(LordOfTime.GlobalRectangle.X, GlobalRectangle.X + GlobalRectangle.Width);
            //    positionX = Math.Max(positionX, GlobalRectangle.X + emptySizeImage);

            //    //                Console.WriteLine(args.X + " -> " + (GlobalRectangle.X + GlobalRectangle.Width));
            //    if (TimeManager!=null && (IsUpdateWithMove | TimeManager.UpdateOnDrag))
            //        TimeManager.CurrentTime = XPositionToDateTime((positionX - GlobalRectangle.X - emptySizeImage) / ScaleMultiplier);
            //    UpdatedTimeElement();
            //    flag = true;
            //};
            LordOfTime.actionForMoveRelative += (x, y) =>
            {
                if (TimeManager != null && (IsUpdateWithMove | TimeManager.UpdateOnDrag))
                    TimeManager.CurrentTime = XPositionToDateTime(x);
                UpdatedTimeElement();
            };

            this.Add(lineLineAfterLord);
            this.Add(lineLineBeforeLord);
            

            this.Add(timeLabelTime);
            this.Add(timeLabelDate);
            this.Add(LordOfTime);

        }

        private Vector2 previousMousePosition;
        // TODO: go to frame

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
