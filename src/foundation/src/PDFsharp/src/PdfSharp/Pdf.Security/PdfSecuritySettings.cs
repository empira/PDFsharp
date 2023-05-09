// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Security
{
    /// <summary>
    /// Encapsulates access to the security settings of a PDF document.
    /// </summary>
    public sealed class PdfSecuritySettings
    {
        internal PdfSecuritySettings(PdfDocument document)
        {
            _document = document;
        }

        readonly PdfDocument _document;

        /// <summary>
        /// Indicates whether the granted access to the document is 'owner permission'. Returns true if the document 
        /// is unprotected or was opened with the owner password. Returns false if the document was opened with the
        /// user password.
        /// </summary>
        public bool HasOwnerPermissions
        {
            internal get => _hasOwnerPermissions;
            set => _hasOwnerPermissions = value;
        }
        /*internal*/
        bool _hasOwnerPermissions = true;

        /// <summary>
        /// Sets the user password of the document. Setting a password automatically sets the
        /// PdfDocumentSecurityLevel to PdfDocumentSecurityLevel.Encrypted128Bit if its current
        /// value is PdfDocumentSecurityLevel.None.
        /// </summary>
        public string UserPassword
        {
            set => SecurityHandler.UserPassword = value;
        }

        /// <summary>
        /// Sets the owner password of the document. Setting a password automatically sets the
        /// PdfDocumentSecurityLevel to PdfDocumentSecurityLevel.Encrypted128Bit if its current
        /// value is PdfDocumentSecurityLevel.None.
        /// </summary>
        public string OwnerPassword
        {
            set => SecurityHandler.OwnerPassword = value;
        }

        /// <summary>
        /// Determines whether the document can be saved.
        /// </summary>
        internal bool CanSave(ref string message)
        {
            var effectiveSecurityHandler = EffectiveSecurityHandler;

            if (effectiveSecurityHandler != null)
            {
                if (String.IsNullOrEmpty(effectiveSecurityHandler.UserPassword) && String.IsNullOrEmpty(effectiveSecurityHandler.OwnerPassword))
                {
                    message = PSSR.UserOrOwnerPasswordRequired;
                    return false;
                }
            }
            return true;
        }

        #region Permissions
        //TODO: Use documentation from our English Acrobat 6.0 version.

        /// <summary>
        /// Permits printing the document. Should be used in conjunction with PermitFullQualityPrint.
        /// </summary>
        public bool PermitPrint
        {
            get => (SecurityHandler.Permissions & PdfUserAccessPermission.PermitPrint) != 0;
            set
            {
                var permissions = SecurityHandler.Permissions;
                if (value)
                    permissions |= PdfUserAccessPermission.PermitPrint;
                else
                    permissions &= ~PdfUserAccessPermission.PermitPrint;
                SecurityHandler.Permissions = permissions;
            }
        }

        /// <summary>
        /// Permits modifying the document.
        /// </summary>
        public bool PermitModifyDocument
        {
            get => (SecurityHandler.Permissions & PdfUserAccessPermission.PermitModifyDocument) != 0;
            set
            {
                var permissions = SecurityHandler.Permissions;
                if (value)
                    permissions |= PdfUserAccessPermission.PermitModifyDocument;
                else
                    permissions &= ~PdfUserAccessPermission.PermitModifyDocument;
                SecurityHandler.Permissions = permissions;
            }
        }

        /// <summary>
        /// Permits content copying or extraction.
        /// </summary>
        public bool PermitExtractContent
        {
            get => (SecurityHandler.Permissions & PdfUserAccessPermission.PermitExtractContent) != 0;
            set
            {
                var permissions = SecurityHandler.Permissions;
                if (value)
                    permissions |= PdfUserAccessPermission.PermitExtractContent;
                else
                    permissions &= ~PdfUserAccessPermission.PermitExtractContent;
                SecurityHandler.Permissions = permissions;
            }
        }

        /// <summary>
        /// Permits commenting the document.
        /// </summary>
        public bool PermitAnnotations
        {
            get => (SecurityHandler.Permissions & PdfUserAccessPermission.PermitAnnotations) != 0;
            set
            {
                var permissions = SecurityHandler.Permissions;
                if (value)
                    permissions |= PdfUserAccessPermission.PermitAnnotations;
                else
                    permissions &= ~PdfUserAccessPermission.PermitAnnotations;
                SecurityHandler.Permissions = permissions;
            }
        }

        /// <summary>
        /// Permits filling of form fields.
        /// </summary>
        public bool PermitFormsFill
        {
            get => (SecurityHandler.Permissions & PdfUserAccessPermission.PermitFormsFill) != 0;
            set
            {
                var permissions = SecurityHandler.Permissions;
                if (value)
                    permissions |= PdfUserAccessPermission.PermitFormsFill;
                else
                    permissions &= ~PdfUserAccessPermission.PermitFormsFill;
                SecurityHandler.Permissions = permissions;
            }
        }

        /// <summary>
        /// Permits to insert, rotate, or delete pages and create bookmarks or thumbnail images even if
        /// PermitModifyDocument is not set.
        /// </summary>
        public bool PermitAssembleDocument
        {
            get => (SecurityHandler.Permissions & PdfUserAccessPermission.PermitAssembleDocument) != 0;
            set
            {
                var permissions = SecurityHandler.Permissions;
                if (value)
                    permissions |= PdfUserAccessPermission.PermitAssembleDocument;
                else
                    permissions &= ~PdfUserAccessPermission.PermitAssembleDocument;
                SecurityHandler.Permissions = permissions;
            }
        }

        /// <summary>
        /// Permits to print in high quality. insert, rotate, or delete pages and create bookmarks or thumbnail images
        /// even if PermitModifyDocument is not set.
        /// </summary>
        public bool PermitFullQualityPrint
        {
            get => (SecurityHandler.Permissions & PdfUserAccessPermission.PermitFullQualityPrint) != 0;
            set
            {
                var permissions = SecurityHandler.Permissions;
                if (value)
                    permissions |= PdfUserAccessPermission.PermitFullQualityPrint;
                else
                    permissions &= ~PdfUserAccessPermission.PermitFullQualityPrint;
                SecurityHandler.Permissions = permissions;
            }
        }

        /// <summary>
        /// Returns true if there are permissions set to zero, that were introduced with security handler revision 3.
        /// </summary>
        /// <param name="permission">The permission uint value containing the PdfUserAccessPermission flags.</param>
        public static bool HasPermissionsOfRevision3OrHigherSetTo0(uint permission)
        {
            var forbidFormsFill = (permission & (uint)PdfUserAccessPermission.PermitFormsFill) == 0;
            var forbidAssembleDocument = (permission & (uint)PdfUserAccessPermission.PermitAssembleDocument) == 0;
            var forbidFullQualityPrint = (permission & (uint)PdfUserAccessPermission.PermitFullQualityPrint) == 0;

            return forbidFormsFill || forbidAssembleDocument || forbidFullQualityPrint;
        }
        #endregion

        /// <summary>
        /// Gets the standard security handler and creates it, if not existing.
        /// </summary>
        internal PdfStandardSecurityHandler SecurityHandler => _document.Trailer.SecurityHandler;

        /// <summary>
        /// Gets the standard security handler, if existing and encryption is active.
        /// </summary>
        internal PdfStandardSecurityHandler? EffectiveSecurityHandler => _document.Trailer.EffectiveSecurityHandler;
    }
}
