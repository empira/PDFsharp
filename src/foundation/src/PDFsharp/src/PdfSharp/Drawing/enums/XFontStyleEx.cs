// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies style information applied to text.
    /// </summary>
    [Flags]
    public enum XFontStyleEx  // Same values as System.Drawing.FontStyle.
    {
        // Will be renamed to XGdiFontStyle or XWinFontStyle.

        /// <summary>
        /// Normal text.
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Bold text.
        /// </summary>
        Bold = 1,

        /// <summary>
        /// Italic text.
        /// </summary>
        Italic = 2,

        /// <summary>
        /// Bold and italic text.
        /// </summary>
        BoldItalic = 3,

        /// <summary>
        /// Underlined text.
        /// </summary>
        Underline = 4,

        /// <summary>
        /// Text with a line through the middle.
        /// </summary>
        Strikeout = 8,

        // Possible additional flags:
        // BoldSimulation
        // ItalicSimulation
    }
}
