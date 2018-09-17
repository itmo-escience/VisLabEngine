using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using FusionUI.UI.Plots;

namespace FusionUI.UI.Plots2_0
{

    public class PlotPoint
    {        
        public PlotPoint()
        {
            Index = NextIndex();
            PointsList.Add(this);
        }

        public virtual void Remove()
        {
            FreeIndicies.Add(Index);
            PointsList.Remove(this);
        }

        public int Index { get; protected set; }
        public virtual string Name => $"{Index}";
        private static List<int> FreeIndicies = new List<int>();

        private static int MaxIndex;

        public bool IsActive;
        public List<PlotData> Data = new List<PlotData>();
        public static List<PlotPoint> PointsList = new List<PlotPoint>();

        public static int NextIndex()
        {
            if (FreeIndicies.Count == 0)
            {
                FreeIndicies.Add(PointsList.Count);
            }
            var ans = FreeIndicies.Min();
            FreeIndicies.Remove(ans);
            return ans;
        }

        public PlotContainer Container;
    }

    //single variable data for single point
    public class PlotData 
    {
        public List<double> Depths => Data.Keys.ToList();        
        public Dictionary<double, List<DVector2>> Data;
        public HashSet<double> ActiveDepths = new HashSet<double>();
        public Dictionary<double, RectangleD> LimitsByDepth = new Dictionary<double, RectangleD>();
        public string DepthUnits;
        bool isActive = true;
        public virtual void OnDraw() { }

        public bool Dirty
        {
            set
            {
                if (value)
                {
                    ClearApproximation();
                    UpdateLimits();
                }
            }
        }

        public string varName;

        public PlotPoint Point;
        public PlotVariable Variable;        

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public bool IsPresent
        {
            get { return isActive && IsLoaded && (Point?.IsActive ?? true); }
            set { isActive = value; }
        }

        private bool isLoaded = false;

        public bool IsBarChart = false;
        public bool FixLimits = true;
        public bool IsLoaded
        {
            get { return isLoaded; }
            set { isLoaded = value; if (isLoaded) Approximate(30, false); }
        }

        public Color BaseColor = Color.Pink;
        public Dictionary<double, Color> ColorsByDepth = new Dictionary<double, Color>();

        public virtual void UpdateColors(ColorConfig cc)
        {
            if (Data == null) return;
            foreach (var k in Data.Keys)
            {
                ColorsByDepth[k] = cc.NextColor();
            }
        }

        public virtual void RepairColors(ColorConfig cc)
        {
            BaseColor = cc.NextColor();
            foreach (var k in Depths)
            {
                if (!ColorsByDepth.ContainsKey(k)) ColorsByDepth[k] = cc.NextColor();                
            }            
        }

        public void UpdateLimits()
        {
            foreach (var depth in Depths)
            {
                UpdateLimits(depth);
            }
        }

        public void UpdateLimits(double depth)
        {
            if (!Data.ContainsKey(depth)) { LimitsByDepth[depth] = new RectangleD(0, 0, 0, 0); return;}            
            var data = Data[depth].Where(a => !(double.IsNaN(a.X) || double.IsNaN(a.Y))).ToList();
            if (!data.Any()) return;
            var ld= new RectangleD() {
                
                Left = data.Min(a => a.X),
                Top = data.Min(a => a.Y),
                Right = data.Max(a => a.X),
                Bottom = data.Max(a => a.Y),
            };
            if (StartFromZeroX)
            {
                ld.Left = Math.Min(ld.Left, 0);
            }

            if (StartFromZeroY)
            {
                ld.Top = Math.Min(ld.Top, 0);
            }
            if (FixLimits)
            {
                ld.Left -= 1;
                ld.Width += 1;
                var h = ld.Height;
                ld.Top -=  h * 0.1f;
                ld.Bottom += h * 0.1f;
            }
            ld.Width = Math.Max(ld.Width, float.Epsilon);
            ld.Height = Math.Max(ld.Height, float.Epsilon);
            LimitsByDepth[depth] = ld;            
        }        

