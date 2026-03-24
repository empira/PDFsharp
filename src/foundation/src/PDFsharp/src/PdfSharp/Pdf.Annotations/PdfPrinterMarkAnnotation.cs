// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF printer’s mark annotation.
    /// </summary>
    public sealed class PdfPrinterMarkAnnotation : PdfAnnotation
    {
        // Reference 2.0: 12.5.6.20  Printer’s mark annotations / Page 501

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPrinterMarkAnnotation"/> class.
        /// </summary>
        public PdfPrinterMarkAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfPrinterMarkAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.PrinterMark);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) An arbitrary name identifying the type of printer’s mark, such as ColorBar
            /// or RegistrationTarget.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string MN = "/MN";

            // ReSharper restore InconsistentNaming
            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}