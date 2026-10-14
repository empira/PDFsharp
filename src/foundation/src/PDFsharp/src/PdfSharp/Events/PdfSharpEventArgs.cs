// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;

// v7.0.0 Ready

namespace PdfSharp.Events
{
    /// <summary>
    /// Base class for EventArgs in PDFsharp.
    /// </summary>
    public abstract class PdfSharpEventArgs(PdfDocument source) : EventArgs
    {
        /// <summary>
        /// The source PDF document of the event.
        /// </summary>
        public PdfDocument Source { get; set; } = source;
    }
}
