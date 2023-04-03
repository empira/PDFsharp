// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security.Encryption;

namespace PdfSharp.Pdf.Security
{
    /// <summary>
    /// Represents the standard PDF security handler.
    /// </summary>
    public sealed class PdfStandardSecurityHandler : PdfSecurityHandler
    {
        internal PdfStandardSecurityHandler(PdfDocument document) : base(document)
        { }

        internal PdfStandardSecurityHandler(PdfDictionary dict) : base(dict)
        { }

        void EnsureEncryptionIsSet()
        {
            if (_encryption == null)
                SetDefaultEncryption();
        }

        void SetDefaultEncryption()
        {
            SetEncryptionToV4UsingAES(); // Initialize with the most recent encryption method not requiring PDF 2.0.
        }

        /// <summary>
        /// Do not encrypt the PDF file. Resets the user and owner password.
        /// </summary>

        public void SetEncryptionToNoneAndResetPasswords()
        {
            _userPassword = "";
            _ownerPassword = "";
            _encryption = null;
        }

        /// <summary>
        /// Set the encryption according to the given parameter.
        /// </summary>
        public void SetEncryption(DefaultEncryption encryption)
        {
            switch (encryption)
            {
                case DefaultEncryption.None:
                    SetEncryptionToNoneAndResetPasswords();
                    break;
                case DefaultEncryption.Default:
                    SetDefaultEncryption();
                    break;
                case DefaultEncryption.V1:
                    SetEncryptionToV1();
                    break;
                case DefaultEncryption.V2With40Bits:
                    SetEncryptionToV2();
                    break;
                case DefaultEncryption.V2With128Bits:
                    SetEncryptionToV2With128Bits();
                    break;
                case DefaultEncryption.V4UsingRC4:
                    SetEncryptionToV4UsingRC4();
                    break;
                case DefaultEncryption.V4UsingAES:
                    SetEncryptionToV4UsingAES();
                    break;
                case DefaultEncryption.V5:
                    SetEncryptionToV5();
                    break;
            }
        }

        /// <summary>
        /// Encrypt with Version 1 (RC4 and a file encryption key length of 40 bits).
        /// </summary>

        public void SetEncryptionToV1()
        {
            SetEncryptionFieldToV1To4().SetEncryptionToV1();
        }

        /// <summary>
        /// Encrypt with Version 2 (RC4 and a file encryption key length of more than 40 bits, PDF 1.4).
        /// </summary>
        /// <param name="length">The file encryption key length - a multiple of 8 from 40 to 128 bit.</param>
        public void SetEncryptionToV2(int length = 40)
        {
            SetEncryptionFieldToV1To4().SetEncryptionToV2(length);
        }

        /// <summary>
        /// Encrypt with Version 2 (RC4 and a file encryption key length of more than 40 bits, PDF 1.4) with a file encryption key length of 128 bits.
        /// This was the default encryption in PDFsharp 1.5.
        /// </summary>
        public void SetEncryptionToV2With128Bits()
        {
            SetEncryptionFieldToV1To4().SetEncryptionToV2(128);
        }

        /// <summary>
        /// Encrypt with Version 4 (RC4 or AES and a file encryption key length of 128 bits using a crypt filter, PDF 1.5) using RC4.
        /// </summary>
        /// <param name="encryptMetadata">The document metadata stream shall be encrypted (default: true).</param>
        // ReSharper disable once InconsistentNaming
        public void SetEncryptionToV4UsingRC4(bool encryptMetadata = true)
        {
            SetEncryptionFieldToV1To4().SetEncryptionToV4UsingRC4(encryptMetadata);
        }

        /// <summary>
        /// Encrypt with Version 4 (RC4 or AES and a file encryption key length of 128 bits using a crypt filter, PDF 1.5) using AES (PDF 1.6).
        /// </summary>
        /// <param name="encryptMetadata">The document metadata stream shall be encrypted (default: true).</param>
        public void SetEncryptionToV4UsingAES(bool encryptMetadata = true)
        {
            SetEncryptionFieldToV1To4().SetEncryptionToV4UsingAES(encryptMetadata);
        }

        /// <summary>
        /// Encrypt with Version 5 (AES and a file encryption key length of 256 bits using a crypt filter, PDF 2.0).
        /// </summary>
        /// <param name="encryptMetadata">The document metadata stream shall be encrypted (default: true).</param>
        public void SetEncryptionToV5(bool encryptMetadata = true)
        {
            SetEncryptionFieldToV5().Initialize(encryptMetadata);
        }

