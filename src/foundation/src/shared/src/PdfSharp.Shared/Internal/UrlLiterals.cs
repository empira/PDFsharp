// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Internal
{
    /// <summary>
    /// URLs used in PDFsharp are maintained only here.
    /// </summary>
    class UrlLiterals  // CHECK_BEFORE_RELEASE
    {
#if DEBUG
        const string DocsPdfSharpUrl = "http://localhost:8094/";
#else
        const string DocsPdfSharpUrl = "https://docs.pdfsharp.net/";
#endif

        /// <summary>
        /// URL for index page.
        /// "https://docs.pdfsharp.net/"
        /// </summary>
        public const string LinkToRoot = DocsPdfSharpUrl;

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

        /// <summary>
        /// URL for missing MigraDoc error font.
        /// "https://docs.pdfsharp.net/link/migradoc-font-resolving-6.2.html"
        /// </summary>
        public const string LinkToMigraDocFontResolving = DocsPdfSharpUrl + "link/migradoc-font-resolving-6.2.html";

        /// <summary>
        /// URL shown when a PDF file cannot be opened/parsed.
        /// "https://docs.pdfsharp.net/link/cannot-open-pdf-6.2.html"
        /// </summary>
        public const string LinkToCannotOpenPdfFile = DocsPdfSharpUrl + "link/cannot-open-pdf-6.2.html";
    }
}
