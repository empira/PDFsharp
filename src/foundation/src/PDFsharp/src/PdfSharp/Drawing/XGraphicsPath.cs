// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
using SysPoint = System.Windows.Point;
using SysSize = System.Windows.Size;
using SysRect = System.Windows.Rect;
using WpfBrushes = System.Windows.Media.Brushes;
#endif
#if UWP
using Windows.UI.Xaml.Media;
using SysPoint = Windows.Foundation.Point;
using SysSize = Windows.Foundation.Size;
using SysRect = Windows.Foundation.Rect;
#endif
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Represents a series of connected lines and curves.
    /// </summary>
    public sealed class XGraphicsPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XGraphicsPath"/> class.
        /// </summary>
        public XGraphicsPath()
        {
#if CORE
            CorePath = new CoreGraphicsPath();
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath = new GraphicsPath();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            PathGeometry = new PathGeometry();
#endif
        }

#if GDI
        /// <summary>
        /// Initializes a new instance of the <see cref="XGraphicsPath"/> class.
        /// </summary>
        public XGraphicsPath(PointF[] points, byte[] types, XFillMode fillMode)
        {
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath = new GraphicsPath(points, types, (FillMode)fillMode);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF  // Is true only in Hybrid build.
            _pathGeometry = new PathGeometry();
            _pathGeometry.FillRule = FillRule.EvenOdd;
#endif
        }
#endif

#if WPF || UWP
        /// <summary>
        /// Gets the current path figure.
        /// </summary>
        PathFigure CurrentPathFigure
        {
            get
            {
                int count = PathGeometry.Figures.Count;
                if (count == 0)
                {
                    // Create new figure if there is none.
                    PathGeometry.Figures.Add(new PathFigure());
                    count++;
                }
                else
                {
                    PathFigure lastFigure = PathGeometry.Figures[count - 1];
                    if (lastFigure.IsClosed)
                    {
                        if (lastFigure.Segments.Count > 0)
                        {
                            // Create new figure if previous one was closed.
                            PathGeometry.Figures.Add(new PathFigure());
                            count++;
                        }
                        else
                        {
                            Debug.Assert(false);
                        }
                    }
                }
                // Return last figure in collection.
                return PathGeometry.Figures[count - 1];
            }
        }

        /// <summary>
        /// Gets the current path figure, but never created a new one.
        /// </summary>
        PathFigure PeekCurrentFigure
        {
            get
            {
                int count = PathGeometry.Figures.Count;
                return count == 0 ? new PathFigure() : PathGeometry.Figures[count - 1];
            }
        }
#endif

        /// <summary>
        /// Clones this instance.
        /// </summary>
        public XGraphicsPath Clone()
        {
            var path = (XGraphicsPath)MemberwiseClone();
#if CORE
            CorePath = new CoreGraphicsPath(CorePath);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                path.GdipPath = (GraphicsPath)GdipPath.Clone();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            path.PathGeometry = PathGeometry.Clone();
#endif
            return path;
        }

        // ----- AddLine ------------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a line segment to current figure.
        /// </summary>
        public void AddLine(System.Drawing.Point pt1, System.Drawing.Point pt2)
        {
            AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }
#endif

