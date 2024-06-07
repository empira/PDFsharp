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
        WinAnsi = 1,

        /// <summary>
        /// Causes a font to use Unicode encoding to encode text rendered with this font.
        /// </summary>
        Unicode = 2,
    }
}
