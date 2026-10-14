// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Base class of PdfPolygonAnnotation and PdfPolyLineAnnotation.
    /// </summary>
    public abstract class PdfPolyAnnotation : PdfMarkupAnnotation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPolyAnnotation"/> class.
        /// </summary>
        protected PdfPolyAnnotation(PdfDocument document)

            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPolyAnnotation"/> class.
        /// </summary>
        protected PdfPolyAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            // Reference 2.0: Table 181 — Additional entries specific to a polygon or polyline annotation / Page 491

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required unless a Path key is present, in which case it shall be ignored) An array
            /// of numbers specifying the alternating horizontal and vertical coordinates, respectively,
            /// of each vertex, in default user space.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Vertices = "/Vertices";

            /// <summary>
            /// (Optional; meaningful only for polyline annotations) An array of two names that shall
            /// specify the line ending styles. The first and second elements of the array shall specify
            /// the line ending styles for the endpoints defined, respectively, by the first and last
            /// pairs of coordinates in the Vertices array. "Table 179 — Line ending styles" shows the
            /// allowed values. Default value: [/None /None].
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string LE = "/LE";

            /// <summary>
            /// (Optional) A border style dictionary (see "Table 168 — Entries in a border style dictionary")
            /// specifying the width and dash pattern that shall be used in drawing the line.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string BS = "/BS";

            /// <summary>
            /// (Optional) An array of numbers that shall be in the range 0.0 to 1.0 and shall specify
            /// the interior color with which to fill the annotation’s line endings
            /// (see "Table 179 — Line ending styles"). The number of array elements determines the
            /// colour space in which the colour shall be defined:
            /// 0 No colour; transparent
            /// 1 DeviceGray
            /// 3 DeviceRGB
            /// 4 DeviceCMYK
            /// For Polyline annotations, the value of the IC key is used to fill only the line ending.
            /// However, for Polygon annotations, the value of the IC key is used to fill the entire
            /// shape, much as the F operator would fill a shape in a content stream.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string IC = "/IC";

            /// <summary>
            /// (Optional; meaningful only for polygon annotations) A border effect dictionary that
            /// shall describe an effect applied to the border described by the BS entry
            /// (see "Table 169 — Entries in a border effect dictionary").
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string BE = "/BE";

            /// <summary>
            /// (Optional; PDF 1.6) A name that shall describe the intent of the polygon or polyline
            /// annotation (see also "Table 172 — Additional entries in an annotation dictionary
            /// specific to markup annotations").
            /// The following values shall be valid:
            /// PolygonCloud The annotation is intended to function as a cloud object.
            /// PolyLineDimension (PDF 1.7) The polyline annotation is intended to function as a dimension.
            /// PolygonDimension (PDF 1.7) The polygon annotation is intended to function as a dimension.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string IT = "/IT";

            /// <summary>
            /// (Optional; PDF 1.7) A measure dictionary (see "Table 266 — Entries in a measure dictionary")
            /// that shall specify the scale and units that apply to the annotation.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string Measure = "/Measure";

            /// <summary>
            /// (Optional; PDF 2.0) An array of n arrays, each supplying the operands for a path building
            /// operator (m, l or c). If this key is present the Vertices key shall not be present.
            /// Each of the n arrays shall contain pairs of values specifying the points (x and y values)
            /// for a path drawing operation. The first array shall be of length 2 and specifies the
            /// operand of a moveto operator which establishes a current point. Subsequent arrays of
            /// length 2 specify the operands of lineto operators. Arrays of length 6 specify the operands
            /// for curveto operators.Each array is processed in sequence to construct the path. The
            /// current graphics state shall control the path width, dash pattern, etc.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Path = "/Path";

            // ReSharper restore InconsistentNaming

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }

    /// <summary>
    /// Represents a PDF polygon annotation.
    /// </summary>
    public sealed class PdfPolygonAnnotation : PdfPolyAnnotation
    {
        // Reference 2.0: 12.5.6.9  Polygon and polyline annotations / Page 491

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPolygonAnnotation"/> class.
        /// </summary>
        public PdfPolygonAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfPolygonAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Polygon);
        }
    }

    /// <summary>
    /// Represents a PDF polyline annotation.
    /// </summary>
    public sealed class PdfPolyLineAnnotation : PdfPolyAnnotation
    {
        // Reference 2.0: 12.5.6.9  Polygon and polyline annotations / Page 491

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPolyLineAnnotation"/> class.
        /// </summary>
        public PdfPolyLineAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfPolyLineAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.PolyLine);
        }
    }
}