#if WPF
        /// <summary>
        /// Adds a line segment to current figure.
        /// </summary>
        public void AddLine(SysPoint pt1, SysPoint pt2)
        {
            AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a line segment to current figure.
        /// </summary>
        public void AddLine(PointF pt1, PointF pt2)
        {
            AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }
#endif

        /// <summary>
        /// Adds a line segment to current figure.
        /// </summary>
        public void AddLine(XPoint pt1, XPoint pt2)
        {
            AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        /// <summary>
        /// Adds a line segment to current figure.
        /// </summary>
        public void AddLine(double x1, double y1, double x2, double y2)
        {
#if CORE
            CorePath.MoveOrLineTo(x1, y1);
            CorePath.LineTo(x2, y2, false);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddLine((float)x1, (float)y1, (float)x2, (float)y2);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
            PathFigure figure = CurrentPathFigure;
            if (figure.Segments.Count == 0)
            {
                figure.StartPoint = new SysPoint(x1, y1);
#if true
                var lineSegment = new LineSegment(new SysPoint(x2, y2), true);
#else
                var lineSegment = new LineSegment { Point = new Point(x2, y2) };
#endif
                figure.Segments.Add(lineSegment);
            }
            else
            {
#if true
                var lineSegment1 = new LineSegment(new SysPoint(x1, y1), true);
                var lineSegment2 = new LineSegment(new SysPoint(x2, y2), true);
#else
                var lineSegment1 = new LineSegment { Point = new Point(x1, y1) };
                var lineSegment2 = new LineSegment { Point = new Point(x2, y2) };
#endif
                figure.Segments.Add(lineSegment1);
                figure.Segments.Add(lineSegment2);
            }
#endif
        }

        // ----- AddLines -----------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a series of connected line segments to current figure.
        /// </summary>
        public void AddLines(System.Drawing.Point[] points)
        {
            AddLines(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

#if WPF
        /// <summary>
        /// Adds a series of connected line segments to current figure.
        /// </summary>
        public void AddLines(SysPoint[] points)
        {
            AddLines(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

#if GDI
        /// <summary>
        /// Adds a series of connected line segments to current figure.
        /// </summary>
        public void AddLines(PointF[] points)
        {
            AddLines(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

        /// <summary>
        /// Adds a series of connected line segments to current figure.
        /// </summary>
        public void AddLines(XPoint[] points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            int count = points.Length;
            if (count == 0)
                return;
#if CORE
            CorePath.MoveOrLineTo(points[0].X, points[0].Y);
            for (int idx = 1; idx < count; idx++)
                CorePath.LineTo(points[idx].X, points[idx].Y, false);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddLines(XGraphics.MakePointFArray(points));
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
            PathFigure figure = CurrentPathFigure;
            if (figure.Segments.Count == 0)
            {
                figure.StartPoint = new SysPoint(points[0].X, points[0].Y);
                for (int idx = 1; idx < count; idx++)
                {
#if true
                    LineSegment lineSegment = new LineSegment(new SysPoint(points[idx].X, points[idx].Y), true);
#else
                    LineSegment lineSegment = new LineSegment();
                    lineSegment.Point = new Point(points[idx].X, points[idx].Y); // ,true?
#endif
                    figure.Segments.Add(lineSegment);
                }
            }
            else
            {
                for (int idx = 0; idx < count; idx++)
                {
                    // figure.Segments.Add(new LineSegment(new SysPoint(points[idx].x, points[idx].y), true));
#if true
                    LineSegment lineSegment = new LineSegment(new SysPoint(points[idx].X, points[idx].Y), true);
#else
                    LineSegment lineSegment = new LineSegment();
                    lineSegment.Point = new Point(points[idx].X, points[idx].Y); // ,true?
#endif
                    figure.Segments.Add(lineSegment);
                }
            }
#endif
        }

        // ----- AddBezier ----------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(System.Drawing.Point pt1, System.Drawing.Point pt2, System.Drawing.Point pt3, System.Drawing.Point pt4)
        {
            AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }
#endif

#if WPF
        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(SysPoint pt1, SysPoint pt2, SysPoint pt3, SysPoint pt4)
        {
            AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4)
        {
            AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }
#endif

        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(XPoint pt1, XPoint pt2, XPoint pt3, XPoint pt4)
        {
            AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }

        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
#if CORE
            CorePath.MoveOrLineTo(x1, y1);
            CorePath.BezierTo(x2, y2, x3, y3, x4, y4, false);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddBezier((float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3, (float)x4, (float)y4);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
            PathFigure figure = CurrentPathFigure;
            if (figure.Segments.Count == 0)
                figure.StartPoint = new SysPoint(x1, y1);
            else
            {
                // figure.Segments.Add(new LineSegment(new SysPoint(x1, y1), true));
#if true
                LineSegment lineSegment = new LineSegment(new SysPoint(x1, y1), true);
#else
                LineSegment lineSegment = new LineSegment();
                lineSegment.Point = new Point(x1, y1);
#endif
                figure.Segments.Add(lineSegment);
            }
            //figure.Segments.Add(new BezierSegment(
            //  new SysPoint(x2, y2),
            //  new SysPoint(x3, y3),
            //  new SysPoint(x4, y4), true));
#if true
            BezierSegment bezierSegment = new BezierSegment(
                new SysPoint(x2, y2),
                new SysPoint(x3, y3),
                new SysPoint(x4, y4), true);
#else
            BezierSegment bezierSegment = new BezierSegment();
            bezierSegment.Point1 = new Point(x2, y2);
            bezierSegment.Point2 = new Point(x3, y3);
            bezierSegment.Point3 = new Point(x4, y4);
#endif
            figure.Segments.Add(bezierSegment);
#endif
        }

        // ----- AddBeziers ---------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a sequence of connected cubic Bézier curves to the current figure.
        /// </summary>
        public void AddBeziers(System.Drawing.Point[] points)
        {
            AddBeziers(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

#if WPF
        /// <summary>
        /// Adds a sequence of connected cubic Bézier curves to the current figure.
        /// </summary>
        public void AddBeziers(SysPoint[] points)
        {
            AddBeziers(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

#if GDI
        /// <summary>
        /// Adds a sequence of connected cubic Bézier curves to the current figure.
        /// </summary>
        public void AddBeziers(PointF[] points)
        {
            AddBeziers(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

        /// <summary>
        /// Adds a sequence of connected cubic Bézier curves to the current figure.
        /// </summary>
        public void AddBeziers(XPoint[] points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            int count = points.Length;
            if (count < 4)
                throw new ArgumentException("At least four points required for bezier curve.", nameof(points));

            if ((count - 1) % 3 != 0)
                throw new ArgumentException("Invalid number of points for bezier curve. Number must fulfil 4+3n.",
                    nameof(points));

#if CORE
            CorePath.MoveOrLineTo(points[0].X, points[0].Y);
            for (int idx = 1; idx < count; idx += 3)
            {
                CorePath.BezierTo(points[idx].X, points[idx].Y, points[idx + 1].X, points[idx + 1].Y,
                    points[idx + 2].X, points[idx + 2].Y, false);
            }
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddBeziers(XGraphics.MakePointFArray(points));
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
            PathFigure figure = CurrentPathFigure;
            if (figure.Segments.Count == 0)
                figure.StartPoint = new SysPoint(points[0].X, points[0].Y);
            else
            {
                // figure.Segments.Add(new LineSegment(new SysPoint(points[0].x, points[0].y), true));
#if true
                LineSegment lineSegment = new LineSegment(new SysPoint(points[0].X, points[0].Y), true);
#else
                LineSegment lineSegment = new LineSegment();
                lineSegment.Point = new Point(points[0].X, points[0].Y);
#endif
                figure.Segments.Add(lineSegment);
            }
            for (int idx = 1; idx < count; idx += 3)
            {
                //figure.Segments.Add(new BezierSegment(
                //                      new SysPoint(points[idx].x, points[idx].y),
                //                      new SysPoint(points[idx + 1].x, points[idx + 1].y),
                //                      new SysPoint(points[idx + 2].x, points[idx + 2].y), true));
#if true
                BezierSegment bezierSegment = new BezierSegment(
                                      new SysPoint(points[idx].X, points[idx].Y),
                                      new SysPoint(points[idx + 1].X, points[idx + 1].Y),
                                      new SysPoint(points[idx + 2].X, points[idx + 2].Y), true);
#else
                BezierSegment bezierSegment = new BezierSegment();
                bezierSegment.Point1 = new Point(points[idx].X, points[idx].Y);
                bezierSegment.Point2 = new Point(points[idx + 1].X, points[idx + 1].Y);
                bezierSegment.Point3 = new Point(points[idx + 2].X, points[idx + 2].Y);
#endif
                figure.Segments.Add(bezierSegment);
            }
#endif
        }

        // ----- AddCurve -----------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(System.Drawing.Point[] points)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

#if WPF
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(SysPoint[] points)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(PointF[] points)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(XPoint[] points)
        {
            AddCurve(points, 0.5);
        }

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(System.Drawing.Point[] points, double tension)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }
#endif

#if WPF
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(SysPoint[] points, double tension)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(PointF[] points, double tension)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }
#endif

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(XPoint[] points, double tension)
        {
            int count = points.Length;
            if (count < 2)
                throw new ArgumentException("AddCurve requires two or more points.", nameof(points));
#if CORE
            CorePath.AddCurve(points, tension);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddCurve(XGraphics.MakePointFArray(points), (float)tension);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
            tension /= 3;

            PathFigure figure = CurrentPathFigure;
            if (figure.Segments.Count == 0)
                figure.StartPoint = new SysPoint(points[0].X, points[0].Y);
            else
            {
                // figure.Segments.Add(new LineSegment(new SysPoint(points[0].x, points[0].y), true));
#if true
                LineSegment lineSegment = new LineSegment(new SysPoint(points[0].X, points[0].Y), true);
#else
                LineSegment lineSegment = new LineSegment();
                lineSegment.Point = new Point(points[0].X, points[0].Y);
#endif
                figure.Segments.Add(lineSegment);
            }

            if (count == 2)
            {
                figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[0], points[0], points[1], points[1], tension));
            }
            else
            {
                figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[0], points[0], points[1], points[2], tension));
                for (int idx = 1; idx < count - 2; idx++)
                    figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[idx - 1], points[idx], points[idx + 1], points[idx + 2], tension));
                figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[count - 3], points[count - 2], points[count - 1], points[count - 1], tension));
            }
#endif
        }

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(System.Drawing.Point[] points, int offset, int numberOfSegments, float tension)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), offset, numberOfSegments, tension);
        }
#endif

#if WPF
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(SysPoint[] points, int offset, int numberOfSegments, float tension)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), offset, numberOfSegments, tension);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(PointF[] points, int offset, int numberOfSegments, float tension)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), offset, numberOfSegments, tension);
        }
