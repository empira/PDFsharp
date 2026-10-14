// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Base class of PdfSquareAnnotation and PdfCircleAnnotation.
    /// </summary>
    public abstract class PdfShapeAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.8  Square and circle annotations / Page 489

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfShapeAnnotation"/> class.
        /// </summary>
        protected PdfShapeAnnotation(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfShapeAnnotation"/> class.
        /// </summary>
        protected PdfShapeAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        //public XColor FillColor
        //{
        //    get
        //    {
        //        return XColor.Empty;
        //    }
        //    set
        //    {
        //    }
        //}

        internal static PdfShapeAnnotation Initialize(PdfShapeAnnotation annot, PdfRectangle rect, XColor color, XColor fillColor)
        {
            annot.Page!.Annotations.Add(annot);

            annot.PageRectangle = new XRect(rect.Location, rect.Size);
            if (!color.IsEmpty)
                annot.Color = color;

            // Interior color.
            if (!fillColor.IsEmpty)
            {
                // IMPROVE: Write helper for this.
                var array = new PdfArray(annot.Page.Document);
                array.Elements.Add(new PdfReal(fillColor.R / 255.0));
                array.Elements.Add(new PdfReal(fillColor.G / 255.0));
                array.Elements.Add(new PdfReal(fillColor.B / 255.0));
                annot.Elements[PdfShapeAnnotation.Keys.IC] = array;
            }

            return annot;
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            // Reference 2.0: Table 180 — Additional entries specific to a square or circle annotation / Page 490

            // ReSharper disable InconsistentNaming

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            /// <summary>
            /// (Optional) A border style dictionary specifying the line width and dash pattern that
            /// shall be used in drawing the rectangle or ellipse.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string BS = "/BS";

            /// <summary>
            /// (Optional; PDF 1.4) An array of numbers that shall be in the range 0.0 to 1.0 and shall
            /// specify the interior colour with which to fill the annotation’s rectangle or ellipse.
            /// The number of array elements determines the colour space in which the colour shall be
            /// defined:
            /// 0 No colour; transparent
            /// 1 DeviceGray
            /// 3 DeviceRGB
            /// 4 DeviceCMYK
            /// </summary>
            [KeyInfo("1.4", KeyType.Array | KeyType.Optional)]
            public const string IC = "/IC";

            /// <summary>
            /// (Optional; PDF 1.5) A border effect dictionary describing an effect applied to the
            /// border described by the BS entry.
            /// </summary>
            [KeyInfo("1.5", KeyType.Dictionary | KeyType.Optional)]
            public const string BE = "/BE";

            /// <summary>
            /// (Optional; PDF 1.5) A set of four numbers that shall describe the numerical differences
            /// between two rectangles: the Rect entry of the annotation and the actual boundaries of
            /// the underlying square or circle. Such a difference may occur in situations where a border
            /// effect (described by BE) causes the size of the Rect to increase beyond that of the
            /// square or circle.
            /// The four numbers shall correspond to the differences in default user space between the
            /// left, top, right, and bottom coordinates of Rect and those of the square or circle,
            /// respectively. Each value shall be greater than or equal to 0. The sum of the top and
            /// bottom differences shall be less than the height of Rect, and the sum of the left and
            /// right differences shall be less than the width of Rect.
            /// </summary>
            [KeyInfo("1.5", KeyType.Rectangle | KeyType.Optional)]
            public const string RD = "/RD";

            // ReSharper restore InconsistentNaming

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }

    /// <summary>
    /// Represents a PDF square annotation.
    /// </summary>
    public sealed class PdfSquareAnnotation : PdfShapeAnnotation
    {
        // Reference 2.0: 12.5.6.8 Square and circle annotations / Page 489

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSquareAnnotation"/> class.
        /// </summary>
        public PdfSquareAnnotation(PdfDocument document)
            : base(document)
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Square);
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfSquareAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        public static PdfSquareAnnotation Create(PdfPage page, PdfRectangle rect, XColor color, XColor fillColor)
        {
            var annot = new PdfSquareAnnotation(page.Document);
            page.Annotations.Add(annot);
            Initialize(annot, rect, color, fillColor);
            return annot;
        }
    }

    /// <summary>
    /// Represents a PDF circle annotation.
    /// </summary>
    public sealed class PdfCircleAnnotation : PdfShapeAnnotation
    {
        // Reference 2.0: 12.5.6.8 Square and circle annotations / Page 489

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfCircleAnnotation"/> class.
        /// </summary>
        public PdfCircleAnnotation(PdfDocument document)
            : base(document)
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Circle);
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfCircleAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        public static PdfCircleAnnotation Create(PdfPage page, PdfRectangle rect, XColor color, XColor fillColor)
        {
            var annot = new PdfCircleAnnotation(page.Document);
            page.Annotations.Add(annot);
            Initialize(annot, rect, color, fillColor);
            return annot;
        }
    }
}
