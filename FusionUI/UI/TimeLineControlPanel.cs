using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Common;
using FusionUI.UI.Elements;

namespace FusionUI.UI
{
    public class TimeLineControlPanel : FreeFrame {

        private FrameProcessor ui;

        private AbstractTimeManager timeManager;

        public AbstractTimeManager TimeManager
        {
            get { return timeManager; }
            set
            {
                timeManager = value;
                buttonLoop.ToggleOnOff(!timeManager.IsAutoRewind, true);
                buttonPlay.ToggleOnOff(timeManager.State == AbstractTimeManager.TimeState.Stop);
                timeLine.TimeManager = timeManager;
                updateStep();
                initTimeLine();
            }
        }

        public TimeLine timeLine;

        float sizeButton = UIConfig.UnitMenuButtonWidth;
        public Button buttonStepBack;
        public Button buttonPlay;
        public Button buttonStepForward;
        public Button buttonLoop;

        public Button buttonEnsembles;
        public Button buttonMinimize;

        public String templateDate = "dd/MM/yyyy";
        public ScalableFrame labelStartTime;
        public ScalableFrame labelEndTime;
        public ScalableFrame stepValueLabel;

        public Button upStepButton;
        public Button downStepButton;

        public ScalableFrame NameLabel;
        public string NameLabelText;

		bool addToolButtons;


		public int currentStep = 0;
        public List<Tuple<double, string>> listStepName = new List<Tuple<double, string>>()
        {
            new Tuple<double, string>(1, "1h"),
            new Tuple<double, string>(3, "3h"),
            new Tuple<double, string>(6, "6h"),
            new Tuple<double, string>(12, "12h"),
            new Tuple<double, string>(24, "1d"),
            new Tuple<double, string>(48, "2d"),
            new Tuple<double, string>(4*24, "4d"),
            new Tuple<double, string>(7*24, "1w"),
            new Tuple<double, string>(14*24, "2w"),
            new Tuple<double, string>(30*24, "1m"),
        };

        public bool ShowStartAndEndDate = true;
        

        public TimeLineControlPanel(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor, bool addToolButtons = true) : base(ui, x, y, w, h, text, backColor)
        {
            NameLabelText = text;
            Text = "";
            this.ui = ui;
            ((Frame) this).Ghost = false;
			this.addToolButtons = addToolButtons;

			Init();
            SuppressActions = true;
            //Anchor = FrameAnchor.Bottom | FrameAnchor.Left | FrameAnchor.Right;

            this.buttonStepBack.ButtonAction += (b) => { this.timeManager?.StepBackward(); };
            this.buttonPlay.ButtonAction += (b) => {
                if (b)
                {
                    buttonPlay.Tooltip = "Pause";
                    this.timeManager?.PlayForward();
                }
                else
                {
                    buttonPlay.Tooltip = "Play";
                    this.timeManager?.Stop();
                }
            };
            this.buttonStepForward.ButtonAction += (b) => { this.timeManager?.StepForward(); };
            //            TimeLineControlPanel.buttonStepBack.Click += (sender, args) => { timeManager.StepBackward(); };

            this.buttonLoop.ButtonAction += b => { timeManager.IsAutoRewind = b; };

            this.downStepButton.ButtonAction += (b) =>
            {
                this.setPrevStep();
            };
            this.upStepButton.ButtonAction += (b) =>
            {
                this.setNextStep();
            };
        }

        private void Init()
        {
            var lordTexture = ui.Game.Content.Load<DiscTexture>("ui/timeline/fv-icons_timeline-slider.png");
            timeLine = new TimeLine(ui, 0, 0, this.UnitWidth,
                UIConfig.UnitTimelineHeight, "", Color.Zero, lordTexture, 4);
            this.Add(timeLine);
            timeLine.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsClick) return;
                if (timeManager != null)
                timeLine.LordOfTime.XVal = args.X - (float)(timeLine.LordOfTime.Width) / 2 - timeLine.GetBorderedRectangle().X;
                timeLine.LordOfTime.UpdatePosition();
                //timeManager.CurrentTime = timeLine.XPositionToDateTime(timeLine.LordOfTime.GetRelative().X);                
            };
            timeLine.LordOfTime.ActionLost += (ControlActionArgs args, ref bool flag) =>
            {
                var date = timeLine.XPositionToDateTime(timeLine.LordOfTime.GetRelativeX());
                if (timeManager != null)
                    timeManager.CurrentTime = date;
            };


            createControlButtons(this.UnitWidth/2 - sizeButton*5/2, UIConfig.UnitTimelineHeight, this.addToolButtons);
            //this.Resize += TimeLineControlPanel_Resize;

