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
        /// be modified, but you can extract pages from it.
        /// </summary>
        Import,

        /// <summary>
        /// The PDF stream is completely read into memory, but cannot be modified. This mode preserves the
        /// original internal structure of the document and is useful for analyzing existing PDF files.
        /// </summary>
        [Obsolete("ReadOnly is not implemented, use Import instead.")]
        ReadOnly,

        /// <summary>
        /// The PDF stream is partially read for information purposes only. The only valid operation is to
        /// call the Info property at the imported document. This option is very fast and needs less memory
        /// and is e.g. useful for browsing information about a collection of PDF documents in a user interface.
        /// </summary>
        [Obsolete("InformationOnly is not implemented, use Import instead.")]
        InformationOnly,

        // Note: New members must be appended here to keep the numeric values of the existing members stable.

        /// <summary>
        /// Like <see cref="Modify"/>, but the object numbering of the original file is preserved: unreachable
        /// objects are not removed and the cross-reference table is not renumbered. This is a prerequisite for
        /// an append-only incremental update, where the bytes of the original file are written unchanged and
        /// every object must keep the object number it has in that file.
        /// A document opened in this mode can only be saved with
        /// <see cref="PdfDocument.SaveIncremental(System.IO.Stream, bool)"/> and its overloads.
        /// </summary>
        ModifyIncremental,
    }
}
