// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Security
{
    using Internal;

    public class SecurityManager : ManagerBase
    {
        SecurityManager(PdfDocument document) : base(document)
        {
            Initialize();
        }

        public PdfSecuritySettings SecuritySettings
            => Document.SecuritySettings;

        public string EncryptionType { get; } = "(TODO)";  // TODO

        public string? UserPassword  // TODO
        {
            get => _userPassword;
            set
            {
                if (value == null)
                {
                    _userPassword = null;
                }
                else if (_userPassword is null)
                {
                    _userPassword = value;
                }
                else
                    throw new InvalidOperationException("Cannot change user password if once set.");
            }
        }
        string? _userPassword = "(TODO)";  // TODO

        public string? OwnerPassword  // TODO
        {
            get => _ownerPassword;
            set
            {
                var i = base.IsInitialized;
                if (value == null)
                {
                    _ownerPassword = null;
                }
                else if (_ownerPassword is null)
                {
                    _ownerPassword = value;
                }
                else
                    throw new InvalidOperationException("Cannot change owner password if once set.");
            }
        }
        string? _ownerPassword = "(TODO)";  // TODO

        /// <summary>
        /// Gets or creates the SecurityManager singleton for the specified document.
        /// </summary>
        public static SecurityManager ForDocument(PdfDocument document)
            => document.SecurityManager ??= new(document);

        void Initialize()
        {
            Document.EnsureNotDisposed();
            if (!IsInitialized)
            {
                IsInitialized = true;
                Document.EnsureNotYetSaved();
                if (Document.IsImported)
                {
                    // TODO: Get PDF/A conformance
                }
            }
        }
    }
}