        PdfEncryptionV1To4 SetEncryptionFieldToV1To4()
        {
            // If necessary, set _encryption to a new PdfEncryptionV1To4.
            if (_encryption is not PdfEncryptionV1To4 encryption1To4)
                _encryption = encryption1To4 = new PdfEncryptionV1To4(this);

            return encryption1To4;
        }
        PdfEncryptionV5 SetEncryptionFieldToV5()
        {
            // If necessary, set _encryption to a new PdfEncryptionV5.
            if (_encryption is not PdfEncryptionV5 encryption5)
                _encryption = encryption5 = new PdfEncryptionV5(this);

            return encryption5;
        }

        /// <summary>
        /// Returns this SecurityHandler, if it shall be written to PDF (if an encryption is chosen).
        /// </summary>
        internal PdfStandardSecurityHandler? GetIfEncryptionActive() => IsEncrypted ? this : null;

        internal bool IsEncrypted => _encryption != null;

        /// <summary>
        /// Sets the user password of the document.
        /// </summary>
        public string UserPassword
        {
            internal get => _userPassword;
            set
            {
                EnsureEncryptionIsSet();
                _userPassword = value;
            }
        }
        string _userPassword = "";

        /// <summary>
        /// Sets the owner password of the document.
        /// </summary>
        public string OwnerPassword
        {
            internal get => _ownerPassword;
            set
            {
                EnsureEncryptionIsSet();
                _ownerPassword = value;
            }
        }

        private string _ownerPassword = "";

        /// <summary>
        /// Gets or sets the user access permission represented as an integer in the P key.
        /// </summary>
        internal PdfUserAccessPermission Permissions
        {
            get
            {
                var permissions = (PdfUserAccessPermission)Elements.GetInteger(Keys.P);
                if (permissions == 0)
                    permissions = PdfUserAccessPermission.PermitAll;
                return permissions;
            }
            set => Elements.SetInteger(Keys.P, (int)value);
        }

        /// <summary>
        /// Gets the PermissionsValue with some corrections that shall be done for saving.
        /// </summary>
        internal uint GetCorrectedPermissionsValue()
        {
            var permissionsValue = (uint)Permissions;

            // Correct permission bits
            permissionsValue &= 0xfffffffc; // 1... 1111 1111 1100 - Bit 1 & 2 must be 0.
            permissionsValue |= 0x000002c0; // 0... 0010 1100 0000 - Bit 7 & 8 must be 1. Also Bit 10 is no longer used and shall be always set to 1.

            return permissionsValue;
        }

        /// <summary>
        /// Decrypts the whole document.
        /// </summary>
        internal void DecryptDocument()
        {
            foreach (var iref in _document._irefTable.AllReferences)
            {
                if (!ReferenceEquals(iref.Value, this))
                    DecryptObject(iref.Value);
            }
        }

        /// <summary>
        /// Has to be called if an indirect PdfObject is entered for encryption/decryption.
        /// </summary>
        internal void EnterObject(PdfObjectID id)
        {
            GetEncryption().EnterObject(id);
        }

        /// <summary>
        /// Should be called if a PdfObject is leaved from encryption/decryption.
        /// </summary>
        internal void LeaveObject()
        {
            GetEncryption().LeaveObject();
        }

        /// <summary>
        /// Decrypts an indirect PdfObject.
        /// </summary>
        void DecryptObject(PdfObject value)
        {
            Debug.Assert(value.Reference != null);

            EnterObject(value.ObjectID);

            switch (value)
            {
                case PdfDictionary vDict:
                    DecryptDictionary(vDict);
                    break;
                case PdfArray vArray:
                    DecryptArray(vArray);
                    break;
                case PdfStringObject vStr:
                    DecryptString(vStr);
                    break;
            }

            LeaveObject();
        }

        /// <summary>
        /// Decrypts a dictionary.
        /// </summary>
        void DecryptDictionary(PdfDictionary dict)
        {
            foreach (var item in dict.Elements)
            {
                switch (item.Value)
                {
                    case PdfString vStr:
                        DecryptString(vStr);
                        break;
                    case PdfDictionary vDict:
                        DecryptDictionary(vDict);
                        break;
                    case PdfArray vArray:
                        DecryptArray(vArray);
                        break;
                }
            }
            if (dict.Stream != null!)
            {
                var bytes = dict.Stream.Value;
                if (bytes.Length != 0)
                {
                    DecryptStream(ref bytes, dict);
                    dict.Stream.Value = bytes;
                }
            }
        }