        private Dictionary<int, Dictionary<double, List<DVector2>>> ApproxPoints = new Dictionary<int, Dictionary<double, List<DVector2>>>();
        private int baseCount = -1;

        public int BaseCount
        {
            get
            {
                if (baseCount < 0) baseCount = Data.Max(a => a.Value.Count);
                return baseCount;
            }
        }

        public void Approximate(int pointsCount, bool delayed = true)
        {
            foreach (var depth in Depths)
            {
                Approximate(depth, pointsCount, delayed);
            }

        }

        public void Approximate(double depth, int pointsCount, bool delayed = true)
        {
            if (!ApproxPoints.ContainsKey(BaseCount))
            {
                var t = new Dictionary<double, List<DVector2>>(Data);
                ApproxPoints.Add(BaseCount, t);
            }
            var basePoints = Data[depth];
            pointsCount = Math.Min(pointsCount, BaseCount);
            if (ApproxPoints.ContainsKey(pointsCount) && ApproxPoints[pointsCount].ContainsKey(depth)) return;
            int ind = pointsCount;
            if (!acessTimes.ContainsKey(ind))
                acessTimes[ind] = new Dictionary<double, DateTime>();
            if (!ApproxPoints.ContainsKey(ind))
                ApproxPoints[ind] = new Dictionary<double, List<DVector2>>();            
            var nInd = ApproxPoints.Where(kv => kv.Value.ContainsKey(depth)).Min(a => Math.Abs(a.Key - ind));
            nInd = ApproxPoints.First(a => Math.Abs(a.Key - ind) == nInd && a.Value.ContainsKey(depth)).Key;
            if (acessTimes.ContainsKey(nInd))
                acessTimes[nInd][depth] = DateTime.Now;
            ApproxPoints[ind][depth] = ApproxPoints[nInd][depth];

            pointsCount = Math.Min(pointsCount, basePoints.Count);
            acessTimes[ind][depth] = DateTime.Now;
            if (delayed)
            {
                Gis.ResourceWorker.Post((x) =>
                {
                    x.ProcessQueue.Post((xx) =>
                    {
                        List<DVector2> newPointsMin = new List<DVector2>();
                        List<DVector2> newPointsMax = new List<DVector2>();
                        float d = (float)basePoints.Count / pointsCount;
                        double globalAverage = 0;
                        for (int i = 0; i < pointsCount; i++)
                        {
                            int left = (int) Math.Floor(Math.Max(0, (i - 0.5f) * d));
                            int right = (int) Math.Ceiling(Math.Min((i + 0.5f) * d, basePoints.Count));
                            double center = 0;
                            double min = double.MaxValue;
                            double max = double.MinValue;
                            int c = 0;
                            for (int t = left; t < right; t++)
                            {
                                if(double.IsNaN(basePoints[t].X) || double.IsNaN(basePoints[t].Y)) continue;
                                c++;
                                min = Math.Min(basePoints[t].Y, min);
                                max = Math.Max(basePoints[t].Y, max);
                                center += basePoints[t].X;
                                globalAverage += basePoints[t].Y;
                            }
                            newPointsMin.Add(new DVector2(center / c, min));
                            newPointsMax.Add(new DVector2(center / c, max));
                            //globalAverage += (min + max) / 2;
                        }
                        globalAverage /= basePoints.Count;
                        var newPoints = new List<DVector2>();
                        for (int i = 0; i < newPointsMax.Count; i++)
                        {
                            var avg = (newPointsMax[i].Y + newPointsMin[i].Y) / 2;
                            newPoints.Add(avg > globalAverage ? newPointsMax[i] : newPointsMin[i]);
                        }
                        acessTimes[ind][depth] = DateTime.Now;
                        ApproxPoints[ind][depth] = newPoints;
                    }, null);
                }, null);
            }
            else
            {
                List<DVector2> newPoints = new List<DVector2>();
                int d = basePoints.Count / pointsCount;
                for (int i = 0; i < pointsCount; i++)
                {
                    int left = (int)Math.Floor(Math.Max(0, (i - 0.5f) * d));
                    int right = (int)Math.Ceiling(Math.Min((i + 0.5f) * d, basePoints.Count));
                    DVector2 sum = DVector2.Zero;
                    for (int t = left; t < right; t++)
                    {
                        sum += basePoints[t];
                    }
                    newPoints.Add(sum / (right - left));
                }
                acessTimes[ind][depth] = DateTime.Now;
                ApproxPoints[ind][depth] = newPoints;
            }
        }

