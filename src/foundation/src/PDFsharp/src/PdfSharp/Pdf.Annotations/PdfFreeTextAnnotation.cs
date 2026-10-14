// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF free text annotation.
    /// </summary>
    public sealed class PdfFreeTextAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.6  Free text annotations / Page 484

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFreeTextAnnotation"/> class.
        /// </summary>
        public PdfFreeTextAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFreeTextAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.FreeText);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            // ReSharper disable InconsistentNaming

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            /// <summary>
            /// (Required) The default appearance string that shall be used in formatting the text.
            /// The annotation dictionary’s AP entry, if present, shall take precedence over the DA
            /// entry.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Required)]
            public const string DA = "/DA";

            /// <summary>
            /// (Optional; PDF 1.4) A code specifying the form of quadding (justification) that shall
            /// be used in displaying the annotation’s text:
            /// 0 Left-justified
            /// 1 Centred
            /// 2 Right-justified
            /// Default value: 0 (left-justified).
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Q = "/Q";

            /// <summary>
            /// (Optional; PDF 1.5) A rich text string that shall be used to generate the appearance
            /// of the annotation.
            /// NOTE As freetext annotations do not have an open state this cannot apply to the popup
            /// window as described for the RC key in "Table 172 — Additional entries in an annotation
            /// dictionary specific to markup annotations".
            /// </summary>
            //[KeyInfo(KeyType.TextString | KeyType.Stream | KeyType.Optional)] // @@@@STLA
            [KeyInfo(KeyType.TextStringOrStream | KeyType.Optional)] // @@@@STLA // #US373
            public const string RC = "/RC";

            /// <summary>
            /// (Optional; PDF 1.5) A default style string, as described in Adobe XML Architecture,
            /// XML Forms Architecture (XFA) Specification, version 3.3.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string DS = "/DS";

            /// <summary>
            /// (Optional; meaningful only if IT is FreeTextCallout; PDF 1.6) An array of four or six
            /// numbers specifying a callout line attached to the free text annotation. Six numbers
            /// [x1 y1 x2 y2 x3 y3] represent the starting, knee point, and ending coordinates of the
            /// line in default user space. Four numbers [x1 y1 x2 y2] represent the starting and ending
            /// coordinates of the line.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string CL = "/CL";

            /// <summary>
            /// (Optional; PDF 1.6) A name describing the intent of the free text annotation. The following
            /// values shall be valid:
            /// FreeText   The annotation is intended to function as a plain free-text annotation.A plain
            /// free-text annotation is also known as a text box comment.
            /// FreeTextCallout   The annotation is intended to function as a callout. The callout is
            /// associated with an area on the page through the callout line specified in CL.
            /// FreeTextTypeWriter   The annotation is intended to function as a click-to-type or typewriter
            /// object and no callout line is drawn.
            /// Default value: FreeText
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string IT = "/IT";

            /// <summary>
            /// (Optional; PDF 1.6) A border effect dictionary used in conjunction with the border style
            /// dictionary specified by the BS entry.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string BE = "/BE";

            /// <summary>
            /// (Optional; PDF 1.6) A set of four numbers describing the numerical differences between
            /// two rectangles: the Rect entry of the annotation and a rectangle contained within that
            /// rectangle. The inner rectangle is where the annotation’s text should be displayed.
            /// Any border styles and/or border effects specified by BS and BE entries, respectively,
            /// shall be applied to the border of the inner rectangle.
            /// The four numbers correspond to the differences in default user space between the left,
            /// top, right, and bottom coordinates of Rect and those of the inner rectangle, respectively.
            /// Each value shall be greater than or equal to 0. The sum of the top and bottom differences
            /// shall be less than the height of Rect, and the sum of the left and right differences
            /// shall be less than the width of Rect.
            /// </summary>
            [KeyInfo(KeyType.Rectangle | KeyType.Optional)]
            public const string RD = "/RD";

            /// <summary>
            /// (Optional; PDF 1.6) A border style dictionary specifying the line width and dash pattern
            /// that shall be used in drawing the annotation’s border.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string BS = "/BS";

            /// <summary>
            /// (Optional; meaningful only if CL is present; PDF 1.6) A name specifying the line ending
            /// style that shall be used in drawing the callout line specified in CL. The name shall
            /// specify the line ending style for the endpoint defined by the pairs of coordinates (x1, y1).
            /// Default value: None.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string LE = "/LE";

            // ReSharper restore InconsistentNaming

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
