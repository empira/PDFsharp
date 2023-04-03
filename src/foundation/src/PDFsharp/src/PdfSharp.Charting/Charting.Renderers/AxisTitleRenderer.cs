// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a axis title renderer used for x and y axis titles.
    /// </summary>
    class AxisTitleRenderer : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the AxisTitleRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal AxisTitleRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Calculates the space used for the axis title.
        /// </summary>
        internal override void Format()
        {
            var gfx = _rendererParms.Graphics;

            var atri = ((AxisRendererInfo)_rendererParms.RendererInfo).AxisTitleRendererInfo;
            if (atri.AxisTitleText != "")
            {
                XSize size = gfx.MeasureString(atri.AxisTitleText, atri.AxisTitleFont);
                if (atri.AxisTitleOrientation != 0)
                {
                    XPoint[] points = new XPoint[2];
                    points[0].X = 0;
                    points[0].Y = 0;
                    points[1].X = size.Width;
                    points[1].Y = size.Height;

                    XMatrix matrix = new XMatrix();
                    matrix.RotatePrepend(-atri.AxisTitleOrientation);
                    matrix.TransformPoints(points);

                    size.Width = Math.Abs(points[1].X - points[0].X);
                    size.Height = Math.Abs(points[1].Y - points[0].Y);
                }
                atri.X = 0;
                atri.Y = 0;
                atri.Height = size.Height;
                atri.Width = size.Width;
            }
        }

        /// <summary>
        /// Draws the axis title.
        /// </summary>
        internal override void Draw()
        {
            var ari = (AxisRendererInfo)_rendererParms.RendererInfo;
            var atri = ari.AxisTitleRendererInfo;
            if (atri.AxisTitleText != "")
            {
                var gfx = _rendererParms.Graphics;
                if (atri.AxisTitleOrientation != 0)
                {
                    XRect layout = atri.Rect;
                    layout.X = -(layout.Width / 2);
                    layout.Y = -(layout.Height / 2);

                    double x = atri.AxisTitleAlignment switch
                    {
                        HorizontalAlignment.Center => atri.X + atri.Width / 2,
                        HorizontalAlignment.Right => atri.X + atri.Width - layout.Width / 2,
                        HorizontalAlignment.Left => atri.X,
                        _ => atri.X
                    };

                    double y = atri.AxisTitleVerticalAlignment switch
                    {
                        VerticalAlignment.Center => atri.Y + atri.Height / 2,
                        VerticalAlignment.Bottom => atri.Y + atri.Height - layout.Height / 2,
                        VerticalAlignment.Top => atri.Y,
                        _ => atri.Y
                    };

                    var xsf = new XStringFormat();
                    xsf.Alignment = XStringAlignment.Center;
                    xsf.LineAlignment = XLineAlignment.Center;

                    var state = gfx.Save();
                    gfx.TranslateTransform(x, y);
                    gfx.RotateTransform(-atri.AxisTitleOrientation);
                    gfx.DrawString(atri.AxisTitleText, atri.AxisTitleFont, atri.AxisTitleBrush, layout, xsf);
                    gfx.Restore(state);
                }
                else
                {
                    var format = new XStringFormat();
                    format.Alignment = atri.AxisTitleAlignment switch
                    {
                        HorizontalAlignment.Center => XStringAlignment.Center,
                        HorizontalAlignment.Right => XStringAlignment.Far,
                        HorizontalAlignment.Left => XStringAlignment.Near,
                        _ => XStringAlignment.Near
                    };

                    format.LineAlignment = atri.AxisTitleVerticalAlignment switch
                    {
                        VerticalAlignment.Center => XLineAlignment.Center,
                        VerticalAlignment.Bottom => XLineAlignment.Far,
                        VerticalAlignment.Top => XLineAlignment.Near,
                        _ => XLineAlignment.Near
                    };

                    gfx.DrawString(atri.AxisTitleText, atri.AxisTitleFont, atri.AxisTitleBrush, atri.Rect, format);
                }
            }
        }
    }
}
