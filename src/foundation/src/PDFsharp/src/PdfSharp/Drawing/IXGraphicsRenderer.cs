// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Represents an abstract drawing surface for PdfPages.
    /// </summary>
    interface IXGraphicsRenderer
    {
        void Close();

        #region Drawing

        ///// <summary>
        ///// Fills the entire drawing surface with the specified color.
        ///// </summary>
        //[Obsolete("Will be removed.")]
        //void Clear(XColor color);

        /// <summary>
        /// Draws a straight line.
        /// </summary>
        void DrawLine(XPen pen, double x1, double y1, double x2, double y2);

        /// <summary>
        /// Draws a series of straight lines.
        /// </summary>
        void DrawLines(XPen pen, XPoint[] points);

        /// <summary>
        /// Draws a Bézier spline.
        /// </summary>
        void DrawBezier(XPen pen, double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4);

        /// <summary>
        /// Draws a series of Bézier splines.
        /// </summary>
        void DrawBeziers(XPen pen, XPoint[] points);

        /// <summary>
        /// Draws a cardinal spline.
        /// </summary>
        void DrawCurve(XPen pen, XPoint[] points, double tension);

        /// <summary>
        /// Draws an arc.
        /// </summary>
        void DrawArc(XPen pen, double x, double y, double width, double height, double startAngle, double sweepAngle);

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        void DrawRectangle(XPen? pen, XBrush? brush, double x, double y, double width, double height);

        /// <summary>
        /// Draws a series of rectangles.
        /// </summary>
        void DrawRectangles(XPen? pen, XBrush? brush, XRect[] rects);

        /// <summary>
        /// Draws a rectangle with rounded corners.
        /// </summary>
        void DrawRoundedRectangle(XPen? pen, XBrush? brush, double x, double y, double width, double height, double ellipseWidth, double ellipseHeight);

        /// <summary>
        /// Draws an ellipse.
        /// </summary>
        void DrawEllipse(XPen? pen, XBrush? brush, double x, double y, double width, double height);

        /// <summary>
        /// Draws a polygon.
        /// </summary>
        void DrawPolygon(XPen? pen, XBrush? brush, XPoint[] points, XFillMode fillmode);

        /// <summary>
        /// Draws a pie.
        /// </summary>
        void DrawPie(XPen? pen, XBrush? brush, double x, double y, double width, double height, double startAngle, double sweepAngle);

        /// <summary>
        /// Draws a cardinal spline.
        /// </summary>
        void DrawClosedCurve(XPen? pen, XBrush? brush, XPoint[] points, double tension, XFillMode fillmode);

        /// <summary>
        /// Draws a graphical path.
        /// </summary>
        void DrawPath(XPen? pen, XBrush? brush, XGraphicsPath path);

        /// <summary>
        /// Draws a series of glyphs identified by the specified text and font.
        /// </summary>
        void DrawString(string s, XFont font, XBrush brush, XRect layoutRectangle, XStringFormat format);
        
        /// <summary>
        /// Draws a series of glyphs identified by the specified text and font.
        /// </summary>
        [Obsolete ("Not yet implemented.")]
        void DrawString(string s, XGlyphTypeface typeface, XBrush brush, XRect layoutRectangle, XStringFormat format);

        /// <summary>
        /// Draws an image.
        /// </summary>
        void DrawImage(XImage image, double x, double y, double width, double height);

        void DrawImage(XImage image, XRect destRect, XRect srcRect, XGraphicsUnit srcUnit);

        #endregion

        #region Save and Restore

        /// <summary>
        /// Saves the current graphics state without changing it.
        /// </summary>
        void Save(XGraphicsState state);

        /// <summary>
        /// Restores the specified graphics state.
        /// </summary>
        void Restore(XGraphicsState state);

        /// <summary>
        /// TODO
        /// </summary>
        void BeginContainer(XGraphicsContainer container, XRect dstrect, XRect srcrect, XGraphicsUnit unit);

        /// <summary>
        /// TODO
        /// </summary>
        void EndContainer(XGraphicsContainer container);

        #endregion

        #region Transformation

        /// <summary>
        /// Gets or sets the transformation matrix.
        /// </summary>
        //XMatrix Transform {get; set;}

        void AddTransform(XMatrix transform, XMatrixOrder matrixOrder);

        #endregion

        #region Clipping

        void SetClip(XGraphicsPath path, XCombineMode combineMode);

        void ResetClip();

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Writes a comment to the output stream. Comments have no effect on the rendering of the output.
        /// </summary>
        void WriteComment(string comment);

        #endregion
    }
}
