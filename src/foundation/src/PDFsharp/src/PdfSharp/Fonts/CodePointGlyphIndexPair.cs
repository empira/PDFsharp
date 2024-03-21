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
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// The combination of a UTF-16 code unit (either BMP code point or surrogate pair),
    /// a Unicode code point, and the glyph index of the code point in a particular
    /// font face.
    /// </summary>
    public struct CodePointGlyphIndexPair(int codePoint, int glyphIndex)
    {
        /// <summary>
        /// The Unicode code point of the Character value.
        /// The code point can be 0 to indicate that the original character is not a valid UTF-32 code unit.
        /// This can happen when a string contains a single high or low surrogate without its counterpart.
        /// </summary>
        public int CodePoint = codePoint;

        /// <summary>
        /// The glyph identifier of the code point for a specific OpenType font.
        /// The value is 0 if the specific font has no glyph for the code point.
        /// </summary>
        public int GlyphIndex = glyphIndex;
    }
}