        /// <summary>
        /// Decrypts an array.
        /// </summary>
        void DecryptArray(PdfArray array)
        {
            var count = array.Elements.Count;
            for (var idx = 0; idx < count; idx++)
            {
                var item = array.Elements[idx];

                switch (item)
                {
                    case PdfString vStr:
                        DecryptString(vStr);
                        break;
                    case PdfDictionary vDict:
                        DecryptDictionary(vDict);
                        break;
                    case PdfArray vArray:
                        DecryptArray(vArray);
                        break;
                }
            }
        }

        /// <summary>
        /// Decrypts a string.
        /// </summary>
        void DecryptString(PdfStringObject value)
        {
            if (value.Length == 0)
                return;

            var bytes = value.EncryptionValue;
            DecryptString(ref bytes);
            value.EncryptionValue = bytes;

        }

        /// <summary>
        /// Decrypts a string.
        /// </summary>
        void DecryptString(PdfString value)
        {
            if (value.Length == 0)
                return;

            var bytes = value.EncryptionValue;
            DecryptString(ref bytes);
            value.EncryptionValue = bytes;
        }

        /// <summary>
        /// Encrypts a string.
        /// </summary>
        /// <param name="bytes">The byte representation of the string.</param>
        internal void EncryptString(ref byte[] bytes)
        {
            if (bytes.Length == 0)
                return;

            if (VersionSupportsCryptFilter())
            {
                var cryptFilter = _defaultCryptFilterStrings;
                if (cryptFilter is not null)
                {
                    if (cryptFilter.EncryptForEnteredObject(ref bytes))
                        return;
                }
            }

            GetEncryption().EncryptForEnteredObject(ref bytes);
        }

        /// <summary>
        /// Decrypts a string.
        /// </summary>
        /// <param name="bytes">The byte representation of the string.</param>
        internal void DecryptString(ref byte[] bytes)
        {
            if (bytes.Length == 0)
                return;

            if (VersionSupportsCryptFilter())
            {
                var cryptFilter = _defaultCryptFilterStrings;
                if (cryptFilter is not null)
                {
                    if (cryptFilter.DecryptForEnteredObject(ref bytes))
                        return;
                }
            }

            GetEncryption().DecryptForEnteredObject(ref bytes);
        }

        /// <summary>
        /// Encrypts a stream.
        /// </summary>
        /// <param name="bytes">The byte representation of the stream.</param>
        /// <param name="dictionary">The PdfDictionary holding the stream.</param>
        internal void EncryptStream(ref byte[] bytes, PdfDictionary dictionary)
        {
            if (bytes.Length == 0)
                return;

            if (VersionSupportsCryptFilter())
            {
                var cryptFilter = GetCryptFilter(dictionary);
                if (cryptFilter is not null)
                {
                    if (cryptFilter.EncryptForEnteredObject(ref bytes))
                        return;
                }
            }

            GetEncryption().EncryptForEnteredObject(ref bytes);
        }

        /// <summary>
        /// Decrypts a stream.
        /// </summary>
        /// <param name="bytes">The byte representation of the stream.</param>
        /// <param name="dictionary">The PdfDictionary holding the stream.</param>
        internal void DecryptStream(ref byte[] bytes, PdfDictionary dictionary)
        {
            if (bytes.Length == 0)
                return;

            if (VersionSupportsCryptFilter())
            {
                var cryptFilter = GetCryptFilter(dictionary);
                if (cryptFilter is not null)
                {
                    if (cryptFilter.DecryptForEnteredObject(ref bytes))
                        return;
                }
            }

            GetEncryption().DecryptForEnteredObject(ref bytes);
        }



        /// <summary>
        /// Does all necessary initialization for reading and decrypting the document with this security handler.
        /// </summary>
        internal void PrepareForReading()
        {
            var filterValue = Elements.GetName(PdfSecurityHandler.Keys.Filter);
            if (filterValue != "/Standard")
                throw TH.PdfReaderException_UnknownEncryption();

            // Encryption to use for reading the file depends on the version value.
            var versionValue = Elements.GetInteger(PdfSecurityHandler.Keys.V);
            if (PdfEncryptionV1To4.IsVersionSupported(versionValue))
                SetEncryptionFieldToV1To4();
            else if (PdfEncryptionV5.IsVersionSupported(versionValue))
                SetEncryptionFieldToV5();
            GetEncryption().InitializeFromLoadedSecurityHandler();

            // Load, initialize and prepare crypt filters.
            LoadCryptFilters(true);
            if (_loadedCryptFilters is not null)
            {
                foreach (var cryptFilter in _loadedCryptFilters.Values)
                    cryptFilter.PrepareForProcessing();
            }
        }

