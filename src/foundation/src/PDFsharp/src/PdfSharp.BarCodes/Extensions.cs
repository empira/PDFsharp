using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Drawing.BarCodes;

namespace PdfSharp
{
    /// <summary>
    /// Extension methods for drawing bar codes.
    /// </summary>
    public static class Extensions
    {
        // ----- DrawBarCode --------------------------------------------------------------------------

        /// <summary>
        /// Draws the specified bar code.
        /// </summary>
        public static void DrawBarCode(this XGraphics gfx, BarCode barcode, XPoint position)
        {
            barcode.Render(gfx, XBrushes.Black, null, position);
        }

        /// <summary>
        /// Draws the specified bar code.
        /// </summary>
        public static void DrawBarCode(this XGraphics gfx, BarCode barcode, XBrush brush, XPoint position)
        {
            barcode.Render(gfx, brush, null, position);
        }

        /// <summary>
        /// Draws the specified bar code.
        /// </summary>
        public static void DrawBarCode(this XGraphics gfx, BarCode barcode, XBrush brush, XFont font, XPoint position)
        {
            barcode.Render(gfx, brush, font, position);
        }

        // ----- DrawMatrixCode -----------------------------------------------------------------------

        /// <summary>
        /// Draws the specified data matrix code.
        /// </summary>
        public static void DrawMatrixCode(this XGraphics gfx, MatrixCode matrixCode, XPoint position)
        {
            matrixCode.Render(gfx, XBrushes.Black, position);
        }

        /// <summary>
        /// Draws the specified data matrix code.
        /// </summary>
        public static void DrawMatrixCode(this XGraphics gfx, MatrixCode matrixCode, XBrush brush, XPoint position)
        {
            matrixCode.Render(gfx, brush, position);
        }
    }
}
