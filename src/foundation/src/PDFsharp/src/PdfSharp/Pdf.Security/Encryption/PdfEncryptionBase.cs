// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Security.Encryption
{
    /// <summary>
    /// Abstract class for StandardSecurityHandler's encryption versions implementations.
    /// </summary>
    abstract class PdfEncryptionBase
    {
        protected PdfEncryptionBase(PdfStandardSecurityHandler securityHandler)
        {
            SecurityHandler = securityHandler;
        }

        /// <summary>
        /// Initializes the PdfEncryptionBase with the values that were saved in the security handler.
        /// </summary>
        public abstract void InitializeFromLoadedSecurityHandler();

        /// <summary>
        /// Has to be called if a PdfObject is entered for encryption/decryption.
        /// </summary>
        public abstract void EnterObject(PdfObjectID id);

        /// <summary>
        /// Should be called if a PdfObject is left from encryption/decryption.
        /// </summary>
        public abstract void LeaveObject();

        /// <summary>
        /// Encrypts the given bytes for the entered indirect PdfObject.
        /// </summary>
        public abstract void EncryptForEnteredObject(ref byte[] bytes);

        /// <summary>
        /// Decrypts the given bytes for the entered indirect PdfObject.
        /// </summary>
        public abstract void DecryptForEnteredObject(ref byte[] bytes);

        protected bool HandleCryptographicExceptionOnDecryption()
        {
            if (Capabilities.Compatibility.IgnoreErrorsOnDecryption)
            {
                LogHost.Logger.LogWarning("A cryptographic exception occurred while decrypting an object. PDFsharp will handle the content as not encrypted.\n" +
                                          "If PDFsharp will not be able to load the file due to consequential errors, the encryption of the file does not meet the specifications for the supported encryption methods.\n" +
                                          "If PDFsharp will load the file and the contents seem to be correct, the file is at least partly not encrypted as expected.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the encryption dictionary's values for saving.
        /// </summary>
        public abstract void PrepareEncryptionForSaving(string userPassword, string ownerPassword);

        /// <summary>
        /// Validates the password.
        /// </summary>
        public abstract PasswordValidity ValidatePassword(string inputPassword);

        protected PdfStandardSecurityHandler SecurityHandler;

        public int? VersionValue { get; protected set; }

        public int? RevisionValue { get; protected set; }

        public int? LengthValue { get; protected set; }

        public int? ActualLength { get; protected set; }

        public bool EncryptMetadata { get; protected set; }
    }
}