        /// <summary>
        /// Does all necessary initialization for encrypting and writing the document with this security handler.
        /// </summary>
        internal void PrepareForWriting()
        {
            Elements[PdfSecurityHandler.Keys.Filter] = new PdfName("/Standard");

            GetEncryption().PrepareEncryptionForSaving(UserPassword, OwnerPassword);
            
            // Load and prepare crypt filters.
            LoadCryptFilters(false);
            if (_loadedCryptFilters is not null)
            {
                foreach (var cryptFilter in _loadedCryptFilters.Values)
                    cryptFilter.PrepareForProcessing();
            }
        }

        /// <summary>
        /// Checks the password.
        /// </summary>
        /// <param name="inputPassword">Password or null if no password is provided.</param>
        internal PasswordValidity ValidatePassword(string? inputPassword)
        {
            inputPassword ??= "";

            var passwordValidity = GetEncryption().ValidatePassword(inputPassword);
            _document.SecuritySettings.HasOwnerPermissions = passwordValidity == PasswordValidity.OwnerPassword;

            return passwordValidity;
        }

        internal override void WriteObject(PdfWriter writer)
        {
            // Don't encrypt myself.
            var securityHandler = writer.SecurityHandler;
            writer.SecurityHandler = null;
            base.WriteObject(writer);
            writer.SecurityHandler = securityHandler;
        }

        /// <summary>
        /// Gets the encryption (not nullable). Use this in cases where the encryption must be set.
        /// </summary>
        internal PdfEncryptionBase GetEncryption()
        {
            if (_encryption == null)
                throw TH.InvalidOperationException_NoEncryptionSet();

            return _encryption;
        }
        PdfEncryptionBase? _encryption;


#region CryptFilters
        bool VersionSupportsCryptFilter()
        {
            return GetEncryption().VersionValue is 4 or 5;
        }

        void EnsureCryptFiltersAreSupported()
        {
            if (!VersionSupportsCryptFilter())
                throw TH.InvalidOperationException_CryptFiltersNotSupportedForChosenEncryptionVersion();
        }

        bool VersionNeedsCryptFilter()
        {
            return VersionSupportsCryptFilter();
        }

        void EnsureCryptFiltersAreNotNeeded()
        {
            if (VersionNeedsCryptFilter())
                throw TH.InvalidOperationException_CryptFiltersNeededForChosenEncryptionVersion();
        }

        /// <summary>
        /// Removes all crypt filters from the document.
        /// </summary>
        public void RemoveCryptFilters()
        {
            RemoveCryptFilters(false);
        }

        void RemoveCryptFilters(bool force)
        {
            if (!force)
                EnsureCryptFiltersAreNotNeeded();

            Elements.Remove(PdfSecurityHandler.Keys.CF);
            Elements.Remove(PdfSecurityHandler.Keys.StmF);
            Elements.Remove(PdfSecurityHandler.Keys.StrF);
            Elements.Remove(PdfSecurityHandler.Keys.EFF);

            ResetCryptFilterEntriesInAllElements();
        }

        /// <summary>
        /// Creates a crypt filter belonging to standard security handler.
        /// </summary>
        public PdfCryptFilter CreateCryptFilter()
        {
            return new PdfCryptFilter(this);
        }

        /// <summary>
        /// Returns the StdCF as it shall be used in encryption version 4 and 5.
        /// If not yet existing, it is created regarding the asDefaultIfNew parameter, which will set StdCF as default for streams, strings, and embedded file streams.
        /// </summary>
        /// <param name="asDefaultIfNew">If true and the crypt filter is newly created, the crypt filter is referred to as default for any strings, and streams in StmF, StrF and EFF keys.</param>
        public PdfCryptFilter GetOrAddStandardCryptFilter(bool asDefaultIfNew = true)
        {
            EnsureCryptFiltersAreSupported();

            var pdfCryptFilters = (PdfCryptFilters?)Elements.GetValue(PdfSecurityHandler.Keys.CF);
            var standardCryptFilter = pdfCryptFilters?.GetCryptFilter(CryptFilterConstants.StandardFilterName);

            if (standardCryptFilter is null)
            {
                standardCryptFilter = CreateCryptFilter();
                AddCryptFilter(CryptFilterConstants.StandardFilterName, standardCryptFilter, asDefaultIfNew);
            }

            return standardCryptFilter;
        }

