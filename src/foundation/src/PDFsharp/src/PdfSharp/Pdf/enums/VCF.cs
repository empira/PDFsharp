// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Value creation flags. Specifies whether and how a value that does not exist is created.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public enum VCF
        // ReSharper restore InconsistentNaming
    {
        /// <summary>
        /// Don’t create the value if it does not yet exist.
        /// If the value exists try a type transformation.
        /// </summary>
        None,

        /// <summary>
        /// Create the value as direct object if it does not yet exist.
        /// </summary>
        Create,

        /// <summary>
        /// Create the value as indirect object if it does not yet exist.
        /// </summary>
        CreateIndirect,

        /// <summary>
        /// If the value exists return it immediately.
        /// Do not try a type transformation.
        /// </summary>
        NoTransform
    }
}
