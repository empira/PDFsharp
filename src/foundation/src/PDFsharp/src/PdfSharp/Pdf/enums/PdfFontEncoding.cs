// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies the encoding schema used for an XFont when converting into PDF.
    /// </summary>
    public enum PdfFontEncoding
    {
        /// <summary>
        /// Lets PDFsharp decide which encoding is used when drawing text depending
        /// on the used characters.
        /// </summary>
        Automatic = 0,

        /// <summary>
        /// Causes a font to use Windows-1252 encoding to encode text rendered with this font.
        /// </summary>
        ///// Same as Windows1252 encoding.
        WinAnsi = 1,

        ///// <summary>
        ///// Causes a font to use Windows-1252 (aka WinAnsi) encoding to encode text rendered with this font.
        ///// </summary>
        //Windows1252 = 0,

        /// <summary>
        /// Causes a font to use Unicode encoding to encode text rendered with this font.
        /// </summary>
        Unicode = 2,

        //AutoSelect = 2 #NFM

        ///// <summary>
        ///// Unicode encoding.
        ///// </summary>
        //[Obsolete("Use WinAnsi or Unicode")]
        //Automatic = 1,  // Force Unicode when used.

        // Implementation note: PdfFontEncoding uses incorrect terms.
        // WinAnsi corresponds to WinAnsiEncoding, while Unicode uses glyph indices.
        // Furthermore, the term WinAnsi is an oxymoron.
        // Reference: TABLE D.1  Latin-text encodings / Page 996
    }
}
