using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace FusionUI.UI.Plots
{
    public class PlotVariable {
        public virtual string Name { get; set; }
        public virtual string NiceName { get; set; }

        public virtual double[] Depths { get; set; }

        public virtual string DepthUnits { get; set; }

	    public struct TimeData
	    {
		    public DateTime Time;
		    public float	Data;
	    }

        public Dictionary<string, string> Metadata = new Dictionary<string, string>();
        public Dictionary<float /*depth*/, List<TimeData>> Data = new Dictionary<float /*depth*/, List<TimeData>>();
    }

    public class PlotMapPoint
    {
        public virtual Vector3 Point { get; set; }
        public int Index;
        public Dictionary<PlotVariable, PlotData> Plots = new Dictionary<PlotVariable, PlotData>();
        public bool Active;        

        public static TextPointsGisLayer pointLayer;

        public static List<PlotMapPoint> PointsList = new List<PlotMapPoint>();

        private static List<int> FreeIndicies = new List<int>();

        private bool Alive = true;        

        public virtual string Name => (GeoHelper.CartesianToSpherical(new DVector3(Point)) * 360 / Math.PI).ToString("0.##");    

        protected PlotMapPoint()
        {
        }

        public PlotMapPoint(Vector3 point)
        {
            Point = point;
            PointsList.Add(this);
            Index = NextIndex();
            pointLayer.AddPoint(Index, Point, Color.White, $"  {Index:0}  ");
        }

        public void Update()
        {
            pointLayer.UpdatePoint(Index, Point, Color.White, $"  {Index:0}  ");
        }

        public void Remove()
        {
            if (!Alive) return;
            Alive = false;
            FreeIndicies.Add(Index);
            PointsList.Remove(this);
            pointLayer.RemovePoint(Index);
        }

        //public PlotViewerNew plotWindow;

        public bool Any => Active && Plots.Values.Any(pv => pv.Active);
        public bool AnimAny => Active && Plots.Values.Any(pv => pv.AnimActive);

        static private int MaxIndex;

        protected static int NextIndex()
        {
            if (FreeIndicies.Count == 0)
            {
                FreeIndicies.Add(PointsList.Count);
            }
            var ans = FreeIndicies.Min();
            FreeIndicies.Remove(ans);
            return ans;
        }

        public IEnumerable<string> VariableNames => Plots!=null?Plots.Keys.Select(k => k.NiceName).Distinct():null;

        public void MergeTo(PlotMapPoint point, bool keepNew = true)
        {
            foreach (var plot in point.Plots)
            {
                if (keepNew || !Plots.ContainsKey(plot.Key))
                {
                    Plots[plot.Key] = plot.Value;
                }
            }
        }
    }
    
    public class PlotData
    {

        public PlotVariable Variable;

        public PlotData(PlotVariable variable)
        {
            Variable = variable;
        }

        public void SetColors(ColorConfig ColorConfig)
        {
            foreach(var depth in Variable.Depths)
            {
                Colors[(float)depth] = ColorConfig.NextColor();
            }
            AnimColor = ColorConfig.NextColorAnim();
        }

        public DVector3 Point { get; set; }
        public Dictionary<float /*depth*/, List<Vector2> /*Data*/> ValueList;

        public DateTime StartTime;
        private bool active;

        public bool AnimActive => active && Ready && DepthValues.Count > 1;

        public bool Active
        {
            get { return active && Ready && ActiveDepths.Count != 0; }
            set {
                active = value;
                if (Ready && ValueList.Keys.Count <= 1)
                {
                    if (value) foreach (var v in ValueList.Keys)
                    {
                        ActiveDepths.Add(v);
                    } else ActiveDepths.Clear();
                }
            }
        }

        private HashSet<float> activeDepths = new HashSet<float>();
        public HashSet<float> ActiveDepths => activeDepths;

        public List<float> DepthValues => ValueList.Keys.ToList();

        public Dictionary<float, Color> Colors = new Dictionary<float, Color>();
        public Color AnimColor;
        public enum  DataType {
            Plot1D, Plot2D
        }

        public DataType Type;

        public bool Dirty;
        public bool Ready;

        public TargetTexture BakedPlot;
        //public RenderTarget2D BakedPlot;
            
        public float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
        public float minXAnim = float.MaxValue, maxXAnim = float.MinValue, minYAnim = float.MaxValue, maxYAnim = float.MinValue;

        public float MinX
        {
            get { return minX; }
            set
            {
                minX = value;
                if (Math.Abs(minX - maxX) < float.Epsilon) maxX = minX + float.Epsilon;
            }
        }

        public float MaxX
        {
            get { return maxX; }
            set
            {
                maxX = value;
                if (Math.Abs(minX - maxX) < float.Epsilon) maxX = minX + float.Epsilon;
            }
        }

        public float MinY
        {
            get { return minY; }
            set
            {
                minY = value;
                if (Math.Abs(minY - maxY) < float.Epsilon) maxY = minY + float.Epsilon;
            }
        }

        public float MaxY
        {
            get { return maxY; }
            set
            {
                maxY = value;
                if (Math.Abs(minY - maxY) < float.Epsilon) maxY = minY + float.Epsilon;
            }
        }



        public float MinXAnim
        {
            get { return minXAnim; }
            set
            {
                minXAnim = value;
                if (Math.Abs(minXAnim - maxXAnim) < float.Epsilon) maxXAnim = minXAnim + 1;
            }
        }

        public float MaxXAnim
        {
            get { return maxXAnim; }
            set
            {
                maxXAnim = value;
                if (Math.Abs(minXAnim - maxXAnim) < float.Epsilon) maxXAnim = minXAnim + 1;
            }
        }

        public float MinYAnim
        {
            get { return minYAnim; }
            set
            {
                minYAnim = value;
                if (Math.Abs(minYAnim - maxYAnim) < float.Epsilon) maxYAnim = minYAnim + 1;
            }
        }

        public float MaxYAnim
        {
            get { return maxYAnim; }
            set
            {
                maxYAnim = value;
                if (Math.Abs(minYAnim - maxYAnim) < float.Epsilon) maxYAnim = minYAnim + 1;
            }
        }

        public void UpdateMinMax()
        {            
                MinX = ValueList.Values.Min(a => a.Where(v => !float.IsNaN(v.X)).Min(v => v.X));
                MaxX = ValueList.Values.Max(a => a.Where(v => !float.IsNaN(v.X)).Max(v => v.X));
                MinY = ValueList.Values.Min(a => a.Where(v => !float.IsNaN(v.Y)).Min(v => v.Y));
                MaxY = ValueList.Values.Max(a => a.Where(v => !float.IsNaN(v.Y)).Max(v => v.Y));
            MinYAnim = ValueList.Keys.Min();
            MaxYAnim = ValueList.Keys.Max();
            MinXAnim = MinY;
            MaxXAnim = MaxY;
            Dirty = true;            
        }
    }

    public class ColorConfig
    {
        private int next = 0, nextAnim = 0;

        public static int[] indexcolors = new int[]
        {
            0XA6CEE3, 0X1F78B4, 0XB2DF8A, 0X33A02C, 0XFB9A99, 0XE31A1C, 0XFDBF6F, 0XFF7F00, 0XCAB2D6, 0X6A3D9A,
            0XFFFF99, 0XB15928,
            ((1 << 16) + (0 << 8) + 103), ((213 << 16) + (255 << 8) + 0), ((255 << 16) + (0 << 8) + 86), ((158 << 16) + (0 << 8) + 142),
            ((14 << 16) + (76 << 8) + 161), ((255 << 16) + (229 << 8) + 2), ((0 << 16) + (95 << 8) + 57), ((0 << 16) + (255 << 8) + 0),
            ((149 << 16) + (0 << 8) + 58), ((255 << 16) + (147 << 8) + 126), ((164 << 16) + (36 << 8) + 0),
            ((0 << 16) + (21 << 8) + 68), ((145 << 16) + (208 << 8) + 203), ((98 << 16) + (14 << 8) + 0),
            ((107 << 16) + (104 << 8) + 130), ((0 << 16) + (0 << 8) + 255), ((0 << 16) + (125 << 8) + 181),
            ((106 << 16) + (130 << 8) + 108), ((0 << 16) + (174 << 8) + 126), ((194 << 16) + (140 << 8) + 159),
            ((190 << 16) + (153 << 8) + 112), ((0 << 16) + (143 << 8) + 156), ((95 << 16) + (173 << 8) + 78),
            ((255 << 16) + (0 << 8) + 0), ((255 << 16) + (0 << 8) + 246), ((255 << 16) + (2 << 8) + 157),
            ((104 << 16) + (61 << 8) + 59), ((255 << 16) + (116 << 8) + 163), ((150 << 16) + (138 << 8) + 232),
            ((152 << 16) + (255 << 8) + 82), ((167 << 16) + (87 << 8) + 64), ((1 << 16) + (255 << 8) + 254),
            ((255 << 16) + (238 << 8) + 232), ((254 << 16) + (137 << 8) + 0), ((189 << 16) + (198 << 8) + 255),
            ((1 << 16) + (208 << 8) + 255), ((187 << 16) + (136 << 8) + 0), ((117 << 16) + (68 << 8) + 177),
            ((165 << 16) + (255 << 8) + 210), ((255 << 16) + (166 << 8) + 254), ((119 << 16) + (77 << 8) + 0),
            ((122 << 16) + (71 << 8) + 130), ((38 << 16) + (52 << 8) + 0), ((0 << 16) + (71 << 8) + 84), ((67 << 16) + (0 << 8) + 44),
            ((181 << 16) + (0 << 8) + 255), ((255 << 16) + (177 << 8) + 103), ((255 << 16) + (219 << 8) + 102),
            ((144 << 16) + (251 << 8) + 146), ((126 << 16) + (45 << 8) + 210), ((189 << 16) + (211 << 8) + 147),
            ((229 << 16) + (111 << 8) + 254), ((222 << 16) + (255 << 8) + 116), ((0 << 16) + (255 << 8) + 120),
            ((0 << 16) + (155 << 8) + 255), ((0 << 16) + (100 << 8) + 1), ((0 << 16) + (118 << 8) + 255),
            ((133 << 16) + (169 << 8) + 0), ((0 << 16) + (185 << 8) + 23), ((120 << 16) + (130 << 8) + 49),
            ((0 << 16) + (255 << 8) + 198), ((255 << 16) + (110 << 8) + 65), ((232 << 16) + (94 << 8) + 190),
        };

        public void Reset()
        {
            next = 0;
            nextAnim = 0;
        }

        public Color NextColor()
        {
            if (next == indexcolors.Length) next = 0;
            var color = new Color(indexcolors[next++] * 256);
            while (color.R < 50 && color.G < 50 && color.B < 50)
            {
                if (next == indexcolors.Length) next = 0;
                color = new Color(indexcolors[next++] * 256);
            }
            color.A = 255;
            return color;
            //return new Color(
            //    new Vector3(random.NextFloat(0.25f, 1), random.NextFloat(0.25f, 1), random.NextFloat(0.25f, 1)), 1);
        }
        public Color NextColorAnim()
        {
            if (nextAnim == indexcolors.Length) nextAnim = 0;
            var color = new Color(indexcolors[nextAnim++] * 256);
            while (color.R < 50 && color.G < 50 && color.B < 50)
            {
                if (nextAnim == indexcolors.Length) nextAnim = 0;
                color = new Color(indexcolors[nextAnim++] * 256);
            }
            color.A = 255;
            return color;
            //return new Color(
            //    new Vector3(random.NextFloat(0.25f, 1), random.NextFloat(0.25f, 1), random.NextFloat(0.25f, 1)), 1);
        }
    }
}
