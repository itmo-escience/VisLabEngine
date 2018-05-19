using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Plots2_0
{
    public class PlotViewer : FreeFrame
    {
        public PlotCanvas Plot;        

        public List<PlotScale> Scales = new List<PlotScale>();

        public PlotLegend Legend;

        public bool ShowScaleX = true, ShowScaleY = true;

        public PlotViewer(FrameProcessor ui, float x, float y, float w, float h, Color backColor,
            AbstractTimeManager tm, PlotCanvas plot = null) : base(ui, x, y, w, h, "", backColor)
        {
            if (plot == null)
                Plot = new PlotCanvas(ui, 0, 0, 100, 100, Color.Zero)
                {                    
                };

            else Plot = plot;
            this.Add(Plot);
            Plot.DataContainer = new PlotContainer();
            AddLegend();
            Dirty = true;
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
        public bool IsTime;
        protected List<double> predefinedTimeSteps = new List<double>() {1.0/1440, 1.0 /288, 1.0/144, 1.0/48, 1.0/24, 1.0/12, 1.0/4, 1.0/2, 1, 2, 5, 10, 30, 60, 180, 365, 365*2, 365*5, 365*10};

        public ScaleParams ScaleSettings = ScaleParams.DrawMeasure;
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
            foreach (var variable in Plot.DataContainer.Data) {
	            var name = variable.Value.Name;
                if (!(first && ShowScaleX || ShowScaleY)) break;
                if (!scales.ContainsKey(name))
                {
                    scales[name] = new PlotScale(ui, Plot)
                    {
                        Settings = (ShowScaleY 
                                       ? ScaleParams.DrawScaleY | ScaleParams.UseFixedStepY 
                                       : 0) | 
                                   (first && ShowScaleX
                                       ? ScaleParams.DrawScaleX | ScaleParams.UseFixedStepX | ScaleParams.NoActiveCheckX
                                       : 0) | 
                                   ScaleSettings,
                        Dirty = true,
                        YLabel = name,
                        Measure = variable.Value.Units,
                        XLabel = "Time",
                        ZOrder =  1000,
                        FontHolder = scaleFont,
                        BoldFontHolder = UIConfig.FontSubtitleAlt,
                        ForeColor = UIConfig.InactiveTextColor,    
                        SuggestedStepX = scaleStepX,
                        SuggestedStepY = scaleStepY,
                        CaptionsColor = scaleFontColor,
    };
                    if (variable.Value.Depths.Count == 1)
                    {
                        scales[name].LineColor = variable.Value.Data.Values.First().Values.First().ColorsByDepth.First().Value;
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
            Plot.UnitHeight = Legend.UnitY - this.UnitPaddingTop - UIConfig.UnitPlotScaleHeight - UIConfig.UnitPlotLegendOffset;
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
