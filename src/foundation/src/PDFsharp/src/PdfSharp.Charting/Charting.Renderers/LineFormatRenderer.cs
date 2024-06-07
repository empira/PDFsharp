// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Drawing;
using PdfSharp.Drawing;
using PdfSharp.Internal;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a renderer specialized to draw lines in various styles, colors and widths.
    /// </summary>
    class LineFormatRenderer
    {
        /// <summary>
        /// Initializes a new instance of the LineFormatRenderer class with the specified graphics, line format,
        /// and default width.
        /// </summary>
        public LineFormatRenderer(XGraphics gfx, LineFormat? lineFormat, double defaultWidth)
        {
            _gfx = gfx;
            bool visible = false;
            double width = 0;

            if (lineFormat != null)
            {
                width = lineFormat.Width.Point;
                if (DoubleUtil.IsZero(width) && !lineFormat.Color.IsEmpty)
                    width = defaultWidth;
                visible = lineFormat.Visible || width > 0 || !lineFormat.Color.IsEmpty;
            }

            //if (visible)
            if (visible && lineFormat != null)
            {
                _pen = new XPen(lineFormat.Color, width)
                {
                    DashStyle = lineFormat.DashStyle
                };
            }
        }

        /// <summary>
        /// Initializes a new instance of the LineFormatRenderer class with the specified graphics and
        /// line format.
        /// </summary>
        public LineFormatRenderer(XGraphics gfx, LineFormat lineFormat) :
            this(gfx, lineFormat, 0)
        { }

        /// <summary>
        /// Initializes a new instance of the LineFormatRenderer class with the specified graphics and pen.
        /// </summary>
        public LineFormatRenderer(XGraphics gfx, XPen? pen)
        {
            _gfx = gfx;
            _pen = pen;
        }

        /// <summary>
        /// Draws a line from point pt0 to point pt1.
        /// </summary>
        public void DrawLine(XPoint pt0, XPoint pt1)
        {
            if (_pen != null)
                _gfx.DrawLine(_pen, pt0, pt1);
        }

        /// <summary>
        /// Draws a line specified by rect.
        /// </summary>
        public void DrawRectangle(XRect rect)
        {
            if (_pen != null)
                _gfx.DrawRectangle(_pen, rect);
        }

        /// <summary>
        /// Draws a line specified by path.
        /// </summary>
        public void DrawPath(XGraphicsPath path)
        {
            if (_pen != null)
                _gfx.DrawPath(_pen, path);
        }

        /// <summary>
        /// Surface to draw the line.
        /// </summary>
        readonly XGraphics _gfx;

        /// <summary>
        /// Pen used to draw the line.
        /// </summary>
        readonly XPen? _pen;
    }
}
