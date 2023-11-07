// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;
using PdfSharp.Pdf.Security.Encryption;

namespace PdfSharp.Pdf.Security
{
    /// <summary>
    /// Represents a crypt filter dictionary as written into the CF dictionary of a security handler (PDFCryptFilters).
    /// </summary>
    public class PdfCryptFilter : CryptFilterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfCryptFilter"/> class.
        /// </summary>
        /// <param name="parentStandardSecurityHandler">The parent standard security handler.</param>
        public PdfCryptFilter(PdfStandardSecurityHandler? parentStandardSecurityHandler)
        {
            Initialize(parentStandardSecurityHandler);
            _parentStandardSecurityHandler?._document.SetRequiredVersion(15);
        }

        internal PdfCryptFilter(PdfDictionary dict) : base(dict)
        { }

        /// <summary>
        /// Initializes _parentStandardSecurityHandler to do the correct interpretation of the length key.
        /// </summary>
        public void Initialize(PdfStandardSecurityHandler? parentStandardSecurityHandler)
        {
            _parentStandardSecurityHandler = parentStandardSecurityHandler;
        }

        /// <summary>
        /// The encryption shall be handled by the security handler.
        /// </summary>
        public void SetEncryptionToNone()
        {
            Initialize(CryptFilterMethod.None);
        }

        /// <summary>
        /// Encrypt with RC4 and the given file encryption key length, using the algorithms of encryption V4 (PDF 1.5).
        /// For encryption V4 the file encryption key length shall be 128 bits.
        /// </summary>
        /// <param name="length">The file encryption key length - a multiple of 8 from 40 to 256 bit.</param>
        // ReSharper disable once InconsistentNaming
        public void SetEncryptionToRC4ForV4(int length = 128)
        {
            Initialize(CryptFilterMethod.V2, length);
        }

        /// <summary>
        /// Encrypt with AES and a file encryption key length of 128 bits, using the algorithms of encryption V4 (PDF 1.6).
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void SetEncryptionToAESForV4()
        {
            Initialize(CryptFilterMethod.AESV2, 128);
            _parentStandardSecurityHandler?._document.SetRequiredVersion(16);
        }

        /// <summary>
        /// Encrypt with AES and a file encryption key length of 256 bits, using the algorithms of encryption V5 (PDF 2.0).
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void SetEncryptionToAESForV5()
        {
            Initialize(CryptFilterMethod.AESV3, 256);
            _parentStandardSecurityHandler?._document.SetRequiredVersion(20);
        }

        void Initialize(CryptFilterMethod method, int lengthValue = 40)
        {
            CheckLength(method, lengthValue);

            Elements.SetName(Keys.Type, "/CryptFilter");
            Elements.SetName(Keys.AuthEvent, "/DocOpen");

            SetCryptFilterMethod(method);

            // The standard security handler expresses the Length entry in bytes.
            // Actually the length key is only checked for validity and written - crypt filter delegates the reading to the security handler via PdfEncryptionBase and uses its length key, if needed.
            if (_parentStandardSecurityHandler is not null)
                lengthValue /= 8;
            Elements.SetInteger(Keys.Length, lengthValue);
        }
        
        static void CheckLength(CryptFilterMethod method, int lengthValue)
        {
            switch (method)
            {
                case CryptFilterMethod.None:
                    // For None (redirect to Security Handler), Length should be irrelevant. So we don't check it here.
                    break;
                case CryptFilterMethod.V2:
                    if (lengthValue is < 40 or > 256 || lengthValue % 8 > 0)
                        throw TH.InvalidOperationException_InvalidLengthValueForCryptFilterMethodV2();
                    break;
                case CryptFilterMethod.AESV2:
                    if (lengthValue is not 128)
                        throw TH.InvalidOperationException_InvalidLengthValueForCryptFilterMethodAESV2();
                    break;
                case CryptFilterMethod.AESV3:
                    if (lengthValue is not 256)
                        throw TH.InvalidOperationException_InvalidLengthValueForCryptFilterMethodAESV3();
                    break;
                default:
                    throw TH.InvalidOperationException_InvalidCryptFilterMethod();
            }
        }

        /// <summary>
        /// Sets the AuthEvent Key to /EFOpen, in order authenticate embedded file streams when accessing the embedded file.
        /// </summary>
        public void SetAuthEventForEmbeddedFiles()
        {
            Elements.SetName(Keys.AuthEvent, "/EFOpen");
        }

