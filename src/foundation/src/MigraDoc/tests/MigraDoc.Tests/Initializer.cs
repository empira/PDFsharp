// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Runtime.CompilerServices;
using PdfSharp;

namespace MigraDoc.Tests
{
    public static class Initializer
    {
        /// <summary>
        /// Set AGPL license once when the assembly is initialized.
        /// </summary>
        [ModuleInitializer]
        public static void SetLicense()
        {
            PdfSharpLicense.SetAgplLicense();
        }
    }
}
