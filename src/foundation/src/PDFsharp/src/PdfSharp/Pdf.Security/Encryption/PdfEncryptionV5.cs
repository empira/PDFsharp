// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Numerics;
using PdfSharp.Pdf.IO;
using System.Security.Cryptography;
using System.Text;
using PdfSharp.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.Security.Encryption
{
    /// <summary>
    /// Implements StandardSecurityHandler's encryption version 5 (PDF 20).
    /// </summary>
    class PdfEncryptionV5 : PdfEncryptionBase
    {
        public PdfEncryptionV5(PdfStandardSecurityHandler securityHandler) : base(securityHandler)
        {
            SecurityHandler._document.SetRequiredVersion(20);
        }

        /// <summary>
        /// Initializes the encryption. Has to be called after the security handler's encryption has been set to this object.
        /// </summary>
        /// <param name="encryptMetadata">True, if the document metadata stream shall be encrypted (default: true).</param>
        public void Initialize(bool encryptMetadata = true)
        {
            VersionValue = 5; // Always 5 for PdfEncryptionV5.
            RevisionValue = 6; // Always 6 for PdfEncryptionV5.
            LengthValue = null; // Deprecated in PDF 2.0.

            ActualLength = 256; // Always 256 for PdfEncryptionV5.

            EncryptMetadata = encryptMetadata;

            SecurityHandler.GetOrAddStandardCryptFilter().SetEncryptionToAESForV5();
        }

        /// <summary>
        /// Initializes the PdfEncryptionV5 with the values that were saved in the security handler.
        /// </summary>
        public override void InitializeFromLoadedSecurityHandler()
        {
            var encryptMetadata = (SecurityHandler.Elements[PdfStandardSecurityHandler.Keys.EncryptMetadata] as PdfBoolean)?.Value ?? true; // GetBoolean() returns false if not existing, but default is true.
            Initialize(encryptMetadata);
        }

        static void CheckValues(int? versionValue, int? revisionValue, int? lengthValue)
        {
            if (versionValue is not 5)
                throw TH.InvalidOperationException_InvalidVersionValueForEncryptionVersion5();

            if (revisionValue is not 6)
                throw TH.InvalidOperationException_InvalidRevisionValueForEncryptionVersion5();

            if (lengthValue is not (null or 256))
                throw TH.InvalidOperationException_InvalidLengthValueForEncryptionVersion5();
        }

        public static bool IsVersionSupported(int? versionValue)
        {
            return versionValue is 5;
        }

        /// <summary>
        /// Has to be called if a PdfObject is entered for encryption/decryption.
        /// </summary>
        public override void EnterObject(PdfObjectID id)
        {
            // Used encryption key doesn't depend on PDFObject ID, so there is nothing to do.
        }

        /// <summary>
        /// Should be called if a PdfObject is left from encryption/decryption.
        /// </summary>
        public override void LeaveObject()
        {
            // Used encryption key doesn't depend on PDFObject ID, so there is nothing to do.
        }

        /// <summary>
        /// Encrypts the given bytes for the entered indirect PdfObject.
        /// </summary>
        public override void EncryptForEnteredObject(ref byte[] bytes)
        {
            using var aes = CreateAesForObjectsCryptography();
            var iv = aes.IV;

            // Encrypt bytes and prepend the 16 byte AES initialization vector.
            using var encryptor = aes.CreateEncryptor(_encryptionKey, iv);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            bytes = aes.IV.Concat(encrypted).ToArray();
        }

        /// <summary>
        /// Decrypts the given bytes for the entered indirect PdfObject.
        /// </summary>
        public override void DecryptForEnteredObject(ref byte[] bytes)
        {
            try
            {
                using var aes = CreateAesForObjectsCryptography();

                // Read the prepended 16 byte AES initialization vector.
                if (bytes.Length < 16)
                    throw TH.CryptographicException_InputDataTooShort();
                var iv = new byte[16];
                Array.Copy(bytes, 0, iv, 0, 16);

                // Decrypt the rest of the original bytes.
                using var decryptor = aes.CreateDecryptor(_encryptionKey, iv);
                var decrypted = decryptor.TransformFinalBlock(bytes, 16, bytes.Length - 16);
                bytes = decrypted;
            }
            catch (CryptographicException)
            {
                if (!HandleCryptographicExceptionOnDecryption())
                    throw;
            }
        }

        static Aes CreateAesForObjectsCryptography()
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.BlockSize = 128; // 16 bytes
            aes.KeySize = 256;

            return aes;
        }

        static Aes CreateAesForHashCryptography()
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            aes.BlockSize = 128; // 16 bytes
            aes.KeySize = 128;

            return aes;
        }

        static Aes CreateAesForKeyCryptography()
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            aes.BlockSize = 128; // 16 bytes
            aes.KeySize = 256;

            return aes;
        }

        static Aes CreateAesForPermissionsCryptography()
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            aes.BlockSize = 128; // 16 bytes
            aes.KeySize = 256;

            return aes;
        }

        /// <summary>
        /// Sets the encryption dictionary's values for saving.
        /// </summary>
        public override void PrepareEncryptionForSaving(string userPassword, string ownerPassword)
        {
            // Ensure properties are set to the only valid values for PdfEncryptionV5.
            CheckValues(VersionValue, RevisionValue, LengthValue);

            CreateAndStoreEncryptionKey();

            SecurityHandler.Elements.SetInteger(PdfSecurityHandler.Keys.V, VersionValue!.Value);

            // In PDF reference, Length is marked as deprecated in PDF 2.0, but Adobe Reader cannot read V5 encrypted files if this key is omitted.
            SecurityHandler.Elements.SetInteger(PdfSecurityHandler.Keys.Length, ActualLength!.Value);

            var permissionsValue = SecurityHandler.GetCorrectedPermissionsValue();
            SecurityHandler.Elements.SetInteger(PdfStandardSecurityHandler.Keys.P, (int)permissionsValue);

            var permsValueArray = ComputePermsValue(permissionsValue);
            var permsValue = PdfEncoders.RawEncoding.GetString(permsValueArray, 0, permsValueArray.Length);
            SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.Perms, permsValue);

            SecurityHandler.Elements.SetInteger(PdfStandardSecurityHandler.Keys.R, RevisionValue!.Value);

            // Only write if differing from default value (true).
            if (!EncryptMetadata)
            {
                SecurityHandler.Elements.SetBoolean(PdfStandardSecurityHandler.Keys.EncryptMetadata, EncryptMetadata);

                var metadata = SecurityHandler._document.Catalog.Elements.GetDictionary(PdfCatalog.Keys.Metadata);
                if (metadata is null)
                    throw TH.InvalidOperationException_CouldNotFindMetadataDictionary();

                SecurityHandler.SetIdentityCryptFilter(metadata);
            }

            // Use user password twice if no owner password provided.
            if (String.IsNullOrEmpty(ownerPassword))
                ownerPassword = userPassword;

            Debug.Assert(ownerPassword.Length > 0, "Empty owner password.");

            var utf8userPassword = CreateUtf8Password(userPassword);
            var utf8ownerPassword = CreateUtf8Password(ownerPassword);

            var (userValueArray, userEValueArray) = ComputeUserValues(utf8userPassword);
            var (ownerValueArray, ownerEValueArray) = ComputeOwnerValues(utf8ownerPassword, userValueArray);

            var userValue = PdfEncoders.RawEncoding.GetString(userValueArray, 0, userValueArray.Length);
            SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.U, userValue);

            var userEValue = PdfEncoders.RawEncoding.GetString(userEValueArray, 0, userEValueArray.Length);
            SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.UE, userEValue);

            var ownerValue = PdfEncoders.RawEncoding.GetString(ownerValueArray, 0, ownerValueArray.Length);
            SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.O, ownerValue);

            var ownerEValue = PdfEncoders.RawEncoding.GetString(ownerEValueArray, 0, ownerEValueArray.Length);
            SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.OE, ownerEValue);
        }

        /// <summary>
        /// Creates and stores the encryption key for writing a file.
        /// </summary>
        void CreateAndStoreEncryptionKey()
        {
            // The file encryption key shall be a 256-bit (32-byte) value generated with a strong random number generator.
            _encryptionKey = new byte[32];
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                cryptoProvider.GetBytes(_encryptionKey);
            }
        }

        /// <summary>
        /// Creates the UTF-8 password.
        /// Corresponding to "7.6.4.3.3 Algorithm 2.A: Retrieving the file encryption key from
        /// an encrypted document in order to decrypt it (revision 6 and later)" steps a) - b)
        /// </summary>
        static byte[] CreateUtf8Password(string password)
        {
            // a) Generate UTF-8 password by processing password with the SASLprep profile of stringprep using the Normalize and BiDi options,
            // and then converting to a UTF-8 representation.
            string saslPrep;
            try
            {
                saslPrep = SASLprep.PrepareQuery(password);
            }
            catch (Exception e)
            {
                throw TH.ArgumentException_WrappingSASLprepException(e);
            }

            var utf8Bytes = Encoding.UTF8.GetBytes(saslPrep);

            // b) Truncate, if longer than 127 bytes.
            if (utf8Bytes.Length > 127)
                utf8Bytes = utf8Bytes.Take(127).ToArray();

            return utf8Bytes;
        }

        /// <summary>
        /// Computes userValue and userEValue.
        /// Corresponding to "7.6.4.4.7 Algorithm 8: Computing the encryption dictionary’s U (user password)
        /// and UE (user encryption) values (Security handlers of revision 6)"
        /// </summary>
        (byte[] UserValue, byte[] UserEValue) ComputeUserValues(byte[] utf8InputPassword)
        {
            // a) Generate random bytes for user validation salt and user key salt.
            byte[] validationSalt = new byte[8], 
                keySalt = new byte[8];
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {                
                cryptoProvider.GetBytes(validationSalt);
                cryptoProvider.GetBytes(keySalt);
            }

            // Compute the hash with an input string consisting of the UTF-8 password concatenated with the user validation salt.
            // The string consisting of the hash followed by the user validation salt followed by the user key salt is stored as userValue.
            var hash = ComputeUserHash(utf8InputPassword, validationSalt);
            var userValue = hash.Concat(validationSalt).Concat(keySalt).ToArray();

            // b) Compute the hash with an input string consisting of the UTF-8 password concatenated with the user key salt.
            var eHash = ComputeUserHash(utf8InputPassword, keySalt);

            using var aes = CreateAesForKeyCryptography();

            // Using this hash as the key, encrypt the file encryption key using AES-256 in CBC mode with no padding and an
            // initialization vector of zero. The resulting string is stored as userEValue.
            var ivZero = new byte[16];
            using var encryptor = aes.CreateEncryptor(eHash, ivZero);
            var userEValue = encryptor.TransformFinalBlock(_encryptionKey, 0, _encryptionKey.Length);

            return (userValue, userEValue);
        }

        /// <summary>
        /// Computes ownerValue and ownerEValue.
        /// Corresponding to "7.6.4.4.8 Algorithm 9: Computing the encryption dictionary’s O (owner password)
        /// and OE (owner encryption) values (Security handlers of revision 6)"
        /// </summary>
        (byte[] OwnerValue, byte[] OwnerEValue) ComputeOwnerValues(byte[] utf8InputPassword, byte[] userValue)
        {
            // a) Generate random bytes for owner validation salt and owner key salt.

            byte[] validationSalt = new byte[8], 
                keySalt = new byte[8];
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {                
                cryptoProvider.GetBytes(validationSalt);
                cryptoProvider.GetBytes(keySalt);
            }

            // Compute the hash with an input string consisting of the UTF-8 password concatenated with
            // the owner validation salt and the userValue.
            // The string consisting of the hash followed by the owner validation salt followed by the owner key salt is stored as ownerValue.
            var hash = ComputeOwnerHash(utf8InputPassword, validationSalt, userValue);
            var ownerValue = hash.Concat(validationSalt).Concat(keySalt).ToArray();

            // b) Compute the hash with an input string consisting of the UTF-8 password concatenated with the owner key salt and userValue.
            var eHash = ComputeOwnerHash(utf8InputPassword, keySalt, userValue);

            using var aes = CreateAesForKeyCryptography();

            // Using this hash as the key, encrypt the file encryption key using AES-256 in CBC mode with no padding and an
            // initialization vector of zero. The resulting string is stored as ownerEValue.
            var ivZero = new byte[16];
            using var encryptor = aes.CreateEncryptor(eHash, ivZero);
            var ownerEValue = encryptor.TransformFinalBlock(_encryptionKey, 0, _encryptionKey.Length);

            return (ownerValue, ownerEValue);
        }

        /// <summary>
        /// Computes the hash for a user password.
        /// Corresponding to "7.6.4.3.4 Algorithm 2.B: Computing a hash (revision 6 and later)"
        /// </summary>
        static byte[] ComputeUserHash(byte[] password, byte[] salt)
        {
            return ComputeHashInternal(password, salt, false, null);
        }

        /// <summary>
        /// Computes the hash for an owner password.
        /// Corresponding to "7.6.4.3.4 Algorithm 2.B: Computing a hash (revision 6 and later)"
        /// </summary>
        static byte[] ComputeOwnerHash(byte[] password, byte[] salt, byte[]? userValue)
        {
            return ComputeHashInternal(password, salt, true, userValue);
        }

        /// <summary>
        /// Computes the hash.
        /// Corresponding to "7.6.4.3.4 Algorithm 2.B: Computing a hash (revision 6 and later)"
        /// </summary>
        static byte[] ComputeHashInternal(byte[] password, byte[] salt, bool computeOwnerHash, byte[]? userValue)
        {
            // Take the SHA-256 hash of input and name it k.
            var input = password.Concat(salt);
            if (computeOwnerHash)
                input = input.Concat(userValue!); // Shall not be null, if computeOwnerHash is true.
            var k = SHA256.Create().ComputeHash(input.ToArray());

            var lastByteOfE = new byte();

            using var aes = CreateAesForHashCryptography();

            // Run steps (a)-(d) at least 64 times
            // and afterwards as long as the last byte of e is greater than round number - 32. e) is f) this check only.
            for (var i = 0; i < 64 || lastByteOfE > i - 32; i++)
            {
                // a): Create k1 containing 64 repetitions of input and k
                var sequenceEnumerable = password.Concat(k);
                // For computing the owner hash also concat the user value).
                if (computeOwnerHash)
                    sequenceEnumerable = sequenceEnumerable.Concat(userValue!);
                var sequence = sequenceEnumerable.ToArray();

                IEnumerable<byte> k1Enumerable = Array.Empty<byte>();
                for (var j = 0; j < 64; j++)
                    k1Enumerable = k1Enumerable.Concat(sequence);
                var k1 = k1Enumerable.ToArray();

                // b): Create e: Encrypt k1 using AES-128 (CBC, no padding), with the first 16 bytes of k as the key and the second 16 bytes of k as the initialization vector.
                var aesKey = k.Take(16).ToArray();
                var aesIV = k.Skip(16).Take(16).ToArray();
                using var encryptor = aes.CreateEncryptor(aesKey, aesIV);
                var e = encryptor.TransformFinalBlock(k1, 0, k1.Length);

                // c) + d): Take the first 16 bytes of e as an unsigned big-endian integer.
                var e16 = e.Take(16).ToArray();
                var e16BigEndianUnsigned = new BigInteger(e16.Reverse().ToArray());//, true, true);
                // Calculate the remainder of the result by modulo 3
                // and according to that result choose the SHA algorithm to calculate the new k from e.
                BigInteger.DivRem(e16BigEndianUnsigned, 3, out var remainder);
                if (remainder == 0)
                    k = SHA256.Create().ComputeHash(e);
                else if (remainder == 1)
                    k = SHA384.Create().ComputeHash(e);
                else if (remainder == 2)
                    k = SHA512.Create().ComputeHash(e);

                lastByteOfE = e.Last();
            }

            // The first 32 bytes of the final K are the output of the algorithm.
            var result = k.Take(32).ToArray();
            return result;
        }

        /// <summary>
        /// Computes permsValue.
        /// Corresponding to "Algorithm 10: Computing the encryption dictionary’s Perms (permissions) value (Security handlers of revision 6)"
        /// </summary>
        byte[] ComputePermsValue(uint pValue)
        {
            // Fill a 16-byte block;
            var perms = new byte[16];

            // a) + b) Extend pValue to 64 bits by setting the upper 32 bits to all 1’s.
            // Record the 8 bytes of permission in the bytes 0-7 of the block, low order byte first.
            perms[0] = (byte)pValue;
            perms[1] = (byte)(pValue >> 8);
            perms[2] = (byte)(pValue >> 16);
            perms[3] = (byte)(pValue >> 24);
            // Instead of extending pValue to 64 bits by setting the upper bits to 1, we add 255 to the upper bytes. The result is the same.
            for (var i = 4; i < 8; i++)
                perms[i] = 255;

            // c) Set byte 8 to the ASCII character "T" or "F" according to the EncryptMetadata boolean.
            perms[8] = (byte)(EncryptMetadata ? 'T' : 'F');

            // d) Set bytes 9-11 to the ASCII characters "a", "d", "b".
            perms[9] = (byte)'a';
            perms[10] = (byte)'d';
            perms[11] = (byte)'b';

            // e) Set bytes 12-15 to 4 bytes of random data, which will be ignored.
            var randomData = new byte[4];
            using (var cryptoProvider = new RNGCryptoServiceProvider())
                cryptoProvider.GetBytes(randomData);
            Array.Copy(randomData, 0, perms, 12, randomData.Length);

            // f) Encrypt the block using AES-256 in ECB mode with an initialization vector of zero, using the file encryption key as the key.
            using var aes = CreateAesForPermissionsCryptography();

            var ivZero = new byte[16];
            using var encryptor = aes.CreateEncryptor(_encryptionKey, ivZero);
            var permsEncrypted = encryptor.TransformFinalBlock(perms, 0, perms.Length);

            // The result is stored as the Perms string, and checked for validity when the file is opened.
            return permsEncrypted;
        }

        /// <summary>
        /// Validates the password.
        /// </summary>
        public override PasswordValidity ValidatePassword(string inputPassword)
        {
            VersionValue = SecurityHandler.Elements.GetInteger(PdfSecurityHandler.Keys.V);
            RevisionValue = SecurityHandler.Elements.GetInteger(PdfStandardSecurityHandler.Keys.R);
            LengthValue = SecurityHandler.Elements.ContainsKey(PdfSecurityHandler.Keys.Length) ? SecurityHandler.Elements.GetInteger(PdfSecurityHandler.Keys.Length) : null;

            // Ensure properties are set to the only valid values for PdfEncryptionV5.
            CheckValues(VersionValue, RevisionValue, LengthValue);

            var userValue = PdfEncoders.RawEncoding.GetBytes(SecurityHandler.Elements.GetString(PdfStandardSecurityHandler.Keys.U));
            var userEValue = PdfEncoders.RawEncoding.GetBytes(SecurityHandler.Elements.GetString(PdfStandardSecurityHandler.Keys.UE));

            var ownerValue = PdfEncoders.RawEncoding.GetBytes(SecurityHandler.Elements.GetString(PdfStandardSecurityHandler.Keys.O));
            var ownerEValue = PdfEncoders.RawEncoding.GetBytes(SecurityHandler.Elements.GetString(PdfStandardSecurityHandler.Keys.OE));

            var permissionsValue = (uint)SecurityHandler.Elements.GetInteger(PdfStandardSecurityHandler.Keys.P);
            var permsValue = PdfEncoders.RawEncoding.GetBytes(SecurityHandler.Elements.GetString(PdfStandardSecurityHandler.Keys.Perms));

            EncryptMetadata = (SecurityHandler.Elements[PdfStandardSecurityHandler.Keys.EncryptMetadata] as PdfBoolean)?.Value ?? true; // GetBoolean() returns false if not existing, but default is true.

            // 7.6.4.3.3 a) - b): Create UTF-8 password.
            var utf8InputPassword = CreateUtf8Password(inputPassword);

            RetrieveAndStoreEncryptionKey(utf8InputPassword, userValue, userEValue, ownerValue, ownerEValue);

            var result = PasswordValidity.Invalid;

            // Try owner password first.
            if (ValidateOwnerPassword(utf8InputPassword, userValue, ownerValue))
                result = PasswordValidity.OwnerPassword;
            else if (ValidateUserPassword(utf8InputPassword, userValue))
                result = PasswordValidity.UserPassword;

            // Validate permissions only if password has been correctly validated.
            // If no password has been successful validated (and thus the retrieved encryption key is wrong),
            // it doesn't make sense to react to a not correctly decrypted Perms (in PDF reference, Perms is instead already mostly validated when retrieving the encryption key).
            if (result != PasswordValidity.Invalid)
            {
                if (!ValidatePermissions(permsValue, permissionsValue))
                    throw TH.PdfReaderException_CouldNotVerifyPWithPermsKey();
            }

            return result;
        }

        /// <summary>
        /// Retrieves and stores the encryption key for reading a file.
        /// Corresponding to "7.6.4.3.3 Algorithm 2.A: Retrieving the file encryption key from
        /// an encrypted document in order to decrypt it (revision 6 and later)"
        /// </summary>
        void RetrieveAndStoreEncryptionKey(byte[] utf8InputPassword, byte[] userValue, byte[] userEValue, byte[] ownerValue, byte[] ownerEValue)
        {
            var ivZero = new byte[16];

            // a) - b): Create UTF-8 password: Done in ValidatePassword().

            // c) Test the password against the owner key:
            // Compute a hash with an input string consisting of the UTF-8 password concatenated with the saved owner Validation Salt,
            // concatenated with the saved user key.
            var ownerHashValue = GetUserOwnerHashValue(ownerValue);
            var ownerValidationSalt = GetUserOwnerValidationSalt(ownerValue);

            var computedOwnerHash = ComputeOwnerHash(utf8InputPassword, ownerValidationSalt, userValue);

            var aes = CreateAesForKeyCryptography();

            // If the result matches the saved owner hash value, inputPassword is the owner password.
            // The PDF reference algorithm steps don't state that d) and e) shall be inside these if conditions, but logically it has to be this way,
            // as d) states to work with the owner password and e) states to work with the user password.
            // If the same password was used for user and owner, both forks generate the same encryption key.
            if (computedOwnerHash.SequenceEqual(ownerHashValue))
            {
                // d) Compute an intermediate owner key by computing a hash with an input string consisting of the UTF-8 owner password
                // concatenated with the owner Key Salt, concatenated with saved user key.
                var ownerKeySalt = GetUserOwnerKeySalt(ownerValue);
                var intermediateOwner = ComputeOwnerHash(utf8InputPassword, ownerKeySalt, userValue);

                // The result is the key used to decrypt the OE string using AES-256 in CBC mode with no padding and an initialization vector of zero.
                // The result is the file encryption key.
                using var ownerDecryptor = aes.CreateDecryptor(intermediateOwner, ivZero);
                _encryptionKey = ownerDecryptor.TransformFinalBlock(ownerEValue, 0, ownerEValue.Length);
            }
            // inputPassword seems to be the user password.
            else
            {
                // e) Compute an intermediate user key by computing a hash with an input string consisting of the UTF-8 user password
                // concatenated with the user Key Salt.
                var userKeySalt = GetUserOwnerKeySalt(userValue);
                var intermediateUser = ComputeUserHash(utf8InputPassword, userKeySalt);

                // The result is the key used to decrypt the UE string using AES-256 in CBC mode with no padding and an initialization vector of zero.
                // The result is the file encryption key.
                using var userDecryptor = aes.CreateDecryptor(intermediateUser, ivZero);
                _encryptionKey = userDecryptor.TransformFinalBlock(userEValue, 0, userEValue.Length);
            }

            // f) Decrypt the Perms string using AES-256 in ECB mode with an initialization vector of zero and the file encryption key as the key.
            // Verify that bytes 9-11 of the result are the characters "a", "d", "b". Bytes 0-3 of the decrypted Perms entry, treated as a little-endian integer, are the user permissions.
            // They shall match the value in the P key.

            // We did not implement these checks here, as they will be made later in ValidatePermissions()
            // (corresponding to "7.6.4.4.12 Algorithm 13: Validating the permissions (Security handlers of revision 6)").
            // If implemented here, errors in Perms are recognized even if the retrieved encryption key is wrong due to a wrong password.
            // So it seems to make more sense to check Perms and react to recognized errors only, if a password has already successfully been validated.
        }

        /// <summary>
        /// Gets the bytes 1 - 32 (1-based) of the user or owner value, the hash value.
        /// </summary>
        static byte[] GetUserOwnerHashValue(byte[] userOwnerValue)
        {
            return userOwnerValue.Take(32).ToArray();
        }

        /// <summary>
        /// Gets the bytes 33 - 40 (1-based) of the user or owner value, the validation salt.
        /// </summary>
        static byte[] GetUserOwnerValidationSalt(byte[] userOwnerValue)
        {
            return userOwnerValue.Skip(32).Take(8).ToArray();
        }

        /// <summary>
        /// Gets the bytes 41 - 48 (1-based) of the user or owner value, the key salt.
        /// </summary>
        static byte[] GetUserOwnerKeySalt(byte[] userOwnerValue)
        {
            return userOwnerValue.Skip(40).Take(8).ToArray();
        }

        /// <summary>
        /// Validates the user password.
        /// Corresponding to "7.6.4.4.10 Algorithm 11: Authenticating the user password (Security handlers of revision 6)"
        /// </summary>
        static bool ValidateUserPassword(byte[] utf8InputPassword, byte[] userValue)
        {
            // a) Test inputPassword against the userValue by computing the hash with an input string consisting of the UTF-8 password
            // concatenated with the user validation salt.
            var validationSalt = GetUserOwnerValidationSalt(userValue);
            var hash = ComputeUserHash(utf8InputPassword, validationSalt);

            // If the result matches the user hash value, this is the user password.
            var userHashValue = GetUserOwnerHashValue(userValue);

            return hash.SequenceEqual(userHashValue);
        }

        /// <summary>
        /// Validates the owner password.
        /// Corresponding to "7.6.4.4.11 Algorithm 12: Authenticating the owner password (Security handlers of revision 6)"
        /// </summary>
        static bool ValidateOwnerPassword(byte[] utf8InputPassword, byte[] userValue, byte[] ownerValue)
        {
            // a) Test inputPassword against the ownerValue by computing the hash with an input string consisting of the UTF-8 password
            // concatenated with the owner validation salt and userValue.
            var validationSalt = GetUserOwnerValidationSalt(ownerValue);
            var hash = ComputeOwnerHash(utf8InputPassword, validationSalt, userValue);

            // If the result matches the owner hash value, this is the owner password.
            var ownerHashValue = GetUserOwnerHashValue(ownerValue);

            return hash.SequenceEqual(ownerHashValue);
        }

        /// <summary>
        /// Validates the permissions.
        /// Corresponding to "7.6.4.4.12 Algorithm 13: Validating the permissions (Security handlers of revision 6)"
        /// </summary>
        bool ValidatePermissions(byte[] permsValue, uint pValue)
        {
            // a) Decrypt permsValue using AES-256 in ECB mode with an initialization vector of zero and the file encryption key as the key.
            using var aes = CreateAesForPermissionsCryptography();

            var ivZero = new byte[16];
            using var decryptor = aes.CreateDecryptor(_encryptionKey, ivZero);
            var permsDecrypted = decryptor.TransformFinalBlock(permsValue, 0, permsValue.Length);

            // Verify that bytes 9-11 of the result are the characters "a", "d", "b".
            if (permsDecrypted[9] != 'a')
                return false;
            if (permsDecrypted[10] != 'd')
                return false;
            if (permsDecrypted[11] != 'b')
                return false;

            // Bytes 0-3 of the decrypted Perms entry, treated as a little-endian integer, are the user permissions. They should match the value in the P key.
            var pFromPerms = BitConverter.ToUInt32(permsDecrypted.Take(4).ToArray(), 0); // Little-endian is default, so we don't have to change the order.
            if (pFromPerms != pValue)
                return false;

            // Byte 8 should match the ASCII character "T" or "F" according to the boolean value of the EncryptMetadata key.
            var encryptMetaDataChar = EncryptMetadata ? 'T' : 'F';
            if (permsDecrypted[8] != encryptMetaDataChar)
                return false;

            return true;
        }

        /// <summary>
        /// The encryption key.
        /// </summary>
        byte[] _encryptionKey = null!;
    }
}
