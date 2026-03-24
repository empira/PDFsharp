// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents the border effect of all annotations.
    /// </summary>
    public class PdfBorderEffect : PdfDictionary
    {
        // Reference 2.0: 12.5.4  Border styles / Page 472

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfBorderEffect"/> class.
        /// </summary>
        protected PdfBorderEffect()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfBorderEffect"/> class.
        /// </summary>
        protected PdfBorderEffect(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfBorderEffect(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            // TODO
            //Elements.SetName(Keys.Type, "/Annot");
            //Elements.SetDateTime(Keys.M, DateTimeOffset.Now);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase
        {
            // Reference 2.0: Table 168 — Entries in a border style dictionary / Page 473

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) A name representing the border effect to apply. Values are:
            /// C The border should appear "cloudy"; that is, the border should be drawn as a series
            /// of convex curved line segments in a manner that simulates the appearance of a cloud.
            /// The width and dash array specified by BS shall be honoured.
            /// Default value: S.
            /// S No effect: the border shall be as described by the annotation dictionary’s BS entry.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string S = "/S";

            /// <summary>
            /// (Optional; valid only if the value of S is C) A number describing the intensity of the
            /// effect, in the range 0 to 2. Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string I = "/I";

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
