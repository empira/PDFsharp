// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;

namespace PdfSharp.Events
{
    /// <summary>
    /// Base class for EventArgs in PDFsharp.
    /// </summary>
    public abstract class PdfSharpEventArgs(PdfObject source) : EventArgs
    {
        /// <summary>
        /// The source of the event.
        /// </summary>
        public PdfObject Source { get; set; } = source;
    }
}
