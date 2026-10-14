// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF popup annotation.
    /// </summary>
    public sealed class PdfPopupAnnotation : PdfAnnotation
    {
        // Reference 2.0: 12.5.6.14  Popup annotations / Page 495

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPopupAnnotation"/> class.
        /// </summary>
        public PdfPopupAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfPopupAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Popup);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional; shall be an indirect reference) The parent annotation with which this popup
            /// annotation shall be associated. If this entry is present, the parent annotation’s Contents,
            /// M, C, and T entries (see "Table 170 — Entries in an appearance dictionary") shall override
            /// those of the popup annotation itself. NOTE See also the Popup entry in "Table 172 — Additional
            /// entries in an annotation dictionary specific to markup annotations".
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string Parent = "/Parent";

            /// <summary>
            /// (Optional) A flag specifying whether the popup annotation shall initially be displayed open.
            /// Default value: false (closed).
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string Open = "/Open";

            // ReSharper restore InconsistentNaming
            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}