        public void ClearApproximation(float depth, int pointsCount)
        {
            pointsCount = Math.Min(pointsCount, BaseCount);
            if (ApproxPoints.ContainsKey(pointsCount) && ApproxPoints[pointsCount].ContainsKey(depth))
            {
                ApproxPoints[pointsCount].Remove(depth);
            }
        }

        public void ClearApproximation()
        {
            ApproxPoints.Clear();
        }

        public void ClearApproximation(TimeSpan lifetime)
        {
            foreach (var pcv in ApproxPoints)
            {
                foreach (var dv in ApproxPoints[pcv.Key].Keys.ToList())
                {
                    if (acessTimes.ContainsKey(pcv.Key) && acessTimes[pcv.Key].ContainsKey(dv) && DateTime.Now - lifetime > acessTimes[pcv.Key][dv])
                    {
                        ApproxPoints[pcv.Key].Remove(dv);                        
                    }
                }
            }
        }

        private Dictionary<int, Dictionary<double, DateTime>> acessTimes = new Dictionary<int, Dictionary<double, DateTime>>();

        public List<DVector2> this[int pointsCount, double depth]
        {
            get
            {
                pointsCount = Math.Min(pointsCount, BaseCount);
                if (pointsCount == 0) return Data[depth];
                if (!ApproxPoints.ContainsKey(pointsCount) || !ApproxPoints[pointsCount].ContainsKey(depth))
                {
                    Approximate(depth, pointsCount);
                }
                if (pointsCount != BaseCount)
                {
                    acessTimes[pointsCount][depth] = DateTime.Now;                 
                }
                ClearApproximation(TimeSpan.FromSeconds(10));
                return ApproxPoints[pointsCount][depth];
            }
        }

        public bool StartFromZeroX, StartFromZeroY;
        public RectangleD Limits
        {
            get
            {
                RectangleD? ans = null;
                if (Depths == null) return RectangleD.Empty;                
                foreach (var depth in Depths)
                {
                    if (ans == null)
                    {
                        if (!LimitsByDepth.ContainsKey(depth)) UpdateLimits(depth);
                        ans = LimitsByDepth[depth];
                    }
                    else
                    {
                        var r = ans.Value;
                        if (!LimitsByDepth.ContainsKey(depth)) UpdateLimits(depth);
                        r.Left = Math.Min(ans.Value.Left, LimitsByDepth[depth].Left);
                        r.Right = Math.Max(ans.Value.Right, LimitsByDepth[depth].Right);
                        r.Top = Math.Min(ans.Value.Top, LimitsByDepth[depth].Top);
                        r.Bottom = Math.Max(ans.Value.Bottom, LimitsByDepth[depth].Bottom);
                        ans = r;
                    }
                }
                if (ans != null)
                {
                    var rect = ans.Value;
                    rect.Width = Math.Max(rect.Width, float.Epsilon);
                    rect.Height = Math.Max(rect.Height, float.Epsilon);
                    if (IsBarChart)
                    {
                    //    rect.Left -= 1;
                    //    rect.Width += 2;
                        var h = rect.Height;
                        if (rect.Bottom > 0) rect.Bottom += h * 0.1f;
                        if (rect.Top < 0) rect.Top -= h * 0.1f;
                    }
                    return rect;
                }
                return RectangleD.Empty;                
            }
        }

