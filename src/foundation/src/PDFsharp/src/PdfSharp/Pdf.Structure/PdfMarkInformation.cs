// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Structure
{
    /// <summary>
    /// Represents a mark information dictionary.
    /// </summary>
    public sealed class PdfMarkInformation : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfMarkInformation"/> class.
        /// </summary>
        public PdfMarkInformation()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfMarkInformation"/> class.
        /// </summary>
        /// <param name="document">The document that owns this object.</param>
        public PdfMarkInformation(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal class Keys : KeysBase
        {
            // Reference: TABLE 10.8  Entries in the mark information dictionary / Page 856

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) A flag indicating whether the document conforms to Tagged PDF conventions.
            /// Default value: false.
            /// Note: If Suspects is true, the document may not completely conform to Tagged PDF conventions.
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string Marked = "/Marked";

            /// <summary>
            /// (Optional; PDF 1.6) A flag indicating the presence of structure elements
            /// that contain user properties attributes.
            /// Default value: false.
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string UserProperties = "/UserProperties";

            /// <summary>
            /// (Optional; PDF 1.6) A flag indicating the presence of tag suspects.
            /// Default value: false.
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string Suspects = "/Suspects";

            // ReSharper restore InconsistentNaming
        }
    }
}
