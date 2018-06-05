using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace FusionUI.UI.Plots2_0
{
    public class HeatCanvas : PlotCanvas
    {
        public HeatCanvas(FrameProcessor ui, float x, float y, float w, float h, Color backColor) : base(ui, x, y, w, h, backColor)
        {
        }

        public double[,] points;
        public double MinValue = -1, MaxValue = 1;

        public bool Average;
        public bool MinMaxFromData; 
        public Rectangle dataRange = new Rectangle(0, 0, 5, 5);
        public void UpdatePoints(PlotData pd)
        {
            points = new double[dataRange.Width, dataRange.Height];            
            var counts = new int[dataRange.Width, dataRange.Height];
            foreach (var pl in pd.Data)
            {
                int iy = (int)Math.Floor(pl.Key - dataRange.Top);
                foreach (var p in pl.Value)
                {
                    int ix = (int)Math.Floor(p.X - dataRange.Left);
                    if (ix >= 0 && ix < points.GetLength(0) && iy >= 0 && iy < points.GetLength(1))
                    {
                        points[ix, iy] += p.Y;
                        counts[ix, iy] += 1;
                    }
                }
            }

            if (Average)
            {
                for (int i = 0; i < dataRange.Width; i++)
                {
                    for (int j = 0; j < dataRange.Height; j++)
                    {
                        points[i, j] /= counts[i, j];
                    }
                }
            }

            if (MinMaxFromData)
            {
                MinValue = double.MaxValue;
                MaxValue = double.MinValue;
                for (int i = 0; i < dataRange.Width; i++)
                {
                    for (int j = 0; j < dataRange.Height; j++)
                    {
                        MinValue = Math.Min(MinValue, points[i, j]);
                        MaxValue = Math.Max(MaxValue, points[i, j]);
                    }
                }
            }
        }

        public string Palette = "palette";
        public override void DrawPlot(SpriteLayer sb, PlotData pd, int clipRectIndex)
        {
            UpdatePoints(pd);
            var paletteTex = Game.Content.Load<DiscTexture>(Palette);

            float dw = (float) Width / points.GetLength(0);
            float dh = (float)Height / points.GetLength(1);
            float x = 0;
            float y = 0;
            for (int i = 0; i < points.GetLength(0);i++)
            {
                y = 0;
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    float value = (float)((points[i, j] - MinValue) / (MaxValue - MinValue));
                    value = MathUtil.Clamp(value, 0.01f, 0.99f);
                    sb.DrawUV(paletteTex, GlobalRectangle.Left + x, GlobalRectangle.Top + y, dw, dh, Color.White, value, value, 0, 0, clipRectIndex);
                    y += dh;
                }
                x += dw;
            }
        }
    }
}
