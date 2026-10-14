// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF RichMedia annotation.
    /// </summary>
    public sealed class PdfRichMediaAnnotation : PdfAnnotation
    {
        // Reference 2.0: 13.7.2  RichMedia annotations / Page 700

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRichMediaAnnotation"/> class.
        /// </summary>
        public PdfRichMediaAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfRichMediaAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.RichMedia);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required; PDF 2.0) A RichMediaContent dictionary that stores the rich media artwork
            /// and information as to how it should be configured and viewed. See "Table 341 — Entries
            /// in a RichMediaContent dictionary".
            /// </summary>
            [KeyInfo("2.0", KeyType.Dictionary | KeyType.Required)]
            public const string RichMediaContent = "/RichMediaContent";

            /// <summary>
            /// (Optional; PDF 2.0) A RichMediaSettings dictionary that stores conditions and responses
            /// that determine when the annotation should be activated and deactivated by an interactive
            /// PDF processor and the initial state of artwork in those states. See "Table 334 — Entries
            /// in a RichMediaSettings dictionary". Default value: If no RichMediaSettings dictionary
            /// is present, the first configuration is loaded.
            /// </summary>
            [KeyInfo("2.0", KeyType.Dictionary | KeyType.Required)]
            public const string RichMediaSettings = "/RichMediaSettings";


            // ReSharper restore InconsistentNaming
            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
