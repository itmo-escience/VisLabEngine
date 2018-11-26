using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace FusionUI.UI.Plots2_0
{
    public class ViolinCanvas : PlotCanvas
    {
        public ViolinCanvas(FrameProcessor ui, float x, float y, float w, float h, Color backColor) : base(ui, x, y, w,
            h, backColor)
        {
        }


        public override void DrawPlot(SpriteLayer sb, PlotData pd, int clipRectIndex)
        {
            if (pd.ActiveDepths.Count == 0) return;
            float d = (float)Width / (pd.ActiveDepths.Count);

            if (!pd.IsLoaded || !pd.IsPresent) return;
            var whiteTex = Game.Instance.RenderSystem.WhiteTexture;
            int cat = 0;
            foreach (var depth in pd.ActiveDepths)
            {
                var points = pd[Width, depth];
                var limits = pd.Variable.LimitsAligned;

                var prev = points[0];
                var prevPlot = (prev - limits.TopLeft) / limits.Size;
                //prevPlot.X = 0.1 + 0.8 * (1 - prevPlot.X);
                prevPlot = new DVector2(prevPlot.Y, prevPlot.X);
                var prevScreen = new DVector2(prevPlot.X * d * 0.8f, prevPlot.Y * Height);
                //var prevScreen = new DVector2(prevPlot.Y * d * 0.8f, prevPlot.X * Height);

                for (int i = 1; i < points.Count; i++)
                {

                    var next = points[i];
                    if (double.IsNaN(next.X) || double.IsNaN(next.Y))
                    {
                        prevScreen = new Vector2(float.NaN, float.NaN);
                        continue;
                    }

                    var nextPlot = (next - limits.TopLeft) / limits.Size;
                    //nextPlot.X = 0.1 + 0.8 * (1 - nextPlot.X);

                    nextPlot = new DVector2(nextPlot.Y, nextPlot.X);
                    var nextScreen = new DVector2(nextPlot.X * d * 0.5f * 0.8f, nextPlot.Y * Height);

                    //var nextScreen = new DVector2(nextPlot.Y * d * 0.5f * 0.8f, nextPlot.X * Height);

                    if (!double.IsNaN(prevScreen.X) && !double.IsNaN(prevScreen.Y))
                    {

                        if (!pd.ColorsByDepth.ContainsKey(depth))
                        {
                            DataContainer.RepairColors();
                            Log.Warning("Colors not yet initialized");
                        }

                        var color = pd.ColorsByDepth[depth];
                        color.A = 255 / 3;
                        var pLTPlot = new Vector2((0.5f + cat) * d + (float)nextScreen.X, (float)nextScreen.Y) / GlobalRectangle.Size;
                        var pRTPlot = new Vector2((0.5f + cat) * d - (float)nextScreen.X, (float)nextScreen.Y) / GlobalRectangle.Size;
                        var pLBPlot = new Vector2((0.5f + cat) * d + (float)prevScreen.X, (float)prevScreen.Y) / GlobalRectangle.Size;
                        var pRBPlot = new Vector2((0.5f + cat) * d - (float)prevScreen.X, (float)prevScreen.Y) / GlobalRectangle.Size;
                        var pLTRect = ((pLTPlot - ScaleRect.TopLeft) / ScaleRect.Size).ToVector2();
                        var pRTRect = ((pRTPlot - ScaleRect.TopLeft) / ScaleRect.Size).ToVector2();
                        var pLBRect = ((pLBPlot - ScaleRect.TopLeft) / ScaleRect.Size ).ToVector2();
                        var pRBRect = ((pRBPlot - ScaleRect.TopLeft) / ScaleRect.Size).ToVector2();
                        if (pLTRect.X > 1 && pLBRect.X > 1 || pRTRect.X < 0 && pRBRect.X < 0) continue;
                        if (pLBRect.Y > 1 || pLTRect.Y < 0) continue;

                        var tl = GlobalRectangle.TopLeft + pLTRect * GlobalRectangle.Size;
                        var tr = GlobalRectangle.TopLeft + pRTRect * GlobalRectangle.Size;
                        var bl = GlobalRectangle.TopLeft + pLBRect * GlobalRectangle.Size;
                        var br = GlobalRectangle.TopLeft + pRBRect * GlobalRectangle.Size;
                        sb.DrawFreeUV(whiteTex,
                            tl,
                            tr,
                            bl,
                            br,
                            pd.ColorsByDepth[depth],
                            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), clipRectIndex);

                        //sb.DrawBeam(beamTex,

                        //     pd.ColorsByDepth[depth],
                        //    pd.ColorsByDepth[depth],
                        //    UIConfig.UnitPlotLineWidth * ScaleMultiplier, clipRectIndex: clipRectIndex);

                    }
                    else
                    {

                    }

                    prevPlot = nextPlot;
                    prevScreen = nextScreen;
                }

                cat++;
            }
        }
    }
}
