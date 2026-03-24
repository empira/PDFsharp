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
        public static double Deg2Rad => Math.PI / 180;

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

#if true_  // Who needs this? Should be deleted.
                PageSize.Undefined => expr,
                PageSize.RA0 => expr,
                PageSize.RA1 => expr,
                PageSize.RA2 => expr,
                PageSize.RA3 => expr,
                PageSize.RA4 => expr,
                PageSize.RA5 => expr,
                PageSize.B0 => expr,
                PageSize.B1 => expr,
                PageSize.B2 => expr,
                PageSize.B3 => expr,
                PageSize.Foolscap => expr,
                PageSize.GovernmentLetter => expr,
                PageSize.Post => expr,
                PageSize.Crown => expr,
                PageSize.LargePost => expr,
                PageSize.Demy => expr,
                PageSize.Medium => expr,
                PageSize.Royal => expr,
                PageSize.Elephant => expr,
                PageSize.DoubleDemy => expr,
                PageSize.QuadDemy => expr,
                PageSize.STMT => expr,
#endif
                _ => throw new ArgumentException("Invalid PageSize.")
            };
        }
    }
}
