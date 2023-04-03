// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Internal
{
    /// <summary>
    /// Helper functions for RGB and CMYK colors.
    /// </summary>
    static class ColorSpaceHelper
    {
        /// <summary>
        /// Checks whether a color mode and a color match.
        /// </summary>
        public static XColor EnsureColorMode(PdfColorMode colorMode, XColor color)
        {
#if true
            if (colorMode == PdfColorMode.Rgb && color.ColorSpace != XColorSpace.Rgb)
                return XColor.FromArgb((int)(color.A * 255), color.R, color.G, color.B);

            if (colorMode == PdfColorMode.Cmyk && color.ColorSpace != XColorSpace.Cmyk)
                return XColor.FromCmyk(color.A, color.C, color.M, color.Y, color.K);

            return color;
#else
      if (colorMode == PdfColorMode.Rgb && color.ColorSpace != XColorSpace.Rgb)
        throw new InvalidOperationException(PSSR.InappropriateColorSpace(colorMode, color.ColorSpace));

      if (colorMode == PdfColorMode.Cmyk && color.ColorSpace != XColorSpace.Cmyk)
        throw new InvalidOperationException(PSSR.InappropriateColorSpace(colorMode, color.ColorSpace));
#endif
        }

        /// <summary>
        /// Checks whether the color mode of a document and a color match.
        /// </summary>
        public static XColor EnsureColorMode(PdfDocument document, XColor color)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            return EnsureColorMode(document.Options.ColorMode, color);
        }

        /// <summary>
        /// Determines whether two colors are equal referring to their CMYK color values.
        /// </summary>
        public static bool IsEqualCmyk(XColor x, XColor y)
        {
            if (x.ColorSpace != XColorSpace.Cmyk || y.ColorSpace != XColorSpace.Cmyk)
                return false;
            return x.C.Equals(y.C) && x.M.Equals(y.M) && x.Y.Equals(y.Y) && x.K.Equals(y.K);
        }
    }
}