        /// <summary>
        /// Adds a crypt filter to the document.
        /// </summary>
        /// <param name="name">The name to identify the crypt filter.</param>
        /// <param name="cryptFilter">The crypt filter.</param>
        /// <param name="asDefault">If true, the crypt filter is referred to as default for any strings and streams in StmF, StrF and EFF keys.</param>
        public void AddCryptFilter(string name, PdfCryptFilter cryptFilter, bool asDefault = false)
        {
            EnsureCryptFiltersAreSupported();

            // If necessary, create cryptFilters PdfDictionary.
            var pdfCryptFilters = (PdfCryptFilters?)Elements.GetValue(PdfSecurityHandler.Keys.CF);
            if (pdfCryptFilters is null)
            {
                pdfCryptFilters = new PdfCryptFilters();
                Elements.SetObject(PdfSecurityHandler.Keys.CF, pdfCryptFilters);
            }
            
            // Add CryptFilter.
            pdfCryptFilters.AddCryptFilter(name, cryptFilter);

            // Set CryptFilter as default for streams, strings, and embedded file streams, if desired.
            if (asDefault)
                SetCryptFilterAsDefault(name);
        }

        /// <summary>
        /// Encrypts embedded file streams only by setting a crypt filter only in the security handler's EFF key and
        /// setting the crypt filter's AuthEvent Key to /EFOpen, in order authenticate embedded file streams when accessing the embedded file.
        /// </summary>
        public void EncryptEmbeddedFilesOnly()
        {
#if true
            throw TH.NotImplementedException_EncryptEmbeddedFilesOnlyCurrentlyShutOff();
#else
            // TODO: Find and fix error in order to produce files readable by common PDF readers. When done enable SecurityTests.Test_OnlyEmbeddedFileStreamEncrypted().

            if (!VersionSupportsCryptFilter())
                throw TH.InvalidOperationException_InvalidVersionForEncryptEmbeddedFilesOnly();

            var pdfCryptFilters = (PdfCryptFilters?)Elements.GetValue(PdfSecurityHandler.Keys.CF);
            var defaultCryptFilter = pdfCryptFilters?.GetCryptFilter(CryptFilterConstants.StandardFilterName);

            if (defaultCryptFilter is null)
                throw TH.InvalidOperationException_MissingDefaultCryptFilter(CryptFilterConstants.StandardFilterName);

            defaultCryptFilter.SetAuthEventForEmbeddedFiles();

            SetCryptFilterAsDefaultForStreams(null);
            SetCryptFilterAsDefaultForStrings(null);
            SetCryptFilterAsDefaultForEmbeddedFileStreams(CryptFilterConstants.StandardFilterName);
#endif
        }

        /// <summary>
        /// Sets the given crypt filter as default for streams, strings, and embedded streams.
        /// The crypt filter must be manually added crypt filter, "Identity" or null to remove the StmF, StrF and EFF key.
        /// </summary>
        public void SetCryptFilterAsDefault(string? name)
        {
            SetCryptFilterAsDefaultForStreams(name);
            SetCryptFilterAsDefaultForStrings(name);
            SetCryptFilterAsDefaultForEmbeddedFileStreams(null); // If not present, default for streams is used.
        }

        /// <summary>
        /// Sets the given crypt filter as default for streams.
        /// The crypt filter must be manually added crypt filter, "Identity" or null to remove the StmF key.
        /// </summary>
        public void SetCryptFilterAsDefaultForStreams(string? name)
        {
            SetCryptFilterAsDefaultInternal(PdfSecurityHandler.Keys.StmF, name);
        }

        /// <summary>
        /// Sets the given crypt filter as default for strings.
        /// The crypt filter must be manually added crypt filter, "Identity" or null to remove the StrF key.
        /// </summary>
        public void SetCryptFilterAsDefaultForStrings(string? name)
        {
            SetCryptFilterAsDefaultInternal(PdfSecurityHandler.Keys.StrF, name);
        }

        /// <summary>
        /// Sets the given crypt filter as default for embedded file streams.
        /// The crypt filter must be manually added crypt filter, "Identity" or null to remove the EFF key.
        /// </summary>
        public void SetCryptFilterAsDefaultForEmbeddedFileStreams(string? name)
        {
            SetCryptFilterAsDefaultInternal(PdfSecurityHandler.Keys.EFF, name);
        }

