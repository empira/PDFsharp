// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using System.Reflection;
using PdfSharp.Pdf;

namespace PdfSharp.Quality
{
    /// <summary>
    /// Contains information of a PDF document created for testing purposes.
    /// </summary>
    public class DocTag
    {
        /// <summary>
        /// Gets or sets the title of the document.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; } = "";
        
        // PageSize, Orientation, ...
    }

    /// <summary>
    /// Static helper functions for file IO.
    /// </summary>
    public static class PdfDocUtility
    {
        /// <summary>
        /// Creates a PDF test document.
        /// </summary>
        public static PdfDocument CreateNewPdfDocument(string? title = null)  // Title, A4.., ...
        {
            var tag = new DocTag()
            {
                Title = title ?? ""
            };

            var document = new PdfDocument
            {
                Tag = tag
            };
            document.Info.Title = "Created with PDFsharp";
            document.Info.Subject = $"OS: {Environment.OSVersion}";
            document.PageLayout = PdfPageLayout.SinglePage;
            return document;
        }
    }
}