        public RectangleD ActiveLimits
        {
            get
            {
                RectangleD? ans = null;
                if (ActiveDepths == null) return RectangleD.Empty;
                foreach (var depth in ActiveDepths)
                {
                    if (ans == null)
                    {
                        if (!LimitsByDepth.ContainsKey(depth)) UpdateLimits(depth);
                        ans = LimitsByDepth[depth];
                    }
                    else
                    {
                        var r = ans.Value;
                        if (!LimitsByDepth.ContainsKey(depth)) UpdateLimits(depth);
                        r.Left = Math.Min(ans.Value.Left, LimitsByDepth[depth].Left);
                        r.Right = Math.Max(ans.Value.Right, LimitsByDepth[depth].Right);
                        r.Top = Math.Min(ans.Value.Top, LimitsByDepth[depth].Top);
                        r.Bottom = Math.Max(ans.Value.Bottom, LimitsByDepth[depth].Bottom);
                        ans = r;
                    }
                }
                if (ans != null)
                {
                    var rect = ans.Value;
                    rect.Width = Math.Max(rect.Width, float.Epsilon);
                    rect.Height = Math.Max(rect.Height, float.Epsilon);
                    //if (IsBarChart)
                    //{
                    //    rect.Left -= 1;
                    //    rect.Width += 2;
                    //    rect.Height *= 1.1f;
                    //}
                    return rect;
                }
                return RectangleD.Empty;
            }
        }
    }

    //single variable data for multiple point
    public class PlotVariable
    {
        public Action OnUpdate;

        public void UpdateColors(ColorConfig cc)
        {
            foreach (var v in Data.Values)
            {
                foreach (var kv in v)
                {
                    kv.Value.UpdateColors(cc);
                }                
            }
        }

        public void RepairColors(ColorConfig cc)
        {
            foreach (var v in Data.Values)
            {
                foreach (var plotData in v.Values)
                {
                    plotData.RepairColors(cc);
                }
                
            }
        }

        public PlotContainer Container;

        public string Units = "", CatUnits = "";

        public List<PlotPoint> Points => Data.Keys.ToList();
        public Dictionary<PlotPoint, Dictionary<string, PlotData>> Data = new Dictionary<PlotPoint, Dictionary<string, PlotData>>();
        public bool IsActive = true;
        public bool IsPresent => IsActive && Data.Any(a => a.Value.Any(b => b.Value.IsPresent));
        public Func<double, double, string> xFunction = (v, w) => v.ToString("0.###"), yFunction = (v, w) => v.ToString("0.###");

        public Func<double, double, string> bcFunction = (v, w) =>
            v.ToString($"F{(int) Math.Max(0, 3 - Math.Floor(Math.Log(Math.Max(1, v), 10)))}");
        public virtual void OnDraw() { }
        public String Name = "", NiceName = "";

        public static RectangleD AggregateLimits(List<RectangleD> data)
        {
            if (data.Count == 0) return RectangleD.Empty;
            return data.Aggregate((r1, r2) =>
            {
                if (r1.IsEmpty) return r2;
                if (r2.IsEmpty) return r1;
                return RectangleD.Union(r1, r2);
            });
        }

        public RectangleD LimitsAnim;
        public RectangleD Limits
        {
            get
            {
                return AggregateLimits(Data.Values
                    .Where(v => v.Any(a => a.Value.IsPresent) && v.Any(a => a.Value.Depths.Count > 0))
                    .Select(a => AggregateLimits(a.Where(b => b.Value.IsPresent).Select(v => v.Value.Limits).ToList())).ToList());
                //RectangleD? ans = null;
                //foreach (var v in Data.Values.Where(v => v.Any(a => a.Value.IsPresent) && v.Any(a => a.Value.ActiveDepths.Count > 0)))
                //{
                //    var l = AggregateLimits(v.Select(a => a.Value.Limits).ToList());               
                //    if (ans == null)
                //    {
                //        ans = v.Limits;
                //    }
                //    else
                //    {
                //        var r = ans.Value;
                //        var l = v.Limits;
                //        r.Left = Math.Min(ans.Value.Left, l.Left);
                //        r.Right = Math.Max(ans.Value.Right, l.Right);
                //        r.Top = Math.Min(ans.Value.Top, l.Top);
                //        r.Bottom = Math.Max(ans.Value.Bottom, l.Bottom);
                //        ans = r;
                //    }
                //}
                //if (ans != null)
                //{
                //    var rect = ans.Value;
                //    rect.Width = Math.Max(rect.Width, float.Epsilon);
                //    rect.Height = Math.Max(rect.Height, float.Epsilon);
                //    return rect;
                //}
                //return RectangleD.Empty;
            }
        }

