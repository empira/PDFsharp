// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// Review: OK - StL/14-10-05

using System;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies the font style for the outline (bookmark) text.
    ///  </summary>
    [Flags]
    public enum PdfOutlineStyle  // Reference:  TABLE 8.5 Outline Item flags / Page 587
    {
        /// <summary>
        /// Outline text is displayed using a regular font.
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Outline text is displayed using an italic font.
        /// </summary>
        Italic = 1,

        /// <summary>
        /// Outline text is displayed using a bold font.
        /// </summary>
        Bold = 2,

        /// <summary>
        /// Outline text is displayed using a bold and italic font.
        /// </summary>
        BoldItalic = 3,
    }
}