#endif

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(XPoint[] points, int offset, int numberOfSegments, double tension)
        {
#if CORE
            throw new NotImplementedException("AddCurve not yet implemented.");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddCurve(XGraphics.MakePointFArray(points), offset, numberOfSegments, (float)tension);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
            throw new NotImplementedException("AddCurve not yet implemented.");
#endif
        }

        // ----- AddArc -------------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(Rectangle rect, double startAngle, double sweepAngle)
        {
            AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }
#endif

#if GDI
        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(RectangleF rect, double startAngle, double sweepAngle)
        {
            AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }
#endif

        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(XRect rect, double startAngle, double sweepAngle)
        {
            AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(double x, double y, double width, double height, double startAngle, double sweepAngle)
        {
#if CORE
            CorePath.AddArc(x, y, width, height, startAngle, sweepAngle);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddArc((float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
            PathFigure figure = CurrentPathFigure;
            SysPoint startPoint;
            ArcSegment seg = GeometryHelper.CreateArcSegment(x, y, width, height, startAngle, sweepAngle, out startPoint);
            if (figure.Segments.Count == 0)
                figure.StartPoint = startPoint;
            else
            {
                LineSegment lineSegment = new LineSegment();
                lineSegment.Point = startPoint;
                figure.Segments.Add(lineSegment);
            }

            figure.Segments.Add(seg);

            //figure.Segments.Add(
            //if (figure.Segments.Count == 0)
            //  figure.StartPoint = new SysPoint(points[0].x, points[0].y);
            //else
            //  figure.Segments.Add(new LineSegment(new SysPoint(points[0].x, points[0].y), true));

            //for (int idx = 1; idx < 5555; idx += 3)
            //  figure.Segments.Add(new BezierSegment(
            //    new SysPoint(points[idx].x, points[idx].y),
            //    new SysPoint(points[idx + 1].x, points[idx + 1].y),
            //    new SysPoint(points[idx + 2].x, points[idx + 2].y), true));
#endif
        }

        /// <summary>
        /// Adds an elliptical arc to the current figure. The arc is specified WPF like.
        /// </summary>
        public void AddArc(XPoint point1, XPoint point2, XSize size, double rotationAngle, bool isLargeArg, XSweepDirection sweepDirection)
        {
#if CORE
            CorePath.AddArc(point1, point2, size, rotationAngle, isLargeArg, sweepDirection);
#endif
#if GDI
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddArc");
#endif
#if WPF
            PathFigure figure = CurrentPathFigure;
            if (figure.Segments.Count == 0)
                figure.StartPoint = point1.ToPoint();
            else
            {
                // figure.Segments.Add(new LineSegment(point1.ToPoint(), true));
#if true
                LineSegment lineSegment = new LineSegment(point1.ToPoint(), true);
#else
                LineSegment lineSegment = new LineSegment();
                lineSegment.Point = point1.ToPoint();
#endif
                figure.Segments.Add(lineSegment);
            }

            // figure.Segments.Add(new ArcSegment(point2.ToPoint(), size.ToSize(), rotationAngle, isLargeArg, sweepDirection, true));
#if true
            ArcSegment arcSegment = new ArcSegment(point2.ToPoint(), size.ToSize(), rotationAngle, isLargeArg, (SweepDirection)sweepDirection, true);
#else
            ArcSegment arcSegment = new ArcSegment();
            arcSegment.Point = point2.ToPoint();
            arcSegment.Size = size.ToSize();
            arcSegment.RotationAngle = rotationAngle;
            arcSegment.IsLargeArc = isLargeArg;
            arcSegment.SweepDirection = (SweepDirection)sweepDirection;
#endif
            figure.Segments.Add(arcSegment);
#endif
        }

        // ----- AddRectangle -------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(Rectangle rect)
        {
            AddRectangle(new XRect(rect));
        }
#endif

#if GDI
        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(RectangleF rect)
        {
            AddRectangle(new XRect(rect));
        }
#endif

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(XRect rect)
        {
#if CORE
            CorePath.MoveTo(rect.X, rect.Y);
            CorePath.LineTo(rect.X + rect.Width, rect.Y, false);
            CorePath.LineTo(rect.X + rect.Width, rect.Y + rect.Height, false);
            CorePath.LineTo(rect.X, rect.Y + rect.Height, true);
            CorePath.CloseSubpath();
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                // If rect is empty GDI+ removes the rect from the path.
                // This is not intended if the path is used for clipping.
                // See http://forum.pdfsharp.net/viewtopic.php?p=9433#p9433
                // _gdipPath.AddRectangle(rect.ToRectangleF());

                // Draw the rectangle manually.
                GdipPath.StartFigure();
                GdipPath.AddLines(new PointF[] { rect.TopLeft.ToPointF(), rect.TopRight.ToPointF(), rect.BottomRight.ToPointF(), rect.BottomLeft.ToPointF() });
                GdipPath.CloseFigure();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
            StartFigure();
            PathFigure figure = CurrentPathFigure;
            figure.StartPoint = new SysPoint(rect.X, rect.Y);

            // figure.Segments.Add(new LineSegment(new SysPoint(rect.x + rect.width, rect.y), true));
            // figure.Segments.Add(new LineSegment(new SysPoint(rect.x + rect.width, rect.y + rect.height), true));
            // figure.Segments.Add(new LineSegment(new SysPoint(rect.x, rect.y + rect.height), true));
#if true
            LineSegment lineSegment1 = new LineSegment(new SysPoint(rect.X + rect.Width, rect.Y), true);
            LineSegment lineSegment2 = new LineSegment(new SysPoint(rect.X + rect.Width, rect.Y + rect.Height), true);
            LineSegment lineSegment3 = new LineSegment(new SysPoint(rect.X, rect.Y + rect.Height), true);
#else
            LineSegment lineSegment1 = new LineSegment();
            lineSegment1.Point = new Point(rect.X + rect.Width, rect.Y);
            LineSegment lineSegment2 = new LineSegment();
            lineSegment2.Point = new Point(rect.X + rect.Width, rect.Y + rect.Height);
            LineSegment lineSegment3 = new LineSegment();
            lineSegment3.Point = new Point(rect.X, rect.Y + rect.Height);
#endif
            figure.Segments.Add(lineSegment1);
            figure.Segments.Add(lineSegment2);
            figure.Segments.Add(lineSegment3);
            CloseFigure();
#endif
        }

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(double x, double y, double width, double height)
        {
            AddRectangle(new XRect(x, y, width, height));
        }

        // ----- AddRectangles ------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a series of rectangles to this path.
        /// </summary>
        public void AddRectangles(Rectangle[] rects)
        {
            int count = rects.Length;
            for (int idx = 0; idx < count; idx++)
                AddRectangle(rects[idx]);

            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddRectangles(rects);
            }
            finally { Lock.ExitGdiPlus(); }
        }
#endif

#if GDI
        /// <summary>
        /// Adds a series of rectangles to this path.
        /// </summary>
        public void AddRectangles(RectangleF[] rects)
        {
            int count = rects.Length;
            for (int idx = 0; idx < count; idx++)
                AddRectangle(rects[idx]);

            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddRectangles(rects);
            }
            finally { Lock.ExitGdiPlus(); }
        }
#endif

        /// <summary>
        /// Adds a series of rectangles to this path.
        /// </summary>
        public void AddRectangles(XRect[] rects)
        {
            int count = rects.Length;
            for (int idx = 0; idx < count; idx++)
            {
#if CORE
                AddRectangle(rects[idx]);
#endif
#if GDI
                try
                {
                    Lock.EnterGdiPlus();
                    GdipPath.AddRectangle(rects[idx].ToRectangleF());
                }
                finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
                StartFigure();
                PathFigure figure = CurrentPathFigure;
                XRect rect = rects[idx];
                figure.StartPoint = new SysPoint(rect.X, rect.Y);

                // figure.Segments.Add(new LineSegment(new SysPoint(rect.x + rect.width, rect.y), true));
                // figure.Segments.Add(new LineSegment(new SysPoint(rect.x + rect.width, rect.y + rect.height), true));
                // figure.Segments.Add(new LineSegment(new SysPoint(rect.x, rect.y + rect.height), true));
#if true
                LineSegment lineSegment1 = new LineSegment(new SysPoint(rect.X + rect.Width, rect.Y), true);
                LineSegment lineSegment2 = new LineSegment(new SysPoint(rect.X + rect.Width, rect.Y + rect.Height), true);
                LineSegment lineSegment3 = new LineSegment(new SysPoint(rect.X, rect.Y + rect.Height), true);
#else
                LineSegment lineSegment1 = new LineSegment();
                lineSegment1.Point = new Point(rect.X + rect.Width, rect.Y);
                LineSegment lineSegment2 = new LineSegment();
                lineSegment2.Point = new Point(rect.X + rect.Width, rect.Y + rect.Height);
                LineSegment lineSegment3 = new LineSegment();
                lineSegment3.Point = new Point(rect.X, rect.Y + rect.Height);
#endif
                figure.Segments.Add(lineSegment1);
                figure.Segments.Add(lineSegment2);
                figure.Segments.Add(lineSegment3);
                CloseFigure();
#endif
            }
        }

        // ----- AddRoundedRectangle ------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(Rectangle rect, System.Drawing.Size ellipseSize)
        {
            AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, ellipseSize.Width, ellipseSize.Height);
        }
