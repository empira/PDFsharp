// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using Microsoft.Extensions.Logging;
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
            snippet.RenderSnippetAsPdf();
            snippet.SaveAndShowFile(snippet.PdfBytes, "", true);
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
            // Save the PDF document...
            var filename = SaveDocument(document, filenameTag);

            // ... and start a viewer.
            Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });

            return filename;
        }

        /// <summary>
        /// Saves a PDF document into a file.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        /// <param name="filenameTag">The tag of the PDF file.</param>
        protected string SaveDocument(PdfDocument document, string filenameTag)
        {
            var filename = Invariant($"{Guid.NewGuid():N}_{filenameTag}_tempfile.pdf");
            document.Save(filename);
            return filename;
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
                LogHost.Logger.LogError(ex, $"{nameof(ReadWritePdfDocument)} failed with file '{filename}'.");
                throw;
            }
            return outFilename;
        }

#if true_
        Task<ProcessorArchitecture> WhatProcessor()
        {
            var t = new TaskCompletionSource<ProcessorArchitecture>();
            var w = new WebView();
            w.AllowedScriptNotifyUris = WebView.AnyScriptNotifyUri;
            w.NavigateToString("<html />");
            NotifyEventHandler h = null;
            h = (s, e) =>
            {
                // http://blogs.msdn.com/b/ie/archive/2012/07/12/ie10-user-agent-string-update.aspx
                // IE10 on Windows RT: Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; ARM; Trident/6.0;)
                // 32-bit IE10 on 64-bit Windows: Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)
                // 64-bit IE10 on 64-bit Windows: Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Win64; x64; Trident/6.0)
                // 32-bit IE10 on 32-bit Windows: Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0) 
                try
                {
                    if (e.Value.Contains("ARM;"))
                        t.SetResult(Windows.System.ProcessorArchitecture.Arm);
                    else if (e.Value.Contains("WOW64;") || e.Value.Contains("Win64;") || e.Value.Contains("x64;"))
                        t.SetResult(Windows.System.ProcessorArchitecture.X64);
                    else
                        t.SetResult(Windows.System.ProcessorArchitecture.X86);
                }
                catch (Exception ex) { t.SetException(ex); }
                finally { /* release */ w.ScriptNotify -= h; }
            };
            w.ScriptNotify += h;
            w.InvokeScript("execScript", new[] { "window.external.notify(navigator.userAgent); " });
            return t.Task;
        }
#endif
    }
}
