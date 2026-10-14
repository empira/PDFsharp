// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF redact annotation.
    /// </summary>
    public sealed class PdfRedactAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.23  Redaction annotations / Page 504

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRedactAnnotation"/> class.
        /// </summary>
        public PdfRedactAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfRedactAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Redact);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) An array of 8 x n numbers specifying the coordinates of n quadrilaterals
            /// in default user space, as described in "Table 182 — Additional entries specific to
            /// text markup annotations" for text markup annotations. If present, these quadrilaterals
            /// denote the content region that is intended to be removed. If this entry is not present,
            /// the Rect entry denotes the content region that is intended to be removed.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string QuadPoints = "/QuadPoints";

            /// <summary>
            /// (Optional) An array of three numbers in the range 0.0 to 1.0 specifying the components,
            /// in the DeviceRGB colour space, of the interior colour with which to fill the redacted
            /// region after the affected content has been removed. If this entry is absent, the interior
            /// of the redaction region is left transparent.
            /// This entry is ignored if the RO entry is present.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string IC = "/IC";

            /// <summary>
            /// (Optional) A form XObject specifying the overlay appearance for this redaction annotation.
            /// After this redaction is applied and the affected content has been removed, the overlay
            /// appearance should be drawn such that its origin lines up with the lower-left corner
            /// of the annotation rectangle. This form XObject is not necessarily related to other
            /// annotation appearances, and may or may not be present in the AP dictionary. This entry
            /// takes precedence over the IC, OverlayText, DA, and Q entries.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string RO = "/RO";

            /// <summary>
            /// (Optional) A text string specifying the overlay text that should be drawn over the redacted
            /// region after the affected content has been removed. This entry is ignored if the RO entry
            /// is present.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string OverlayText = "/OverlayText";

            /// <summary>
            /// (Optional) If true, then the text specified by OverlayText should be repeated to fill
            /// the redacted region after the affected content has been removed. This entry is ignored
            /// if the RO entry is present. Default value: false.
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string Repeat = "/Repeat";

            /// <summary>
            /// (Required if OverlayText is present, ignored otherwise) The appearance string that shall
            /// be used in formatting the overlay text when it is drawn after the affected content has
            /// been removed (see 12.7.4.3, "Variable text"). This entry is ignored if the RO entry
            /// is present.
            /// </summary>
            [KeyInfo(KeyType.ByteString | KeyType.Optional)]
            public const string DA = "/DA";

            /// <summary>
            /// (Optional) A code specifying the form of quadding (justification) that shall be used
            /// in laying out the overlay text:
            /// 0 Left-justified
            /// 1 Centred
            /// 2 Right-justified This entry is ignored if the RO entry is present.
            /// Default value: 0 (left-justified).
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Q = "/Q";

            // ReSharper restore InconsistentNaming
            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