#endif

#if WPF || UWP
        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(SysRect rect, SysSize ellipseSize)
        {
            AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, ellipseSize.Width, ellipseSize.Height);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(RectangleF rect, SizeF ellipseSize)
        {
            AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, ellipseSize.Width, ellipseSize.Height);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(XRect rect, SizeF ellipseSize)
        {
            AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, ellipseSize.Width, ellipseSize.Height);
        }
#endif

        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(double x, double y, double width, double height, double ellipseWidth, double ellipseHeight)
        {
#if CORE
#if true
            double arcWidth = ellipseWidth / 2;
            double arcHeight = ellipseHeight / 2;
#if true  // Clockwise
            CorePath.MoveTo(x + width - arcWidth, y);
            CorePath.QuadrantArcTo(x + width - arcWidth, y + arcHeight, arcWidth, arcHeight, 1, true);

            CorePath.LineTo(x + width, y + height - arcHeight, false);
            CorePath.QuadrantArcTo(x + width - arcWidth, y + height - arcHeight, arcWidth, arcHeight, 4, true);

            CorePath.LineTo(x + arcWidth, y + height, false);
            CorePath.QuadrantArcTo(x + arcWidth, y + height - arcHeight, arcWidth, arcHeight, 3, true);

            CorePath.LineTo(x, y + arcHeight, false);
            CorePath.QuadrantArcTo(x + arcWidth, y + arcHeight, arcWidth, arcHeight, 2, true);

            CorePath.CloseSubpath();
#else  // Counterclockwise
            _corePath.MoveTo(x + arcWidth, y);
            _corePath.QuadrantArcTo(x + arcWidth, y + arcHeight, arcWidth, arcHeight, 2, false);

            _corePath.LineTo(x, y + height - arcHeight, false);
            _corePath.QuadrantArcTo(x + arcWidth, y + height - arcHeight, arcWidth, arcHeight, 3, false);

            _corePath.LineTo(x + width - arcWidth, y + height, false);
            _corePath.QuadrantArcTo(x + width - arcWidth, y + height - arcHeight, arcWidth, arcHeight, 4, false);

            _corePath.LineTo(x + width, y + arcHeight, false);
            _corePath.QuadrantArcTo(x + width - arcWidth, y + arcHeight, arcWidth, arcHeight, 1, false);

            _corePath.CloseSubpath();
#endif
#else
            // AddArc not yet implemented
            AddArc((float)(x + width - ellipseWidth), (float)y, (float)ellipseWidth, (float)ellipseHeight, -90, 90);
            AddArc((float)(x + width - ellipseWidth), (float)(y + height - ellipseHeight), (float)ellipseWidth,
                (float)ellipseHeight, 0, 90);
            AddArc((float)x, (float)(y + height - ellipseHeight), (float)ellipseWidth, (float)ellipseHeight, 90, 90);
            AddArc((float)x, (float)y, (float)ellipseWidth, (float)ellipseHeight, 180, 90);
            CloseFigure();
#endif
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.StartFigure();
                GdipPath.AddArc((float)(x + width - ellipseWidth), (float)y, (float)ellipseWidth, (float)ellipseHeight, -90, 90);
                GdipPath.AddArc((float)(x + width - ellipseWidth), (float)(y + height - ellipseHeight), (float)ellipseWidth, (float)ellipseHeight, 0, 90);
                GdipPath.AddArc((float)x, (float)(y + height - ellipseHeight), (float)ellipseWidth, (float)ellipseHeight, 90, 90);
                GdipPath.AddArc((float)x, (float)y, (float)ellipseWidth, (float)ellipseHeight, 180, 90);
                GdipPath.CloseFigure();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            double ex = ellipseWidth / 2;
            double ey = ellipseHeight / 2;
            StartFigure();
            PathFigure figure = CurrentPathFigure;
            figure.StartPoint = new SysPoint(x + ex, y);

#if true
            figure.Segments.Add(new LineSegment(new SysPoint(x + width - ex, y), true));
#else
            figure.Segments.Add(new LineSegment { Point = new SysPoint(x + width - ex, y) });
#endif

            // TODOWPF
#if true
            figure.Segments.Add(new ArcSegment(new SysPoint(x + width, y + ey),
                new SysSize(ex, ey), 0, false,
                SweepDirection.Clockwise, true));
            //figure.Segments.Add(new LineSegment(new SysPoint(x + width, y + ey), true));
#else
            figure.Segments.Add(new ArcSegment
            {
                Point = new SysPoint(x + width, y + ey),
                Size = new SysSize(ex, ey),
                //RotationAngle = 0,
                //IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise
            });
#endif

#if true
            figure.Segments.Add(new LineSegment(new SysPoint(x + width, y + height - ey), true));
#else
            figure.Segments.Add(new LineSegment { Point = new SysPoint(x + width, y + height - ey) });
#endif

            // TODOWPF
#if true
            figure.Segments.Add(new ArcSegment(new SysPoint(x + width - ex, y + height), new SysSize(ex, ey), 0, false, SweepDirection.Clockwise, true));
            //figure.Segments.Add(new LineSegment(new SysPoint(x + width - ex, y + height), true));
#else
            figure.Segments.Add(new ArcSegment
            {
                Point = new SysPoint(x + width - ex, y + height),
                Size = new SysSize(ex, ey),
                //RotationAngle = 0,
                //IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise
            });
#endif

#if true
            figure.Segments.Add(new LineSegment(new SysPoint(x + ex, y + height), true));
#else
            figure.Segments.Add(new LineSegment { Point = new SysPoint(x + ex, y + height) });
#endif

            // TODOWPF
#if true
            figure.Segments.Add(new ArcSegment(new SysPoint(x, y + height - ey),
                new SysSize(ex, ey), 0, false,
                SweepDirection.Clockwise, true));
            //figure.Segments.Add(new LineSegment(new SysPoint(x, y + height - ey), true));
#else
            figure.Segments.Add(new ArcSegment
            {
                Point = new SysPoint(x, y + height - ey),
                Size = new SysSize(ex, ey),
                //RotationAngle = 0,
                //IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise
            });
#endif

#if true
            figure.Segments.Add(new LineSegment(new SysPoint(x, y + ey), true));
#else
            figure.Segments.Add(new LineSegment { Point = new SysPoint(x, y + ey) });
#endif

            // TODOWPF
#if true
            figure.Segments.Add(new ArcSegment(new SysPoint(x + ex, y), new SysSize(ex, ey), 0, false, SweepDirection.Clockwise, true));
            //figure.Segments.Add(new LineSegment(new SysPoint(x + ex, y), true));
#else
            figure.Segments.Add(new ArcSegment
            {
                Point = new SysPoint(x + ex, y),
                Size = new SysSize(ex, ey),
                //RotationAngle = 0,
                //IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise
            });
#endif
            CloseFigure();
#endif
        }

        // ----- AddEllipse ---------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(Rectangle rect)
        {
            AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }
