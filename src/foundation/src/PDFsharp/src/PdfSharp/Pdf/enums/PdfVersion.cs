// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Identifies the PDF version of a document.
    /// NOT YET USED.
    /// </summary>
    [Flags]
    // ReSharper disable InconsistentNaming because we want to use underscores in identifiers.
    public enum PdfVersion
    {
        /// <summary>
        /// PDF version is undefined.
        /// </summary>
        PDF_None = 0,

        /// <summary>
        /// PDF version 1.0 (%PDF–1.0).
        /// </summary>
        PDF_1_0 = 10,

        /// <summary>
        /// PDF version 1.1 (%PDF–1.1).
        /// </summary>
        PDF_1_1 = 11,

        /// <summary>
        /// PDF version 1.2 (%PDF–1.2).
        /// </summary>
        PDF_1_2 = 12,

        /// <summary>
        /// PDF version 1.3 (%PDF–1.3).
        /// </summary>
        PDF_1_3 = 13,

        /// <summary>
        /// PDF version 1.4 (%PDF–1.4).
        /// </summary>
        PDF_1_4 = 14,

        /// <summary>
        /// PDF version 1.5 (%PDF–1.5).
        /// </summary>
        PDF_1_5 = 15,

        /// <summary>
        /// PDF version 1.6 (%PDF–1.6.
        /// </summary>
        PDF_1_6 = 16,

        /// <summary>
        /// PDF version 1.7 (%PDF–1.7).
        /// </summary>
        PDF_1_7 = 17,

        /// <summary>
        /// PDF version 2.0 (%PDF–2.0).
        /// </summary>
        PDF_2_0 = 20
    }
    // ReSharper restore InconsistentNaming
}
