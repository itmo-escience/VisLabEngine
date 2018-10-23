using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;

namespace FusionUI.UI.Plots2_0
{
    public class PlotCanvas : ScalableFrame
    {
        public bool SingleScale;

        public PlotScale ActiveScale = null;        

        public PlotCanvas(FrameProcessor ui, float x, float y, float w, float h, Color backColor) : base(ui, x, y, w, h,
            "", backColor)
        {
            AddBoxActions();
        }
        protected RectangleD DragRect = RectangleD.Empty;
        protected bool IsDraggingBox;

        protected Stack<RectangleD> prevScales = new Stack<RectangleD>();

        public void AddBoxActions()
        {
            IsDraggingBox = false;
            prevScales.Push(new RectangleD(0, 0, 1, 1));
            ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (args.IsAltClick && Game.Keyboard.IsKeyDown(Keys.LeftControl) ||
                    Game.Keyboard.IsKeyDown(Keys.RightControl))
                {
                    if (Game.Keyboard.IsKeyDown(Keys.LeftShift) || Game.Keyboard.IsKeyDown(Keys.RightShift))
                    {
                        prevScales.Clear();
                        prevScales.Push(new RectangleD(0, 0, 1, 1));
                        ScaleRect = new RectangleD(0, 0, 1, 1);
                    }
                    if (prevScales.Count > 0)
                        ScaleRect = prevScales.Pop();
                }
                if (prevScales.Count == 0) prevScales.Push(new RectangleD(0, 0, 1, 1));
            };

            ActionDown += (ControlActionArgs args, ref bool flag) =>
            {
                if (GlobalRectangle.Contains(args.Position) && args.IsAltClick && !(Game.Keyboard.IsKeyDown(Keys.LeftControl) ||
                                                                                    Game.Keyboard.IsKeyDown(Keys.RightControl)))
                {
                    DragRect = new RectangleD(
                        ((float)args.X - GlobalRectangle.Left) / GlobalRectangle.Width,
                        ((float)args.Y - GlobalRectangle.Top) / GlobalRectangle.Height,
                        0, 0);                    
                    IsDraggingBox = true;
                }
            };

            ActionOut = ActionUp = (ControlActionArgs args, ref bool flag) =>
            {
                if (IsDraggingBox && !(Game.Keyboard.IsKeyDown(Keys.LeftControl) ||
                                       Game.Keyboard.IsKeyDown(Keys.RightControl)))
                {
                    IsDraggingBox = false;
                    if (Math.Abs(DragRect.Width * GlobalRectangle.Width) < 10 || Math.Abs(DragRect.Height * GlobalRectangle.Height) < 10) return;
                    var newRect = new RectangleD(Math.Min(DragRect.Left, DragRect.Right),
                        Math.Min(DragRect.Top, DragRect.Bottom),
                        Math.Max(Math.Abs(DragRect.Width), 0.1), Math.Max(Math.Abs(DragRect.Height), 0.1));
                    newRect.Left = DMathUtil.Clamp(newRect.Left, 0, 1);
                    newRect.Right = DMathUtil.Clamp(newRect.Right, 0, 1);
                    newRect.Top = DMathUtil.Clamp(newRect.Top, 0, 1);
                    newRect.Bottom = DMathUtil.Clamp(newRect.Bottom, 0, 1);
                    prevScales.Push(ScaleRect);                   
                    ScaleRect = new RectangleD()
                    {
                        Left = DMathUtil.Lerp(ScaleRect.Left, ScaleRect.Right, Math.Min(newRect.Left, newRect.Right)),
                        Right =
                            DMathUtil.Lerp(ScaleRect.Left, ScaleRect.Right, Math.Max(newRect.Left, newRect.Right)),
                        Top = DMathUtil.Lerp(ScaleRect.Top, ScaleRect.Bottom, Math.Min(newRect.Top, newRect.Bottom)),
                        Bottom = DMathUtil.Lerp(ScaleRect.Top, ScaleRect.Bottom,
                            Math.Max(newRect.Top, newRect.Bottom)),
                    };
                }
            };