#endif

#if GDI
        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(RectangleF rect)
        {
            AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }
#endif

        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(XRect rect)
        {
            AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(double x, double y, double width, double height)
        {
#if CORE
            double w = width / 2;
            double h = height / 2;
            double xc = x + w;
            double yc = y + h;
            CorePath.MoveTo(x + w, y);
            CorePath.QuadrantArcTo(xc, yc, w, h, 1, true);
            CorePath.QuadrantArcTo(xc, yc, w, h, 4, true);
            CorePath.QuadrantArcTo(xc, yc, w, h, 3, true);
            CorePath.QuadrantArcTo(xc, yc, w, h, 2, true);
            CorePath.CloseSubpath();
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddEllipse((float)x, (float)y, (float)width, (float)height);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
#if true
            PathGeometry.AddGeometry(new EllipseGeometry(new Rect(x, y, width, height)));
#else
            var figure = new PathFigure();
            figure.StartPoint = new SysPoint(x, y + height / 2);
            var segment = new ArcSegment
            {
                Point = new SysPoint(x + width, y + height / 2),
                Size = new SysSize(width / 2, height / 2),
                IsLargeArc = true,
                RotationAngle = 180,
                SweepDirection = SweepDirection.Clockwise,
            };
            figure.Segments.Add(segment);
            segment = new ArcSegment
            {
                Point = figure.StartPoint,
                Size = new SysSize(width / 2, height / 2),
                IsLargeArc = true,
                RotationAngle = 180,
                SweepDirection = SweepDirection.Clockwise,
            };
            figure.Segments.Add(segment);
            _pathGeometry.Figures.Add(figure);
#endif
            // StartFigure() isn't needed because AddGeometry() implicitly starts a new figure,
            // but CloseFigure() is needed for the next adding not to continue this figure.
            CloseFigure();
#endif
        }

        // ----- AddPolygon ---------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        public void AddPolygon(System.Drawing.Point[] points)
        {
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddPolygon(points);
            }
            finally { Lock.ExitGdiPlus(); }
        }
#endif

#if WPF || UWP
        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        public void AddPolygon(SysPoint[] points)
        {
            // TODO: fill mode unclear here
#if true
            PathGeometry.AddGeometry(GeometryHelper.CreatePolygonGeometry(points, XFillMode.Alternate, true));
            CloseFigure(); // StartFigure() isn't needed because AddGeometry() implicitly starts a new figure, but CloseFigure() is needed for the next adding not to continue this figure.
#else
            AddPolygon(XGraphics.MakeXPointArray(points, 0, points.Length));
#endif
        }
#endif

#if GDI
        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        public void AddPolygon(PointF[] points)
        {
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddPolygon(points);
            }
            finally { Lock.ExitGdiPlus(); }
        }