        void SetCryptFilterAsDefaultInternal(string key, string? name)
        {
            EnsureCryptFiltersAreSupported();

            if (name is null)
            {
                Elements.Remove(key);
                return;
            }

            if (name != PdfName.RemoveSlash(CryptFilterConstants.IdentityFilterValue))
            {
                var pdfCryptFilters = (PdfCryptFilters?)Elements.GetValue(PdfSecurityHandler.Keys.CF);
                if (pdfCryptFilters?.GetCryptFilter(name) is null)
                    throw TH.ArgumentException_UnknownCryptFilterSetToDefault();
            }
            
            Elements.SetName(key, name);
        }

        void LoadCryptFilters(bool initializeCryptFilters)
        {
            var pdfCryptFilters = (PdfCryptFilters?)Elements.GetValue(PdfSecurityHandler.Keys.CF);

            if (!VersionSupportsCryptFilter() || pdfCryptFilters is null || pdfCryptFilters.IsEmpty())
            {
                _loadedCryptFilters = null;
                _defaultCryptFilterStreams = null;
                _defaultCryptFilterStrings = null;
                _defaultCryptFilterEmbeddedFileStreams = null;
                return;
            }


            _loadedCryptFilters = pdfCryptFilters.GetCryptFiltersAsDictionary();
            if (initializeCryptFilters)
            {
                foreach (var loadedCryptFilter in _loadedCryptFilters)
                    loadedCryptFilter.Value.Initialize(this);
            }

            _defaultCryptFilterStreams = GetDefaultCryptFilter(PdfName.RemoveSlash(Elements.GetName(PdfSecurityHandler.Keys.StmF)));
            _defaultCryptFilterStrings = GetDefaultCryptFilter(PdfName.RemoveSlash(Elements.GetName(PdfSecurityHandler.Keys.StrF)));
            _defaultCryptFilterEmbeddedFileStreams = GetDefaultCryptFilter(PdfName.RemoveSlash(Elements.GetName(PdfSecurityHandler.Keys.EFF)), _defaultCryptFilterStreams);
        }

        CryptFilterBase GetDefaultCryptFilter(string cryptFilterName)
        {
            return GetDefaultCryptFilter(cryptFilterName, IdentityCryptFilter.Instance);
        }

        CryptFilterBase GetDefaultCryptFilter(string cryptFilterName, CryptFilterBase @default)
        {
            if (cryptFilterName == PdfName.RemoveSlash(CryptFilterConstants.IdentityFilterValue))
                return IdentityCryptFilter.Instance;

            if (string.IsNullOrEmpty(cryptFilterName))
                return @default;
            
            var cryptFilter = _loadedCryptFilters?[cryptFilterName];

            if (cryptFilter is null)
                throw TH.ArgumentException_UnknownCryptFilter();

            return cryptFilter;
        }


        /// <summary>
        /// Resets the explicitly set crypt filter of a dictionary.
        /// </summary>
        public void ResetCryptFilter(PdfDictionary dictionary)
        {
            dictionary.Elements.ArrayOrSingleItem.Remove<PdfName>(PdfStream.Keys.Filter, CryptFilterConstants.FilterValue);
            dictionary.Elements.ArrayOrSingleItem.Remove(PdfStream.Keys.DecodeParms, CryptFilterConstants.DecodeParmsPredicate);
        }
        
        void ResetCryptFilterEntriesInAllElements()
        {
            foreach (var iref in _document._irefTable.AllReferences)
            {
                var pdfObject = iref.Value;
                if (pdfObject is not PdfDictionary dictionary)
                    continue;

                ResetCryptFilter(dictionary);
            }
        }

        /// <summary>
        /// Sets the dictionary's explicitly set crypt filter to the Identity crypt filter.
        /// </summary>
        public void SetIdentityCryptFilter(PdfDictionary dictionary)
        {
            SetCryptFilter(dictionary, CryptFilterConstants.IdentityFilterValue);
        }

