// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents a generic field. Used for AcroForm dictionaries unknown to PDFsharp.
    /// </summary>
    public sealed class PdfGenericField : PdfAcroField
    {
        /// <summary>
        /// Initializes a new instance of PdfGenericField.
        /// </summary>
        internal PdfGenericField(PdfDocument document)
            : base(document)
        { }

        internal PdfGenericField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfAcroField.Keys
        {
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
