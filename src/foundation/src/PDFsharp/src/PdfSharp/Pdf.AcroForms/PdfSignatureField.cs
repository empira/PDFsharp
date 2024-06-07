// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the signature field.
    /// </summary>
    public sealed class PdfSignatureField : PdfAcroField
    {
        /// <summary>
        /// Initializes a new instance of PdfSignatureField.
        /// </summary>
        internal PdfSignatureField(PdfDocument document)
            : base(document)
        { }

        internal PdfSignatureField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Writes a key/value pair of this signature field dictionary.
        /// </summary>
        internal override void WriteDictionaryElement(PdfWriter writer, PdfName key)
        {
            // Don’t encrypt Contents key’s value (PDF Reference 2.0: 7.6.2, Page 71).
            if (key.Value == Keys.Contents)
            {
                var effectiveSecurityHandler = writer.EffectiveSecurityHandler;
                writer.EffectiveSecurityHandler = null;
                base.WriteDictionaryElement(writer, key);
                writer.EffectiveSecurityHandler = effectiveSecurityHandler;
            }
            else
                base.WriteDictionaryElement(writer, key);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfAcroField.Keys
        {
            /// <summary>
            /// (Optional) The type of PDF object that this dictionary describes; if present,
            /// must be Sig for a signature dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Type = "/Type";

            /// <summary>
            /// (Required; inheritable) The name of the signature handler to be used for
            /// authenticating the field’s contents, such as Adobe.PPKLite, Entrust.PPKEF,
            /// CICI.SignIt, or VeriSign.PPKVS.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string Filter = "/Filter";

            /// <summary>
            /// (Optional) The name of a specific submethod of the specified handler.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string SubFilter = "/SubFilter";

            /// <summary>
            /// (Required) An array of pairs of integers (starting byte offset, length in bytes)
            /// describing the exact byte range for the digest calculation. Multiple discontinuous
            /// byte ranges may be used to describe a digest that does not include the
            /// signature token itself.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required)]
            public const string ByteRange = "/ByteRange";

            /// <summary>
            /// (Required) The encrypted signature token.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Required)]
            public const string Contents = "/Contents";

            /// <summary>
            /// (Optional) The name of the person or authority signing the document.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Name = "/Name";

            /// <summary>
            /// (Optional) The time of signing. Depending on the signature handler, this
            /// may be a normal unverified computer time or a time generated in a verifiable
            /// way from a secure time server.
            /// </summary>
            [KeyInfo(KeyType.Date | KeyType.Optional)]
            public const string M = "/M";

            /// <summary>
            /// (Optional) The CPU host name or physical location of the signing.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Location = "/Location";

            /// <summary>
            /// (Optional) The reason for the signing, such as (I agree…).
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Reason = "/Reason";

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
