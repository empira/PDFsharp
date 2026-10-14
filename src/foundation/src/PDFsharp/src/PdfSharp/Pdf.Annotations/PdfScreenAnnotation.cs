// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF screen annotation.
    /// </summary>
    public sealed class PdfScreenAnnotation : PdfAnnotation
    {
        // Reference 2.0: 12.5.6.18  Screen annotations / Page 497

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLineAnnotation"/> class.
        /// </summary>
        public PdfScreenAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfScreenAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Screen);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) The title of the screen annotation.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string T = "/T";

            /// <summary>
            /// (Optional) An appearance characteristics dictionary (see "Table 192 — Entries in an
            /// appearance characteristics dictionary"). The I entry of this dictionary provides the
            /// icon used in generating the appearance referred to by the screen annotation’s AP entry.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string MK = "/MK";

            /// <summary>
            /// (Optional; PDF 1.1) An action that shall be performed when the annotation is activated
            /// (see 12.6, "Actions").
            /// </summary>
            [KeyInfo("1.1", KeyType.Dictionary | KeyType.Optional)]
            public const string A = "/A";

            /// <summary>
            /// (Optional; PDF 1.2) An additional-actions dictionary defining the screen annotation’s
            /// behaviour in response to various trigger events (see 12.6.3, "Trigger events").
            /// </summary>
            [KeyInfo("1.2", KeyType.Dictionary | KeyType.Optional)]
            public const string AA = "/AA";

            // ReSharper restore InconsistentNaming
            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}