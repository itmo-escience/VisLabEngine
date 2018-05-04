using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace FusionUI.UI.Plots2_0
{

    [Flags]
    public enum ScaleParams
    {
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
        NoActiveCheckX = 512,
        NoActiveCheckY = 1024,
        NotDisplayName = 2048,
    }

    public class PlotScale : ScalableFrame
    {        
        //TODO: change to plot type when it'll be developed
        public PlotCanvas Plot;
        public List<PlotVariable> PlotData = new List<PlotVariable>();

        public string YLabel = "";
        public string XLabel = "";
        public string Measure = "";
        public bool Dirty = true;        
        public List<double> PredefinedStepsX, predefinedStepsY;
        public UIConfig.FontHolder BoldFontHolder;
        public SpriteFont BoldFont => BoldFontHolder[ApplicationInterface.uiScale];
        
        public int Index = 0;

        //public Func<double, string> XStringFunc = f => $"{f:0.##}", YStringFunc = f => $"{f:0.##}";

        public double StepY, StepX;
        public float SuggestedStepX = 20, SuggestedStepY = 10;
        

        public ScaleParams Settings;

        public PlotScale (FrameProcessor ui, PlotCanvas plotWindow) : base (ui, plotWindow.UnitX, plotWindow.UnitY, plotWindow.UnitWidth, plotWindow.UnitHeight, "", Color.Zero) {
            Plot = plotWindow;
                predefinedStepsY = new List<double>() { 10e-9f, 2 * 10e-9f, 10e-8f, 2 * 10e-8f, 10e-7f, 2 * 10e-7f, 10e-6f, 2 * 10e-6f, 10e-5f, 2 * 10e-5f, 10e-4f, 2 * 10e-4f, 10e-3f, 2 * 10e-3f, 10e-2f, 2 * 10e-2f, 0.1f, 0.5f, 1, 5, 10, 25, 50, 100, 200, 500, 1000, 2000, 3000, 5000, 10000 };
                PredefinedStepsX = new List<double>() { 1, 2, 5, 10, 30, 60, 90, 180, 365, 730, 1095};

            ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (args.IsClick)
                {
                    if (Plot.ActiveScale == this) Plot.ActiveScale = null;
                    else Plot.ActiveScale = this;
                }
            };
        }

        public void UpdateScaleStepsX()
        {
            float topValue, bottomValue, leftValue, rightValue;
            if ((Settings & ScaleParams.DrawScaleX) != 0)
            {
                var MinX = (float)Limits.Left;
                var MaxX = (float)Limits.Right;
                if (MaxX - MinX < float.Epsilon) return;
                leftValue = (Settings & ScaleParams.StartFromZeroX) != 0 ? 0 : MinX;
                float dist = MaxX - leftValue;
                double st = Math.Abs(dist / (Plot.UnitWidth / SuggestedStepX));
                if (st < float.Epsilon) return;
                if ((Settings & ScaleParams.UseFixedStepX) == 0)
                {
                    StepX = st;
                    return;
                }
                int ind = PredefinedStepsX.BinarySearch(st);
                if (ind <= 0)
                {
                    ind = ~ind;
                    if (ind == 0)
                    {
                        while (st < PredefinedStepsX[0] && st > float.Epsilon)
                            PredefinedStepsX.Insert(0, PredefinedStepsX[0] / 2);
                        StepX = PredefinedStepsX[0];
                    }
                    else if (ind == PredefinedStepsX.Count)
                    {
                        while (st > PredefinedStepsX[PredefinedStepsX.Count - 1] * 2)
                            PredefinedStepsX.Add(PredefinedStepsX[PredefinedStepsX.Count - 1] * 2);
                        StepX = PredefinedStepsX[PredefinedStepsX.Count - 1];
                    }
                    else
                    {
                        if (st < PredefinedStepsX[ind - 1] / 2)
                            StepX = PredefinedStepsX[ind - 1];
                        else
                            StepX = PredefinedStepsX[ind];
                    }
                }
                else
                {
                    StepX = PredefinedStepsX[ind - 1]; // IMPOSIBURU!
                }
            }
        }

        public void UpdateScaleStepsY()
        {
            float topValue, bottomValue, leftValue, rightValue;
            if ((Settings & ScaleParams.DrawScaleY) != 0)
            {
                var MinY = (float)Limits.Top;
                var MaxY = (float)Limits.Bottom;
                if (MaxY - MinY < float.Epsilon) return;
                bottomValue = (Settings & ScaleParams.StartFromZeroY) != 0 ? 0 : MinY;
                float dist = MaxY - bottomValue;
                double st = Math.Abs(dist / (Plot.UnitHeight / SuggestedStepY));
                if ((Settings & ScaleParams.UseFixedStepY) == 0)
                {
                    StepY = st;
                    return;
                }
                int ind = predefinedStepsY.BinarySearch(st);
                if (ind <= 0)
                {
                    if (ind != 0) ind = ~ind;
                    if (ind == 0)
                    {
                        while (st < predefinedStepsY[0])
                            predefinedStepsY.Insert(0, predefinedStepsY[0] / 5);
                        StepY = predefinedStepsY[0];
                    }
                    else if (ind == predefinedStepsY.Count)
                    {
                        while (st > predefinedStepsY[predefinedStepsY.Count - 1] * 2)
                            predefinedStepsY.Add(predefinedStepsY[predefinedStepsY.Count - 1] * 2);
                        StepY = predefinedStepsY[predefinedStepsY.Count - 1];
                    }
                    else
                    {
                        if (st < predefinedStepsY[ind - 1] / 2)
                            StepY = predefinedStepsY[ind - 1];
                        else
                            StepY = predefinedStepsY[ind];
                    }
                }
                else
                {
                    StepY = predefinedStepsY[ind - 1]; // IMPOSIBURU!
                }
                //bottomValue = (float) (Math.Floor(bottomValue / StepY) * StepY);
                //topValue = (float) (bottomValue + Math.Ceiling((MaxY - bottomValue) / StepY) * StepY);
                //MinY = bottomValue;
                //MaxY = topValue;
            }
        }

        public void UpdateScaleSteps ()
        {
            UpdateScaleStepsX();
            UpdateScaleStepsY();
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
            //if (!Active) return;            
            //if (!PlotData.Data.Any()) return;            
            X = -parent.GlobalRectangle.X;
            Y = -parent.GlobalRectangle.Y;
            Width = ui.RootFrame.GlobalRectangle.Width;
            Height = ui.RootFrame.GlobalRectangle.Height;
            if ((Settings & ScaleParams.DrawScaleY) != 0) {
                DrawVertical (gameTime, sb, clipRectIndex);
            }
            if ((Settings & ScaleParams.DrawScaleX) != 0) {
                DrawHorizontal (gameTime, sb, clipRectIndex);
            }
        }

        public virtual RectangleD Limits
        {
            get
            {
                if (PlotData.Count > 0)
                {
                    var lim = PlotData[0].LimitsAligned;
                    for (int i = 1; i < PlotData.Count; i++)
                    {
                        lim = RectangleD.Union(lim, PlotData[i].Limits);
                    }
                    return new RectangleD()
                    {
                        Left = DMathUtil.Lerp(lim.Left, lim.Right, Plot.ScaleRect.Left),
                        Right = DMathUtil.Lerp(lim.Left, lim.Right, Plot.ScaleRect.Right),
                        Top = DMathUtil.Lerp(lim.Bottom, lim.Top, Plot.ScaleRect.Bottom),
                        Bottom = DMathUtil.Lerp(lim.Bottom, lim.Top, Plot.ScaleRect.Top),
                    };
                }
                return RectangleD.Empty;
            }
        }

        protected virtual Func<double, string> xFunc => (f => PlotData[0]?.xFunction?.Invoke(f, StepX));
        protected virtual Func<double, string> yFunc => (f => PlotData[0]?.yFunction?.Invoke(f, StepY));
        private void DrawVertical (GameTime gameTime, SpriteLayer sb, int clipRectIndex) {
            if ((!Active || !PlotData.Any(d => d.Data.Any())) && (Settings & ScaleParams.NoActiveCheckY) == 0) return;
            if (Dirty)
            {
                UpdateScaleSteps();
                Dirty = false;
            }
            var whiteTex = this.Game.RenderSystem.WhiteTexture;
            bool left = Index % 2 == 0;
            int offset = (int)(Index / 2 * (UIConfig.UnitPlotScaleWidth * ScaleMultiplier * (left ? -1 : 1)) + (left ? 0 : Plot.Width));

            sb.Draw (whiteTex, new RectangleF (Plot.GlobalRectangle.Left + offset, Plot.GlobalRectangle.Top, UIConfig.UnitPlotScaleLineWidth * ScaleMultiplier, Plot.GlobalRectangle.Height + UIConfig.UnitPlotNumbersHeight * ScaleMultiplier),
                    UIConfig.PlotScaleColor, clipRectIndex);

            UnitX = Plot.UnitX + offset / ScaleMultiplier - (left ? UIConfig.UnitPlotScaleWidth : 0);
            UnitY = Plot.UnitY;
            //UnitWidth = UIConfig.UnitPlotScaleWidth;
            //UnitHeight = Plot.UnitHeight;

            var color = Plot.ActiveScale == this || Plot.ActiveScale == null ? UIConfig.ActiveTextColor : ForeColor;
            
            //sb.Draw(whiteTex, new RectangleF(Plot.GlobalRectangle.Left + offset + (int)(r.Height / 2) - r.Height, Plot.GlobalRectangle.Bottom + UIConfig.UnitPlotNumbersHeight + UIConfig.UnitPlotVerticalOffset, r.Height, r.Width), Color.Green);
            if ((Settings & ScaleParams.NotDisplayName) == 0)
            {
                var r = Font.MeasureStringF(this.YLabel);
                Font.DrawString(sb, this.YLabel, Plot.GlobalRectangle.Left + offset - (int) (r.Height / 2) * 0,
                    Plot.GlobalRectangle.Bottom + r.Width * 0 +
                    (UIConfig.UnitPlotNumbersHeight + UIConfig.UnitPlotVerticalOffset) * ScaleMultiplier, color,
                    clipRectIndex,
                    useBaseLine: false, flip: true);
            }

            var MaxY = (float)Limits.Bottom;
            var MinY = (float)Limits.Top;
            if (StepY < float.Epsilon || (MaxY - MinY) / StepY > 100) return;
            for (double pos = Math.Floor(MinY / StepY) * StepY; pos <= MaxY; pos += StepY) {                
                var s = yFunc.Invoke(pos) ?? "";
                                var b = Font.MeasureStringF(s);
                float h = (float)(Plot.GlobalRectangle.Bottom - ((pos - MinY) / (MaxY - MinY) * Plot.GlobalRectangle.Height));
                if (left)
                {
                    if (h < Plot.GlobalRectangle.Bottom)
                        Font.DrawString (sb, s, -b.Width + Plot.GlobalRectangle.Left + offset, h + (b.Height + b.Y) / 2,
                            color, clipRectIndex);
                } else {
                    if (h < Plot.GlobalRectangle.Bottom)
                        Font.DrawString (sb, s, Plot.GlobalRectangle.Left + offset, h + (b.Height + b.Y) / 2,
                            color, clipRectIndex);
                }

                if (pos < 0 && pos + StepY > 0 && -pos > b.Height * 2 && pos + StepY > b.Height * 2)
                {
                    s = yFunc.Invoke(0) ?? "";
                    h = Plot.GlobalRectangle.Bottom - ((0 - MinY) / (MaxY - MinY) * Plot.GlobalRectangle.Height);
                    if (left)
                    {
                        Font.DrawString(sb, s, -b.Width + Plot.GlobalRectangle.Left + offset, h + (b.Height+b.Y) / 2,
                            color, clipRectIndex);
                    }
                    else
                    {
                        Font.DrawString(sb, s, Plot.GlobalRectangle.Left + offset, h + (b.Height + b.Y) / 2,
                            color, clipRectIndex);
                    }
                }                
            }           
        }

        private void DrawHorizontal (GameTime gameTime, SpriteLayer sb, int clipRectIndex) {
            if ((!Active || !PlotData.Any(d => d.Data.Any())) && (Settings & ScaleParams.NoActiveCheckX) == 0) return;
            if (Dirty)
            {
                UpdateScaleSteps();
                Dirty = false;
            }
            var MinX = (float)Limits.Left;
            var MaxX = (float)Limits.Right;
            var MinY = (float)Limits.Top;
            var MaxY = (float)Limits.Bottom;
            var whiteTex = this.Game.RenderSystem.WhiteTexture;            
            int zeroH = (Settings & ScaleParams.DrawAtZero) != 0 ? MathUtil.Clamp((int) ((0 - MinY) / (MaxY - MinY) * Plot.GlobalRectangle.Height), 0, Plot.GlobalRectangle.Height) : 0;
            sb.Draw(whiteTex,
                new RectangleF(Plot.GlobalRectangle.Left, Plot.GlobalRectangle.Bottom - zeroH + Index * UIConfig.UnitPlotScaleHeight * ScaleMultiplier,
                    Plot.GlobalRectangle.Width, UIConfig.UnitPlotScaleLineWidth * ScaleMultiplier),
                UIConfig.PlotScaleColor, clipRectIndex);
            var r = Font.MeasureStringF(XLabel);
            if ((Settings & ScaleParams.NotDisplayName) == 0)
            {
                Font.DrawString(sb, XLabel, Plot.GlobalRectangle.Center.X - r.Width / 2,
                    Plot.GlobalRectangle.Bottom - zeroH + (Index + 1) * UIConfig.UnitPlotScaleHeight * ScaleMultiplier,
                    UIConfig.ActiveTextColor, clipRectIndex);
            }

            if (StepX < float.Epsilon || (MaxX - MinX) / StepX > 100) return;
            for (double pos = MinX; pos <= MaxX + StepX; pos += StepX) {
                var s = xFunc.Invoke(pos) ?? "";
                var b = Font.MeasureStringF(s);
                float w =  (float)((pos - MinX)/(MaxX - MinX)*Plot.GlobalRectangle.Width - b.Width/2);
                if (w > b.Width/2 && w <= Plot.GlobalRectangle.Width - b.Width / 2) {
                    Font.DrawString (sb, s, Plot.GlobalRectangle.Left + w, Plot.GlobalRectangle.Bottom + Index * UIConfig.UnitPlotScaleHeight * ScaleMultiplier + b.Height - ((float)zeroH/ Plot.GlobalRectangle.Height > 0.9 ? 0 : zeroH),
                        Color.White, clipRectIndex);
                    if ((Settings & ScaleParams.DrawMeasure) != 0)
                    {
                        sb.Draw(whiteTex, Plot.GlobalRectangle.Left + w + b.Width / 2 - 1, Plot.GlobalRectangle.Bottom - zeroH + Index * UIConfig.UnitPlotScaleHeight * ScaleMultiplier - 8, 1, 12, Color.White, clipRectIndex);
                    }
                }
            }
        }

        public void AddData(PlotData data)
        {            
        }

        public bool ForceActive;
        public bool Active => ForceActive || PlotData != null && PlotData.Any(d => d.Data.Values.Any(a => a.Any(b => b.Value.IsPresent)));        
    }    
}
