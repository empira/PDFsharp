// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF caret annotation.
    /// </summary>
    public sealed class PdfCaretAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.11  Caret annotations / Page 493

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfCaretAnnotation"/> class.
        /// </summary>
        public PdfCaretAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfCaretAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Caret);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            // Reference 2.0: Table 183 — Additional entries specific to a caret annotation / Page 493

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional; PDF 1.5) A set of four numbers that shall describe the numerical differences
            /// between two rectangles: the Rect entry of the annotation and the actual boundaries
            /// of the underlying caret. Such a difference can occur. When a paragraph symbol specified
            /// by Sy is displayed along with the caret.
            /// The four numbers shall correspond to the differences in default user space between
            /// the left, top, right, and bottom coordinates of Rect and those of the caret, respectively.
            /// Each value shall be greater than or equal to 0. The sum of the top and bottom differences
            /// shall be less than the height of Rect, and the sum of the left and right differences
            /// shall be less than the width of Rect.
            /// </summary>
            //[KeyInfo("1.5", KeyType.Rectangle | KeyType.Optional)]
            [KeyInfo("1.5", KeyType.Array | KeyType.Optional)]
            public const string RD = "/RD";  // left, top, right, bottom

            /// <summary>
            /// (Optional) A name specifying a symbol that shall be associated with the caret:
            /// P     A new paragraph symbol(¶) shall be associated with the caret.
            /// None  No symbol shall be associated with the caret.
            /// Default value: None.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Sy = "/Sy";

            // ReSharper restore InconsistentNaming

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
