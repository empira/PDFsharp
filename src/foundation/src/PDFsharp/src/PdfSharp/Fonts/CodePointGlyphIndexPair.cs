// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Fonts.OpenType;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// The combination of a Unicode code point and the glyph index of this code point in a particular font face.
    /// </summary>
    public struct CodePointGlyphIndexPair(int codePoint, ushort glyphIndex)
    {
        /// <summary>
        /// The Unicode code point of the Character value.
        /// The code point can be 0 to indicate that the original character is not a valid UTF-32 code unit.
        /// This can happen when a string contains a single high or low surrogate without its counterpart.
        /// </summary>
        public int CodePoint = codePoint;

        /// <summary>
        /// The glyph index of the code point for a specific OpenType font.
        /// The value is 0 if the specific font has no glyph for the code point.
        /// </summary>
        public ushort GlyphIndex = glyphIndex;

        /// <summary>
        /// The color-record of the Glyph if provided by the font.
        /// </summary>
        internal ColrTable.GlyphRecord? Color = null;
    }
}