        /// <summary>
        /// Does all necessary initialization for reading and decrypting or encrypting and writing the document with this crypt filter.
        /// </summary>
        internal void PrepareForProcessing()
        {
            // Cache crypt filter method and encryption.
            GetCryptFilterMethod();
            _encryption = _parentStandardSecurityHandler!.GetEncryption();
            _encryptionV1To4 = _encryption as PdfEncryptionV1To4;
            _encryptionV5 = _encryption as PdfEncryptionV5;

            // Check configuration.
            if (_encryption.RevisionValue == null)
                throw TH.InvalidOperationException_EncryptionRevisionValueNotYetCalculated();
            if (_encryption.RevisionValue is not (4 or 6))
                throw TH.InvalidOperationException_CryptFiltersNotSupportedForChosenEncryptionRevision();
            if (_encryption.RevisionValue == 4 && _cryptFilterMethod is not (CryptFilterMethod.V2 or CryptFilterMethod.AESV2))
                throw TH.InvalidOperationException_InvalidCryptFilterMethodForEncryptionRevision4();
            if (_encryption.RevisionValue == 6 && _cryptFilterMethod is not CryptFilterMethod.AESV3)
                throw TH.InvalidOperationException_InvalidCryptFilterMethodForEncryptionRevision6();
        }

        /// <summary>
        /// Encrypts the given bytes.
        /// </summary>
        /// <returns>True, if the crypt filter encrypted the bytes, or false, if the security handler shall do it.</returns>
        internal override bool EncryptForEnteredObject(ref byte[] bytes)
        {
            // V2, AESV2 and AESV3 all shall ask the security handler for the encryption key and use an encryption supported by the chosen encryption version:
            // - Version 4 => Revision 4 => Method V2 or AESV2
            // - Version 5 => Revision 6 => Method AESV3
            // For that reason there should be no problem in using the PdfEncryptionBase instance of the security handler instead of a own instance.
            // If the crypt filter really needs its own PdfEncryptionBase instance, it had to be made independent of the security handler.
            
            switch (_cryptFilterMethod)
            {
                case CryptFilterMethod.None:
                    // The application shall not encrypt data but shall direct the input stream to the security handler for encryption.
                    return false;
                case CryptFilterMethod.V2:
                    // The application shall ask the security handler for the file encryption key and shall implicitly encrypt data with
                    // 7.6.3.2, "Algorithm 1: Encryption of data using the RC4 or AES algorithms", using the RC4 algorithm.
                    _encryptionV1To4!.EncryptForEnteredObjectUsingRC4(ref bytes);
                    return true;
                case CryptFilterMethod.AESV2:
                    // The application shall ask the security handler for the file encryption key and shall implicitly encrypt data with
                    // 7.6.3.2, "Algorithm 1: Encryption of data using the RC4 or AES algorithms", using the AES algorithm.
                    _encryptionV1To4!.EncryptForEnteredObjectUsingAES(ref bytes);
                    return true;
                case CryptFilterMethod.AESV3:
                    // The application shall ask the security handler for the file encryption key and shall implicitly encrypt data with
                    // 7.6.3.3, "Algorithm 1.A: Encryption of data using the AES algorithms", using the AES-256 algorithm
                    // in Cipher Block Chaining (CBC) with padding mode with a 16-byte block size and an initialization vector that is
                    // randomly generated and placed as the first 16 bytes in the stream or string. The key size (Length) shall be 256 bits.
                    _encryptionV5!.EncryptForEnteredObject(ref bytes);
                    return true;
                default:
                    throw TH.InvalidOperationException_InvalidCryptFilterMethod();
            }
        }

        /// <summary>
        /// Decrypts the given bytes.
        /// </summary>
        /// <returns>True, if the crypt filter decrypted the bytes, or false, if the security handler shall do it.</returns>
        internal override bool DecryptForEnteredObject(ref byte[] bytes)
        {
            // Pendant to EncryptForEnteredObject().
            
            switch (_cryptFilterMethod)
            {
                case CryptFilterMethod.None:
                    // The application shall not decrypt data but shall direct the input stream to the security handler for decryption.
                    return false;
                case CryptFilterMethod.V2:
                    // The application shall ask the security handler for the file encryption key and shall implicitly decrypt data with
                    // 7.6.3.2, "Algorithm 1: Encryption of data using the RC4 or AES algorithms", using the RC4 algorithm.
                    _encryptionV1To4!.DecryptForEnteredObjectUsingRC4(ref bytes);
                    return true;
                case CryptFilterMethod.AESV2:
                    // The application shall ask the security handler for the file encryption key and shall implicitly decrypt data with
                    // 7.6.3.2, "Algorithm 1: Encryption of data using the RC4 or AES algorithms", using the AES algorithm.
                    _encryptionV1To4!.DecryptForEnteredObjectUsingAES(ref bytes);
                    return true;
                case CryptFilterMethod.AESV3:
                    // The application shall ask the security handler for the file encryption key and shall implicitly decrypt data with
                    // 7.6.3.3, "Algorithm 1.A: Encryption of data using the AES algorithms", using the AES-256 algorithm
                    // in Cipher Block Chaining (CBC) with padding mode with a 16-byte block size and an initialization vector that is
                    // randomly generated and placed as the first 16 bytes in the stream or string. The key size (Length) shall be 256 bits.
                    _encryptionV5!.DecryptForEnteredObject(ref bytes);
                    return true;
                default:
                    throw TH.InvalidOperationException_InvalidCryptFilterMethod();
            }
        }

