// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Identifies the state of the document.
    /// </summary>
    [Flags]
    enum DocumentState
    {
        /// <summary>
        /// The document was created from scratch.
        /// </summary>
        Created = 0x0001,

        /// <summary>
        /// The document was created by opening an existing PDF file.
        /// </summary>
        Imported = 0x0002,

        /// <summary>
        /// The document is disposed.
        /// </summary>
        Disposed = 0x8000,
    }
}
