// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 TODO review

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// The signature dictionary added to a PDF file when it is to be signed.
    /// </summary>
    public sealed class PdfSignature : PdfDictionary
    {
        ///// <summary>
        ///// Initializes a new instance of the <see cref="PdfSignature"/> class.
        ///// </summary>
        //public PdfSignature()
        //{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSignature"/> class.
        /// </summary>
        public PdfSignature(PdfDocument document) : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfSignature(PdfDictionary dict) : base(dict)
        { }

        ///// <summary>
        ///// Encrypts the given bytes. Returns true if the crypt filter encrypted the bytes, or false, if the security handler shall do it.
        ///// </summary>
        //internal abstract bool EncryptForEnteredObject(ref byte[] bytes);

        ///// <summary>
        ///// Decrypts the given bytes. Returns true if the crypt filter decrypted the bytes, or false, if the security handler shall do it.
        ///// </summary>
        //internal abstract bool DecryptForEnteredObject(ref byte[] bytes);

        /// <summary>
        /// Predefined keys of this dictionary.
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public class Keys : KeysBase
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
