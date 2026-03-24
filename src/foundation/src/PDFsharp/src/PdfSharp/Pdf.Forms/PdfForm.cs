// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 TODO review, Fields OTT

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Forms
{
    public sealed class PdfForm : PdfDictionary
    {
        // Reference 2.0: 12.7.3  Interactive form dictionary / Page 529

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfForm" /> class.
        /// </summary>
        internal PdfForm(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfForm(PdfDictionary dictionary)
            : base(dictionary)
        { }

        /// <summary>
        /// Gets the fields collection of this form.
        /// It is created if it does not exist.
        /// </summary>
        public PdfFormFields Fields
        {
            get
            {
                var fields = Elements.GetRequiredValue<PdfFormFields>(Keys.Fields, VCF.CreateIndirect);
                return fields;
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal class Keys : KeysBase
        {
            // Reference 2.0: Table 224 — Entries in the interactive form dictionary / Page 529

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required) An array of references to the document’s root fields (those with no ancestors
            /// in the field hierarchy).
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required, typeof(PdfFormFields))]
            public const string Fields = "/Fields";

            /// <summary>
            /// (Optional; deprecated in PDF 2.0) A flag specifying whether to construct appearance
            /// streams and appearance dictionaries for all widget annotations in the document.
            /// Default value: false.
            /// A PDF writer shall include this key, with a value of true, if it has not provided
            /// appearance streams for all visible widget annotations present in the document.
            /// NOTE
            /// Appearance streams are required in PDF 2.0 and later.
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string NeedAppearances = "/NeedAppearances";

            /// <summary>
            /// (Optional; PDF 1.3) A set of flags specifying various document-level characteristics
            /// related to signature fields.
            /// Default value: 0.
            /// </summary>
            [KeyInfo("1.3", KeyType.Integer | KeyType.Optional)]
            public const string SigFlags = "/SigFlags";

            /// <summary>
            /// (Required if any fields in the document have additional-actions dictionaries containing
            /// a C entry; PDF 1.3) An array of indirect references to field dictionaries with calculation
            /// actions, defining  the calculation order in which their values will be recalculated when
            /// the value of any field changes.
            /// </summary>
            [KeyInfo(KeyType.Array)]
            public const string CO = "/CO";

            /// <summary>
            /// (Optional) A resource dictionary containing default resources (such as fonts, patterns,
            /// or colour spaces) that shall be used by form field  appearance streams. At a minimum,
            /// this dictionary shall contain a Font entry specifying  the resource name and font
            /// dictionary of the default font for displaying text.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string DR = "/DR";

            /// <summary>
            /// (Optional) A document-wide default value for the DA attribute of variable text fields.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Optional)]
            public const string DA = "/DA";  // E.g. "/DA (/Helv 0 Tf 0 g)"

            /// <summary>
            /// (Optional) A document-wide default value for the Q attribute of variable text fields.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Q = "/Q";

            /// <summary>
            /// (Optional; deprecated in PDF 2.0) A stream or array containing an XFA resource,
            /// whose format shall conform to the Data Package (XDP) Specification.
            /// </summary>
            [KeyInfo(KeyType.StreamOrArray | KeyType.Optional)]
            public const string XFA = "/XFA";

            // ReSharper restore InconsistentNaming

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
