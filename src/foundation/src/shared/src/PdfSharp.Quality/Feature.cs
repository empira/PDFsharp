// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Quality
{
    /// <summary>
    /// Base class for features.
    /// </summary>
    public abstract class Feature : FeatureAndSnippetBase
    {
        /// <summary>
        /// Renders a code snippet to PDF.
        /// </summary>
        /// <param name="snippet">A code snippet.</param>
        protected void RenderSnippetAsPdf(Snippet snippet)
        {
            //snippet.RenderSnippetAsPdf(WidthInPoint, HeightInPoint, XGraphicsUnit.Presentation);
            snippet.RenderSnippetAsPdf();
            snippet.SaveAndShowFile(snippet.PdfBytes, snippet.PathName, true);
        }

        /// <summary>
        /// Renders a code snippet to PDF.
        /// </summary>
        /// <param name="snippet"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="graphicsUnit"></param>
        /// <param name="pageDirection"></param>
        protected void RenderSnippetAsPdf(Snippet snippet, XUnit width, XUnit height, XGraphicsUnit graphicsUnit, XPageDirection pageDirection)
        {
            snippet.RenderSnippetAsPdf(width, height, graphicsUnit, pageDirection);
            snippet.SaveAndShowFile(snippet.PdfBytes, snippet.PathName, true);
        }

        /// <summary>
        /// Renders all code snippets to PDF.
        /// </summary>
        protected virtual void RenderAllSnippets()
        {
            // Do nothing in base class.
        }

        /// <summary>
        /// Creates a PDF test document.
        /// </summary>
        protected PdfDocument CreateNewPdfDocument()
        {
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";
            document.Info.Subject = Invariant($"OS: {Environment.OSVersion}");
            document.PageLayout = PdfPageLayout.SinglePage;
            return document;
        }

        /// <summary>
        /// Saves a PDF document to stream or save to file.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="filenameTag">The filename tag.</param>
        /// <param name="show">if set to <c>true</c> [show].</param>
        protected string? SaveToStreamOrSaveToFile(PdfDocument document, Stream? stream, string filenameTag, bool show)
        {
            string? filename;

            // Save and show the document.
            if (stream == null)
            {
                if (show)
                    filename = SaveAndShowDocument(document, filenameTag);
                else
                    filename = SaveDocument(document, filenameTag);
            }
            else
            {
                document.Save(stream, false);
                stream.Position = 0;
                filename = null;
            }

            return filename;
        }

        /// <summary>
        /// Saves a PDF document and show it document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="filenameTag">The filename tag.</param>
        protected string SaveAndShowDocument(PdfDocument document, string filenameTag)
        {
            return PdfFileUtility.SaveAndShowDocument(document, filenameTag);

            // Save the PDF document...
            //var filename = SaveDocument(document, filenameTag);

            //// ... and start a viewer.
            //PdfFileUtility.ShowDocument(filename);
            //return filename;
        }

        /// <summary>
        /// Saves a PDF document into a file.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        /// <param name="filenameTag">The tag of the PDF file.</param>
        protected string SaveDocument(PdfDocument document, string filenameTag)
        {
            return PdfFileUtility.SaveDocument(document, filenameTag);

            //string filename = filenameTag;
            //if (!filenameTag.Contains("_tempfile"))
            //    filename = Invariant($"{Guid.NewGuid():N}_{filenameTag}_tempfile.pdf");
            //document.Save(filename);
            //return filename;
        }

        /// <summary>
        /// Reads and writes a PDF document.
        /// </summary>
        /// <param name="filename">The PDF file to read.</param>
        /// <param name="passwordProvider">The password provider if the file is protected.</param>
        protected string ReadWritePdfDocument(string filename, PdfPasswordProvider? passwordProvider = null)
        {
            var outFilename = Path.GetFileNameWithoutExtension(filename) + "_" + Path.GetExtension(filename);
            try
            {
                PdfDocument document;
                if (passwordProvider is null)
                    document = PdfReader.Open(filename, PdfDocumentOpenMode.Import);
                else
                    document = PdfReader.Open(filename, PdfDocumentOpenMode.Import, passwordProvider);

                var outDocument = new PdfDocument();
                foreach (var page in document.Pages)
                {
                    outDocument.AddPage(page);
                }

                outDocument.Save(outFilename);
            }
            catch (Exception ex)
            {
                LogHost.Logger.LogError(ex, $"{nameof(ReadWritePdfDocument)} failed with file '{{filename}}'.",
                    filename);
                throw;
            }

            return outFilename;
        }

        /// <summary>
        /// Creates and sets a logger factory for test code.
        /// </summary>
        public static void SetDefaultLoggerFactory()
        {
            if (_defaultLoggerFactory != null)
                return;

            _defaultLoggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole();
            });
            LogHost.Factory = _defaultLoggerFactory;
        }
        static ILoggerFactory? _defaultLoggerFactory;
    }
}
