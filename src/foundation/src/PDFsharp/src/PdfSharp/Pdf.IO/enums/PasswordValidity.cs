// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Determines the type of the password.
    /// </summary>
    public enum PasswordValidity
    {
        /// <summary>
        /// Password is neither user nor owner password.
        /// </summary>
        Invalid,

        /// <summary>
        /// Password is user password.
        /// </summary>
        UserPassword,

        /// <summary>
        /// Password is owner password.
        /// </summary>
        OwnerPassword,
    }
}
