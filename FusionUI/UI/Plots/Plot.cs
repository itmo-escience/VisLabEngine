using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Vector2 = Fusion.Core.Mathematics.Vector2;

namespace FusionUI.UI.Plots
{
    public class Plot : ScalableFrame
    {
		protected Plot()
		{
		}
		public string ScenarioName;
        public SerializableDictionary<string, PlotScale> Scales = new SerializableDictionary<string, PlotScale>();
        public SerializableDictionary<int, PlotMapPoint> MapPoints = new SerializableDictionary<int, PlotMapPoint>();
        public AbstractTimeManager TimeManager;
        public bool IsDynamic;
        public bool IsAnimated;

        public bool Any => Active && IsAnimated
            ? MapPoints.Values.Any(mp => mp.AnimAny)
            : MapPoints.Values.Any(mp => mp.Any);

        private int LastIndex = -1;

        //public float MinX, MaxX, MinY, MaxY;

        public Plot(FrameProcessor ui, float x, float y, float w, float h, Color backColor, AbstractTimeManager tm) : base(ui, x, y, w, h, "", backColor)
        {
            TimeManager = tm;
        }

        private DateTime startTime, endTime;

        private float getCorrectedTime(DateTime localStartTime, float secs)
        {
            return secs + (float) (localStartTime - startTime).TotalSeconds;
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex)
        {
            base.DrawFrame(gameTime, spriteLayer, clipRectIndex);
            if (!Any) return;
            
            if (!IsAnimated)
            {
                startTime = MapPoints.Min(mp => (mp.Value.Plots.Where(p => p.Value.Active)
                    .Min(p => p.Value.StartTime + TimeSpan.FromSeconds(p.Value.MinX))));
                endTime = MapPoints.Max(mp => (mp.Value.Plots.Where(p => p.Value.Active)
                                                  .Max(p => p.Value.StartTime + TimeSpan.FromSeconds(p.Value.MaxX))));

                MinX = 0;
                MaxX = (float)(endTime - startTime).TotalSeconds;
                foreach (var point in MapPoints.Values)
                {
                    if (!point.Active) continue;
                    foreach (var plot in point.Plots)
                    {
                        
                        DrawPlot(spriteLayer, clipRectIndex, plot.Value);
                    }
                }
                DrawRunner(spriteLayer);
            }
            else
            {
                startTime = TimeManager.StartTime;
                foreach (var point in MapPoints.Values)
                {
                    if (!point.Active) continue;
                    foreach (var plot in point.Plots)
                    {
                        AnimPreparePlot(plot.Value);
                    }
                }
                foreach (var point in MapPoints.Values)
                {
                    if (!point.Active) continue;
                    foreach (var plot in point.Plots)
                    {
                        if (plot.Value.AnimActive)
                        {
                            AnimDrawPlot(spriteLayer, clipRectIndex, plot.Value);
                        }
                    }
                }                
            }
            Dirty = false;
        }

        public bool AnimFixedSizeX, AnimFixedSizeY;
        public float AnimMinY = float.MaxValue, AnimMaxY = float.MinValue, AnimMinX = float.MaxValue, AnimMaxX = float.MinValue;

        public float MinX, MaxX;
        void AnimPreparePlot(PlotData plotData)
        {
            if (IsAnimated && plotData.Ready && plotData.AnimActive)
            {
                AnimMinX = Math.Min(plotData.MinXAnim, AnimMinX);
                AnimMinY = Math.Min(plotData.MinYAnim, AnimMinY);
                AnimMaxX = Math.Max(plotData.MaxXAnim, AnimMaxX);
                AnimMaxY = Math.Max(plotData.MaxYAnim, AnimMaxY);
            }
        }

