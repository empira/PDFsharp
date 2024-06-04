// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using System.Drawing.Text;
#endif
#if WPF
using System.Windows.Media;
#endif
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Fonts.Internal;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Helper function for code points and glyph indices.
    /// </summary>
    public static class GlyphHelper
    {
        /// <summary>
        /// Returns the glyph ID for the specified code point,
        /// or 0, if the specified font has no glyph for this code point.
        /// </summary>
        /// <param name="codePoint">The code point the glyph ID is requested for.</param>
        /// <param name="font">The font to be used.</param>
        public static ushort GlyphIndexFromCodePoint(int codePoint, XFont font)
        {
            // BMP code points are in cmap table type 4 and the rest in type 12.
            return codePoint < UnicodeHelper.UnicodePlane01Start
                ? font.OpenTypeDescriptor.BmpCodepointToGlyphIndex((char)codePoint)
                : font.OpenTypeDescriptor.CodepointToGlyphIndex(codePoint);
        }

        /// <summary>
        /// Maps the characters of a UTF-32 string to an array of glyph indexes.
        /// Never fails, invalid surrogate pairs are simply skipped.
        /// </summary>
        /// <param name="font">The font to be used.</param>
        /// <param name="s">The string to be mapped.</param>
        public static CodePointGlyphIndexPair[] GlyphIndicesFromString(string s, XFont font)
            => font.OpenTypeDescriptor.GlyphIndicesFromString(s);

#if NET6_0_OR_GREATER_
        /// <summary>
        /// Maps the characters of a string to an array of code points.
        /// It uses the concept of runes, which is not implemented in .NET Framework or .NET Standard.
        /// </summary>
        static int[] CodepointsFromStringUsingRunes(string s)
        {
            var result = s.EnumerateRunes().Select(run => run.Value).ToArray();
            return result;
        }
#endif
    }
}