            ActionDrag += (ControlActionArgs args, ref bool flag) =>
            {
                if (args.IsAltClick && IsDraggingBox)
                {
                    DragRect.Right = ((float) args.X - GlobalRectangle.Left) / GlobalRectangle.Width;
                    DragRect.Bottom = ((float) args.Y - GlobalRectangle.Top) / GlobalRectangle.Height;
                }              
            };
        }

        public AbstractTimeManager TimeManager;

        public Func<PlotCanvas, float> LineFunc;

        public PlotContainer DataContainer;

        public int PointCount = 1;        

        public Color LineColor = Color.White;
         

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            base.DrawFrame(gameTime, sb, clipRectIndex);
            //PointCount = (int)((ScalableFrame)Parent).UnitWidth;
            AlignZero();

            var whiteTex = Game.RenderSystem.WhiteTexture;
            if (ActiveScale != null)
            {
                var MaxY = (float)ActiveScale.Limits.Bottom;
                var MinY = (float)ActiveScale.Limits.Top;
                if (ActiveScale.StepY < float.Epsilon || (MaxY - MinY) / ActiveScale.StepY > 100) return;
                int i = 0;
                for (double pos = Math.Floor(MinY / ActiveScale.StepY) * ActiveScale.StepY;
                    pos <= MaxY;
                    pos += ActiveScale.StepY)
                {
                    i++;
                    float t = (float)(ActiveScale.GlobalRectangle.Bottom -
                                       ((pos - MinY) / (MaxY - MinY) * GlobalRectangle.Height));
                    float b = (float)(ActiveScale.GlobalRectangle.Bottom -
                                      ((pos + ActiveScale.StepY - MinY) / (MaxY - MinY) * ActiveScale.GlobalRectangle.Height));
                    b = Math.Min(b, GlobalRectangle.Bottom);
                    var l = Math.Min(ActiveScale.GlobalRectangle.Left, GlobalRectangle.Left);
                    var r = Math.Max(ActiveScale.GlobalRectangle.Right, GlobalRectangle.Right);
                    sb.Draw(whiteTex, l, t, r - l, b - t, i % 2 == 0 ? UIConfig.BackColorLayer : Color.Zero, clipRectIndex);
                }
            }

            hintPoints = new List<Tuple<Vector2, string, double, PlotData>>();
            int nBarCharts = DataContainer.Data.Values.Sum(pv =>
                pv.IsActive
                    ? pv.Data.Values.Sum(pd =>
                        pd.Sum(a =>
                            a.Value.IsPresent && a.Value.IsBarChart ? 1 : 0))
                    : 0);
            int plotIndex = 0;
            foreach (var pv in DataContainer.Data.Values)
            {
                if (pv.IsActive)
                {
                    var limits = pv.LimitsAligned;
                    foreach (var pd in pv.Data.Values)
                    {
                        if (pd.Any(a => a.Value.IsPresent))
                        {
                            foreach (var kv in pd)
                            {
                                if (kv.Value.IsBarChart)
                                {
                                    DrawBarChart(sb, kv.Value, clipRectIndex, plotIndex, nBarCharts);
                                    plotIndex++;
                                }
                                else
                                {
                                    DrawPlot(sb, kv.Value, clipRectIndex);
                                }
                            }
                        }
                    }
                }
            }
            var beamTex = Game.Content.Load<DiscTexture>(@"UI/beam");

