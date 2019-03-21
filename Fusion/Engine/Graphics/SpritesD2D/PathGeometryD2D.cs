using System.Collections.Generic;
using System.Linq;
using SharpDX.Direct2D1;
using Fusion.Core;
using Fusion.Core.Mathematics;
using SharpDX.Mathematics.Interop;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class PathGeometryD2D : DisposableBase
    {
        internal readonly PathGeometry PathGeometry;
        private GeometrySink _sink;

        public PathGeometryD2D(SpriteLayerD2D layer)
        {
            PathGeometry = new PathGeometry(layer.Factory);
        }

        public void BeginFigure(Vector2 startPoint, FigureBeginD2D figureBegin)
        {
            _sink = PathGeometry.Open();
            _sink.BeginFigure(startPoint.ToRawVector2(), figureBegin == FigureBeginD2D.Filled ? FigureBegin.Filled : FigureBegin.Hollow);
        }

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

        //_sink.AddArc(new ArcSegment(){});

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
}