        void AnimDrawPlot(SpriteLayer sb, int clipRectIndex, PlotData plotData)
        {
            if (AnimFixedSizeX)
            {
                plotData.MaxXAnim = AnimMaxX;                
                plotData.MinXAnim = AnimMinX;                
            }
            if (AnimFixedSizeY)
            {
                plotData.MaxYAnim = AnimMaxY;
                plotData.MinYAnim = AnimMinY;
            }
            var whiteTex = this.Game.RenderSystem.WhiteTexture;
            var rect = GetBorderedRectangle();

            List<float> depths = plotData.ValueList.Keys.ToList();
            depths.Sort();
            depths.Reverse();
            float t = (float)((TimeManager.CurrentTime - plotData.StartTime).TotalSeconds /
                              (TimeManager.EndTime - plotData.StartTime).TotalSeconds *
                              (plotData.ValueList[depths[0]].Count - 1));
            t = Math.Max(t, 0);
            t = Math.Min(t, plotData.ValueList[depths[0]].Count - 1);
            int t1 = (int)Math.Min(Math.Ceiling(t), plotData.ValueList[depths[0]].Count - 1);

            Vector2 last = Vector2.Lerp(
                new Vector2(plotData.ValueList[depths[0]][(int)Math.Floor(t)].Y, depths[0]),
                new Vector2(plotData.ValueList[depths[0]][t1].Y, depths[0]), (float)(t - Math.Floor(t)));

            for (int i = 0; i < depths.Count - 1; i++)
            {
                Vector2 next = Vector2.Lerp(
                    new Vector2(plotData.ValueList[depths[i + 1]][(int)Math.Floor(t)].Y, depths[i + 1]),
                    new Vector2(plotData.ValueList[depths[i + 1]][t1].Y, depths[i + 1]),
                    (float)(t - Math.Floor(t)));

                if (float.IsNaN(last.X) || float.IsNaN(last.Y) || float.IsNaN(next.X) || float.IsNaN(next.Y))
                {
                    //skip this point
                }
                else
                {
                    var lastPlot = new Vector2(
                        (last.X - plotData.MinXAnim) / (plotData.MaxXAnim - plotData.MinXAnim),
                        1 - (last.Y - plotData.MinYAnim) / (plotData.MaxYAnim - plotData.MinYAnim));
                    var nextPlot = new Vector2(
                        (next.X - plotData.MinXAnim) / (plotData.MaxXAnim - plotData.MinXAnim),
                        1 - (next.Y - plotData.MinYAnim) / (plotData.MaxYAnim - plotData.MinYAnim));
                    var lastScreen = rect.TopLeft + lastPlot * new Vector2(rect.Width, rect.Height);
                    var nextScreen = rect.TopLeft + nextPlot * new Vector2(rect.Width, rect.Height);
                    sb.DrawBeam(whiteTex, lastScreen, nextScreen, plotData.AnimColor, plotData.AnimColor,
                        UIConfig.UnitPlotLineWidth * ScaleMultiplier, clipRectIndex: clipRectIndex);
                }

                last = next;
            }            
        }

        private Dictionary<PlotData, Dictionary<float, List<Vector2>>> points = new Dictionary<PlotData, Dictionary<float, List<Vector2>>>();
        public bool Dirty = true;

