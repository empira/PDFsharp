// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Structure
{
    /// <summary>
    /// Represents a PDF table attributes object.
    /// </summary>
    public class PdfTableAttributes : PdfAttributesBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTableAttributes"/> class.
        /// </summary>
        /// <param name="document">The document that owns this object.</param>
        internal PdfTableAttributes(PdfDocument document)
            : base(document)
        {
            SetOwner();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTableAttributes"/> class.
        /// </summary>
        public PdfTableAttributes()
        {
            SetOwner();
        }

        void SetOwner()
        {
            Elements.SetName(PdfAttributesBase.Keys.O, "/Table");
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAttributesBase.Keys
        {
            // Reference: TABLE 10.36  Standard table attributes / Page 935

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional; not inheritable) The number of rows in the enclosing table that are spanned
            /// by the cell. The cell expands by adding rows in the block-progression direction
            /// specified by the table’s WritingMode attribute. Default value: 1.
            /// This entry applies only to table cells that have structure types TH or TD or that are
            /// role mapped to structure types TH or TD.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string RowSpan = "/RowSpan";

            /// <summary>
            /// (Optional; not inheritable) The number of columns in the enclosing table that are spanned
            /// by the cell. The cell expands by adding columns in the inline-progression direction
            /// specified by the table’s WritingMode attribute. Default value: 1.
            /// This entry applies only to table cells that have structure types TH or TD or that are
            /// role mapped to structure types TH or TD.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string ColSpan = "/ColSpan";

            // ReSharper restore InconsistentNaming
        }
    }
}