            var sizeText = Font.MeasureString(templateDate);
            labelStartTime = new ScalableFrame(ui, UIConfig.UnitTimelineOffsetX, timeLine.UnitY + timeLine.UnitHeight - (sizeText.Height / ScaleMultiplier) + 1,
                (sizeText.Width / ScaleMultiplier), (sizeText.Height / ScaleMultiplier), "", Color.Zero)
            {
                //Anchor = FrameAnchor.Left
                FontHolder = UIConfig.FontCaption,
                TextAlignment = Alignment.MiddleLeft
            };
            labelEndTime = new ScalableFrame(ui, this.UnitWidth - UIConfig.UnitTimelineOffsetX - (sizeText.Width / ScaleMultiplier),
                timeLine.UnitY + timeLine.UnitHeight - (sizeText.Height / ScaleMultiplier) + 1,
                (sizeText.Width / ScaleMultiplier), (sizeText.Height / ScaleMultiplier), "", Color.Zero)
            {
                //Anchor = FrameAnchor.Right
                FontHolder = UIConfig.FontCaption,
                TextAlignment = Alignment.MiddleRight
            };

            NameLabel = new ScalableFrame(ui, UIConfig.UnitTimelineOffsetX, UIConfig.UnitTimelineHeight, this.UnitWidth / 2 - sizeButton * 5 / 2 - UIConfig.UnitTimelineOffsetX, sizeButton, NameLabelText, Color.Zero)
            {
                Ghost = true,
                TextAlignment = Alignment.MiddleLeft,
                ForeColor = new Color(255, 255, 255, 205),
                FontHolder = UIConfig.FontTitle,
            };

