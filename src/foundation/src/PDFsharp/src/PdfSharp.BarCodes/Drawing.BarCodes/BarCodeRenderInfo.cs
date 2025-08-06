// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>
    /// Holds all temporary information needed during rendering.
    /// </summary>
    class BarCodeRenderInfo
    {
        public BarCodeRenderInfo(XGraphics gfx, XBrush brush, XFont? font, XPoint position)
        {
            Gfx = gfx;
            Brush = brush;
            Font = font;
            Position = position;
        }

        public XGraphics Gfx;
        public XBrush Brush;
        public XFont? Font;
        public XPoint Position;
        public double BarHeight;
        public XPoint CurrPos;
        public int CurrPosInString;
        public double ThinBarWidth;
    }
}