#endif

        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        public void AddPolygon(XPoint[] points)
        {
#if CORE
            int count = points.Length;
            if (count == 0)
                return;

            CorePath.MoveTo(points[0].X, points[0].Y);
            for (int idx = 0; idx < count - 1; idx++)
                CorePath.LineTo(points[idx].X, points[idx].Y, false);
            CorePath.LineTo(points[count - 1].X, points[count - 1].Y, true);
            CorePath.CloseSubpath();
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddPolygon(XGraphics.MakePointFArray(points));
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
#if true
            PathGeometry.AddGeometry(GeometryHelper.CreatePolygonGeometry(XGraphics.MakePointArray(points), XFillMode.Alternate, true));
#else
            var figure = new PathFigure();
            figure.StartPoint = new SysPoint(points[0].X, points[0].Y);
            figure.IsClosed = true;

            PolyLineSegment segment = new PolyLineSegment();
            int count = points.Length;
            // For correct drawing the start point of the segment must not be the same as the first point.
            for (int idx = 1; idx < count; idx++)
                segment.Points.Add(new SysPoint(points[idx].X, points[idx].Y));
            seg.IsStroked = true;
            figure.Segments.Add(segment);
            _pathGeometry.Figures.Add(figure);
#endif
            // TODO: NOT NEEDED
            //CloseFigure(); // StartFigure() isn't needed because AddGeometry() implicitly starts a new figure, but CloseFigure() is needed for the next adding not to continue this figure.
#endif
        }

        // ----- AddPie -------------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(Rectangle rect, double startAngle, double sweepAngle)
        {
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddPie(rect, (float)startAngle, (float)sweepAngle);
            }
            finally { Lock.ExitGdiPlus(); }
        }
#endif

#if GDI
        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(RectangleF rect, double startAngle, double sweepAngle)
        {
            AddPie(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }
#endif

        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(XRect rect, double startAngle, double sweepAngle)
        {
            AddPie(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(double x, double y, double width, double height, double startAngle, double sweepAngle)
        {
#if CORE
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddPie");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddPie((float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddPie");
#endif
        }

        // ----- AddClosedCurve ------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(System.Drawing.Point[] points)
        {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), 0.5);
        }
#endif

#if WPF || UWP
        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(SysPoint[] points)
        {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), 0.5);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(PointF[] points)
        {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), 0.5);
        }
#endif

        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(XPoint[] points)
        {
            AddClosedCurve(points, 0.5);
        }

#if GDI
        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(System.Drawing.Point[] points, double tension)
        {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }
#endif

#if WPF || UWP
        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(SysPoint[] points, double tension)
        {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(PointF[] points, double tension)
        {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }
#endif

        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(XPoint[] points, double tension)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));
            int count = points.Length;
            if (count == 0)
                return;
            if (count < 2)
                throw new ArgumentException("Not enough points.", nameof(points));

#if CORE
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddClosedCurve");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddClosedCurve(XGraphics.MakePointFArray(points), (float)tension);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            tension /= 3;

            StartFigure();
            PathFigure figure = CurrentPathFigure;
            figure.StartPoint = new SysPoint(points[0].X, points[0].Y);

            if (count == 2)
            {
                figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[0], points[0], points[1], points[1], tension));
            }
            else
            {
                figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[count - 1], points[0], points[1], points[2], tension));
                for (int idx = 1; idx < count - 2; idx++)
                    figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[idx - 1], points[idx], points[idx + 1], points[idx + 2], tension));
                figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[count - 3], points[count - 2], points[count - 1], points[0], tension));
                figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[count - 2], points[count - 1], points[0], points[1], tension));
            }
#endif
        }

        // ----- AddPath ------------------------------------------------------------------------------

        /// <summary>
        /// Adds the specified path to this path.
        /// </summary>
        public void AddPath(XGraphicsPath path, bool connect)
        {
#if CORE
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddPath");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddPath(path.GdipPath, connect);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            PathGeometry.AddGeometry(path.PathGeometry);
#endif
        }

        // ----- AddString ----------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyleEx style, double emSize, System.Drawing.Point origin, XStringFormat format)
        {
            AddString(s, family, style, emSize, new XRect(origin.X, origin.Y, 0, 0), format);
        }
#endif

#if WPF || UWP
        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyleEx style, double emSize, SysPoint origin, XStringFormat format)
        {
            AddString(s, family, style, emSize, new XPoint(origin), format);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyleEx style, double emSize, PointF origin, XStringFormat format)
        {
            AddString(s, family, style, emSize, new XRect(origin.X, origin.Y, 0, 0), format);
        }
#endif

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyleEx style, double emSize, XPoint origin,
            XStringFormat format)
        {
            try
            {
#if CORE
// ReviewSTLA THHO4STLA
                // EXPERIMENTAL
                switch (Capabilities.Action.GlyphsToPath)
                {
                    case FeatureNotAvailableAction.DoNothing:
                        return;

                    case FeatureNotAvailableAction.FailWithException:
                        DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddString");
                        break;

                    case FeatureNotAvailableAction.LogWarning:
                        break;

                    case FeatureNotAvailableAction.LogError:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
#endif
#if GDI
                if (family.GdiFamily == null)
                    throw new NotImplementedException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));

                PointF p = origin.ToPointF();
                p.Y += SimulateBaselineOffset(family, style, emSize, format);

                try
                {
                    Lock.EnterGdiPlus();
                    GdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, p, format.RealizeGdiStringFormat());
                }
                finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
                if (family.WpfFamily == null)
                    throw new NotImplementedException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));

                XFont font = new XFont(family.Name, emSize, style);

                double x = origin.X;
                double y = origin.Y;

                double lineSpace = font.GetHeight();
                double cyAscent = lineSpace * font.CellAscent / font.CellSpace;
                double cyDescent = lineSpace * font.CellDescent / font.CellSpace;

                Typeface typeface = FontHelper.CreateTypeface(family.WpfFamily, style);
                FormattedText formattedText = FontHelper.CreateFormattedText(s, typeface, emSize, WpfBrushes.Black);

                switch (format.Alignment)
                {
                    case XStringAlignment.Near:
                        // Nothing to do, this is the default.
                        //formattedText.TextAlignment = TextAlignment.Left;
                        break;

                    case XStringAlignment.Center:
                        formattedText.TextAlignment = TextAlignment.Center;
                        break;

                    case XStringAlignment.Far:
                        formattedText.TextAlignment = TextAlignment.Right;
                        break;
                }
                switch (format.LineAlignment)
                {
                    case XLineAlignment.Near:
                        //y += cyAscent;
                        break;

                    case XLineAlignment.Center:
                        // TODO use CapHeight. PDFlib also uses 3/4 of ascent
                        y += -lineSpace / 2; //-formattedText.Baseline + (cyAscent * 2 / 4);
                        break;

                    case XLineAlignment.Far:
                        y += -formattedText.Baseline - cyDescent;
                        break;

                    case XLineAlignment.BaseLine:
                        y -= formattedText.Baseline;
                        break;
                }

                Geometry geo = formattedText.BuildGeometry(new XPoint(x, y));
                PathGeometry.AddGeometry(geo);