        public HashSet<double> Depths => this.Data.Values.Aggregate(new HashSet<double>(),
            (set, data) =>
            {
                set = data.Aggregate(set, (d1, d2) => {
                    d2.Value.Depths.ForEach(a => d1.Add(a));
                    return d1;
                });
                return set;
            });

        public HashSet<double> ActiveDepths => this.Data.Values.Aggregate(new HashSet<double>(),
            (set, data) =>
            {
                set = data.Aggregate(set, (d1, d2) => {
                    d2.Value.ActiveDepths.ToList().ForEach(a => d1.Add(a));
                    return d1;
                });
                return set;
            });

        public bool StartFromZeroX
        {
            set
            {
                foreach (var dataValue in Data.Values)
                {
                    foreach (var dataValueValue in dataValue.Values)
                    {
                        if (dataValueValue.StartFromZeroX != value)
                        {
                            dataValueValue.StartFromZeroX = value;
                            dataValueValue.UpdateLimits();
                        }
                    }
                }
            }   
        }

        public bool StartFromZeroY
        {
            set
            {
                foreach (var dataValue in Data.Values)
                {
                    foreach (var dataValueValue in dataValue.Values)
                    {
                        if (dataValueValue.StartFromZeroY != value)
                        {
                            dataValueValue.StartFromZeroY = value;
                            dataValueValue.UpdateLimits();
                        }
                    }
                }
            }
        }

        public RectangleD ActiveLimits
        {
            get
            {
                var limits = AggregateLimits(Data.Values.Select(a => AggregateLimits(a.Where(b => b.Value.IsPresent && b.Value.ActiveDepths.Count > 0).Select(v => v.Value.ActiveLimits).ToList()))
                    .ToList());                
                return limits;
                //RectangleD? ans = null;
                //foreach (var v in Data.Values)
                //{
                //    if (!v.IsPresent) continue;
                //    if (ans == null)
                //    {
                //        ans = v.ActiveLimits;
                //    }
                //    else
                //    {
                //        var r = ans.Value;
                //        var l = v.Limits;
                //        r.Left = Math.Min(ans.Value.Left, l.Left);
                //        r.Right = Math.Max(ans.Value.Right, l.Right);
                //        r.Top = Math.Min(ans.Value.Top, l.Top);
                //        r.Bottom = Math.Max(ans.Value.Bottom, l.Bottom);
                //        ans = r;
                //    }
                //}
                //if (ans != null)
                //{
                //    var rect = ans.Value;
                //    rect.Width = Math.Max(rect.Width, float.Epsilon);
                //    rect.Height = Math.Max(rect.Height, float.Epsilon);
                //    return rect;
                //}
                //return RectangleD.Empty;
            }
        }

        public RectangleD LimitsAligned;

        public virtual void Merge(PlotVariable another)
        {
            foreach (var kv in another.Data)
            {
                if (!Data.ContainsKey(kv.Key)) Data.Add(kv.Key, kv.Value);
            }
        }
    }

    //multiple variable data for multiple point 
    public class PlotContainer
    {

        public List<string> VariableNames => Data.Keys.ToList();
        public Dictionary<string, PlotVariable> Data = new Dictionary<string, PlotVariable>();

        private ColorConfig ColorConfig = new ColorConfig();

        public void ResetColors()
        {
            ColorConfig.Reset();
            foreach (var v in Data.Values)
            {
                v.UpdateColors(ColorConfig);
            }
        }

        public void RepairColors()
        {
            foreach (var v in Data.Values)
            {
                v.RepairColors(ColorConfig);
            }
        }

        public virtual void Merge(PlotContainer another)
        {
            foreach (var kv in another.Data)
            {
                kv.Value.UpdateColors(ColorConfig);
                if (Data.ContainsKey(kv.Key)) Data[kv.Key].Merge(kv.Value);
                else Data.Add(kv.Key, kv.Value);
            }
        }
    }
}
