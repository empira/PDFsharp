using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using PdfSharp.Pdf;

namespace PdfSharp.Quality
{
    public abstract class FeatureBase : CommonBase
    {
        protected static void RenderSnippetAsPdf(SnippetBase snippet)
        {
            snippet.RenderSnippetAsPdf();
            snippet.SaveAndShowFile(snippet.PdfBytes, "", true);
        }

#if true
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

#if true //CORE || GDI || WPF
        protected string SaveAndShowDocument(PdfDocument document, string filenameTag)
        {
            // Save the PDF document...
            var filename = SaveDocument(document, filenameTag);

            // ... and start a viewer.
            Process.Start(filename);

            return filename;
        }
#endif

        //#if NET/FX_CORE
        //        protected async Task<string> SaveAndShowDocumentAsync(PdfDocument document, string filenameTag)
        //        {
        //            // Save the PDF document...
        //            string filename = await SaveDocumentAsync(document, filenameTag);

        //            // ... and start a viewer.
        //            //Process.Start(filename);
        //            return filename;
        //        }
        //#endif

        protected string SaveDocument(PdfDocument document, string filenameTag)
        {
            var filename = Invariant($"{Guid.NewGuid():N}_{filenameTag}_tempfile.pdf");
            document.Save(filename);
            return filename;
        }

#if false //true || !NET/FX_CORE
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
#endif
    }
}