#endif
            }
            catch
            {
                throw;
            }
        }

#if GDI
        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyleEx style, double emSize, Rectangle layoutRect, XStringFormat format)
        {
            if (family.GdiFamily == null)
                throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));

            Rectangle rect = new Rectangle(layoutRect.X, layoutRect.Y, layoutRect.Width, layoutRect.Height);
            rect.Offset(new System.Drawing.Point(0, (int)SimulateBaselineOffset(family, style, emSize, format)));

            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, rect, format.RealizeGdiStringFormat());
            }
            finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyleEx style, double emSize, RectangleF layoutRect, XStringFormat format)
        {
            if (family.GdiFamily == null)
                throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));

            RectangleF rect = new RectangleF(layoutRect.X, layoutRect.Y, layoutRect.Width, layoutRect.Height);
            rect.Offset(new PointF(0, SimulateBaselineOffset(family, style, emSize, format)));

            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, layoutRect, format.RealizeGdiStringFormat());
            }
            finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Calculates the offset for BaseLine positioning simulation:
        /// In GDI we have only Near, Center and Far as LineAlignment and no BaseLine. For XLineAlignment.BaseLine StringAlignment.Near is returned.
        /// We now return the negative drawed ascender height.
        /// This has to be added to the LayoutRect/Origin before each _gdipPath.AddString().
        /// </summary>
        /// <param name="family"></param>
        /// <param name="style"></param>
        /// <param name="emSize"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private float SimulateBaselineOffset(XFontFamily family, XFontStyleEx style, double emSize, XStringFormat format)
        {
            XFont font = new XFont(family.Name, emSize, style);

            if (format.LineAlignment == XLineAlignment.BaseLine)
            {
                double lineSpace = font.GetHeight();
                int cellSpace = font.FontFamily.GetLineSpacing(font.Style);
                int cellAscent = font.FontFamily.GetCellAscent(font.Style);
                int cellDescent = font.FontFamily.GetCellDescent(font.Style);
                double cyAscent = lineSpace * cellAscent / cellSpace;
                cyAscent = lineSpace * font.CellAscent / font.CellSpace;
                return (float)-cyAscent;
            }
            return 0;
        }

#endif

#if WPF
        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyleEx style, double emSize, Rect rect, XStringFormat format)
        {
            //gdip Path.AddString(s, family.gdiFamily, (int)style, (float)emSize, layoutRect, format.RealizeGdiStringFormat());
            AddString(s, family, style, emSize, new XRect(rect), format);
        }
#endif

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyleEx style, double emSize, XRect layoutRect,
            XStringFormat format)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (family == null)
                throw new ArgumentNullException(nameof(family));

            if (format == null!)
                format = XStringFormats.Default;

            if (format.LineAlignment == XLineAlignment.BaseLine && layoutRect.Height != 0)
                throw new InvalidOperationException(
                    "DrawString: With XLineAlignment.BaseLine the height of the layout rectangle must be 0.");

            if (s.Length == 0)
                return;

            XFont font = new XFont(family.Name, emSize, style);
#if CORE
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddString");
#endif
#if GDI && !WPF
            //Gfx.DrawString(text, font.Realize_GdiFont(), brush.RealizeGdiBrush(), rect,
            //  format != null ? format.RealizeGdiStringFormat() : null);

            if (family.GdiFamily == null)
                throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));

            RectangleF rect = layoutRect.ToRectangleF();
            rect.Offset(new PointF(0, SimulateBaselineOffset(family, style, emSize, format)));

            try
            {
                Lock.EnterGdiPlus();
                GdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, rect, format.RealizeGdiStringFormat());
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF && !GDI
            if (family.WpfFamily == null)
                throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));

            // Just a first sketch, but currently we do not need it and there is enough to do...
            double x = layoutRect.X;
            double y = layoutRect.Y;

            //double lineSpace = font.GetHeight(this);
            //double cyAscent = lineSpace * font.cellAscent / font.cellSpace;
            //double cyDescent = lineSpace * font.cellDescent / font.cellSpace;

            //double cyAscent = family.GetCellAscent(style) * family.GetLineSpacing(style) / family.getl; //fontlineSpace * font.cellAscent / font.cellSpace;
            //double cyDescent =family.GetCellDescent(style); // lineSpace * font.cellDescent / font.cellSpace;
            double lineSpace = font.GetHeight();
            double cyAscent = lineSpace * font.CellAscent / font.CellSpace;
            double cyDescent = lineSpace * font.CellDescent / font.CellSpace;

            bool bold = (style & XFontStyleEx.Bold) != 0;
            bool italic = (style & XFontStyleEx.Italic) != 0;
            bool strikeout = (style & XFontStyleEx.Strikeout) != 0;
            bool underline = (style & XFontStyleEx.Underline) != 0;

            Typeface typeface = FontHelper.CreateTypeface(family.WpfFamily, style);
            //FormattedText formattedText = new FormattedText(s, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, emSize, WpfBrushes.Black);
            FormattedText formattedText = FontHelper.CreateFormattedText(s, typeface, emSize, WpfBrushes.Black);

            switch (format.Alignment)
            {
                case XStringAlignment.Near:
                    // Nothing to do, this is the default.
                    //formattedText.TextAlignment = TextAlignment.Left;
                    break;

                case XStringAlignment.Center:
                    x += layoutRect.Width / 2;
                    formattedText.TextAlignment = TextAlignment.Center;
                    break;

                case XStringAlignment.Far:
                    x += layoutRect.Width;
                    formattedText.TextAlignment = TextAlignment.Right;
                    break;
            }
            //if (PageDirection == XPageDirection.Downwards)
            //{
            switch (format.LineAlignment)
            {
                case XLineAlignment.Near:
                    //y += cyAscent;
                    break;

                case XLineAlignment.Center:
                    // TO/DO use CapHeight. PDFlib also uses 3/4 of ascent
                    //y += -formattedText.Baseline + (cyAscent * 2 / 4) + layoutRect.Height / 2;

                    // GDI seems to make it this simple:
                    // TODO: Check WPF's vertical alignment and make all implementations fit. $MaOs
                    y += layoutRect.Height / 2 - lineSpace / 2;
                    break;

                case XLineAlignment.Far:
                    y += -formattedText.Baseline - cyDescent + layoutRect.Height;
                    break;

                case XLineAlignment.BaseLine:
                    y -= formattedText.Baseline;
                    break;
            }
            //}
            //else
            //{
            //  // TODOWPF
            //  switch (format.LineAlignment)
            //  {
            //    case XLineAlignment.Near:
            //      //y += cyDescent;
            //      break;

            //    case XLineAlignment.Center:
            //      // TODO use CapHeight. PDFlib also uses 3/4 of ascent
            //      //y += -(cyAscent * 3 / 4) / 2 + rect.Height / 2;
            //      break;

            //    case XLineAlignment.Far:
            //      //y += -cyAscent + rect.Height;
            //      break;

            //    case XLineAlignment.BaseLine:
            //      // Nothing to do.
            //      break;
            //  }
            //}

            //if (bold && !descriptor.IsBoldFace)
            //{
            //  // TODO: emulate bold by thicker outline
            //}

            //if (italic && !descriptor.IsItalicFace)
            //{
            //  // TODO: emulate italic by shearing transformation
            //}

            if (underline)
            {
                //double underlinePosition = lineSpace * realizedFont.FontDescriptor.descriptor.UnderlinePosition / font.cellSpace;
                //double underlineThickness = lineSpace * realizedFont.FontDescriptor.descriptor.UnderlineThickness / font.cellSpace;
                //DrawRectangle(null, brush, x, y - underlinePosition, width, underlineThickness);
            }

            if (strikeout)
            {
                //double strikeoutPosition = lineSpace * realizedFont.FontDescriptor.descriptor.StrikeoutPosition / font.cellSpace;
                //double strikeoutSize = lineSpace * realizedFont.FontDescriptor.descriptor.StrikeoutSize / font.cellSpace;
                //DrawRectangle(null, brush, x, y - strikeoutPosition - strikeoutSize, width, strikeoutSize);
            }

            //dc.DrawText(formattedText, layoutRectangle.Location.ToPoint());
            //dc.DrawText(formattedText, new SysPoint(x, y));

            Geometry geo = formattedText.BuildGeometry(new Point(x, y));
            PathGeometry.AddGeometry(geo);
