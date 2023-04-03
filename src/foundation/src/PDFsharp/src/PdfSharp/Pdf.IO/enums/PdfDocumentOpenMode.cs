// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Determines how a PDF document is opened. 
    /// </summary>
    public enum PdfDocumentOpenMode
    {
        /// <summary>
        /// The PDF stream is completely read into memory and can be modified. Pages can be deleted or
        /// inserted, but it is not possible to extract pages. This mode is useful for modifying an
        /// existing PDF document.
        /// </summary>
        Modify,

        /// <summary>
        /// The PDF stream is opened for importing pages from it. A document opened in this mode cannot
        /// be modified.
        /// </summary>
        Import,

        /// <summary>
        /// The PDF stream is completely read into memory, but cannot be modified. This mode preserves the
        /// original internal structure of the document and is useful for analyzing existing PDF files.
        /// </summary>
        ReadOnly,

        /// <summary>
        /// The PDF stream is partially read for information purposes only. The only valid operation is to
        /// call the Info property at the imported document. This option is very fast and needs less memory
        /// and is e.g. useful for browsing information about a collection of PDF documents in a user interface.
        /// </summary>
        InformationOnly,  // TODO: not yet implemented
    }
}
