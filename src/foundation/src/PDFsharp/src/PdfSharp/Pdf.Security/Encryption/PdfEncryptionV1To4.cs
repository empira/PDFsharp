// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Internal;
using System.Security.Cryptography;
using PdfSharp.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Security.Encryption
{
    /// <summary>
    /// Implements StandardSecurityHandler's encryption versions 1, 2, and 4 (3 shall not appear in conforming files).
    /// </summary>
    class PdfEncryptionV1To4 : PdfEncryptionBase
    {
        public PdfEncryptionV1To4(PdfStandardSecurityHandler securityHandler) : base(securityHandler)
        { }

        /// <summary>
        /// Encrypts with RC4 and a file encryption key length of 40 bits.
        /// </summary>
        public void SetEncryptionToV1()
        {
            Initialize(1);
            SecurityHandler.RemoveCryptFilters();
            SecurityHandler._document.SetRequiredVersion(12);
        }

        /// <summary>
        /// Encrypts with RC4 and a file encryption key length of more than 40 bits (PDF 1.4).
        /// </summary>
        /// <param name="length">The file encryption key length - a multiple of 8 from 40 to 128 bit.</param>
        public void SetEncryptionToV2(int length = 40)
        {
            Initialize(2, length);
            SecurityHandler.RemoveCryptFilters();
            SecurityHandler._document.SetRequiredVersion(14);
        }

        /// <summary>
        /// Encrypts with RC4 and a file encryption key length of 128 bits using a crypt filter (PDF 1.5).
        /// </summary>
        /// <param name="encryptMetadata">The document metadata stream shall be encrypted (default: true).</param>
        // ReSharper disable once InconsistentNaming
        public void SetEncryptionToV4UsingRC4(bool encryptMetadata = true)
        {
            Initialize(4, null, encryptMetadata);
            SecurityHandler.GetOrAddStandardCryptFilter().SetEncryptionToRC4ForV4(ActualLength!.Value);
            SecurityHandler._document.SetRequiredVersion(15);
        }

        /// <summary>
        /// Encrypts with AES and a file encryption key length of 128 bits using a crypt filter (PDF 1.6).
        /// </summary>
        /// <param name="encryptMetadata">The document metadata stream shall be encrypted (default: true).</param>
        public void SetEncryptionToV4UsingAES(bool encryptMetadata = true)
        {
            Initialize(4, null, encryptMetadata);
            SecurityHandler.GetOrAddStandardCryptFilter().SetEncryptionToAESForV4();
            SecurityHandler._document.SetRequiredVersion(16);
        }

        /// <summary>
        /// Initializes the PdfEncryptionV1To4 with the values that were saved in the security handler.
        /// </summary>
        public override void InitializeFromLoadedSecurityHandler()
        {
            VersionValue = SecurityHandler.Elements.GetInteger(PdfSecurityHandler.Keys.V);
            RevisionValue = SecurityHandler.Elements.GetInteger(PdfStandardSecurityHandler.Keys.R);
            LengthValue = SecurityHandler.Elements.ContainsKey(PdfSecurityHandler.Keys.Length) ? SecurityHandler.Elements.GetInteger(PdfSecurityHandler.Keys.Length) : null;

            UpdateActualLength();

            EncryptMetadata = (SecurityHandler.Elements[PdfStandardSecurityHandler.Keys.EncryptMetadata] as PdfBoolean)?.Value ?? true; // GetBoolean() returns false if not existing, but default is true.


            CheckVersionAndLength(VersionValue, LengthValue);

            var calculatedRevision = CalculateRevisionValue(VersionValue.Value, SecurityHandler.GetCorrectedPermissionsValue());
            if (calculatedRevision != RevisionValue)
                Debug.Assert(calculatedRevision == RevisionValue);
        }

        void Initialize(int versionValue, int? lengthValue = null, bool encryptMetadata = true)
        {
            CheckVersionAndLength(versionValue, lengthValue);

            VersionValue = versionValue;
            RevisionValue = null; // Revision is calculated later in PrepareEncryptionForSaving().
            LengthValue = lengthValue;

            UpdateActualLength();

            EncryptMetadata = encryptMetadata;
        }

        void UpdateActualLength()
        {
            ActualLength = VersionValue switch
            {
                1 => 40,
                2 => LengthValue ?? 40, // The default for Length value is 40.
                4 => 128,
                _ => throw TH.InvalidOperationException_InvalidVersionValueForEncryptionVersion1To4()
            };
        }

        static void CheckVersionAndLength(int? versionValue, int? lengthValue)
        {
            if (!IsVersionSupported(versionValue))
                throw TH.InvalidOperationException_InvalidVersionValueForEncryptionVersion1To4();

            if (versionValue == 2) // Length is only needed for V2.
            {
                if (lengthValue is null or < 40 or > 128 || lengthValue % 8 > 0)
                    throw TH.InvalidOperationException_InvalidKeyLengthForEncryptionVersion2();
            }
        }

        public static bool IsVersionSupported(int? versionValue)
        {
            return versionValue is 1 or 2 or 4;
        }

        void EnsureIsAESSupported()
        {
            if (VersionValue is not 4)
                throw TH.InvalidOperationException_AESNotSupportedForChosenEncryptionVersion();
        }


        /// <summary>
        /// Sets the encryption dictionary's values for saving.
        /// </summary>
        public override void PrepareEncryptionForSaving(string userPassword, string ownerPassword)
        {
            CheckVersionAndLength(VersionValue, LengthValue);

            SecurityHandler.Elements.SetInteger(PdfSecurityHandler.Keys.V, VersionValue!.Value);
            if (LengthValue.HasValue)
                SecurityHandler.Elements.SetInteger(PdfSecurityHandler.Keys.Length, LengthValue.Value);
            else
                SecurityHandler.Elements.Remove(PdfSecurityHandler.Keys.Length);

            var permissionsValue = SecurityHandler.GetCorrectedPermissionsValue();
            SecurityHandler.Elements.SetInteger(PdfStandardSecurityHandler.Keys.P, (int)permissionsValue);

            RevisionValue = CalculateRevisionValue(VersionValue.Value, permissionsValue);
            SecurityHandler.Elements.SetInteger(PdfStandardSecurityHandler.Keys.R, RevisionValue.Value);

            if (VersionValue == 4)
            {
                // Only write if differing from default value (true).
                if (!EncryptMetadata)
                {
                    SecurityHandler.Elements.SetBoolean(PdfStandardSecurityHandler.Keys.EncryptMetadata, EncryptMetadata);

                    var metadata = SecurityHandler._document.Catalog.Elements.GetDictionary(PdfCatalog.Keys.Metadata);
                    if (metadata is null)
                        throw TH.InvalidOperationException_CouldNotFindMetadataDictionary();

                    SecurityHandler.SetIdentityCryptFilter(metadata);
                }
            }


            // Use user password twice if no owner password provided.
            if (String.IsNullOrEmpty(ownerPassword))
                ownerPassword = userPassword;

            Debug.Assert(ownerPassword.Length > 0, "Empty owner password.");

            var documentId = PdfEncoders.RawEncoding.GetBytes(SecurityHandler._document.Internals.FirstDocumentID);

            var (userValueArray, ownerValueArray) = ComputeOwnerAndUserValues(userPassword, ownerPassword, documentId, permissionsValue);

            var userValue = PdfEncoders.RawEncoding.GetString(userValueArray, 0, userValueArray.Length);
            SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.U, userValue);

            var ownerValue = PdfEncoders.RawEncoding.GetString(ownerValueArray, 0, ownerValueArray.Length);
            SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.O, ownerValue);
        }

        /// <summary>
        /// Calculates the Revision from the set version and permission values.
        /// </summary>
        /// <returns></returns>
        int CalculateRevisionValue(int versionValue, uint permissionsValue)
        {
            var revisionValue = versionValue switch
            {
                1 => PdfSecuritySettings.HasPermissionsOfRevision3OrHigherSetTo0(permissionsValue) ? 3 : 2,
                2 => 3,
                4 => 4,
                _ => throw TH.InvalidOperationException_InvalidVersionValueForEncryptionVersion1To4()
            };
            return revisionValue;
        }

        /// <summary>
        /// Pads a password to a 32 byte array.
        /// </summary>
        static byte[] PadPassword(string password)
        {
            var padded = new byte[32];
            var length = password.Length;

            Array.Copy(PdfEncoders.RawEncoding.GetBytes(password), 0, padded, 0, Math.Min(length, 32));
            if (length < 32)
                Array.Copy(PasswordPadding, 0, padded, length, 32 - length);

            return padded;
        }

        static readonly byte[] PasswordPadding = // 32 bytes password padding defined by Adobe
        {
            0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08,
            0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A,
        };

        /// <summary>
        /// Computes the user and owner values.
        /// </summary>
        (Byte[] userValue, Byte[] ownerValue) ComputeOwnerAndUserValues(string userPassword, string ownerPassword, byte[] documentId, uint permissionsValue)
        {
            var userPad = PadPassword(userPassword);
            var ownerPad = PadPassword(ownerPassword);

            _md5.Initialize();
            var ownerValue = ComputeOwnerValue(userPad, ownerPad);
            var userValue = ComputeUserValue(documentId, userPassword, ownerValue, permissionsValue);

            return (userValue, ownerValue);
        }

        /// <summary>
        /// Computes owner value.
        /// </summary>
        byte[] ComputeOwnerValue(byte[] userPad, byte[] ownerPad)
        {
            // Copy userPad to ownerValue to do the encryption with.
            var ownerValue = new byte[32];
            Array.Copy(userPad, 0, ownerValue, 0, 32);

            var hash = _md5.ComputeHash(ownerPad);

            if (RevisionValue >= 3)
            {
                // The encryption key length (in bytes) shall depend on the Length value (in bits).
                var keyLength = ActualLength!.Value / 8;

                // Hash the pad 50 times
                for (var idx = 0; idx < 50; idx++)
                {
                    // Only hash the first bytes according to keyLength. This is not documented in the PDF reference, but seems to be necessary to compute the owner value correctly for key lengths differing from 128 bit.
                    hash = _md5.ComputeHash(hash, 0, keyLength);
                }

                var newHash = new byte[keyLength];

                // Create encryption key from hash XOR i and encrypt the owner value 20 times.
                for (var i = 0; i < 20; i++)
                {
                    for (var j = 0; j < newHash.Length; ++j)
                        newHash[j] = (byte)(hash[j] ^ i);
                    _rc4.SetKey(newHash);
                    _rc4.Encrypt(ownerValue);
                }
            }
            else
            {
                // Create encryption key from hash with a length of 5 and encrypt the owner value.
                _rc4.SetKey(hash, 5);
                _rc4.Encrypt(ownerValue);
            }

            return ownerValue;
        }

        /// <summary>
        /// Computes the user value and _encryptionKey.
        /// </summary>
        byte[] ComputeUserValue(byte[] documentId, string userPassword, byte[] ownerValue, uint permissions)
        {
            ComputeAndStoreEncryptionKey(documentId, PadPassword(userPassword), ownerValue, permissions);
            return ComputeUserValueByEncryptionKey(documentId);
        }

        /// <summary>
        /// Computes the user value using _encryptionKey.
        /// </summary>
        byte[] ComputeUserValueByEncryptionKey(byte[] documentId)
        {
            var userValue = new byte[32];

            if (RevisionValue >= 3)
            {
                _md5.Initialize();

                _md5.TransformBlock(PasswordPadding, 0, PasswordPadding.Length, PasswordPadding, 0);

                _md5.TransformFinalBlock(documentId, 0, documentId.Length);

                var hash = _md5.Hash!;

                // The actual used content length of the user value shall be 16. Only work with these bytes for creating the user value.
                const int userKeyContentLength = 16;

                Array.Copy(hash, 0, userValue, 0, userKeyContentLength);

                // The other bytes shall be padded with arbitrary content. We add zeros here.
                for (var idx = userKeyContentLength; idx < userValue.Length; idx++)
                    userValue[idx] = 0;

                // Create encryption key with the specified length.
                var keyLength = ActualLength!.Value / 8;
                Debug.Assert(keyLength == _globalEncryptionKey.Length);
                var encryptionKey = new Byte[keyLength];

                // Set encryption key to _globalEncryptionKey XOR i and encrypt the user value with a length of 16 20 times.
                for (var i = 0; i < 20; i++)
                {
                    for (var j = 0; j < keyLength; j++)
                        encryptionKey[j] = (byte)(_globalEncryptionKey[j] ^ i);

                    _rc4.SetKey(encryptionKey);
                    _rc4.Encrypt(userValue, userKeyContentLength);
                }
            }
            else
            {
                // Create encryption key from _globalEncryptionKey and encrypt the user value.
                _rc4.SetKey(_globalEncryptionKey);
                _rc4.Encrypt(PasswordPadding, userValue);
            }

            return userValue;
        }

        /// <summary>
        /// Computes and stores the global encryption key.
        /// </summary>
        void ComputeAndStoreEncryptionKey(byte[] documentId, byte[] paddedPassword, byte[] ownerValue, uint permissions)
        {
            _md5.Initialize();

            _md5.TransformBlock(paddedPassword, 0, paddedPassword.Length, paddedPassword, 0);

            _md5.TransformBlock(ownerValue, 0, ownerValue.Length, ownerValue, 0);

            // Split permission into 4 bytes
            var permission = new byte[4];
            permission[0] = (byte)permissions;
            permission[1] = (byte)(permissions >> 8);
            permission[2] = (byte)(permissions >> 16);
            permission[3] = (byte)(permissions >> 24);
            _md5.TransformBlock(permission, 0, 4, permission, 0);

            _md5.TransformBlock(documentId, 0, documentId.Length, documentId, 0);

            if (RevisionValue >= 4 && !EncryptMetadata)
            {
                var additionalBytes = new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                _md5.TransformBlock(additionalBytes, 0, additionalBytes.Length, additionalBytes, 0);
            }

            // Finalize Hash by calling TransformFinalBlock() with an input count of 0.
            _md5.TransformFinalBlock(permission, 0, 0);

            var hash = _md5.Hash!;

            int keyLength;
            if (RevisionValue >= 3)
            {
                // The encryption and MD5 hashing key length (in bytes) shall depend on the Length value (in bits).
                keyLength = ActualLength!.Value / 8;

                // Create the hash 50 times (only for 128 bit).
                for (var idx = 0; idx < 50; idx++)
                {
                    // Only use hash with a length of keyLength.
                    hash = _md5.ComputeHash(hash, 0, keyLength);
                }
            }
            else
            {
                // The encryption key length for revision 2 shall be 5.
                keyLength = 5;
            }

            // Store global encryption key.
            _globalEncryptionKey = new byte[keyLength];
            Array.Copy(hash, 0, _globalEncryptionKey, 0, keyLength);

            // Store length for object encryption key.
            _objectEncryptionKeySize = _globalEncryptionKey.Length + 5;
            if (_objectEncryptionKeySize > 16)
                _objectEncryptionKeySize = 16;
        }


        /// <summary>
        /// Validates the password.
        /// </summary>
        public override PasswordValidity ValidatePassword(string inputPassword)
        {
            VersionValue = SecurityHandler.Elements.GetInteger(PdfSecurityHandler.Keys.V);
            LengthValue = SecurityHandler.Elements.ContainsKey(PdfSecurityHandler.Keys.Length) ? SecurityHandler.Elements.GetInteger(PdfSecurityHandler.Keys.Length) : null;
            UpdateActualLength();
            CheckVersionAndLength(VersionValue, LengthValue);

            RevisionValue = SecurityHandler.Elements.GetInteger(PdfStandardSecurityHandler.Keys.R);

            var userValue = PdfEncoders.RawEncoding.GetBytes(SecurityHandler.Elements.GetString(PdfStandardSecurityHandler.Keys.U));
            var ownerValue = PdfEncoders.RawEncoding.GetBytes(SecurityHandler.Elements.GetString(PdfStandardSecurityHandler.Keys.O));
            var permissionsValue = (uint)SecurityHandler.Elements.GetInteger(PdfStandardSecurityHandler.Keys.P);
            EncryptMetadata = (SecurityHandler.Elements[PdfStandardSecurityHandler.Keys.EncryptMetadata] as PdfBoolean)?.Value ?? true; // GetBoolean() returns false if not existing, but default is true.

            var documentId = PdfEncoders.RawEncoding.GetBytes(SecurityHandler.Owner.Internals.FirstDocumentID);

            // Try owner password first.
            if (ValidateOwnerPassword(documentId, inputPassword, userValue, ownerValue, permissionsValue))
                return PasswordValidity.OwnerPassword;

            if (ValidateUserPassword(documentId, inputPassword, userValue, ownerValue, permissionsValue))
                return PasswordValidity.UserPassword;

            return PasswordValidity.Invalid;
        }

        bool ValidateUserPassword(byte[] documentId, string inputPassword, byte[] userValue, byte[] ownerValue, uint permissionsValue)
        {
            var computedUserValue = ComputeUserValue(documentId, inputPassword, ownerValue, permissionsValue);
            return EqualsPasswordValue(userValue, computedUserValue);
        }

        bool ValidateOwnerPassword(byte[] documentId, string inputPassword, byte[] userValue, byte[] ownerValue, uint permissionsValue)
        {
            var inputPad = PadPassword(inputPassword);

            // Copy ownerValue to decryptedUserPassword to do the encryption with. User password shall be decrypted from owner value.
            var decryptedUserPassword = new byte[32];
            Array.Copy(ownerValue, 0, decryptedUserPassword, 0, 32);

            var hash = _md5.ComputeHash(inputPad);

            if (RevisionValue >= 3)
            {
                // The encryption key length (in bytes) shall depend on the Length value (in bits).
                var keyLength = ActualLength!.Value / 8;

                // Hash the pad 50 times.
                for (var idx = 0; idx < 50; idx++)
                {
                    // Only hash the first bytes according to keyLength. This is not documented in the PDF reference, but seems to be necessary to compute the owner value correctly for key lengths differing from 128 bit.
                    hash = _md5.ComputeHash(hash, 0, keyLength);
                }

                var newHash = new byte[keyLength];

                // Create encryption key from hash XOR i and encrypt the owner key 20 times.
                for (var i = 19; i >= 0; i--)
                {
                    for (var j = 0; j < newHash.Length; ++j)
                        newHash[j] = (byte)(hash[j] ^ i);
                    _rc4.SetKey(newHash);
                    _rc4.Encrypt(decryptedUserPassword);
                }
            }
            else
            {
                // Create encryption key from hash with a length of 5 and decrypt the owner key.
                _rc4.SetKey(hash, 5);
                _rc4.Encrypt(decryptedUserPassword);
            }

            var decryptedUserPasswordStr = PdfEncoders.RawEncoding.GetString(decryptedUserPassword, 0, decryptedUserPassword.Length);

            return ValidateUserPassword(documentId, decryptedUserPasswordStr, userValue, ownerValue, permissionsValue);
        }

        /// <summary>
        /// Checks whether the password values match.
        /// </summary>
        bool EqualsPasswordValue(byte[] value1, byte[] value2)
        {
            // On revision 3 or higher, only the first 16 bytes have to be compared.
            var comparisonLength = RevisionValue >= 3 ? 16 : 32;

            if (value1.Length != 32 || value2.Length != 32)
                throw TH.InvalidOperationException_InvalidPasswordKeyLengthForEncryptionVersion1To4();

            for (var idx = 0; idx < comparisonLength; idx++)
            {
                if (value1[idx] != value2[idx])
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Has to be called if a PdfObject is entered for encryption/decryption.
        /// </summary>
        public override void EnterObject(PdfObjectID id)
        {
            _objectId = id;

            // Set object encryption keys to null to enforce a new calculation later.
            _objectEncryptionKeyRC4 = null;
            _objectEncryptionKeyAES = null;
        }

        /// <summary>
        /// Should be called if a PdfObject is left from encryption/decryption.
        /// </summary>
        public override void LeaveObject()
        {
            _objectId = null;
            _objectEncryptionKeyRC4 = null;
            _objectEncryptionKeyAES = null;
        }

        /// <summary>
        /// Encrypts the given bytes for the entered indirect PdfObject.
        /// </summary>
        public override void EncryptForEnteredObject(ref byte[] bytes)
        {
            EncryptForEnteredObjectUsingRC4(ref bytes); // Security handler always uses RC4.
        }

        /// <summary>
        /// Decrypts the given bytes for the entered indirect PdfObject.
        /// </summary>
        public override void DecryptForEnteredObject(ref byte[] bytes)
        {
            DecryptForEnteredObjectUsingRC4(ref bytes); // Security handler always uses RC4.
        }

        /// <summary>
        /// Encrypts the given bytes for the entered indirect PdfObject using RC4 encryption.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void EncryptForEnteredObjectUsingRC4(ref byte[] bytes)
        {
            var objectEncryptionKey = GetObjectEncryptionKeyRC4();

            if (objectEncryptionKey is null || _objectEncryptionKeySize is 0)
                throw TH.InvalidOperationException_EncryptionKeyNotSetForEncryptionVersion1To4();


            _rc4.SetKey(objectEncryptionKey, _objectEncryptionKeySize);
            _rc4.Encrypt(bytes);
        }

        /// <summary>
        /// Decrypts the given bytes for the entered indirect PdfObject using RC4 encryption.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void DecryptForEnteredObjectUsingRC4(ref byte[] bytes)
        {
            try
            {
                EncryptForEnteredObjectUsingRC4(ref bytes); // As the RC4 encryption is symmetric, encryption and decryption is the same.
            }
            catch (CryptographicException)
            {
                if (!HandleCryptographicExceptionOnDecryption())
                    throw;
            }
        }

        /// <summary>
        /// Encrypts the given bytes for the entered indirect PdfObject using AES encryption.
        /// </summary>
        public void EncryptForEnteredObjectUsingAES(ref byte[] bytes)
        {
            EnsureIsAESSupported();

            var objectEncryptionKey = GetObjectEncryptionKeyAES();

            if (objectEncryptionKey is null || _objectEncryptionKeySize is 0)
                throw TH.InvalidOperationException_EncryptionKeyNotSetForEncryptionVersion1To4();

            // Initialize AES.
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.BlockSize = 128; // 16 bytes
            aes.KeySize = _objectEncryptionKeySize * 8;

            var iv = aes.IV;

            // Encrypt bytes and prepend the 16 byte AES initialization vector.
            using var encryptor = aes.CreateEncryptor(objectEncryptionKey, iv);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            bytes = iv.Concat(encrypted).ToArray();
        }

        /// <summary>
        /// Decrypts the given bytes for the entered indirect PdfObject using AES encryption.
        /// </summary>
        public void DecryptForEnteredObjectUsingAES(ref byte[] bytes)
        {
            try
            {
                EnsureIsAESSupported();

                var objectEncryptionKey = GetObjectEncryptionKeyAES();

                if (objectEncryptionKey is null || _objectEncryptionKeySize is 0)
                    throw TH.InvalidOperationException_EncryptionKeyNotSetForEncryptionVersion1To4();

                // Initialize AES.
                using var aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.BlockSize = 128; // 16 bytes
                aes.KeySize = _objectEncryptionKeySize * 8;

                // Read the prepended 16 byte AES initialization vector.
                if (bytes.Length < 16)
                    throw TH.CryptographicException_InputDataTooShort();
                var iv = new byte[16];
                Array.Copy(bytes, 0, iv, 0, 16);

                // Decrypt the rest of the original bytes.
                using var decryptor = aes.CreateDecryptor(objectEncryptionKey, iv);
                var decrypted = decryptor.TransformFinalBlock(bytes, 16, bytes.Length - 16);
                bytes = decrypted;
            }
            catch (CryptographicException)
            {
                if (!HandleCryptographicExceptionOnDecryption())
                    throw;
            }
        }

        /// <summary>
        /// Computes the encryption key for the current indirect PdfObject.
        /// </summary>
        byte[]? ComputeObjectEncryptionKey(bool useAES)
        {
            var id = new byte[5];

            _md5.Initialize();

            _md5.TransformBlock(_globalEncryptionKey, 0, _globalEncryptionKey.Length, _globalEncryptionKey, 0);

            // Split the object number and generation
            id[0] = (byte)_objectId!.Value.ObjectNumber;
            id[1] = (byte)(_objectId.Value.ObjectNumber >> 8);
            id[2] = (byte)(_objectId.Value.ObjectNumber >> 16);
            id[3] = (byte)_objectId.Value.GenerationNumber;
            id[4] = (byte)(_objectId.Value.GenerationNumber >> 8);

            _md5.TransformBlock(id, 0, id.Length, id, 0);

            if (useAES)
            {
                // Additional padding needed for AES encryption.
                var aesPadding = new byte[] { 0x73, 0x41, 0x6C, 0x54 }; // 'sAlT'
                var aesPadding2 = "sAlT"u8.ToArray();
                Debug.Assert(aesPadding.Length == 4);
                Debug.Assert(aesPadding2.Length == 4);
                Debug.Assert(aesPadding[0]  == aesPadding2[0]);
                Debug.Assert(aesPadding[1]  == aesPadding2[1]);
                Debug.Assert(aesPadding[2]  == aesPadding2[2]);
                Debug.Assert(aesPadding[3]  == aesPadding2[3]);
                _md5.TransformBlock(aesPadding, 0, aesPadding.Length, aesPadding, 0);
            }

            // Finalize Hash by calling TransformFinalBlock() with an input count of 0.
            _md5.TransformFinalBlock(id, 0, 0);

            return _md5.Hash;
        }


        /// <summary>
        /// The global encryption key.
        /// </summary>
        byte[] _globalEncryptionKey = null!;

        /// <summary>
        /// The current indirect PdfObject ID.
        /// </summary>
        PdfObjectID? _objectId;

        /// <summary>
        /// Gets the RC4 encryption key for the current indirect PdfObject.
        /// </summary>
        byte[]? GetObjectEncryptionKeyRC4() => _objectEncryptionKeyRC4 ??= ComputeObjectEncryptionKey(false);
        byte[]? _objectEncryptionKeyRC4;

        /// <summary>
        /// The AES encryption key for the current indirect PdfObject.
        /// </summary>
        byte[]? GetObjectEncryptionKeyAES() => _objectEncryptionKeyAES ??= ComputeObjectEncryptionKey(true);
        byte[]? _objectEncryptionKeyAES;

        /// <summary>
        /// The encryption key length for the current indirect PdfObject.
        /// </summary>
        int _objectEncryptionKeySize;

        /// <summary>
        /// The message digest algorithm MD5.
        /// </summary>
        readonly MD5 _md5 = MD5.Create();

        /// <summary>
        /// The RC4 encryption algorithm.
        /// </summary>
        readonly RC4 _rc4 = new();
    }
}
