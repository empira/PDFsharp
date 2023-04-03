// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// INTERNAL USE ONLY.
    /// </summary>
    [Flags]
    enum PdfWriterOptions
    {
        /// <summary>
        /// If only this flag is specified, the result is a regular valid PDF stream.
        /// </summary>
        Regular = 0x000000,

        /// <summary>
        /// Omit writing stream data. For debugging purposes only. 
        /// With this option the result is not valid PDF.
        /// </summary>
        OmitStream = 0x000001,

        /// <summary>
        /// Omit inflate filter. For debugging purposes only.
        /// </summary>
        OmitInflation = 0x000002,
    }
}