        /// <summary>
        /// Sets the dictionary's explicitly set crypt filter to the desired crypt filter.
        /// </summary>
        public void SetCryptFilter(PdfDictionary dictionary, string cryptFilterName)
        {
            ResetCryptFilter(dictionary);

            EnsureCryptFiltersAreSupported();
            
            if (PdfName.AddSlash(cryptFilterName) != CryptFilterConstants.IdentityFilterValue)
            {
                var cryptFilters = (PdfCryptFilters?)Elements.GetValue(PdfSecurityHandler.Keys.CF);
                if (cryptFilters?.GetCryptFilter(cryptFilterName) is null)
                    throw TH.ArgumentException_UnknownCryptFilter();
            }

            dictionary.Elements.ArrayOrSingleItem.Add(PdfStream.Keys.Filter, new PdfName(CryptFilterConstants.FilterValue), true);

            var decodeParams = new PdfDictionary(dictionary._document);
            decodeParams.Elements.SetName(CryptFilterConstants.DecodeParmsTypeKey, CryptFilterConstants.DecodeParmsTypeValue);
            decodeParams.Elements.SetName(CryptFilterConstants.DecodeParmsNameKey, cryptFilterName);

            dictionary.Elements.ArrayOrSingleItem.Add(PdfStream.Keys.DecodeParms, decodeParams);
        }

        /// <summary>
        /// Gets the crypt filter that shall be used to decrypt or encrypt the dictionary.
        /// </summary>
        public CryptFilterBase? GetCryptFilter(PdfDictionary dictionary)
        {
            // If a crypt filter is set for this PdfDictionary, try to return the desired crypt filter.
            var filters = dictionary.Elements.ArrayOrSingleItem.GetAll(PdfStream.Keys.Filter).ToList();
            // ReSharper disable once SuspiciousTypeConversion.Global
            var cryptFilterValue = filters.OfType<PdfName>().FirstOrDefault(x => x.Equals(CryptFilterConstants.FilterValue));
            if (cryptFilterValue is not null)
            {
                // Try to return crypt filter declared in corresponding DecodeParms.
                var filterIndex = filters.IndexOf(cryptFilterValue);
                var decodeParms = dictionary.Elements.ArrayOrSingleItem.GetAll(PdfStream.Keys.DecodeParms).ToList();
                var filterDecodeParms = filterIndex < decodeParms.Count ? decodeParms[filterIndex] : null;

                if (filterDecodeParms is PdfDictionary filterDecodeParmsDict)
                {
                    var typeValue = filterDecodeParmsDict.Elements.GetName(CryptFilterConstants.DecodeParmsTypeKey);
                    if (typeValue == CryptFilterConstants.DecodeParmsTypeValue)
                    {
                        var cryptFilterNameValue = filterDecodeParmsDict.Elements.GetName(CryptFilterConstants.DecodeParmsNameKey);

                        // For Identity crypt filter return its instance.
                        if (cryptFilterNameValue == CryptFilterConstants.IdentityFilterValue)
                            return IdentityCryptFilter.Instance;

                        // For others try to load crypt filter form _loadedCryptFilters.
                        var cryptFilterName = PdfName.RemoveSlash(cryptFilterNameValue);
                        if (!string.IsNullOrEmpty(cryptFilterName))
                            return _loadedCryptFilters![cryptFilterName];
                    }

                }

                throw TH.InvalidOperationException_CryptFilterDecodeParmsNotInitializedCorrectly();
            }

            if (PdfEmbeddedFileStream.IsEmbeddedFile(dictionary))
                return _defaultCryptFilterEmbeddedFileStreams;
            
            // Otherwise return the default crypt filter for streams.
            return _defaultCryptFilterStreams;
        }
#endregion CryptFilters


        Dictionary<string, PdfCryptFilter>? _loadedCryptFilters;
        CryptFilterBase? _defaultCryptFilterStreams;
        CryptFilterBase? _defaultCryptFilterStrings;
        CryptFilterBase? _defaultCryptFilterEmbeddedFileStreams;

        /// <summary>
        /// Basic settings to initialize encryption with.
        /// </summary>
        public enum DefaultEncryption
        {
            /// <summary>
            /// Do not encrypt the PDF file.
            /// </summary>
            None,

            /// <summary>
            /// Use V4UsingAES, the most recent encryption method not requiring PDF 2.0.
            /// </summary>
            Default,

            /// <summary>
            /// Encrypt with Version 1 (RC4 and a file encryption key length of 40 bits).
            /// </summary>
            V1,

            /// <summary>
            /// Encrypt with Version 2 (RC4 and a file encryption key length of more than 40 bits, PDF 1.4) with a file encryption key length of 40 bits.
            /// </summary>
            V2With40Bits,

            /// <summary>
            /// Encrypt with Version 2 (RC4 and a file encryption key length of more than 40 bits, PDF 1.4) with a file encryption key length of 128 bits.
            /// This was the default encryption in PDFsharp 1.5.
            /// </summary>
            V2With128Bits,

