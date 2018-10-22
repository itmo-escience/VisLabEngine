using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Plots2_0
{
    public class PlotViewer : PlotViewer<PlotCanvas>
    {
        public PlotViewer(FrameProcessor ui, float x, float y, float w, float h, Color backColor, AbstractTimeManager tm = null, PlotCanvas plot = null) : base(ui, x, y, w, h, backColor, tm, plot, null)
        {
        }
    };

    public class PlotViewer<TP> : FreeFrame where TP : PlotCanvas
    {
        public PlotCanvas Plot;        

        public List<PlotScale> Scales = new List<PlotScale>();

        public PlotLegend Legend;

        public bool ShowScaleX = true, ShowScaleY = true;

        public Func<FrameProcessor, PlotCanvas, PlotScale> scaleConstruct;        
        public PlotViewer(FrameProcessor ui, float x, float y, float w, float h, Color backColor,
            AbstractTimeManager tm = null, TP plot = null, Func<FrameProcessor, TP> plotConstruct = null, Func<FrameProcessor, PlotCanvas, PlotScale> scaleConstruct = null) : base(ui, x, y, w, h, "", backColor)
        {
            if (plot == null)
                Plot = plotConstruct != null ? plotConstruct(ui) : new PlotCanvas(ui, 0, 0, 100, 100, Color.Zero);            
            else Plot = plot;

            this.scaleConstruct = scaleConstruct ?? new Func<FrameProcessor, PlotCanvas, PlotScale>((ui1, pc) => new PlotScale(ui1, pc));

            this.Add(Plot);
            Plot.DataContainer = new PlotContainer();
            AddLegend();
            Dirty = true;            
        }

        public virtual void Init()
        {

        }

        

        public virtual void AddLegend()
        {
            Legend = new PlotLegend(ui, 0, 0, 0, 0, "", Color.Zero)
            {
                PlotData = this.Plot.DataContainer,
                textFunc = a => a,               
            };
            this.Add(Legend);
        }

        public bool Dirty;
        public bool IsInit = false;
        public bool IsTime;
        protected List<double> predefinedTimeSteps = new List<double>() {1.0/1440, 1.0 /288, 1.0/144, 1.0/48, 1.0/24, 1.0/12, 1.0/4, 1.0/2, 1, 2, 5, 10, 30, 60, 180, 365, 365*2, 365*5, 365*10};

        public ScaleParams ScaleSettings = ScaleParams.DrawMeasureX | ScaleParams.DrawMeasureY | ScaleParams.DrawZeroLineX | ScaleParams.DrawZeroLineY;
        private UIConfig.FontHolder scaleFont = UIConfig.FontBody;

        public UIConfig.FontHolder ScaleFont
        {
            get { return scaleFont; }
            set
            {
                scaleFont = value;
                Dirty = true;
            }
        }

        private float scaleStepX = 20;
        private float scaleStepY = 10;

        public float ScaleStepX
        {
            get => scaleStepX;
            set
            {
                scaleStepX = value;
                Dirty = true;
            }
        }

        public float ScaleStepY
        {
            get => scaleStepY;
            set
            {
                scaleStepY = value;
                Dirty = true;
            }
        }

        private Color scaleFontColor = UIConfig.ActiveTextColor;

        public Color ScaleFontColor
        {
            get => scaleFontColor;
            set
            {
                scaleFontColor = value;
                Dirty = true;
            }
        }
        

        public virtual void GenerateScales()
        {
            foreach (var s in Scales)
            {
                this.Remove(s);
            }

            Scales.Clear();
            bool first = true;
            var scales = new Dictionary<string, PlotScale>();
            foreach (var variable in Plot.DataContainer.Data)
            {
                var name = variable.Value.Name;
                if (!(first && ShowScaleX || ShowScaleY)) break;
                if (!scales.ContainsKey(name))
                {

                    scales[name] = scaleConstruct(ui, Plot); //new PlotScale(ui, Plot)
                    //{
                    scales[name].Settings = (ShowScaleY
                                                ? ScaleParams.DrawScaleY | ScaleParams.UseFixedStepY
                                                : 0) |
                                            (first && ShowScaleX
                                                ? ScaleParams.DrawScaleX | ScaleParams.UseFixedStepX |
                                                  ScaleParams.NoActiveCheckX
                                                : 0) |
                                            ScaleSettings;
                    scales[name].Dirty = true;
                    scales[name].YLabel = name;
                    scales[name].Measure = variable.Value.Units;
                    scales[name].XLabel = "Time";
                    scales[name].ZOrder = 1000;
                    scales[name].FontHolder = scaleFont;
                    scales[name].CaptionsFontHolder = UIConfig.FontSubtitleAlt;
                    scales[name].ForeColor = UIConfig.InactiveTextColor;
                    scales[name].SuggestedStepX = scaleStepX;
                    scales[name].SuggestedStepY = scaleStepY;
                    scales[name].CaptionsColor = scaleFontColor;
                    if (variable.Value.Depths.Count == 1)
                    {
                        Plot.DataContainer.RepairColors();
                        scales[name].LineColor = variable.Value.Data.Values.First().Values.First().ColorsByDepth.First()
                            .Value;
                    }

                    if (IsTime) scales[name].PredefinedStepsX = predefinedTimeSteps;
                }

                var scale = scales[name];
                scale.PlotData.Add(variable.Value);
                if (!Scales.Select(a => a.YLabel).Contains(name))
                {
                    Scales.Add(scale);
                    Add(scale);
                    first = false;
                }
            }
        }

        protected int LegendsNum;

        protected override void Update(GameTime gameTime)
        {
            if (Dirty)
            {
                GenerateScales();
                Dirty = false;
            }

            if (!IsInit)
            {
                Init();
                IsInit = true;
            }
            Legend.PlotData = Plot.DataContainer;
            base.Update(gameTime);
            UpdateScales();
            UpdateRect();            
        }

        private float OldWidthPlot, OldHeightPlot; 
        public virtual void UpdateRect()
        {
            //float oldPlotWidth = Plot.UnitWidth, oldPlotHeight = Plot.UnitHeight;            
            Plot.UnitX = this.UnitPaddingLeft + UIConfig.UnitPlotScaleWidth * (float)Math.Ceiling(LegendsNum * 0.5f);
            Plot.UnitWidth = this.UnitWidth - UIConfig.UnitPlotScaleWidth * LegendsNum - this.UnitPaddingLeft - this.UnitPaddingRight;
            Legend.UnitX = this.UnitPaddingLeft + UIConfig.UnitPlotScaleWidth * ((float)Math.Ceiling(LegendsNum * 0.5f)+0.5f);
            Legend.UnitWidth = Plot.UnitWidth - UIConfig.UnitPlotScaleWidth;
            Legend.UnitY = UnitHeight - Legend.UnitHeight - UnitPaddingBottom;
            Plot.UnitY = this.UnitPaddingTop;
            Plot.UnitHeight = Legend.UnitY - UIConfig.UnitPlotScaleHeight - UIConfig.UnitPlotLegendOffset;
        }

        public virtual Size2F MinSize => new Size2F(UIConfig.PlotWindowMinPlotWidth + UnitPaddingLeft + UnitPaddingRight + UIConfig.UnitPlotScaleWidth * LegendsNum,
            Math.Min(UIConfig.PlotWindowMinPlotHeight + Legend.UnitHeight + UnitPaddingTop + UnitPaddingBottom, ui.RootFrame.Height * ScaleMultiplier));

        public virtual void UpdateScales()
        {
            int i = 0;
            bool first = true;
            Scales = Scales.OrderBy(a => Plot.ActiveScale == a ? -1000 : a.Index).ToList();
            foreach (var scale in Scales)
            {
                if (scale.Visible = scale.PlotData.Any(d => d.IsPresent))
                {
                    if (ShowScaleY) scale.Index = i++;
                    if (first)
                    {
                        scale.Settings = (ShowScaleY
                                             ? ScaleParams.DrawScaleY | ScaleParams.UseFixedStepY
                                             : 0) |
                                         (ShowScaleX
                                             ? ScaleParams.DrawScaleX | ScaleParams.UseFixedStepX |
                                               ScaleParams.NoActiveCheckX
                                             : 0) |
                                         ScaleSettings;
                        first = false;
                    }
                    else
                    {
                        scale.Settings = (ShowScaleY
                                             ? ScaleParams.DrawScaleY | ScaleParams.UseFixedStepY
                                             : 0) |
                                         ScaleSettings;
                    }
                }   
                scale.UpdateScaleSteps();             
            }
            LegendsNum = i;
        }
    }
}
