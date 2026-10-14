// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 TODO

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Specifies the annotation types.
    /// </summary>
    [Flags]
    public enum PdfAnnotationStates
    {
        // Reference 2.0: Table 174 — Annotation states / Page 481

        /// <summary>
        /// The user has indicated nothing about the change (the default).
        /// </summary>
        None,

        // ----- States if the state model is Marked --------------------------------------------------------

        /// <summary>
        /// The annotation has been marked by the user.
        /// Markup: Yes
        /// </summary>
        Marked,

        /// <summary>
        /// The annotation has not been marked by the user (the default).
        /// </summary>
        Unmarked,

        // ----- States if the state model is Review --------------------------------------------------------

        /// <summary>
        /// The user agrees with the change.
        /// </summary>
        Accepted,

        /// <summary>
        /// The user disagrees with the change.
        /// </summary>
        Rejected,

        /// <summary>
        /// The change has been cancelled.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The change has been completed.
        /// </summary>
        Completed
    }
}