            if (LineFunc != null)
            {
                var x = LineFunc(this);
                var p1 = new Vector2(this.GlobalRectangle.Left + this.GlobalRectangle.Width * x, this.GlobalRectangle.Top);
                var p2 = new Vector2(this.GlobalRectangle.Left + this.GlobalRectangle.Width * x, this.GlobalRectangle.Bottom);
                sb.DrawBeam(beamTex, p1, p2, LineColor, LineColor,
                    UIConfig.UnitPlotLineWidth * ScaleMultiplier, clipRectIndex: clipRectIndex);
            }
            if (Game.Keyboard.IsKeyDown(Keys.LeftShift) || Game.Keyboard.IsKeyDown(Keys.RightShift) &&
                GlobalRectangle.Contains(Game.Mouse.Position))
            {
                DrawMouseLine(sb, clipRectIndex, beamTex);
            }
            hintPoints.Sort((a, b) => -Math.Abs(a.Item1.Y - Game.Mouse.Position.Y).CompareTo(Math.Abs(b.Item1.Y - Game.Mouse.Position.Y)));
            for (int i = 0; i < hintPoints.Count; i++)
            {
                var pointScreen = hintPoints[i].Item1;
                var s = hintPoints[i].Item2;
                var depth = hintPoints[i].Item3;
                var pd = hintPoints[i].Item4;
                var r = Font.MeasureStringF(s);
                if (pointScreen.Y - r.Height - r.Y < GlobalRectangle.Top && pointScreen.Y > GlobalRectangle.Top)
                {
                    pointScreen.Y = GlobalRectangle.Top + r.Height + r.Y;
                }
                if (pointScreen.Y > GlobalRectangle.Bottom && pointScreen.Y - r.Height - r.Y < GlobalRectangle.Bottom)
                {
                    pointScreen.Y = GlobalRectangle.Bottom;
                }
                sb.Draw(whiteTex, pointScreen.X, pointScreen.Y - r.Height - r.Y, r.Width, r.Height,
                    UIConfig.PopupColor, clipRectIndex);
                Font.DrawString(sb, s, pointScreen.X, pointScreen.Y, depth >= 0 ? pd.ColorsByDepth[depth] : pd.BaseColor, clipRectIndex);
            }

