using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI.Plots
{
    public class PlotScale : ScalableFrame {
		protected PlotScale()
		{
		}
		//TODO: change to plot type when it'll be developed
		public Plot Plot;
        public List<PlotData> PlotData = new List<PlotData>();

        public string Name = "";
        public string Measure = "";

        public List<double> predefinedSteps = new List<double>() { 10e-9f, 2*10e-9f, 10e-8f, 2 * 10e-8f, 10e-7f, 2*10e-7f, 10e-6f, 2*10e-6f, 10e-5f, 2* 10e-5f, 10e-4f, 2*10e-4f, 10e-3f, 2 * 10e-3f, 10e-2f, 2*10e-2f, 0.1f, 0.5f, 1, 5, 10, 25, 50, 100, 200, 500, 1000, 2000, 3000, 5000, 10000 };

        public float MinX = float.MaxValue, MaxX = float.MinValue, MinY = float.MaxValue, MaxY = float.MinValue;

        public int Index = 0;
		[XmlIgnore]
		public Func<double, string> XStringFunc = f => $"{f:0.##}", YStringFunc = f => $"{f:0.##}";

        private double stepY, stepX;
        public float SuggestedStepX = 15, SuggestedStepY = 15;
        [Flags]
        public enum ScaleParams {            
            StartFromZeroX = 1,
            StartFromZeroY = 2,
            StartFromZero = 3,
            DrawMeasure = 4,
            DrawScaleX = 8,
            DrawScaleY = 16,
            DrawAtZero = 32,
            AnimPlot = 64,
            UseFixedStepX = 128,
            UseFixedStepY = 256,
        }

        public ScaleParams Settings;

        public PlotScale (FrameProcessor ui, Plot plotWindow, int index = 0) : base (ui, plotWindow.UnitX, plotWindow.UnitY, plotWindow.UnitWidth, plotWindow.UnitHeight, "", Color.Zero) {
            Plot = plotWindow;
            this.Index = index;
        }

        private bool firstTime = true;
        public void UpdateMinMaxComplete (bool decrease = true) {
            if (decrease)
            {
                MinX = float.MaxValue;
                MinY = float.MaxValue;
                MaxX = float.MinValue;
                MaxY = float.MinValue;
            }
            if (!((Settings & ScaleParams.AnimPlot) != 0))
            {
                foreach (var plotData in PlotData.Where(a => a.Active))
                {
                    plotData.UpdateMinMax();
                    {
                        MinX = Math.Min(MinX, plotData.MinX);
                        MinY = Math.Min(MinY, plotData.MinY);
                        MaxX = Math.Max(MaxX, plotData.MaxX);
                        MaxY = Math.Max(MaxY, plotData.MaxY);
                    }
                }
                if (PlotData.Any(a => a.Active)) UpdateScaleSteps();
            }
            else
            {
                foreach (var plotData in PlotData.Where(a => a.AnimActive))
                {
                    plotData.UpdateMinMax();
                    MinX = Math.Min(MinX, plotData.MinXAnim);
                    MinY = Math.Min(MinY, plotData.MinYAnim);
                    MaxX = Math.Max(MaxX, plotData.MaxXAnim);
                    MaxY = Math.Max(MaxY, plotData.MaxYAnim);

                }
                if (PlotData.Any(a => a.AnimActive)) UpdateScaleSteps();
            }            
        }

        public void UpdatePlotsMinMax()
        {
            if (MinX > MaxX || MinY > MaxY) return;
            //if ((Settings & ScaleParams.UseFixedStepX) != 0) MaxX = MinX + (float)Math.Floor((MaxX - MinX) / StepX/5 + 1) * StepX*5;
            //if ((Settings & ScaleParams.UseFixedStepY) != 0) MaxY = MinY + (float)Math.Floor((MaxY - MinY) / StepY/5 + 1) * StepY*5;
            foreach (var plotData in PlotData)
            {
                if (!((Settings & ScaleParams.AnimPlot) != 0))
                {
                    if ((Settings & ScaleParams.DrawScaleX) != 0)
                    {
                        plotData.MinX = MinX;
                        plotData.MaxX = MaxX;
                    }
                    if ((Settings & ScaleParams.DrawScaleY) != 0)
                    {
                        plotData.MinY = MinY;
                        plotData.MaxY = MaxY;
                    }

                }
                else
                {
                    if ((Settings & ScaleParams.DrawScaleY) != 0)
                    {
                        plotData.MinYAnim = MinY;
                        plotData.MaxYAnim = MaxY;
                    }
                    if ((Settings & ScaleParams.DrawScaleX) != 0)
                    {
                        plotData.MinXAnim = MinX;
                        plotData.MaxXAnim = MaxX;
                    }
                }
            }
        }

        public void UpdateScaleSteps ()
        {
            float topValue, bottomValue, leftValue, rightValue;
            //if ((Settings & ScaleParams.AnimPlot) != 0 && !PlotData.Any(p => p.AnimActive) || (Settings & ScaleParams.AnimPlot) == 0 && (!PlotData.Any(p => p.Active) || )) return;
            if ((Settings & ScaleParams.DrawScaleY) != 0)
            {
                if (MinY > MaxY) return;                
                bottomValue = (Settings & ScaleParams.StartFromZeroY) != 0 ? 0 : MinY;                
                float dist = MaxY - bottomValue;
                double st = Math.Abs(dist / (Plot.UnitHeight / SuggestedStepY));
                if ((Settings & ScaleParams.UseFixedStepY) == 0)
                {
                    stepY = st;
                    return;
                }
                int ind = predefinedSteps.BinarySearch(st);
                if (ind < 0)
                {
                    if (ind != 0) ind = ~ind;
                    if (ind == 0)
                    {
                        while (st < predefinedSteps[0])
                            predefinedSteps.Insert(0, predefinedSteps[0] / 5);
                        stepY = predefinedSteps[0];
                    }
                    else if (ind == predefinedSteps.Count)
                    {
                        while (st > predefinedSteps[predefinedSteps.Count - 1] * 2)
                            predefinedSteps.Add(predefinedSteps[predefinedSteps.Count - 1] * 2);
                        stepY = predefinedSteps[predefinedSteps.Count - 1];
                    }
                    else
                    {
                        if (st < predefinedSteps[ind - 1] / 2)
                            stepY = predefinedSteps[ind - 1];
                        else
                            stepY = predefinedSteps[ind];
                    }
                }
                else
                {
                    stepY = predefinedSteps[ind]; // IMPOSIBURU!
                }
                bottomValue = (float) (Math.Floor(bottomValue / stepY) * stepY);
                topValue = (float) (bottomValue + Math.Ceiling((MaxY - bottomValue) / stepY) * stepY);
                MinY = bottomValue;
                MaxY = topValue;
            }
            if ((Settings & ScaleParams.DrawScaleX) != 0) {
                if (MinX > MaxX) return;
                leftValue = (Settings & ScaleParams.StartFromZeroX) != 0 ? 0 : MinX;
                float dist = MaxX - leftValue;
                double st = Math.Abs(dist / (Plot.UnitWidth / SuggestedStepX));
                if ((Settings & ScaleParams.UseFixedStepX) == 0)
                {
                    stepX = st;
                    return;
                }
                int ind = predefinedSteps.BinarySearch(st);
                if (ind < 0) {
                    ind = ~ind;
                    if (ind == 0) {
                        while (st < predefinedSteps[0] && st > float.Epsilon)
                            predefinedSteps.Insert (0, predefinedSteps[0] / 2);
                        stepX = predefinedSteps[0];
                    } else if (ind == predefinedSteps.Count) {
                        while (st > predefinedSteps[predefinedSteps.Count - 1] * 2)
                            predefinedSteps.Add (predefinedSteps[predefinedSteps.Count - 1] * 2);
                        stepX = predefinedSteps[predefinedSteps.Count - 1];
                    } else
                    {
                        if (st < predefinedSteps[ind - 1] / 2)
                            stepX = predefinedSteps[ind - 1];
                        else
                            stepX = predefinedSteps[ind];
                    }
                } else {
                    stepX = predefinedSteps[ind]; // IMPOSIBURU!
                }
                leftValue = (float)(Math.Floor (leftValue / stepX) * stepX);
                rightValue = (float)(leftValue + Math.Ceiling ((MaxX - leftValue) / stepX) * stepX);
                MinX = leftValue;
                MaxX = rightValue;
            }
            UpdatePlotsMinMax();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //if ((Settings & ScaleParams.AnimPlot) != 0)
            //{
            //    UpdateMinMaxComplete();
            //}
        }

        protected override void DrawFrame (GameTime gameTime, SpriteLayer sb, int clipRectIndex) {            
            if (!Active) return;            
            if (!Plot.Any) return;
            if ((Settings & ScaleParams.DrawScaleY) != 0) {
                DrawVertical (gameTime, sb, clipRectIndex);
            }
            if ((Settings & ScaleParams.DrawScaleX) != 0) {
                DrawHorizontal (gameTime, sb, clipRectIndex);
            }
        }

        private void DrawVertical (GameTime gameTime, SpriteLayer sb, int clipRectIndex) {
            if (stepY == 0) UpdateMinMaxComplete(false);
            if (stepY == 0) return;
            var whiteTex = Game.Instance.RenderSystem.WhiteTexture;
            bool left = Index % 2 == 0;
            int offset = (int)(Index / 2 * (UIConfig.UnitPlotScaleWidth * ScaleMultiplier * (left ? -1 : 1)) + (left ? 0 : Plot.Width));

            sb.Draw (whiteTex, new RectangleF (Plot.GlobalRectangle.Left + offset, Plot.GlobalRectangle.Top, UIConfig.UnitPlotScaleLineWidth * ScaleMultiplier, Plot.GlobalRectangle.Height + UIConfig.UnitPlotNumbersHeight * ScaleMultiplier),
                    UIConfig.PlotScaleColor);

            var r = Font.MeasureStringF(this.Name);
            //sb.Draw(whiteTex, new RectangleF(Plot.GlobalRectangle.Left + offset + (int)(r.Height / 2) - r.Height, Plot.GlobalRectangle.Bottom + UIConfig.UnitPlotNumbersHeight + UIConfig.UnitPlotVerticalOffset, r.Height, r.Width), Color.Green);
            Font.DrawString(sb, this.Name, Plot.GlobalRectangle.Left + offset - (int)(r.Height / 2), Plot.GlobalRectangle.Bottom + r.Width + (UIConfig.UnitPlotNumbersHeight + UIConfig.UnitPlotVerticalOffset)*ScaleMultiplier, ForeColor, useBaseLine:false, flip:true);

            if ((MaxY - MinY) / stepY > 100) return;
            for (double pos = MinY; pos <= MaxY; pos += stepY) {                
                var s = YStringFunc(pos);
                var b = Font.MeasureStringF(s);
                float h = (float)(Plot.GlobalRectangle.Bottom - ((pos - MinY) / (MaxY - MinY) * Plot.GlobalRectangle.Height));
                if (left) {                    
                    Font.DrawString (sb, s, -b.Width + Plot.GlobalRectangle.Left + offset, h,
                        Color.White);
                } else {
                    Font.DrawString (sb, s, Plot.GlobalRectangle.Left + offset, h,
                        Color.White);
                }

                if (pos < 0 && pos + stepY > 0 && -pos > b.Height * 2 && pos + stepY > b.Height * 2)
                {
                    s = YStringFunc(0);
                    h = Plot.GlobalRectangle.Bottom - ((0 - MinY) / (MaxY - MinY) * Plot.GlobalRectangle.Height);
                    if (left)
                    {
                        Font.DrawString(sb, s, -b.Width + Plot.GlobalRectangle.Left + offset, h,
                            Color.White);
                    }
                    else
                    {
                        Font.DrawString(sb, s, Plot.GlobalRectangle.Left + offset, h,
                            Color.White);
                    }
                }                
            }           
        }

        private void DrawHorizontal (GameTime gameTime, SpriteLayer sb, int clipRectIndex) {
            if (stepX == 0) UpdateMinMaxComplete(false);
            if (stepX == 0) return;
            var whiteTex = Game.Instance.RenderSystem.WhiteTexture;            
            int zeroH = (Settings & ScaleParams.DrawAtZero) != 0 ? MathUtil.Clamp((int) ((0 - MinY) / (MaxY - MinY) * Plot.GlobalRectangle.Height), 0, Plot.GlobalRectangle.Height) : 0;
            sb.Draw(whiteTex,
                new RectangleF(Plot.GlobalRectangle.Left, Plot.GlobalRectangle.Bottom - zeroH + Index * UIConfig.UnitPlotScaleHeight * ScaleMultiplier,
                    Plot.GlobalRectangle.Width, UIConfig.UnitPlotScaleLineWidth * ScaleMultiplier),
                UIConfig.PlotScaleColor);
            var r = Font.MeasureStringF(Name);
            Font.DrawString(sb, Name, Plot.GlobalRectangle.Center.X - r.Width/2, Plot.GlobalRectangle.Bottom - zeroH + (Index + 1) * UIConfig.UnitPlotScaleHeight * ScaleMultiplier - r.Height, Color.White);
            if ((MaxX - MinX) / stepX > 100) return;
            for (double pos = MinX; pos <= MaxX; pos += stepX) {
                var s = XStringFunc(pos);
                var b = Font.MeasureStringF(s);
                float w =  (float)((pos - MinX)/(MaxX - MinX)*Plot.GlobalRectangle.Width - b.Width/2);
                if (w > 0 && w <= Plot.GlobalRectangle.Width - b.Width) {
                    Font.DrawString (sb, s, Plot.GlobalRectangle.Left + w - b.Width / 2, Plot.GlobalRectangle.Bottom + Index * UIConfig.UnitPlotScaleHeight * ScaleMultiplier + b.Height - ((float)zeroH/ Plot.GlobalRectangle.Height > 0.9 ? 0 : zeroH),
                        Color.White);
                }
            }
        }

        public void AddData(PlotData data)
        {
            PlotData.Add(data);            
            UpdateMinMaxComplete();
            UpdateScaleSteps();
            UpdatePlotsMinMax();
        }

        public bool ForceActive;
        public bool Active => ForceActive || PlotData.Any(a => a.Active) || (Settings & ScaleParams.AnimPlot) != 0 && PlotData.Any(a => a.AnimActive);        
    }    
}
