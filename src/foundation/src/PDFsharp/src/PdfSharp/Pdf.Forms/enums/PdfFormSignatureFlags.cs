// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 Ready

namespace PdfSharp.Pdf.Forms
{
    /// <summary>
    /// Specifies the flags of interactive form (AcroForm) fields.
    /// </summary>
    [Flags]
    public enum PdfFormSignatureFlags
    {
        // Reference 2.0: Table 225 — Signature flags / Page 530

        /// <summary>
        /// If set, the document contains at least one signature field. This flag allows an interactive
        /// PDF processor to enable user interface items (such as menu items or push-buttons) related to
        /// signature processing without having to scan the entire document for the presence of signature
        /// fields.
        /// </summary>
        SignaturesExist = 1 << (1 - 1),

        /// <summary>
        /// If set, the document contains signatures that may be invalidated if the PDF file is saved
        /// (written) in a way that alters its previous contents, as opposed to an incremental update.
        /// Merely updating the PDF file by appending new information to the end of the previous version
        /// is safe. Interactive PDF processors may use this flag to inform a user requesting a full save
        /// that signatures will be invalidated and require explicit confirmation before continuing with
        /// the operation.
        /// </summary>
        AppendOnly = 1 << (2 - 1),
    }
}
