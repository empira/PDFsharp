// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF Line annotation.
    /// </summary>
    public sealed class PdfLineAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.7  Line annotations / Page 486

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLineAnnotation"/> class.
        /// </summary>
        public PdfLineAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfLineAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Line);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            // ReSharper disable InconsistentNaming

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            /// <summary>
            /// (Required) An array of four numbers, [x1 y1 x2 y2], specifying the starting and ending
            /// coordinates of the line in default user space.
            /// If the LL entry is present, this value shall represent the endpoints of the leader lines
            /// rather than the endpoints of the line itself.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required)]
            public const string L = "/L";

            /// <summary>
            /// (Optional) A border style dictionary specifying the width and dash pattern that shall
            /// be used in drawing the line.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string BS = "/BS";

            /// <summary>
            /// (Optional; PDF 1.4) An array of two names specifying the line ending styles that shall
            /// be used in drawing the line. The first and second elements of the array shall specify
            /// the line ending styles for the endpoints defined, respectively, by the first and second
            /// pairs of coordinates, (x1, y1 ) and (x2, y2 ), in the L array.
            /// "Table 179 — Line ending styles" shows the permitted values. Default value: [ /None /None ].
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string LE = "/LE";

            /// <summary>
            /// (Optional; PDF 1.4) An array of numbers in the range 0.0 to 1.0 specifying the interior
            /// colour that shall be used to fill the annotation’s line endings. The number of array
            /// elements shall determine the colour space in which the colour is defined:
            /// 0 No colour; transparent
            /// 1 DeviceGray
            /// 3 DeviceRGB
            /// 4 DeviceCMYK
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string IC = "/IC";

            /// <summary>
            /// (Required if LLE is present, otherwise optional; PDF 1.6) The length of leader lines
            /// in default user space that extend from each endpoint of the line perpendicular to the
            /// line itself. A positive value shall mean that the leader lines appear in the direction
            /// that is clockwise when traversing the line from its starting point to its ending point
            /// (as specified by L); a negative value shall indicate the opposite direction.
            /// Default value: 0 (no leader lines).
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Required)]
            public const string LL = "/LL";

            /// <summary>
            /// (Optional; PDF 1.6) A non-negative number that shall represent the length of leader
            /// line extensions that extend from the line proper 180 degrees from the leader lines.
            /// Default value: 0 (no leader line extensions).
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Optional)]
            public const string LLE = "/LLE";

            /// <summary>
            /// (Optional; PDF 1.6) If true, the text specified by the Contents or RC entries shall
            /// be replicated as a caption in the appearance of the line. The text shall be rendered
            /// in a manner appropriate to the content, taking into account factors such as writing
            /// direction.
            /// Default value: false.
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string Cap = "/Cap";

            /// <summary>
            /// (Optional; PDF 1.6) A name describing the intent of the line annotation. Valid values
            /// shall be LineArrow, which means that the annotation is intended to function as an arrow,
            /// and LineDimension, which means that the annotation is intended to function as a dimension
            /// line.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string IT = "/IT";

            /// <summary>
            /// (Optional; PDF 1.7) A non-negative number that shall represent the length of the leader
            /// line offset, which is the amount of empty space between the endpoints of the annotation
            /// and the beginning of the leader lines.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Optional)]
            public const string LLO = "/LLO";

            /// <summary>
            /// (Optional; meaningful only if Cap is true; PDF 1.7) A name describing the annotation’s
            /// caption positioning. Valid values are Inline, meaning the caption shall be centred inside
            /// the line, and Top, meaning the caption shall be on top of the line.
            /// Default value: Inline
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string CP = "/CP";

            /// <summary>
            /// (Optional; PDF 1.7) A measure dictionary that shall specify the scale and units that
            /// apply to the line annotation.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string Measure = "/Measure";

            /// <summary>
            /// (Optional; meaningful only if Cap is true; PDF 1.7) An array of two numbers that shall
            /// specify the offset of the caption text from its normal position. The first value shall
            /// be the horizontal offset along the annotation line from its midpoint, with a positive
            /// value indicating offset to the right and a negative value indicating offset to the left.
            /// The second value shall be the vertical offset perpendicular to the annotation line,
            /// with a positive value indicating a shift up and a negative value indicating a shift down.
            /// Default value: [0, 0] (no offset from normal positioning)
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string CO = "/CO";

            // ReSharper restore InconsistentNaming

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
