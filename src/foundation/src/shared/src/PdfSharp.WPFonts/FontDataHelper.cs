// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

// ReSharper disable InconsistentNaming

namespace PdfSharp.WPFonts
{
    /// <summary>
    /// Windows Phone fonts helper class.
    /// </summary>
    public static class FontDataHelper
    {
        /// <summary>
        /// Gets the font face of Segoe WP light.
        /// </summary>
        public static byte[] SegoeWPLight => LoadFontData("PdfSharp.WPFonts.Fonts.SegoeWP-Light.ttf");

        /// <summary>
        /// Gets the font face of Segoe WP semilight.
        /// </summary>
        public static byte[] SegoeWPSemilight => LoadFontData("PdfSharp.WPFonts.Fonts.SegoeWP-Semilight.ttf");

        /// <summary>
        /// Gets the font face of Segoe WP.
        /// </summary>
        public static byte[] SegoeWP => LoadFontData("PdfSharp.WPFonts.Fonts.SegoeWP.ttf");

        /// <summary>
        /// Gets the font face of Segoe WP semibold.
        /// </summary>
        public static byte[] SegoeWPSemibold => LoadFontData("PdfSharp.WPFonts.Fonts.SegoeWP-Semibold.ttf");

        /// <summary>
        /// Gets the font face of Segoe WP bold.
        /// </summary>
        public static byte[] SegoeWPBold => LoadFontData("PdfSharp.WPFonts.Fonts.SegoeWP-Bold.ttf");

        /// <summary>
        /// Gets the font face of Segoe WP black.
        /// </summary>
        public static byte[] SegoeWPBlack => LoadFontData("PdfSharp.WPFonts.Fonts.SegoeWP-Black.ttf");

        /// <summary>
        /// Returns the specified font from an embedded resource.
        /// </summary>
        static byte[] LoadFontData(string name)
        {
            var assembly = typeof(FontDataHelper).Assembly;
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
                throw new ArgumentException($"No resource named '{name}'.");

            int count = (int)stream.Length;
            byte[] data = new byte[count];
            int read = stream.Read(data, 0, count);
            Debug.Assert(read == count);
            return data;
        }
    }
}
