// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF 3D annotation.
    /// </summary>
    public sealed class Pdf3DAnnotation : PdfAnnotation
    {
        // Reference 2.0: 13.6.2  3D annotations / Page 643

        /// <summary>
        /// Initializes a new instance of the <see cref="Pdf3DAnnotation"/> class.
        /// </summary>
        public Pdf3DAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal Pdf3DAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.ThreeD);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required) A 3D stream (see 13.6.3, "3D streams") or 3D reference dictionary
            /// (see 13.6.3.3, "3D reference dictionaries") that specifies the 3D artwork
            /// to be shown.
            /// </summary>
            [KeyInfo(KeyType.StreamOrDictionary | KeyType.Required)]
            public const string _3DD = "/3DD";
            /// <summary>
            /// (Optional) An object that specifies the default initial view of the 3D artwork that
            /// shall be used when the annotation is activated. It may be either a 3D view dictionary
            /// (see 13.6.4, "3D views") or one of the following types specifying an element in the
            /// VA array in the 3D stream (see "Table 311 — Entries in a 3D stream dictionary"):
            /// • An integer specifying an index into the VA array.
            /// • A text string matching the IN entry in one of the views in the VA array.
            /// • A name that indicates the first(F), last(L), or default (D) entries in the VA array.
            /// Default value: the default view in the 3D stream object specified by 3DD.
            /// </summary>
            [KeyInfo(KeyType.Various | KeyType.Optional)]
            public const string _3DV = "/3DV";

            /// <summary>
            /// (Optional) An activation dictionary (see "Table 310 — Entries in a 3D activation dictionary")
            /// that defines the times at which the annotation shall be activated and deactivated and
            /// the state of the 3D artwork instance at those times.
            /// Default value: an activation dictionary containing default values for all its entries.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string _3DA = "/3DA";

            /// <summary>
            /// (Optional) A flag indicating the primary use of the 3D annotation. If true, it is intended
            /// to be interactive; if false, it is intended to be manipulated programmatically, as with
            /// an ECMAScript animation. Interactive PDF processors may present different user interface
            /// controls for interactive 3D annotations (for example, to rotate, pan, or zoom the artwork)
            /// than for those managed by a script or other mechanism.
            /// Default value: true.
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string _3DI = "/3DI";

            /// <summary>
            /// (Optional) The 3D view box, which is the rectangular area in which the 3D artwork shall
            /// be drawn. It shall be within the rectangle specified by the annotation’s Rect entry
            /// and shall be expressed in the annotation’s target coordinate system (see discussion
            /// following this Table). Default value: the annotation’s Rect entry, expressed in the
            /// target coordinate system.This value is [-w/2 -h/2 w/2 h/2], where w and h are the width
            /// and height, respectively, of Rect.
            /// </summary>
            [KeyInfo(KeyType.Rectangle | KeyType.Optional)]
            public const string _3DB = "/3DB"; // TODO Check type.

            /// <summary>
            /// (Optional; PDF 2.0) A 3D units dictionary that specifies the units definitions for
            /// the 3D data associated with this annotation. See "Table 325 — Entries in a 3D units
            /// dictionary".
            /// </summary>
            [KeyInfo("2.0", KeyType.Dictionary | KeyType.Optional)]
            public const string _3DU = "/3DU";

            /// <summary>
            /// (Optional; PDF 2.0) For Geospatial3D requirement type, a geospatial information section
            /// may be present as an attribute within a 3D Annotation. There are further conditions
            /// placed on the GPTS and LPTS arrays within the geo-reference coordinate tables to include
            /// 3D point values. See 12.10.2, "Geospatial measure dictionary".
            /// </summary>
            [KeyInfo("2.0", KeyType.Dictionary | KeyType.Optional)]
            public const string GEO = "/GEO";

            // ReSharper restore InconsistentNaming
            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
