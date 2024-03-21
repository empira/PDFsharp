// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Security.Cryptography;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Internal
{
    /// <summary>
    /// Throw helper class of PDFsharp.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    static class TH
    {
        public static InvalidOperationException InvalidOperationException_CouldNotFindMetadataDictionary() =>
            new("Could not find document's metadata dictionary.");

        #region Reader Messages
        public static ObjectNotAvailableException ObjectNotAvailableException_CannotRetrieveStreamLengthByNow(Exception? innerException = null)
        {
            const string message = "Cannot retrieve stream length from stream object by now.";
            return innerException != null ? new(message, innerException) : new(message);
        }

        public static ObjectNotAvailableException ObjectNotAvailableException_CannotRetrieveStreamLength() =>
            new("Cannot retrieve stream length.");

        public static InvalidOperationException InvalidOperationException_ReferencesOfObjectStreamNotYetRead() =>
            new("References of object stream are not yet read.");

        public static PdfReaderException PdfReaderException_ObjectCouldNotBeFoundInObjectStreams() =>
            new("Object could not be found in object streams.");
        #endregion

        #region Encryption Messages

        #region Common
        public static PdfReaderException PdfReaderException_UnknownEncryption() =>
            new("The PDF document is protected with an encryption not supported by PDFsharp.");

        public static InvalidOperationException InvalidOperationException_NoEncryptionSet() =>
            new("Encryption has to be set to use encryption methods.");

        public static InvalidOperationException InvalidOperationException_EncryptionRevisionValueNotYetCalculated() =>
            new("The encryption revision value has not been calculated.");

        public static CryptographicException CryptographicException_InputDataTooShort() =>
            new("The input data is too short to hold encrypted content.");
        #endregion

        #region Version1-4
        public static InvalidOperationException InvalidOperationException_InvalidVersionValueForEncryptionVersion1To4() =>
            new("The encryption version value must be 1, 2, or 4 for encryptions lower than version 5.");

        public static InvalidOperationException InvalidOperationException_InvalidPasswordKeyLengthForEncryptionVersion1To4(int len) =>
            new($"Password keys must have a length of 32 bytes. Found {len} bytes.");

        public static InvalidOperationException InvalidOperationException_EncryptionKeyNotSetForEncryptionVersion1To4() =>
            new("Encryption key for the entered indirect PdfObject has to be set.");

        public static InvalidOperationException InvalidOperationException_InvalidKeyLengthForEncryptionVersion2() =>
            new("For V2 encryption the key length must be a multiple of 8 in the range 40 to 128.");

        public static InvalidOperationException InvalidOperationException_AESNotSupportedForChosenEncryptionVersion() =>
            new("AES is not supported in encryption versions lower than 4.");
        #endregion

        #region Version5
        public static InvalidOperationException InvalidOperationException_InvalidVersionValueForEncryptionVersion5() =>
            new("The encryption version value must be 5 for encryption version 5.");

        public static InvalidOperationException InvalidOperationException_InvalidRevisionValueForEncryptionVersion5() =>
            new("The encryption revision value must be 6 for encryption version 5.");

        public static InvalidOperationException InvalidOperationException_InvalidLengthValueForEncryptionVersion5() =>
            new("The Length value must be omitted or 256 for encryption version 5.");

        public static PdfReaderException PdfReaderException_CouldNotVerifyPWithPermsKey() =>
            new("The document seems to be not correctly encrypted. Could not verify P with Perms key.");

        public static NotImplementedException NotImplementedException_EncryptEmbeddedFilesOnlyCurrentlyShutOff() =>
            new("The current implementation is shut off, " +
                "as it does not work correctly and produces PDF files common PDF readers cannot access the embedded file stream correctly.");

        // ReSharper disable once UnusedMember.Global // Code currently commented out, but may be reused.
        public static InvalidOperationException InvalidOperationException_InvalidVersionForEncryptEmbeddedFilesOnly() =>
            new("To encrypt embedded files only, version 4 or 5 has to be used in order to use crypt filters.");

        // ReSharper disable once UnusedMember.Global // Code currently commented out, but may be reused.
        public static InvalidOperationException InvalidOperationException_MissingDefaultCryptFilter(string filterName) =>
            new($"The default crypt filter {filterName} has not been created.");
        #endregion

        #region Crypt Filters
        public static InvalidOperationException InvalidOperationException_CryptFiltersNotSupportedForChosenEncryptionVersion() =>
            new("Crypt filters are not supported for the chosen encryption version. Use version 4 or 5 to use crypt filters.");

        public static InvalidOperationException InvalidOperationException_CryptFiltersNotSupportedForChosenEncryptionRevision() =>
            new("Crypt filters are not supported for the chosen encryption revision. Only revisions 4 and 6 support crypt filters.");

        public static InvalidOperationException InvalidOperationException_CryptFiltersNeededForChosenEncryptionVersion() =>
            new("Crypt filters are needed for the chosen encryption version. Use version 1 or 2 to encrypt without crypt filters.");

        public static InvalidOperationException InvalidOperationException_InvalidCryptFilterMethod() =>
            new("Invalid crypt filter method.");

        public static InvalidOperationException InvalidOperationException_InvalidCryptFilterMethodForEncryptionRevision4() =>
            new("For encryption revision 4 the crypt filter method must be V2 (RC4) or AESV2 (AES-128).");

        public static InvalidOperationException InvalidOperationException_InvalidCryptFilterMethodForEncryptionRevision6() =>
            new("For encryption revision 6 the crypt filter method must be AESV3 (AES-256).");

        public static InvalidOperationException InvalidOperationException_InvalidLengthValueForCryptFilterMethodV2() =>
            new("For crypt filter method V2 the default range of the key length is valid. According to this, key length must be a multiple of 8 in the range 40 to 256.");

        public static InvalidOperationException InvalidOperationException_InvalidLengthValueForCryptFilterMethodAESV2() =>
            new("For crypt filter method AESV2 the key length must be 128.");

        public static InvalidOperationException InvalidOperationException_InvalidLengthValueForCryptFilterMethodAESV3() =>
            new("For crypt filter method AESV3 the key length must be 256.");

        public static InvalidOperationException InvalidOperationException_CryptFilterDecodeParmsNotInitializedCorrectly() =>
            new("Crypt filter value for PdfDictionary is set but CryptFilterDecodeParms are not initialized correctly.");

        public static ArgumentException ArgumentException_UnknownCryptFilter() =>
            new("The given crypt filter name is unknown.");

        public static ArgumentException ArgumentException_UnknownCryptFilterSetToDefault() =>
            new("Only known crypt filters or Identity filter can be defined as default.");
        #endregion

        #region SASLprep
        public static ArgumentException ArgumentException_WrappingSASLprepException(Exception innerException) =>
            new("An error occurred while processing the password.", innerException);

        public static ArgumentException ArgumentException_SASLprepProhibitedCharacter(int codePoint, int position) =>
            new($"Prohibited character '0x{codePoint:X}' at position {position}.");

        public static ArgumentException ArgumentException_SASLprepProhibitedUnassignedCodepoint(int codePoint, int position) =>
            new($"Prohibited unassigned code point '0x{codePoint:X}' at position {position}.");

        public static ArgumentException ArgumentException_SASLprepRandALCatAndLCatCharacters() =>
            new("Violation of bidirectional character requirements. String contains RandALCat characters, but also LCat characters.");

        public static ArgumentException ArgumentException_SASLprepRandALCatButFirstOrLastDivergent() =>
            new("Violation of bidirectional character requirements. String contains RandALCat characters, but the first or last character is not RandALCat.");
        #endregion

        #endregion
    }
}