            if (IsDraggingBox)
            {
                Vector2 tlScreen = GlobalRectangle.TopLeft + DragRect.TopLeft.ToVector2() * GlobalRectangle.Size;
                Vector2 brScreen = GlobalRectangle.TopLeft + DragRect.BottomRight.ToVector2() * GlobalRectangle.Size;
                Vector2 trScreen = new Vector2(brScreen.X, tlScreen.Y);
                Vector2 blScreen = new Vector2(tlScreen.X, brScreen.Y);
                sb.DrawBeam(beamTex, tlScreen, trScreen, Color.White, Color.White, UIConfig.UnitPlotLineWidth * ScaleMultiplier, clipRectIndex: clipRectIndex);
                sb.DrawBeam(beamTex, blScreen, brScreen, Color.White, Color.White, UIConfig.UnitPlotLineWidth * ScaleMultiplier, clipRectIndex: clipRectIndex);
                sb.DrawBeam(beamTex, tlScreen, blScreen, Color.White, Color.White, UIConfig.UnitPlotLineWidth * ScaleMultiplier, clipRectIndex: clipRectIndex);
                sb.DrawBeam(beamTex, trScreen, brScreen, Color.White, Color.White, UIConfig.UnitPlotLineWidth * ScaleMultiplier, clipRectIndex: clipRectIndex);
                var c = UIConfig.ActiveColor;
                c.A = 64;
                sb.Draw(whiteTex, tlScreen.X, tlScreen.Y, (float)(DragRect.Width * GlobalRectangle.Width), (float)(DragRect.Height * GlobalRectangle.Height), UIConfig.ActiveColor, clipRectIndex: clipRectIndex);
            }
        }

        protected virtual void DrawMouseLine(SpriteLayer sb, int clipRectIndex, DiscTexture beamTex)
        {
            
                var x = ((float)Game.Mouse.Position.X - GlobalRectangle.Left) / GlobalRectangle.Width;

                var p1 = new Vector2(this.GlobalRectangle.Left + this.GlobalRectangle.Width * x, this.GlobalRectangle.Top);
                var p2 = new Vector2(this.GlobalRectangle.Left + this.GlobalRectangle.Width * x, this.GlobalRectangle.Bottom);
                sb.DrawBeam(beamTex, p1, p2, UIConfig.ActiveColor, UIConfig.ActiveColor,
                    UIConfig.UnitPlotLineWidth * ScaleMultiplier, clipRectIndex: clipRectIndex);            
        }

        public RectangleD ScaleRect = new RectangleD(0, 0, 1, 1);


        public bool KeepZero, KeepStep;
        public float ScaleStep => ActiveScale.SuggestedStepY;

        public virtual void AlignZero()
        {
            RectangleD? r = null;
            MinAligned = MaxAligned = 0;
            if (!SingleScale)
            {
                foreach (var pv in DataContainer.Data.Values)
                {                    
                    if (!pv.IsPresent || pv.ActiveDepths.Count == 0) continue;
                    {
                        var l = pv.ActiveLimits;
                        if (MinAligned == MaxAligned)
                        {
                            MinAligned = l.Left;
                            MaxAligned = l.Right;
                        }
                        else
                        {
                            MinAligned = Math.Min(MinAligned, l.Left);
                            MaxAligned = Math.Max(MaxAligned, l.Right);
                        }
                        pv.LimitsAligned = l;
                        if (!(l.Top * l.Bottom < 0))
                        {
                            continue;
                        }
                        var t = l.Top / l.Height;
                        var b = l.Bottom / l.Height;
                        if (r != null)
                        {
                            var rect = r.Value;
                            rect.Top = Math.Min(rect.Top, t);
                            rect.Bottom = Math.Max(rect.Bottom, b);
                            rect.Left = Math.Min(rect.Left, l.Left);
                            rect.Right = Math.Max(rect.Right, l.Right);
                            r = rect;
                        }
                        else
                        {
                            r = new RectangleD()
                            {
                                Top = t,
                                Bottom = b,
                                Left = l.Left,
                                Right = l.Right,
                            };

                        }
                    }
                }

                if (KeepZero)
                {
                    if (r == null) return;                    
                    var v = r.Value;                    
                    if (v.Top < 0) v.Top = 0;
                    if (v.Bottom > 0) v.Bottom = 0;
                }

                if (KeepStep)
                {

                }

                foreach (var pv in DataContainer.Data.Values)
                {
                    var l = pv.Limits;
                    pv.LimitsAligned = l;
                    if (r != null /*&& l.Top * l.Bottom < 0*/)
                    {
                        pv.LimitsAligned.Top = l.Height * r.Value.Top;
                        pv.LimitsAligned.Bottom = l.Height * r.Value.Bottom;
                    }
                    pv.LimitsAligned.Left = MinAligned;
                    pv.LimitsAligned.Right = MaxAligned;
                }
            }
            else
            {
                foreach (var pv in DataContainer.Data.Values)
                {
                    if (!pv.IsPresent || pv.ActiveDepths.Count == 0) continue;
                    {
                        var l = pv.ActiveLimits;
                        r = r == null ? l : RectangleD.Union(r.Value, l);
                    }
                }
                if (r != null)
                {
                    MinAligned = r.Value.Left;
                    MaxAligned = r.Value.Right;
                }
                foreach (var pv in DataContainer.Data.Values)
                {
                    pv.LimitsAligned = r.Value;                    
                }
            }
        }

        public double MinAligned, MaxAligned;

        public virtual void DrawPlot(SpriteLayer sb, PlotData pd, int clipRectIndex)
        {
            //var whiteTex = this.Game.RenderSystem.WhiteTexture;
            if(!pd.IsLoaded || !pd.IsPresent) return;
            var beamTex = Game.Content.Load<DiscTexture>(@"UI/beam");
            var whiteTex = Game.RenderSystem.WhiteTexture;
            var baseRect = new RectangleD(0, 0, 1, 1);
            var pcNew = PointCount / ScaleRect.Width;            
            
            foreach (var depth in pd.ActiveDepths)
            {
                var points = pd[(int)pcNew, depth];                
                var limits = pd.Variable.LimitsAligned;
                var pointsX = points.ConvertAll(a => a.X);
                var leftLim = limits.Left + ScaleRect.Left * limits.Width;
                var left = pointsX.BinarySearch(leftLim);
                left = left >= 0 ? left : ~left;
                if (left >= pointsX.Count) continue;
                var prev = points[left];
                var prevPlot = (prev - limits.TopLeft) / limits.Size;
                prevPlot.Y = 1 - prevPlot.Y;
                var prevRect = (prevPlot - ScaleRect.TopLeft) / ScaleRect.Size;
                var prevScreen = prevRect.ToVector2() * this.GlobalRectangle.Size + this.GlobalRectangle.TopLeft;
                for (int i = left + 1; i < points.Count; i++)
                {
                    var next = points[i];                    
                    if (double.IsNaN(next.X) || double.IsNaN(next.Y))
                    {                        
                        prevScreen = new Vector2(float.NaN, float.NaN);
                        continue;
                    }                                
                    var nextPlot = (next - limits.TopLeft) / limits.Size;
                    nextPlot.Y = 1 - nextPlot.Y;

                    var nextRect = (nextPlot - ScaleRect.TopLeft) / ScaleRect.Size;
                    if (nextRect.X < 0 || prevRect.X > 1)                        
                    {
                        prevScreen = new Vector2(float.NaN, float.NaN);
                        continue;
                    }                   
                    var nextScreen = nextRect.ToVector2() * this.GlobalRectangle.Size + this.GlobalRectangle.TopLeft;
                    if (!float.IsNaN(prevScreen.X) && !float.IsNaN(prevScreen.Y))
                    {
                        //var r = new RectangleD()
                        //{
                        //    Left = Math.Min(prevRect.X, nextRect.X),
                        //    Right = Math.Max(prevRect.X, nextRect.X),
                        //    Top = Math.Min(prevRect.Y, nextRect.Y),
                        //    Bottom = Math.Max(prevRect.Y, nextRect.Y),
                        //};                        
                        if (!pd.ColorsByDepth.ContainsKey(depth))
                        {
                            DataContainer.RepairColors();
                            Log.Warning("Colors not yet initialized");
                        }
                        //if (baseRect.Intersects(r))
                        //{                        
                        sb.DrawBeam(beamTex, prevScreen, nextScreen, pd.ColorsByDepth[depth],
                            pd.ColorsByDepth[depth],
                            UIConfig.UnitPlotLineWidth * ScaleMultiplier, clipRectIndex: clipRectIndex);
                        //}
                    }
                    else
                    {
                        
                    }
                    prevPlot = nextPlot;
                    prevRect = nextRect;
                    prevScreen = nextScreen;
                    if (prevRect.X > 1) break;
                }
                if ((Game.Keyboard.IsKeyDown(Keys.LeftControl) || Game.Keyboard.IsKeyDown(Keys.RightControl)) && LineFunc != null)
                {
                    points = pd.Data[depth];
                    pointsX = points.ConvertAll(a => a.X);
                    float x;
                    if (Game.Keyboard.IsKeyDown(Keys.LeftShift) || Game.Keyboard.IsKeyDown(Keys.RightShift) && GlobalRectangle.Contains(Game.Mouse.Position))
                    {
                        x = ((float)Game.Mouse.Position.X - GlobalRectangle.Left) / GlobalRectangle.Width;                                                
                        x = (float)(ScaleRect.Left + x * ScaleRect.Width);
                    }
                    else
                    {
                        x = (float)(ScaleRect.Left + LineFunc(this) * ScaleRect.Width);
                    }
                    if (x < 0 || x > 1) continue;
                    var i = pointsX.BinarySearch(limits.Left + x * limits.Width);
                    if (i < 0) i = ~i - 1;                    
                    i = MathUtil.Clamp(i, 0, points.Count - 1);
                    var i1 = MathUtil.Clamp(i + 1, 0, points.Count - 1);
                    var v = points[i];
                    var v1 = points[i1];                    
                    var factor = (limits.Left + x * limits.Width - v.X) / (v1.X - v.X);
                    if (factor < 0 || factor > 1) continue;
                    factor = DMathUtil.Clamp(factor, 0, 1);
                    var value = MathUtil.Lerp(v.Y, v1.Y, factor);
                    
                    var pointPlot = new DVector2(x, 1 - (value - limits.Top) / limits.Height);
                    var pointRect = (pointPlot - ScaleRect.TopLeft) / ScaleRect.Size;
                    var pointScreen = pointRect.ToVector2() * this.GlobalRectangle.Size + this.GlobalRectangle.TopLeft;
                    var s = value.ToString("0.#####");
                    hintPoints.Add(new Tuple<Vector2, string, double, PlotData>(pointScreen, s, depth, pd));                                        
                }
            }                        
        }

        public virtual void DrawBarChart(SpriteLayer sb, PlotData pd, int clipRectIndex, int plotIndex, int nBarCharts)
        {
            //var whiteTex = this.Game.RenderSystem.WhiteTexture;
            if (!pd.IsLoaded || !pd.IsPresent) return;
            var beamTex = Game.Content.Load<DiscTexture>(@"UI/beam");
            var whiteTex = Game.RenderSystem.WhiteTexture;
            var baseRect = new RectangleD(0, 0, 1, 1);
            var pcNew = PointCount / ScaleRect.Width;            
            foreach (var depth in pd.ActiveDepths)
            {
                var points = pd[(int)pcNew, depth];                
                var limits = pd.Variable.LimitsAligned;
                float BarWidth = (0.7f - 0.1f * (nBarCharts - 1)) * Width / (float)limits.Width / nBarCharts;

                float offset = nBarCharts > 1 ? (nBarCharts * 0.5f - plotIndex - 0.5f) * Width / (float) limits.Width / nBarCharts : 0;

                var pointsX = points.ConvertAll(a => a.X);
                var leftLim = limits.Left + ScaleRect.Left * limits.Width;
                var left = pointsX.BinarySearch(leftLim);
                left = left >= 0 ? left : ~left;
                if (left >= pointsX.Count) continue;

                for (int i = left; i < points.Count; i++)
                {
                    var zero = new DVector2(points[i].X, 0);
                    var next = points[i];
                    if (double.IsNaN(next.X) || double.IsNaN(next.Y))
                    {
                        continue;
                    }

                    var nextPlot = (next - limits.TopLeft) / limits.Size;
                    nextPlot.Y = 1 - nextPlot.Y;

                    var zeroPlot = (zero - limits.TopLeft) / limits.Size;
                    zeroPlot.Y = 1 - zeroPlot.Y;

                    var nextRect = (nextPlot - ScaleRect.TopLeft) / ScaleRect.Size;
                    var zeroRect = (zeroPlot - ScaleRect.TopLeft) / ScaleRect.Size;

                    var nextScreen = (nextRect * new DVector2(this.GlobalRectangle.Width, this.GlobalRectangle.Height) + new DVector2(this.GlobalRectangle.Left, this.GlobalRectangle.Top)).ToVector2();
                    var zeroScreen = (zeroRect * new DVector2(this.GlobalRectangle.Width, this.GlobalRectangle.Height) + new DVector2(this.GlobalRectangle.Left, this.GlobalRectangle.Top)).ToVector2();
                    if (!pd.ColorsByDepth.ContainsKey(depth))
                    {
                        DataContainer.RepairColors();
                        Log.Warning("Colors not yet initialized");
                    }
                    
                    sb.Draw(whiteTex, nextScreen.X + offset - BarWidth / 2, nextScreen.Y, BarWidth,
                        zeroScreen.Y - nextScreen.Y, pd.ColorsByDepth[depth], clipRectIndex);
                    //hintPoints.Add(new Tuple<Vector2, string, double, PlotData>(
                        //new Vector2(nextScreen.X - BarWidth / 2, nextScreen.Y), $"{next.Y}", depth, pd));

                    var s = pd.Variable.bcFunction(next.Y, limits.Height);//.ToString($"F{(int)Math.Max(0, 3 - Math.Floor(Math.Log(Math.Max(1, next.Y), 10)))}");//$"{next.Y:()}";
                    var rect = Font.MeasureString(s);
                    if (nextScreen.Y - (next.Y > 0 ? 4 : 4 - rect.Height) < GlobalRectangle.Y + rect.Height)
                    {
                        nextScreen.Y = GlobalRectangle.Y + rect.Height + (next.Y > 0 ? 4 : 4 - rect.Height);
                    }                    

                        Font.DrawString(sb, s, nextScreen.X + offset - rect.Width / 2, Math.Min(nextScreen.Y, zeroScreen.Y) - ((next.Y > 0.0f) ? 4 : 4),
                        /*pd.ColorsByDepth[depth]*/UIConfig.ActiveTextColor, clipRectIndex);

                    if (nextRect.X > 1) break;
                }
            }
        }

        protected List<Tuple<Vector2, string, double, PlotData>> hintPoints;

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            this.UnitHeight = MathUtil.Clamp(this.UnitHeight, UIConfig.PlotWindowMinPlotHeight,
                ui.RootFrame.Height * ScaleMultiplier);
        }
    }
}