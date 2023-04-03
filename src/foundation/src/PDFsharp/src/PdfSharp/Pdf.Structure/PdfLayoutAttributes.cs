// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Structure
{
    /// <summary>
    /// Represents a PDF layout attributes object.
    /// </summary>
    public class PdfLayoutAttributes : PdfAttributesBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLayoutAttributes"/> class.
        /// </summary>
        /// <param name="document">The document that owns this object.</param>
        internal PdfLayoutAttributes(PdfDocument document)
            : base(document)
        {
            SetOwner();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLayoutAttributes"/> class.
        /// </summary>
        public PdfLayoutAttributes()
        {
            SetOwner();
        }

        void SetOwner()
        {
            Elements.SetName(PdfAttributesBase.Keys.O, "/Layout");
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAttributesBase.Keys
        {
            // Reference: TABLE 10.28  Standard attribute owners / Page 914
            // Reference: TABLE 10.29  Standard layout attributes / Page 916
            // Reference: TABLE 10.30  Standard layout attributes common to all standard structure types / Page 917
            // Reference: TABLE 10.31  Additional standard layout attributes specific to block-level structure / Page 922
            // Reference: TABLE 10.32  Standard layout attributes specific to inline-level structure elements / Page 926
            // Reference: TABLE 10.33  Standard column attributes / Page 932

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional for Annot; required for any figure or table appearing in its entirety
            /// on a single page; not inheritable). An array of four numbers in default user
            /// space units giving the coordinates of the left, bottom, right, and top edges,
            /// respectively, of the element’s bounding box (the rectangle that completely
            /// encloses its visible content). This attribute applies to any element that lies
            /// on a single page and occupies a single rectangle.
            /// </summary>
            [KeyInfo(KeyType.Rectangle | KeyType.Optional)]
            public const string BBox = "/BBox";

            // ReSharper restore InconsistentNaming
        }
    }
}
