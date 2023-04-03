// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace PdfSharp.Pdf.Security
{
    /// <summary>
    /// Represents the base of all security handlers.
    /// </summary>
    public abstract class PdfSecurityHandler : PdfDictionary
    {
        internal PdfSecurityHandler(PdfDocument document)
            : base(document)
        { }

        internal PdfSecurityHandler(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal class Keys : KeysBase
        {
            /// <summary>
            /// (Required) The name of the preferred security handler for this document. It shall be
            /// the name of the security handler that was used to encrypt the document. If
            /// SubFilter is not present, only this security handler shall be used when opening
            /// the document. If it is present, a PDF processor can use any security handler
            /// that implements the format specified by SubFilter.
            /// Standard shall be the name of the built-in password-based security handler. Names for other
            /// security handlers may be registered by using the procedure described in Annex E, "Extending PDF".
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string Filter = "/Filter";

            /// <summary>
            /// (Optional; PDF 1.3) A name that completely specifies the format and interpretation of
            /// the contents of the encryption dictionary. It allows security handlers other
            /// than the one specified by Filter to decrypt the document. If this entry is absent, other
            /// security handlers shall not decrypt the document.
            /// </summary>
            [KeyInfo("1.3", KeyType.Name | KeyType.Optional)]
            public const string SubFilter = "/SubFilter";

            /// <summary>
            /// (Required) A code specifying the algorithm to be used in encrypting
            /// and decrypting the document:
            /// 0
            ///     An algorithm that is undocumented. This value shall not be used.
            /// 1 (Deprecated in PDF 2.0)
            ///     Indicates the use of
            ///     7.6.3.2, "Algorithm 1: Encryption of data using the RC4 or AES algorithms" (deprecated in PDF 2.0)
            ///     with a file encryption key length of 40 bits.
            /// 2 (PDF 1.4; deprecated in PDF 2.0)
            ///     Indicates the use of
            ///     7.6.3.2, "Algorithm 1: Encryption of data using the RC4 or AES algorithms" (deprecated in PDF 2.0)
            ///     but permitting file encryption key lengths greater than 40 bits.
            /// 3 (PDF 1.4; deprecated in PDF 2.0)
            ///     An unpublished algorithm that permits file encryption key lengths ranging from 40 to 128 bits.
            ///     This value shall not appear in a conforming PDF file.
            /// 4 (PDF 1.5; deprecated in PDF 2.0)
            ///     The security handler defines the use of encryption and decryption in the document, using
            ///     the rules specified by the CF, StmF, and StrF entries using
            ///     7.6.3.2, "Algorithm 1: Encryption of data using the RC4 or AES algorithms" (deprecated in PDF 2.0)
            ///     with a file encryption key length of 128 bits.
            /// 5 (PDF 2.0)
            ///     The security handler defines the use of encryption and decryption in the document, using
            ///     the rules specified by the CF, StmF, StrF and EFF entries using
            ///     7.6.3.3, "Algorithm 1.A: Encryption of data using the AES algorithms"
            ///     with a file encryption key length of 256 bits.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string V = "/V";

            /// <summary>
            /// (Optional; PDF 1.4; only if V is 2 or 3; deprecated in PDF 2.0) The length of the file encryption key, in bits.
            /// The value shall be a multiple of 8, in the range 40 to 128. Default value: 40.
            /// </summary>
            [KeyInfo("1.4", KeyType.Integer | KeyType.Optional)]
            public const string Length = "/Length";

            /// <summary>
            /// (Optional; meaningful only when the value of V is 4 (PDF 1.5) or 5 (PDF 2.0))
            /// A dictionary whose keys shall be crypt filter names and whose values shall be the corresponding
            /// crypt filter dictionaries. Every crypt filter used in the document shall have an entry
            /// in this dictionary, except for the standard crypt filter names.
            /// Any keys in the CF dictionary that are listed standard crypt filter names
            /// shall be ignored by a PDF processor. Instead, the PDF processor shall use properties of the
            /// respective standard crypt filters.
            /// </summary>
            [KeyInfo("1.5", KeyType.Dictionary | KeyType.Optional, typeof(PdfCryptFilters))]
            public const string CF = "/CF";

            /// <summary>
            /// (Optional; meaningful only when the value of V is 4 (PDF 1.5) or 5 (PDF 2.0))
            /// The name of the crypt filter that shall be used by default when decrypting streams.
            /// The name shall be a key in the CF dictionary or a standard crypt filter name. All streams
            /// in the document, except for cross-reference streams or streams that have a Crypt entry in
            /// their Filter array, shall be decrypted by the security handler, using this crypt filter.
            /// Default value: Identity.
            /// </summary>
            [KeyInfo("1.5", KeyType.Name | KeyType.Optional)]
            public const string StmF = "/StmF";

            /// <summary>
            /// (Optional; meaningful only when the value of V is 4 (PDF 1.5) or 5 (PDF 2.0))
            /// The name of the crypt filter that shall be used when decrypting all strings in the document.
            /// The name shall be a key in the CF dictionary or a standard crypt filter name.
            /// Default value: Identity.
            /// </summary>
            [KeyInfo("1.5", KeyType.Name | KeyType.Optional)]
            public const string StrF = "/StrF";

            /// <summary>
            /// (Optional; meaningful only when the value of V is 4 (PDF 1.6) or 5 (PDF 2.0))
            /// The name of the crypt filter that shall be used when encrypting embedded
            /// file streams that do not have their own crypt filter specifier;
            /// it shall correspond to a key in the CF dictionary or a standard crypt
            /// filter name. This entry shall be provided by the security handler. PDF writers shall respect
            /// this value when encrypting embedded files, except for embedded file streams that have
            /// their own crypt filter specifier. If this entry is not present, and the embedded file
            /// stream does not contain a crypt filter specifier, the stream shall be encrypted using
            /// the default stream crypt filter specified by StmF.
            /// </summary>
            [KeyInfo("1.6", KeyType.Name | KeyType.Optional)]
            public const string EFF = "/EFF";
        }
    }
}
