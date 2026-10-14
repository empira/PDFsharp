// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.License;

namespace PdfSharp
{
    /// <summary>
    /// Static license manager for setting or loading the license keys.
    /// </summary>
    public static class PdfSharpLicense
    {
        static PdfSharpLicense()
        {
            PdfSharpLicenseImpl.Initialize();
        }

        /// <summary>
        /// Loads a license from a base64String.
        /// For more information see https://docs.pdfsharp.net/link/license-loading.html
        /// </summary>
        public static void LoadLicense(string licenseString)
        {
            PdfSharpLicenseImpl.LoadLicense(licenseString);
        }

        /// <summary>
        /// Sets the AGPL license.
        /// For more information see  https://docs.pdfsharp.net/link/license-loading.html
        /// </summary>
        public static void SetAgplLicense()
        {
            PdfSharpLicenseImpl.SetAgplLicense();
        }

        /// <summary>
        /// Unloads the current license.
        /// </summary>
        internal static void ResetLicense()
        {
            PdfSharpLicenseImpl.ResetLicense();
        }

        /// <summary>
        /// Returns true if the loaded license is valid for the current release line.
        /// </summary>
        internal static bool HasValidLicense()
        {
            return PdfSharpLicenseImpl.HasValidLicense();
        }

        /// <summary>
        /// Ensure that the current license is valid for the current release line.
        /// </summary>
        internal static void CheckLicense()
        {
            PdfSharpLicenseImpl.CheckLicense();
        }
    }
}
