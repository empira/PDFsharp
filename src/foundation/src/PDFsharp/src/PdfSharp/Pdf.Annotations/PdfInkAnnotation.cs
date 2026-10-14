// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF ink annotation.
    /// </summary>
    public sealed class PdfInkAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.13  Ink annotations / Page 494

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfInkAnnotation"/> class.
        /// </summary>
        public PdfInkAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfInkAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Ink);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required) An array of n arrays, each representing a stroked path. Each array shall
            /// be a series of alternating horizontal and vertical coordinates in default user space,
            /// specifying points along the path. When drawn, the points shall be connected by straight
            /// lines or curves in an implementation-dependent way.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required)]
            public const string InkList = "/InkList";

            /// <summary>
            /// (Optional) A border style dictionary (see "Table 168 — Entries in a border style dictionary")
            /// specifying the line width and dash pattern that shall be used in drawing the paths.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string BS = "/BS";

            /// <summary>
            /// (Optional; PDF 2.0) An array of n arrays, each supplying the operands for a path building
            /// operator (m, l or c). Each of the n arrays shall contain pairs of values specifying
            /// the points(x and y values) for a path drawing operation. The first array shall be of
            /// length 2 and specifies the operand of a moveto operator which establishes a current point.
            /// Subsequent arrays of length 2 specify the operands of lineto operators. Arrays of length
            /// 6 specify the operands for curveto operators. Each array is processed in sequence to
            /// construct the path. The current graphics state shall control the path width,
            /// dash pattern, etc.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Path = "/Path";

            // ReSharper restore InconsistentNaming
            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}