        void DrawPlot(SpriteLayer sb, int clipRectIndex, PlotData plotData)
        {
            if (!IsAnimated && plotData.Ready && plotData.Active && (plotData.Dirty || plotData.BakedPlot == null || IsDynamic))
            {
                plotData.Dirty = false;                
                var whiteTex = this.Game.RenderSystem.WhiteTexture;
                var rect = GetBorderedRectangle();
                if (Dirty || !points.ContainsKey(plotData))
                {                    
                    points[plotData] = new Dictionary<float, List<Vector2>>();
                    foreach (var depth in plotData.ActiveDepths)
                    {
                        points[plotData][depth] = new List<Vector2>();
                        int increment = (int) Math.Max(1, 2 * plotData.ValueList[depth].Count /
                                                          (this.Width *
                                                           (plotData.ValueList[depth][
                                                                    plotData.ValueList[depth].Count - 1]
                                                                .X - plotData.ValueList[depth][0].X) / (MaxX - MinX)));
                        Vector2 last = plotData.ValueList[depth][0];
                        last.X = getCorrectedTime(plotData.StartTime, last.X);
                        for (int i = 1; i < plotData.ValueList[depth].Count; i += increment)
                        {
                            Vector2 next = plotData.ValueList[depth][i];
                            next.X = getCorrectedTime(plotData.StartTime, next.X);
                            next.Y = plotData.ValueList[depth]
                                         .Skip(Math.Max(0, i - increment / 2))
                                         .Take(increment)
                                         .Where(v => !float.IsNaN(v.Y))
                                         .Sum(v => v.Y) / increment;
                            if (float.IsNaN(last.X) || float.IsNaN(last.Y) || float.IsNaN(next.X) ||
                                float.IsNaN(next.Y))
                            {
                                //skip this point
                            }
                            else
                            {
                                var lastPlot = new Vector2((last.X - MinX) / (MaxX - MinX),
                                    1 - (last.Y - plotData.MinY) / (plotData.MaxY - plotData.MinY));
                                var nextPlot = new Vector2((next.X - MinX) / (MaxX - MinX),
                                    1 - (next.Y - plotData.MinY) / (plotData.MaxY - plotData.MinY));
                                if (i == 1) points[plotData][depth].Add(lastPlot);
                                points[plotData][depth].Add(nextPlot);                                
                            }
                            last = next;
                        }
                    }                
                }
                foreach (var depth in plotData.ActiveDepths)
                {
                    for (int i = 1; i < points[plotData][depth].Count; i ++)
                    {
                        var lastPlot = points[plotData][depth][i - 1];
                        var nextPlot = points[plotData][depth][i];
                        var lastScreen = rect.TopLeft + lastPlot * new Vector2(rect.Width, rect.Height);
                        var nextScreen = rect.TopLeft + nextPlot * new Vector2(rect.Width, rect.Height);
                        sb.DrawBeam(whiteTex, lastScreen, nextScreen, plotData.Colors[depth],
                            plotData.Colors[depth],
                            UIConfig.UnitPlotLineWidth * ScaleMultiplier, clipRectIndex: clipRectIndex);
                    }
                }

                //bake plot texture for further use
                //RenderTarget2D tmpTarget = new RenderTarget2D(sb., );
            }
            if (plotData.BakedPlot != null)
            {
                var b = GetBorderedRectangle();
                sb.DrawUV(plotData.BakedPlot, b.Left, b.Top, b.Width, b.Height, Color.White, 0, 0, 1, 1, clipRectIndex);
            }

        }

        public void AddPoint(PlotMapPoint point)
        {
            if (MapPoints.ContainsKey(point.Index))
            {
                MapPoints[point.Index].MergeTo(point);
            }
            else
            {
                MapPoints.Add(point.Index, point);
            }
        }                
        
        public void DrawRunner(SpriteLayer sb)
        {
            float stripLength = 6;
            float blankLength = 4;
            var whiteTex = this.Game.RenderSystem.WhiteTexture;
            var rect = GetBorderedRectangle();            
            float xPos = (float)((TimeManager.CurrentTime - startTime).TotalSeconds /
                                  (endTime - startTime).TotalSeconds * rect.Width);

            if (xPos < 0 || xPos > rect.Width) return;
            for (float pos = rect.Top; pos < rect.Bottom; pos += stripLength + blankLength)
            {
                Vector2 firstPoint = new Vector2(rect.Left + xPos, pos);
                Vector2 secondPoint = new Vector2(rect.Left + xPos, pos + blankLength);
                sb.DrawBeam(whiteTex, firstPoint, secondPoint, Color.White, Color.White, 2);
            }

            string text = TimeManager.CurrentTime.ToString("dd/MM/yy");
            var srect = Font.MeasureStringF(text);
            if (xPos > srect.Width / 2 && xPos < rect.Width - srect.Width/2) 
                Font.DrawString(sb, text, rect.Left + xPos - srect.Width/2, rect.Top, Color.White);

            srect = Font.MeasureString("Date");
            Font.DrawString(sb, "Date", rect.Center.X - srect.Width/2, rect.Bottom + srect.Height * 2, Color.White);
        }
    }
}
