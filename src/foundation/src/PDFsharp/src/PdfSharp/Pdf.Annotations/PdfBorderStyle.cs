// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents the border styles of all annotations.
    /// </summary>
    public class PdfBorderStyle : PdfDictionary
    {
        // Reference 2.0: 12.5.4  Border styles / Page 472

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfBorderStyle"/> class.
        /// </summary>
        protected PdfBorderStyle()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfBorderStyle"/> class.
        /// </summary>
        protected PdfBorderStyle(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfBorderStyle(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(Keys.Type, "/Border");
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase
        {
            // Reference 2.0: Table 168 — Entries in a border style dictionary / Page 473

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) The type of PDF object that this dictionary describes; if present,
            /// shall be Border for a border style dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional, FixedValue = "Border")]
            public const string Type = "/Type";

            /// <summary>
            /// (Optional) The border width in points. If this value is 0, no border shall be drawn.
            /// Default value: 1.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string W = "/W";

            /// <summary>
            /// (Optional) The border style:<br/>
            /// S (Solid) A solid rectangle surrounding the annotation.Default value.<br/>
            /// D (Dashed) A dashed rectangle surrounding the annotation.The dash pattern may be specified by the D entry.<br/>
            /// B (Beveled) A simulated embossed rectangle that appears to be raised above the surface of the page.<br/>
            /// I (Inset) A simulated engraved rectangle that appears to be recessed below the surface of the page.<br/>
            /// U (Underline) A single line along the bottom of the annotation rectangle.<br/>
            /// An interactive PDF processor shall tolerate other border styles that it does not recognise
            /// and shall use the default value (which is S).
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string S = "/S";

            /// <summary>
            /// (Optional) A dash array defining a pattern of dashes and gaps that shall be used in drawing
            /// a dashed border (border style D in the S entry). The dash array shall be specified in the
            /// same format as in the line dash pattern parameter of the graphics state. The dash phase
            /// shall not be specified and shall be assumed to be 0.
            /// EXAMPLE
            /// A D entry of[3 2] specifies a border drawn with 3-point dashes alternating with 2-point gaps.
            /// Default value: [3].
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string D = "/D";

            // ReSharper restore InconsistentNaming

            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