        void SetCryptFilterMethod(CryptFilterMethod cryptFilterMethod)
        {
            _cryptFilterMethod = cryptFilterMethod;
            Elements.SetName(Keys.CFM, Enum.GetName(typeof(CryptFilterMethod), cryptFilterMethod) ?? throw TH.InvalidOperationException_InvalidCryptFilterMethod());
        }

        CryptFilterMethod GetCryptFilterMethod()
        {
            _cryptFilterMethod ??= (CryptFilterMethod)Enum.Parse(typeof(CryptFilterMethod), PdfName.RemoveSlash(Elements.GetName(Keys.CFM)));
            return _cryptFilterMethod ?? CryptFilterMethod.None;
        }
        
        CryptFilterMethod? _cryptFilterMethod;
        PdfEncryptionBase? _encryption;
        PdfEncryptionV1To4? _encryptionV1To4;
        PdfEncryptionV5? _encryptionV5;

        PdfStandardSecurityHandler? _parentStandardSecurityHandler;

        /// <summary>
        /// The crypt filter method to choose in the CFM key.
        /// </summary>
        public enum CryptFilterMethod
        {
            /// <summary>
            /// The encryption shall be handled by the security handler.
            /// </summary>
            None,

            /// <summary>
            /// Encrypt with RC4. Used for encryption Version 4.
            /// </summary>
            V2,

            /// <summary>
            /// Encrypt with AES and a file encryption key length of 128. Used for encryption Version 4.
            /// </summary>
            AESV2,

            /// <summary>
            /// Encrypt with AES and a file encryption key length of 256. Used for encryption Version 5.
            /// </summary>
            AESV3
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal class Keys : KeysBase
        {
            /// <summary>
            /// (Optional) If present, shall be CryptFilter for a crypt filter dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Type = "/Type";

            /// <summary>
            /// (Optional) The method used, if any, by the PDF reader to decrypt data.
            /// The following values shall be supported:
            /// None
            ///     The application shall not decrypt data but shall direct the input stream to the security handler for decryption.
            /// V2 (Deprecated in PDF 2.0)
            ///     The application shall ask the security handler for the file encryption key and shall implicitly decrypt data with
            ///     7.6.3.2, "Algorithm 1: Encryption of data using the RC4 or AES algorithms",
            ///     using the RC4 algorithm.
            /// AESV2 (PDF 1.6; deprecated in PDF 2.0)
            ///     The application shall ask the security handler for the file encryption key and shall implicitly decrypt data with
            ///     7.6.3.2, "Algorithm 1: Encryption of data using the RC4 or AES algorithms",
            ///     using the AES algorithm in Cipher Block Chaining (CBC) mode with a 16-byte block size and an
            ///     initialization vector that shall be randomly generated and placed as the first 16 bytes in the stream or string.
            ///     The key size (Length) shall be 128 bits.
            /// AESV3 (PDF 2.0)
            ///     The application shall ask the security handler for the file encryption key and shall implicitly decrypt data with
            ///     7.6.3.3, "Algorithm 1.A: Encryption of data using the AES algorithms",
            ///     using the AES-256 algorithm in Cipher Block Chaining (CBC) with padding mode with a 16-byte block size and an
            ///     initialization vector that is randomly generated and placed as the first 16 bytes in the stream or string.
            ///     The key size (Length) shall be 256 bits.
            /// When the value is V2, AESV2 or AESV3, the application may ask once for this file encryption key and cache the key
            /// for subsequent use for streams that use the same crypt filter. Therefore, there shall be a one-to-one relationship
            /// between a crypt filter name and the corresponding file encryption key.
            /// Only the values listed here shall be supported. Applications that encounter other values shall report that the file
            /// is encrypted with an unsupported algorithm.
            /// Default value: None.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            // ReSharper disable once InconsistentNaming
            public const string CFM = "/CFM";

            /// <summary>
            /// (Optional) The event that shall be used to trigger the authorization that is required to access file encryption keys
            /// used by this filter. If authorization fails, the event shall fail. Valid values shall be:
            /// DocOpen
            ///     Authorization shall be required when a document is opened.
            /// EFOpen
            ///     Authorization shall be required when accessing embedded files.
            /// Default value: DocOpen.
            /// If this filter is used as the value of StrF or StmF in the encryption dictionary,
            /// the PDF reader shall ignore this key and behave as if the value is DocOpen.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string AuthEvent = "/AuthEvent";

            /// <summary>
            /// (Required; deprecated in PDF 2.0) Security handlers may define their own use of the Length entry and
            /// should use it to define the bit length of the file encryption key. The bit length of the file encryption key
            /// shall be a multiple of 8 in the range of 40 to 256. The standard security handler expresses the Length entry
            /// in bytes (e.g., 32 means a length of 256 bits) and public-key security handlers express it as is
            /// (e.g., 256 means a length of 256 bits).
            /// When CFM is AESV2, the Length key shall have the value of 128.
            /// When CFM is AESV3, the Length key shall have a value of 256.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string Length = "/Length";
        }
    }
}