            this.Add(NameLabel);
            this.Add(labelStartTime);
            this.Add(labelEndTime);
//            this.Add(stepValueLabel, true);

        }        

        private void createControlButtons(float x, float y, bool addToolButtons)
        {
            downStepButton    =   createClickableButton(x + 0 * sizeButton, y, @"UI\timeline\fv-icons_playback-slow");            
            buttonStepBack    =   createClickableButton(x + 1 * sizeButton, y, @"UI\timeline\fv-icons_playback-stp-bckw");
            buttonPlay        =  createToggleableButton(x + 2 * sizeButton, y, @"UI\timeline\fv-icons_playback-pause", @"UI\timeline\fv-icons_playback-play", true,"",false, 200);
            buttonStepForward =   createClickableButton(x + 3 * sizeButton, y, @"UI\timeline\fv-icons_playback-step-fwd");
            upStepButton      =   createClickableButton(x + 4 * sizeButton, y, @"UI\timeline\fv-icons_playback-fast");
            stepValueLabel    =        createLableFrame(x + 5 * sizeButton, y, listStepName[currentStep].Item2);
            buttonLoop        =  createToggleableButton(x + 6 * sizeButton, y, @"UI\timeline\fv-icons_playback-loop", @"UI\timeline\fv-icons_playback-loop");


            downStepButton.Tooltip = "Slow playback";
            buttonStepBack.Tooltip = "Jump Back";
            buttonPlay.Tooltip = "Play/Pause";
            buttonStepForward.Tooltip = "Jump forward";
            upStepButton.Tooltip = "Fasten playback";
            stepValueLabel.Tooltip = "Current speed";
            buttonLoop.Tooltip = "Auto replay";


			if (addToolButtons)
			{
				buttonEnsembles = createClickableButton(this.UnitWidth - UIConfig.UnitTimelineOffsetX - 2 * sizeButton, y, @"UI\timeline\fv-icons_ensembles");
				buttonMinimize = createClickableButton(this.UnitWidth - UIConfig.UnitTimelineOffsetX - 1 * sizeButton, y, @"UI\timeline\fv-icons_timeline-minimize");
				buttonEnsembles.Tooltip = "Ensembles";
				buttonMinimize.Tooltip = "Minimize ui"; 
			}
            
            this.Resize += (sender, args) =>
            {
                x = this.UnitWidth / 2 - sizeButton * 5 / 2;
                downStepButton    .UnitX = x + 0 * sizeButton;
                buttonStepBack    .UnitX = x + 1 * sizeButton;
                buttonPlay        .UnitX = x + 2 * sizeButton;
                buttonStepForward .UnitX = x + 3 * sizeButton;
                upStepButton      .UnitX = x + 4 * sizeButton;
                stepValueLabel    .UnitX = x + 5 * sizeButton;
                buttonLoop        .UnitX = x + 6 * sizeButton;
            };
        }

        private Button createClickableButton(float x, float y, string nameTexture, bool IsVisible = true, string currentStep="")
        {
            var button = new Button(ui, x, y, sizeButton, sizeButton, "", Color.Zero, UIConfig.ActiveColor, 200)
            {
//                Border = 1,
                Image = nameTexture != null ? ui.Game.Content.Load<DiscTexture>(nameTexture) : null,                
                TextAlignment = Alignment.MiddleCenter,
                Ghost = !IsVisible,
                Visible = IsVisible,
                Text = currentStep,
                ImageColor = UIConfig.TimeLineIconColor,
                ActiveFColor = UIConfig.HighlightTextColor,
                InactiveFColor = UIConfig.HighlightTextColor,
            };
            this.Add(button);
            return button;
        }

        private ScalableFrame createLableFrame(float x, float y, string currentStep = "")
        {
            var button = new ScalableFrame(ui, x, y, sizeButton, sizeButton, currentStep, Color.Zero)
            {
                TextAlignment = Alignment.MiddleCenter,
                Ghost = true,
            };
            this.Add(button);
            return button;
        }

        private Button createToggleableButton(float x, float y, string activeTexture = null, string inactiveTexture = null, bool IsVisible = true, string currentStep = "", bool active = false, int transitionTime = 0)
        {            
            var button = new Button(ui, x, y, sizeButton, sizeButton, "", UIConfig.ActiveColor, UIConfig.InactiveColor, activeTexture != null ? ui.Game.Content.Load<DiscTexture>(activeTexture) : null, inactiveTexture != null ? ui.Game.Content.Load<DiscTexture>(inactiveTexture) : null, null, active, transitionTime)
            {
                //                Border = 1,
                TextAlignment = Alignment.MiddleCenter,
                Ghost = !IsVisible,
                Visible = IsVisible,
                Text = currentStep,
                ImageColor = UIConfig.TimeLineIconColor,
                ActiveFColor = UIConfig.HighlightTextColor,
                InactiveFColor = UIConfig.HighlightTextColor,
            };
            this.Add(button);
            return button;
        }

        public void initTimeLine()
        {
            labelStartTime.Text = timeManager.StartTime.ToString(templateDate);
            labelEndTime.Text = timeManager.EndTime.ToString(templateDate);
            timeLine.initTimeValue(timeManager);            
        }

        public virtual void SetTimeManager(AbstractTimeManager timeManager)
        {
            this.TimeManager = timeManager;
            updateStep();
            initTimeLine();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (timeManager != null) timeLine.CurrentTime = timeManager.CurrentTime;
            if(timeLine.timeLabelTime!=null && timeManager!=null)
                timeLine.timeLabelTime.Text = timeManager.TimeFunc?.Invoke() ?? String.Format("{0:D2}:{1:D2}", timeManager.CurrentTime.Hour, timeManager.CurrentTime.Minute);
            if (timeLine.timeLabelDate != null && timeManager != null)
                timeLine.timeLabelDate.Text = timeManager.DateFunc?.Invoke() ?? String.Format("{0:dd.MM.yyyy}", timeManager.CurrentTime.Date);
            if (ShowStartAndEndDate)
            {
                if (timeManager != null)
                {
                    labelStartTime.Text = timeManager.StartTime.ToString(templateDate);
                    labelEndTime.Text = timeManager.EndTime.ToString(templateDate);
                }
                labelStartTime.Visible = !timeLine.timeLabelDate.GlobalRectangle.Intersects(labelStartTime.GlobalRectangle);
                labelEndTime.Visible = !timeLine.timeLabelDate.GlobalRectangle.Intersects(labelEndTime.GlobalRectangle);
            }
        }

        public virtual void setNextStep()
        {
            if (listStepName.Count > currentStep + 1)
            {
                currentStep++;
                updateStep();
            }
        }

        public void setPrevStep()
        {
            if (currentStep > 0)
            {
                currentStep--;
                updateStep();
            }
        }

        protected virtual void updateStep()
        {
            if (timeManager == null) return;
            var stepConfig = listStepName[currentStep];
            timeManager.TimeStep	= TimeSpan.FromHours(stepConfig.Item1);
            stepValueLabel.Text		= stepConfig.Item2;
        }

		public void AddSideLables(string lableLeftText, string lableRightText )
		{
			var leftTimeLineLable = new Elements.TextFormatting.FormatTextBlock(ui, 0, UIConfig.UnitTimelineHeight, 60, 6, lableLeftText, Color.Zero, this.FontHolder, 0)
			{
				TextAlignment = Alignment.MiddleLeft,
			};
			var rightTimeLineLable = new Elements.TextFormatting.FormatTextBlock(ui, this.UnitWidth - 60, UIConfig.UnitTimelineHeight, 60, 6, lableRightText, Color.Zero, this.FontHolder, 0)
			{
				TextAlignment = Alignment.MiddleCenter,
			};
			this.Add(leftTimeLineLable);
			this.Add(rightTimeLineLable);

		}
	}
}