            /// <summary>
            /// Encrypt with Version 4 (RC4 or AES and a file encryption key length of 128 bits using a crypt filter, PDF 1.5) using RC4.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            V4UsingRC4,

            /// <summary>
            /// Encrypt with Version 4 (RC4 or AES and a file encryption key length of 128 bits using a crypt filter, PDF 1.5) using AES (PDF 1.6).
            /// </summary>
            V4UsingAES,

            /// <summary>
            /// Encrypt with Version 5 (AES and a file encryption key length of 256 bits using a crypt filter, PDF 2.0).
            /// </summary>
            V5
        }

        static class CryptFilterConstants
        {
            public const string FilterValue = "/Crypt";

            public const string DecodeParmsTypeKey = "/Type";

            public const string DecodeParmsTypeValue = "/CryptFilterDecodeParms";

            public const string DecodeParmsNameKey = "/Name";

            public static readonly Func<PdfDictionary, bool> DecodeParmsPredicate = x => x.Elements.GetName(DecodeParmsTypeKey) == DecodeParmsTypeValue;

            public const string IdentityFilterValue = "/Identity";

            public const string StandardFilterName = "StdCF";
        }

#region Keys
        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal new sealed class Keys : PdfSecurityHandler.Keys
        {
            /// <summary>
            /// (Required) A number specifying which revision of the standard security handler
            /// shall be used to interpret this dictionary:
            /// 2 (Deprecated in PDF 2.0)
            ///     if the document is encrypted with a V value less than 2 and does not have any of
            ///     the access permissions set to 0 (by means of the P entry, below) that are designated
            ///     "Security handlers of revision 3 or greater".
            /// 3 (Deprecated in PDF 2.0)
            ///     if the document is encrypted with a V value of 2 or 3, or has any "Security handlers of revision 3 or
            ///     greater" access permissions set to 0.
            /// 4 (Deprecated in PDF 2.0)
            ///     if the document is encrypted with a V value of 4.
            /// 5 (PDF 2.0; deprecated in PDF 2.0)
            ///     Shall not be used. This value was used by a deprecated proprietary Adobe extension.
            /// 6 (PDF 2.0)
            ///     if the document is encrypted with a V value of 5.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string R = "/R";

            /// <summary>
            /// (Required) A byte string,
            /// 32 bytes long if the value of R is 4 or less and 48 bytes long if the value of R is 6,
            /// based on both the owner and user passwords, that shall be
            /// used in computing the file encryption key and in determining whether a valid owner
            /// password was entered.
            /// </summary>
            [KeyInfo(KeyType.ByteString | KeyType.Required)]
            public const string O = "/O";

            /// <summary>
            /// (Required) A byte string,
            /// 32 bytes long if the value of R is 4 or less and 48 bytes long if the value of R is 6,
            /// based on the owner and user password, that shall be used in determining
            /// whether to prompt the user for a password and, if so, whether a valid user or owner
            /// password was entered.
            /// </summary>
            [KeyInfo(KeyType.ByteString | KeyType.Required)]
            public const string U = "/U";

            /// <summary>
            /// (Required if R is 6 (PDF 2.0))
            /// A 32-byte string, based on the owner and user password, that shall be used in computing the file encryption key. 
            /// </summary>
            [KeyInfo("2.0", KeyType.ByteString | KeyType.Optional)]
            public const string OE = "/OE";

            /// <summary>
            /// (Required if R is 6 (PDF 2.0))
            /// A 32-byte string, based on the user password, that shall be used in computing the file encryption key.
            /// </summary>
            [KeyInfo("2.0", KeyType.ByteString | KeyType.Optional)]
            public const string UE = "/UE";

            /// <summary>
            /// (Required) A set of flags specifying which operations shall be permitted when the document
            /// is opened with user access.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string P = "/P";

            /// <summary>
            /// (Required if R is 6 (PDF 2.0))
            /// A 16-byte string, encrypted with the file encryption key, that contains an encrypted copy of the permissions flags.
            /// </summary>
            [KeyInfo("2.0", KeyType.ByteString | KeyType.Optional)]
            public const string Perms = "/Perms";

            /// <summary>
            /// (Optional; meaningful only when the value of V is 4 (PDF 1.5) or 5 (PDF 2.0)) Indicates whether
            /// the document-level metadata stream shall be encrypted.
            /// Default value: true.
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string EncryptMetadata = "/EncryptMetadata";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            public static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;

#endregion
    }
}