#endif
        }

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Closes the current figure and starts a new figure.
        /// </summary>
        public void CloseFigure()
        {
#if CORE
            CorePath.CloseSubpath();
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.CloseFigure();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            PathFigure figure = PeekCurrentFigure;
            if (figure != null && figure.Segments.Count != 0)
                figure.IsClosed = true;
#endif
        }

        /// <summary>
        /// Starts a new figure without closing the current figure.
        /// </summary>
        public void StartFigure()
        {
#if CORE
// ReviewSTLA THHO4STLA
            // TODO: ???
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.StartFigure();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            PathFigure figure = CurrentPathFigure;
            if (figure.Segments.Count != 0)
            {
                figure = new PathFigure();
                PathGeometry.Figures.Add(figure);
            }
#endif
        }

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets an XFillMode that determines how the interiors of shapes are filled.
        /// </summary>
        public XFillMode FillMode
        {
            get { return _fillMode; }
            set
            {
                _fillMode = value;
#if CORE
                // Nothing to do.
#endif
#if GDI
                try
                {
                    Lock.EnterGdiPlus();
                    GdipPath.FillMode = (FillMode)value;
                }
                finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
                PathGeometry.FillRule = value == XFillMode.Winding ? FillRule.Nonzero : FillRule.EvenOdd;
#endif
            }
        }

        XFillMode _fillMode;

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
        /// </summary>
        public void Flatten()
        {
#if CORE
// ReviewSTLA THHO4STLA
            // Just do nothing.
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.Flatten();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            PathGeometry = PathGeometry.GetFlattenedPathGeometry();
#endif
        }

        /// <summary>
        /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
        /// </summary>
        public void Flatten(XMatrix matrix)
        {
#if CORE
// ReviewSTLA THHO4STLA
            // Just do nothing.
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.Flatten(matrix.ToGdiMatrix());
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            PathGeometry = PathGeometry.GetFlattenedPathGeometry();
            PathGeometry.Transform = new MatrixTransform(matrix.ToWpfMatrix());
#endif
        }

        /// <summary>
        /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
        /// </summary>
        public void Flatten(XMatrix matrix, double flatness)
        {
#if CORE
// ReviewSTLA THHO4STLA
            // Just do nothing.
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.Flatten(matrix.ToGdiMatrix(), (float)flatness);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            PathGeometry = PathGeometry.GetFlattenedPathGeometry();
            // TODO: matrix handling not yet tested
            if (!matrix.IsIdentity)
                PathGeometry.Transform = new MatrixTransform(matrix.ToWpfMatrix());
#endif
        }

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
        /// by the specified pen.
        /// </summary>
        public void Widen(XPen pen)
        {
#if CORE
// ReviewSTLA THHO4STLA
            // Just do nothing.
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.Widen(pen.RealizeGdiPen());
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            PathGeometry = PathGeometry.GetWidenedPathGeometry(pen.RealizeWpfPen());
#endif
        }

        /// <summary>
        /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
        /// by the specified pen.
        /// </summary>
        public void Widen(XPen pen, XMatrix matrix)
        {
#if CORE
            // Just do nothing.
#endif
#if CORE
            throw new NotImplementedException("XGraphicsPath.Widen");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.Widen(pen.RealizeGdiPen(), matrix.ToGdiMatrix());
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            PathGeometry = PathGeometry.GetWidenedPathGeometry(pen.RealizeWpfPen());
#endif
        }

        /// <summary>
        /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
        /// by the specified pen.
        /// </summary>
        public void Widen(XPen pen, XMatrix matrix, double flatness)
        {
#if CORE
            // Just do nothing.
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                GdipPath.Widen(pen.RealizeGdiPen(), matrix.ToGdiMatrix(), (float)flatness);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || UWP
            PathGeometry = PathGeometry.GetWidenedPathGeometry(pen.RealizeWpfPen());
#endif
        }

        /// <summary>
        /// Grants access to internal objects of this class.
        /// </summary>
        public XGraphicsPathInternals Internals => new XGraphicsPathInternals(this);

#if CORE
        /// <summary>
        /// Gets access to underlying Core graphics path.
        /// </summary>
        internal CoreGraphicsPath CorePath;
#endif

#if GDI
        /// <summary>
        /// Gets access to underlying GDI+ graphics path.
        /// </summary>
        internal GraphicsPath GdipPath;
#endif

#if WPF || UWP
        /// <summary>
        /// Gets access to underlying WPF/UWP path geometry.
        /// </summary>
        internal PathGeometry PathGeometry;
#endif
    }
}