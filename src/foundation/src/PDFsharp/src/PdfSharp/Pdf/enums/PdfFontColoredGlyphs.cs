// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies whether the glyphs of an XFont are rendered multicolored into PDF.
    /// Useful for emoji fonts that have a COLR and a CPAL table.
    /// </summary>
    public enum PdfFontColoredGlyphs
    {
        /// <summary>
        /// Glyphs are rendered monochrome. This is the default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Glyphs are rendered using color information from the version 0 of the fonts
        /// COLR table. This option has no effect if the font has no COLR table or there
        /// is no entry for a particular glyph index.
        /// </summary>
        Version0 = 1,

        // Note: Version1 is also possible, but with significantly more code. Version 1
        // uses extended graphical capabilities like gradient brushes and much more.
    }
}
