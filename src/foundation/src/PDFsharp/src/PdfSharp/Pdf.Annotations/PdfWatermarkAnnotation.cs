// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF watermark annotation.
    /// </summary>
    public sealed class PdfWatermarkAnnotation : PdfAnnotation
    {
        // Reference 2.0: 12.5.6.22  Watermark annotations / Page 501

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLineAnnotation"/> class.
        /// </summary>
        public PdfWatermarkAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfWatermarkAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Watermark);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        new class Keys : PdfAnnotation.Keys
        {
            // Reference 2.0: Table 193 — Additional entries specific to a watermark annotation / Page 502

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfWatermarkAnnotationFixedPrint))]
            public const string FixedPrint = "/FixedPrint";

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }

    /// <summary>
    /// Represents a fixed print dictionary for a PDF watermark annotation.
    /// </summary>
    public sealed class PdfWatermarkAnnotationFixedPrint : PdfDictionary
    {
        // Reference 2.0: 12.5.6.22 Watermark annotations / Page 501

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfWatermarkAnnotationFixedPrint"/> class.
        /// </summary>
        public PdfWatermarkAnnotationFixedPrint(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfWatermarkAnnotationFixedPrint(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, "/FixedPrint");
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : PdfAnnotation.Keys
        {
            // Reference 2.0: Table 194 — Entries in a fixed print dictionary / Page 502

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            /// <summary>
            /// (Optional) The matrix used to transform the annotation’s rectangle before rendering.
            /// Default value: the identity matrix[1 0 0 1 0 0].
            /// When positioning content near the edge of the media, this entry should be used to provide
            /// a reasonable offset to allow for unprintable margins.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Matrix = "/Matrix";

            /// <summary>
            /// (Optional) The amount to translate the associated content horizontally, as a percentage
            /// of the width of the target media (or if unknown, the width of the page’s MediaBox).
            /// 1.0 represents 100% and 0.0 represents 0%. Negative values should not be used, since
            /// they may cause content to be drawn off the media.
            /// Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Optional)]
            public const string H = "/H";

            /// <summary>
            /// (Optional) The amount to translate the associated content vertically, as a percentage
            /// of the height of the target media (or if unknown, the height of the page’s MediaBox).
            /// 1.0 represents 100% and 0.0 represents 0%. Negative values should not be used, since
            /// they may cause content to be drawn off the media.
            /// Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Optional)]
            public const string V = "/V";

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
