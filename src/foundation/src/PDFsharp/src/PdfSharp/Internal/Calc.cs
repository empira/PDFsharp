// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
#endif
#if WPF
#endif
using PdfSharp.Drawing;

namespace PdfSharp.Internal
{
    /// <summary>
    /// Some static helper functions for calculations.
    /// </summary>
    static class Calc
    {
        /// <summary>
        /// Degree to radiant factor.
        /// </summary>
        public const double Deg2Rad = Math.PI / 180;

        /// <summary>
        /// Get page size in point from specified PageSize.
        /// </summary>
        public static XSize PageSizeToSize(PageSize value)
        {
            return value switch
            {
                // Source: https://www.din-formate.de/reihe-a-din-groessen-mm-pixel-dpi.html
                // See also: PageSizeConverter.cs.
                PageSize.A0 => new(2384, 3370),
                PageSize.A1 => new(1684, 2384),
                PageSize.A2 => new(1191, 1684),
                PageSize.A3 => new(842, 1191),
                PageSize.A4 => new(595, 842),
                PageSize.A5 => new(420, 595),
                PageSize.B4 => new(709, 1001),
                PageSize.B5 => new(499, 709),

                // The strange sizes from overseas...
                PageSize.Letter => new(612, 792),
                PageSize.Legal => new(612, 1008),
                PageSize.Tabloid => new(792, 1224),
                PageSize.Ledger => new(1224, 792),
                PageSize.Statement => new(396, 612),
                PageSize.Executive => new(540, 720),
                PageSize.Folio => new(612, 936),
                PageSize.Quarto => new(576, 720),
                PageSize.Size10x14 => new(720, 1008),

                _ => throw new ArgumentException("Invalid PageSize.")
            };
        }
    }
}
