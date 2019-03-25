using System.Collections.Generic;
using System.Linq;
using SharpDX.Direct2D1;
using Fusion.Core;
using SharpDX;
using SharpDX.Mathematics.Interop;
using Size2 = Fusion.Core.Mathematics.Size2;
using Size2F = Fusion.Core.Mathematics.Size2F;
using Vector2 = Fusion.Core.Mathematics.Vector2;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class PathGeometryD2D : DisposableBase
    {
        internal readonly PathGeometry PathGeometry;
        private GeometrySink _sink;

        public PathGeometryD2D(SpriteLayerD2D layer)
        {
            PathGeometry = new PathGeometry(layer.Factory);
            _sink = PathGeometry.Open();
        }

        public PathGeometryD2D(List<Vector2> points, PathGeometryD2DDesc geometryDesc, SpriteLayerD2D layer) : this(layer)
        {
            SetFillMode(geometryDesc.FillModeD2D);
            BeginFigure(points[0], geometryDesc.FigureBeginD2D);
            AddLines(points.GetRange(1, points.Count - 1));
            EndFigure(geometryDesc.FigureEndD2D);
        }

        public PathGeometryD2D(Vector2 startPoint, List<QuadraticBezierSegmentD2D> segments, PathGeometryD2DDesc geometryDesc, SpriteLayerD2D layer) : this(layer)
        {
            SetFillMode(geometryDesc.FillModeD2D);
            BeginFigure(startPoint, geometryDesc.FigureBeginD2D);
            AddQuadraticBeziers(segments);
            EndFigure(geometryDesc.FigureEndD2D);
        }

        public PathGeometryD2D(Vector2 startPoint, List<BezierSegmentD2D> segments, PathGeometryD2DDesc geometryDesc, SpriteLayerD2D layer) : this(layer)
        {
            SetFillMode(geometryDesc.FillModeD2D);
            BeginFigure(startPoint, geometryDesc.FigureBeginD2D);
            AddBeziers(segments);
            EndFigure(geometryDesc.FigureEndD2D);
        }

        public void BeginFigure(Vector2 startPoint, FigureBeginD2D figureBegin) =>
            _sink.BeginFigure(startPoint.ToRawVector2(), figureBegin == FigureBeginD2D.Filled ? FigureBegin.Filled : FigureBegin.Hollow);

        public void EndFigure(FigureEndD2D figureEnd)
        {
            _sink.EndFigure(figureEnd == FigureEndD2D.Open ? FigureEnd.Open : FigureEnd.Closed);
            _sink.Close();
            _sink.Dispose();
        } 

        public void AddLine(Vector2 point) => _sink.AddLine(point.ToRawVector2());
        public void AddLines(List<Vector2> points) => _sink.AddLines(points.Select(p => p.ToRawVector2()).ToArray());

        public void AddQuadraticBezier(QuadraticBezierSegmentD2D segment) => 
            _sink.AddQuadraticBezier(segment.ToQuadraticBezierSegment());
        public void AddQuadraticBeziers(List<QuadraticBezierSegmentD2D> segments) => 
            _sink.AddQuadraticBeziers(segments.Select(s => s.ToQuadraticBezierSegment()).ToArray());

        public void AddBezier(BezierSegmentD2D segment) => 
            _sink.AddBezier(segment.ToBezierSegment());
        public void AddBeziers(List<BezierSegmentD2D> segments) => 
            _sink.AddBeziers(segments.Select(s => s.ToBezierSegment()).ToArray());

        public void AddArc(ArcD2D arc) => _sink.AddArc(arc.ToArcSegment());

        public void SetFillMode(FillModeD2D fillMode) =>
            _sink.SetFillMode(fillMode == FillModeD2D.Alternate ? FillMode.Alternate : FillMode.Winding);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            PathGeometry.Dispose();
        }
    }

    public class QuadraticBezierSegmentD2D
    {
        public Vector2 Point1 { get; set; }
        public Vector2 Point2 { get; set; }

        internal QuadraticBezierSegment ToQuadraticBezierSegment()
        {
            return new QuadraticBezierSegment()
            {
                Point1 = Point1.ToRawVector2(),
                Point2 = Point2.ToRawVector2()
            };
        }
    }

    public class BezierSegmentD2D
    {
        public Vector2 Point1 { get; set; }
        public Vector2 Point2 { get; set; }
        public Vector2 Point3 { get; set; }

        internal BezierSegment ToBezierSegment()
        {
            return new BezierSegment()
            {
                Point1 = Point1.ToRawVector2(),
                Point2 = Point2.ToRawVector2(),
                Point3 = Point3.ToRawVector2()
            };
        }
    }

    public class ArcD2D
    {
        public ArcSizeD2D ArcSize { get; set; }
        public Vector2 EndPoint { get; set; }
        public float RotationAngle { get; set; }
        public Size2F Size { get; set; }
        public SweepDirectionD2D SweepDirection { get; set; }

        internal ArcSegment ToArcSegment()
        {
            return new ArcSegment()
            {
                ArcSize = (ArcSize == ArcSizeD2D.Small) ? SharpDX.Direct2D1.ArcSize.Small : SharpDX.Direct2D1.ArcSize.Large,
                Point = EndPoint.ToRawVector2(),
                RotationAngle = RotationAngle,
                Size = new SharpDX.Size2F(Size.Width, Size.Height),
                SweepDirection = (SweepDirection == SweepDirectionD2D.Clockwise) ? SharpDX.Direct2D1.SweepDirection.Clockwise : SharpDX.Direct2D1.SweepDirection.CounterClockwise
            };
        }
    }

    public struct PathGeometryD2DDesc
    {
        public FigureBeginD2D FigureBeginD2D { get; set; }
        public FigureEndD2D FigureEndD2D { get; set; }
        public FillModeD2D FillModeD2D { get; set; }
    }

    public enum FigureBeginD2D
    {
        Filled, Hollow
    }

    public enum FigureEndD2D
    {
        Open, Closed
    }

    public enum FillModeD2D
    {
        Alternate, Winding
    }

    public enum ArcSizeD2D
    {
        Small, Large
    }

    public enum SweepDirectionD2D
    {
        Clockwise, CounterClockwise
    }
}
