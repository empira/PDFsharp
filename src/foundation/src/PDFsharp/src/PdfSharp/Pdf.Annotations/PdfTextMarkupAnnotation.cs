// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Base class of all Text Markup annotations.
    /// </summary>
    public abstract class PdfTextMarkupAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.10  Text markup annotations / Page 492
        [Obsolete("PDFsharp 6.4: Use a constructor with a PDF document parameter.")]
        protected PdfTextMarkupAnnotation()
            => throw new NotImplementedException("PDFsharp 6.4: Use a constructor with a PDF document parameter.");

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTextMarkupAnnotation"/> class.
        /// </summary>
        protected PdfTextMarkupAnnotation(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTextMarkupAnnotation"/> class.
        /// </summary>
        protected PdfTextMarkupAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfMarkupAnnotation.Keys
        {
            // Reference 2.0: Table 182 — Additional entries specific to text markup annotations / Page 492

            // ReSharper disable InconsistentNaming

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            /// <summary>
            /// (Required) An array of 8×𝑛 numbers specifying the coordinates of n quadrilaterals in
            /// default user space. Each quadrilateral shall encompasses a word or group of contiguous
            /// words in the text underlying the annotation. The coordinates for each quadrilateral
            /// shall be given in the order:
            ///   𝑥1 𝑦1 𝑥2 𝑦2 𝑥3 𝑦3 𝑥4 𝑦4
            /// specifying the quadrilateral’s four vertices in counterclockwise order
            /// (see "Figure 84 — QuadPoints specification"). The text shall be oriented with respect
            /// to the edge connecting points(x1, y1) and(x2, y2).
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required)]
            public const string QuadPoints = "/QuadPoints";


            // ReSharper restore InconsistentNaming

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }

    /// <summary>
    /// Represents a PDF highlight annotation.
    /// </summary>
    public sealed class PdfHighlightAnnotation : PdfTextMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.10  Text markup annotations / Page 492

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLineAnnotation"/> class.
        /// </summary>
        public PdfHighlightAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfHighlightAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Highlight);
        }
    }

    /// <summary>
    /// Represents a PDF underline annotation.
    /// </summary>
    public sealed class PdfUnderlineAnnotation : PdfTextMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.10  Text markup annotations / Page 492

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfUnderlineAnnotation"/> class.
        /// </summary>
        public PdfUnderlineAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfUnderlineAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Underline);
        }
    }

    /// <summary>
    /// Represents a PDF squiggly annotation.
    /// </summary>
    public sealed class PdfSquigglyAnnotation : PdfTextMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.10  Text markup annotations / Page 492

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSquigglyAnnotation"/> class.
        /// </summary>
        public PdfSquigglyAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfSquigglyAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Squiggly);
        }
    }

    /// <summary>
    /// Represents a PDF strikeout annotation.
    /// </summary>
    public sealed class PdfStrikeOutAnnotation : PdfTextMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.10  Text markup annotations / Page 492

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfStrikeOutAnnotation"/> class.
        /// </summary>
        public PdfStrikeOutAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfStrikeOutAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.StrikeOut);
        }

        ///// <summary>
        ///// Gets the KeysMeta of this dictionary type.
        ///// </summary>
        //internal override DictionaryMeta Meta => Keys.Meta;
    }
}
