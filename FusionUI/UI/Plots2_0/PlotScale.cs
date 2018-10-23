using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        DrawMeasureX = 4,
        DrawScaleX = 8,
        DrawScaleY = 16,
        DrawAtZero = 32,
        AnimPlot = 64,
        UseFixedStepX = 128,
        UseFixedStepY = 256,
        NoActiveCheckX = 512,
        NoActiveCheckY = 1024,
        NotDisplayName = 2048,
        NotWriteNumbersX = 4096,
        DrawMeasureY = 4096 << 1,
        DrawMeasure = DrawMeasureX + DrawMeasureY,
        DrawZeroLineX = 4096 << 2,
        DrawZeroLineY = 4096 << 3,
    }

    public class CatScale : PlotScale
    {
        public CatScale(FrameProcessor ui, PlotCanvas plotWindow) : base(ui, plotWindow)
        {
        }

        public override double StepX => 1;        

        protected override float offsetX => 0.5f;
        protected override float offsetY => 0.0f;

        public override RectangleD Limits
        {
            get
            {
                var pds = PlotData.Where(a => a != null).ToList();
                if (pds.Count() == 0) return RectangleD.Empty;                
                var lim = new RectangleD()
                {
                    Left = pds[0].ActiveDepths.Any() ? pds[0].ActiveDepths.Min() : 0,
                    Right = pds[0].ActiveDepths.Any() ? pds[0].ActiveDepths.Max() : 1,
                    Top = pds[0].Limits.Left,
                    Bottom = pds[0].Limits.Right,
                };
                for (int i = 1; i < pds.Count(); i++)
                {
                    lim = RectangleD.Union(lim, new RectangleD()
                    {
                        Left = pds[i].ActiveDepths.Any() ? pds[i].Depths.Min() : 0,
                        Right = pds[1].ActiveDepths.Any() ? pds[i].Depths.Max() : 1,
                        Top = pds[i].Limits.Left,
                        Bottom = pds[i].Limits.Right,
                    });
                }

                lim.Left = Math.Ceiling(lim.Left);
                lim.Right = Math.Ceiling(lim.Right) + 1;
                var h = lim.Height;
                //lim.Bottom += lim.Height * 0.125;
                //lim.Top += lim.Height * 0.125;
                return new RectangleD()
                {
                    Left = DMathUtil.Lerp(lim.Left, lim.Right, Plot.ScaleRect.Left),
                    Right = DMathUtil.Lerp(lim.Left, lim.Right, Plot.ScaleRect.Right),
                    Top = DMathUtil.Lerp(lim.Bottom, lim.Top, Plot.ScaleRect.Bottom),
                    Bottom = DMathUtil.Lerp(lim.Bottom, lim.Top, Plot.ScaleRect.Top),
                };
            }
        }
    }

    public class HeatScale : PlotScale
    {
        public HeatScale(FrameProcessor ui, PlotCanvas plotWindow) : base(ui, plotWindow)
        {
        }

        public override double StepX => 1;
        public override double StepY => 1;

        protected override float offsetX => 0.5f;
        protected override float offsetY => 0.0f;

        public override RectangleD Limits
        {
            get
            {
                var pds = PlotData.Where(a => a != null).ToList();
                if (pds.Count() == 0) return RectangleD.Empty;
                var lim = new RectangleD()
                {
                    Left = pds[0].Limits.Left,
                    Right = pds[0].Limits.Right,
                    Top = pds[0].Data.Min(a => a.Value.Values.Min(b => b.Data.Keys.Min())),
                    Bottom = pds[0].Data.Max(a => a.Value.Values.Max(b => b.Data.Keys.Max())),
                };
                for (int i = 1; i < pds.Count(); i++)
                {
                    lim = RectangleD.Union(lim, new RectangleD()
                    {
                        Left = pds[i].Limits.Left,
                        Right = pds[i].Limits.Right,
                        Top = pds[i].Data.Min(a => a.Value.Values.Min(b => b.Data.Keys.Min())),
                        Bottom = pds[i].Data.Max(a => a.Value.Values.Max(b => b.Data.Keys.Max())),
                    });
                }

                lim.Left = Math.Ceiling(lim.Left) + 0.5;
                lim.Right = Math.Ceiling(lim.Right) - 0.5;
                lim.Bottom += 0.5;
                lim.Top -= 0.5;
                return new RectangleD()
                {
                    Left = DMathUtil.Lerp(lim.Left, lim.Right, Plot.ScaleRect.Left),
                    Right = DMathUtil.Lerp(lim.Left, lim.Right, Plot.ScaleRect.Right),
                    Top = DMathUtil.Lerp(lim.Bottom, lim.Top, Plot.ScaleRect.Bottom),
                    Bottom = DMathUtil.Lerp(lim.Bottom, lim.Top, Plot.ScaleRect.Top),
                };
            }
        }
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
        public UIConfig.FontHolder CaptionsFontHolder = UIConfig.FontBody;
        public UIConfig.FontHolder NumbersFontHolder = UIConfig.FontBody;
        private SpriteFont CaptionsFont => CaptionsFontHolder[ApplicationInterface.uiScale];
        private SpriteFont NumbersFont => NumbersFontHolder[ApplicationInterface.uiScale];


        public int Index = 0;

        //public Func<double, string> XStringFunc = f => $"{f:0.##}", YStringFunc = f => $"{f:0.##}";

        public virtual double StepY { get; set; }
        public virtual double StepX { get; set; }
        public float SuggestedStepX = 20, SuggestedStepY = 10;
        public Color CaptionsColor = UIConfig.ActiveTextColor;
        public Color LineColor = UIConfig.ActiveTextColor;

        public ScaleParams Settings;

        public PlotScale (FrameProcessor ui, PlotCanvas plotWindow) : base (ui, plotWindow.UnitX, plotWindow.UnitY, plotWindow.UnitWidth, plotWindow.UnitHeight, "", Color.Zero) {
            Plot = plotWindow;
                predefinedStepsY = new List<double>() { 10e-9f, 2 * 10e-9f, 10e-8f, 2 * 10e-8f, 10e-7f, 2 * 10e-7f, 10e-6f, 2 * 10e-6f, 10e-5f, 2 * 10e-5f, 10e-4f, 2 * 10e-4f, 10e-3f, 2 * 10e-3f, 10e-2f, 2 * 10e-2f, 0.1f, 0.5f, 1, 5, 10, 25, 50, 100, 200, 500, 1000, 2000, 3000, 5000, 10000 };
                PredefinedStepsX = new List<double>() { 1, 2, 5, 10, 30, 60, 90, 180, 365, 730, 1095};

            ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                //if (args.IsClick)
                //{
                //    if (Plot.ActiveScale == this) Plot.ActiveScale = null;
                //    else Plot.ActiveScale = this;
                //}
            };
        }

        public virtual void UpdateScaleStepsX()
        {
            float topValue, bottomValue, leftValue, rightValue;
            if ((Settings & ScaleParams.DrawScaleX) != 0)
            {
                var MinX = (float)Limits.Left;
                var MaxX = (float)Limits.Right;
                if (MaxX - MinX < float.Epsilon) return;
                leftValue = MinX;//(Settings & ScaleParams.StartFromZeroX) != 0 ? 0 : MinX;
                float dist = MaxX - leftValue;
                double st = Math.Abs(dist / (Plot.UnitWidth / SuggestedStepX));
                if (st < float.Epsilon) return;
                if ((Settings & ScaleParams.UseFixedStepX) == 0)
                {
                    StepX = st;
                    return;
                }
                int ind = PredefinedStepsX.BinarySearch(st);
                if (ind < 0)
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
                    StepX = PredefinedStepsX[ind]; // IMPOSIBURU!
                }
            }
        }

        public virtual void UpdateScaleStepsY()
        {
            float topValue, bottomValue, leftValue, rightValue;
            if ((Settings & ScaleParams.DrawScaleY) != 0)
            {
                var MinY = (float)Limits.Top;
                var MaxY = (float)Limits.Bottom;
                if (MaxY - MinY < float.Epsilon) return;
                bottomValue = MinY;// (Settings & ScaleParams.StartFromZeroY) != 0 ? 0 : MinY;
                float dist = MaxY - bottomValue;
                double st = Math.Abs(dist / (Plot.UnitHeight / SuggestedStepY));
                if ((Settings & ScaleParams.UseFixedStepY) == 0)
                {
                    StepY = st;
                    return;
                }
                int ind = predefinedStepsY.BinarySearch(st);
                if (ind < 0)
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
                    StepY = predefinedStepsY[ind]; // IMPOSIBURU!
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
            X = -parent.GlobalRectangle.X;
            Y = -parent.GlobalRectangle.Y;
            Width = ui.RootFrame.GlobalRectangle.Width;
            Height = ui.RootFrame.GlobalRectangle.Height;
        }

        protected override void DrawFrame (GameTime gameTime, SpriteLayer sb, int clipRectIndex) {            
            //if (!Active) return;            
            //if (!PlotData.Data.Any()) return;            
            X = -parent.GlobalRectangle.X;
            Y = -parent.GlobalRectangle.Y;
            Width = ui.RootFrame.GlobalRectangle.Width;
            Height = ui.RootFrame.GlobalRectangle.Height;
            if ((Settings & ScaleParams.DrawScaleX) != 0)
            {
                DrawHorizontal(gameTime, sb, clipRectIndex);
            }
            if ((Settings & ScaleParams.DrawScaleY) != 0) {
                DrawVertical (gameTime, sb, clipRectIndex);
            }

        }

        protected virtual float offsetX => 0;
        protected virtual float offsetY => 0;

        public virtual RectangleD Limits
        {
            get
            {
                if (PlotData.Count > 0)
                {
                    PlotData[0].StartFromZeroX = (Settings & ScaleParams.StartFromZeroX) != 0;
                    PlotData[0].StartFromZeroY = (Settings & ScaleParams.StartFromZeroY) != 0;
                    var lim = PlotData[0].LimitsAligned;

                    for (int i = 1; i < PlotData.Count; i++)
                    {
                        PlotData[i].StartFromZeroX = (Settings & ScaleParams.StartFromZeroX) != 0;
                        PlotData[i].StartFromZeroY = (Settings & ScaleParams.StartFromZeroY) != 0;
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
                    LineColor, clipRectIndex);

            UnitX = Plot.UnitX + offset / ScaleMultiplier - (left ? UIConfig.UnitPlotScaleWidth : 0);
            UnitY = Plot.UnitY;
            //UnitWidth = UIConfig.UnitPlotScaleWidth;
            //UnitHeight = Plot.UnitHeight;

            var color = Plot.ActiveScale == this || Plot.ActiveScale == null ? CaptionsColor : ForeColor;
            
            //sb.Draw(whiteTex, new RectangleF(Plot.GlobalRectangle.Left + offset + (int)(r.Height / 2) - r.Height, Plot.GlobalRectangle.Bottom + UIConfig.UnitPlotNumbersHeight + UIConfig.UnitPlotVerticalOffset, r.Height, r.Width), Color.Green);
            var wMax = 0.0f;

            if (Measure != "")
            {
                var measure = TryGetText(this.Measure);
                var r = Font.MeasureStringF(measure);
                CaptionsFont.DrawString(sb, measure, Plot.GlobalRectangle.Left + offset - (int)(r.Width) * (left ? 0 : 1) + (left ? 8 : -8),
                    Plot.GlobalRectangle.Top, color,
                    0,
                    useBaseLine: false);
            }

            var MaxY = (float)Limits.Bottom;
            var MinY = (float)Limits.Top;
            if (StepY < float.Epsilon || (MaxY - MinY) / StepY > 100) return;
            for (double pos = Math.Floor(MinY / StepY) * StepY; pos <= MaxY; pos += StepY) {                
                var s = yFunc.Invoke(pos) ?? "";
                                var b = Font.MeasureStringF(s);
                float h = (float)(Plot.GlobalRectangle.Bottom - ((pos - MinY - offsetY * StepY) / (MaxY - MinY) * Plot.GlobalRectangle.Height));
                if (left)
                {
                    if (h < Plot.GlobalRectangle.Bottom)
                        NumbersFont.DrawString (sb, s, -b.Width + Plot.GlobalRectangle.Left + offset - 10, h + (b.Height) * 0.25f,
                            color, clipRectIndex);
                } else {
                    if (h < Plot.GlobalRectangle.Bottom)
                        NumbersFont.DrawString (sb, s, Plot.GlobalRectangle.Left + offset + 22, h + (b.Height) / 2,
                            color, clipRectIndex);
                }

                wMax = Math.Max(wMax, b.Width);
                if ((Settings & ScaleParams.DrawZeroLineX) != 0 && Math.Abs(pos) < StepY)
                {
                    sb.DrawDottedLine(whiteTex, new Vector2(Plot.GlobalRectangle.Left, h), new Vector2(Plot.GlobalRectangle.Right, h), UIConfig.PlotScaleColor, 1, dotSize: 8, spaceSize: 6);
                }

                if (pos < 0 && pos + StepY > 0 && -pos > b.Height * 2 && pos + StepY > b.Height * 2)
                {
                    s = yFunc.Invoke(0) ?? "";
                    h = Plot.GlobalRectangle.Bottom - ((0 - MinY) / (MaxY - MinY) * Plot.GlobalRectangle.Height);
                    if (left)
                    {
                        NumbersFont.DrawString(sb, s, -b.Width + Plot.GlobalRectangle.Left + offset - 10, h + (b.Height) / 2,
                            color, clipRectIndex);
                    }
                    else
                    {
                        NumbersFont.DrawString(sb, s, Plot.GlobalRectangle.Left + offset + 2, h + (b.Height) / 2,
                            color, clipRectIndex);
                    }
                    
                }
                if (h > Plot.GlobalRectangle.Top && h < Plot.GlobalRectangle.Bottom && (Settings & ScaleParams.DrawMeasureY) != 0)
                {
                    sb.Draw(whiteTex, Plot.GlobalRectangle.Left - 4, h - 1, 8, 2, CaptionsColor, clipRectIndex);
                 
                }
            }
            if ((Settings & ScaleParams.NotDisplayName) == 0)
            {
                var yLabel = TryGetText(YLabel);
                var r = Font.MeasureStringF(yLabel);
                CaptionsFont.DrawString(sb, yLabel, -r.Height + Plot.GlobalRectangle.Left + offset - wMax - 10,
                    Plot.GlobalRectangle.Center.Y + r.Width * 0.5f, color,
                    0,
                    useBaseLine: false, flip: true);
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
                UIConfig.PlotLineColor, clipRectIndex);
            var xLabel = TryGetText(XLabel);
            var r = Font.MeasureStringF(xLabel);
            if ((Settings & ScaleParams.NotDisplayName) == 0)
            {
                CaptionsFont.DrawString(sb, xLabel, Plot.GlobalRectangle.Center.X - r.Width / 2,
                    Plot.GlobalRectangle.Bottom - zeroH + (Index + 1) * UIConfig.UnitPlotScaleHeight * ScaleMultiplier,
                    CaptionsColor, clipRectIndex);
            }

            if (StepX < float.Epsilon || (MaxX - MinX) / StepX > 100) return;
            for (double pos = RoundToStep(MinX, StepX); pos <= MaxX + StepX; pos += StepX) {
                var s = xFunc.Invoke(pos) ?? "";
                var b = Font.MeasureStringF(s);
                bool flip = false;//b.Width > StepX / (MaxX - MinX) * Plot.GlobalRectangle.Width * 1.25f;
                float w =  (float)((pos - offsetX * StepX - MinX)/(MaxX - MinX)*Plot.GlobalRectangle.Width - (flip ? 0 : b.Width/2));                
                if (w > -b.Width / 2 && w < Plot.GlobalRectangle.Width - b.Width / 2 &&
                    (Settings & ScaleParams.NotWriteNumbersX) == 0)
                {
                    if (!flip)
                    {
                        NumbersFont.DrawString(sb, s, Plot.GlobalRectangle.Left + w,
                            Plot.GlobalRectangle.Bottom + Index * UIConfig.UnitPlotScaleHeight * ScaleMultiplier +
                            Font.LineHeight - ((float) zeroH / Plot.GlobalRectangle.Height > 0.9 ? 0 : zeroH),
                            CaptionsColor, clipRectIndex);
                    }
                    else
                    {
                        NumbersFont.DrawString(sb, s, Plot.GlobalRectangle.Left + w,
                            Plot.GlobalRectangle.Bottom + b.Width,
                            CaptionsColor, 0, flip:true);
                    }
                }

                if (w > 0 && w < Plot.GlobalRectangle.Width && (Settings & ScaleParams.NotWriteNumbersX) == 0 && (Settings & ScaleParams.DrawMeasureX) != 0)
                {
                    sb.Draw(whiteTex, Plot.GlobalRectangle.Left + w + b.Width / 2 - 1, Plot.GlobalRectangle.Bottom - zeroH + Index * UIConfig.UnitPlotScaleHeight * ScaleMultiplier - 4, 2, 8, UIConfig.PlotLineColor, clipRectIndex);
                    if ((Settings & ScaleParams.DrawZeroLineY) != 0 && Math.Abs(pos) < StepX)
                    {
                        sb.DrawDottedLine(whiteTex, new Vector2(Plot.GlobalRectangle.Left + w + b.Width / 2 - 1, Plot.GlobalRectangle.Top), new Vector2(Plot.GlobalRectangle.Left + w + b.Width / 2 - 1, Plot.GlobalRectangle.Bottom), UIConfig.PlotScaleColor, 1, dotSize:8, spaceSize:6);
                    }
                }
            }
        }

        private static double RoundToStep(double value, double step)
        {
            var offset = value % step;
            var d = value - offset;
            return d;
        }
    
        public void AddData(PlotData data)
        {            
        }

        public bool ForceActive;
        public bool Active => ForceActive || PlotData != null && PlotData.Any(d => d.Data.Values.Any(a => a.Any(b => b.Value.IsPresent)));        
    }    
}
