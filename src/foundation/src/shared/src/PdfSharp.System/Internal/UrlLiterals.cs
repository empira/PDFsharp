// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Internal
{
    /// <summary>
    /// URLs used in PDFsharp are maintained only here.
    /// </summary>
    public class UrlLiterals  // #CHECK_BEFORE_RELEASE
    {
#if DEBUG
        const string DocsPdfSharpUrl = "http://localhost:8094/";
#else
        const string DocsPdfSharpUrl = "https://docs.pdfsharp.net/";
#endif

        /// <summary>
        /// URL for missing assets error message.
        /// "https://docs.pdfsharp.net/link/download-assets.html"
        /// </summary>
        public const string LinkToAssetsDoc = DocsPdfSharpUrl + "link/download-assets.html";

        /// <summary>
        /// URL for missing font resolver.
        /// "https://docs.pdfsharp.net/link/font-resolving.html"
        /// </summary>
        public const string LinkToFontResolving = DocsPdfSharpUrl + "link/font-resolving.html";

    }
}
