// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Fonts;
using PdfSharp.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;

#pragma warning disable 1591
namespace PdfSharp.Features.IO
{
    public class LargePdfFiles : Feature
    {
        public void LargePdfFileTest()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("PDFsharp", LogLevel.Warning)
                    .AddConsole();
            });
            LogHost.Factory = loggerFactory;

            if (Capabilities.Build.IsCoreBuild)
                GlobalFontSettings.FontResolver = new FailsafeFontResolver();

            var document = CreateNewPdfDocument();

            var stopwatch = new Stopwatch();

            stopwatch.Start();
            const int pageCount = 150_000;
            PdfFileCreator creator = new();
            creator.AddPages(document, pageCount);
            stopwatch.Stop();
            PdfSharpLogHost.Logger.LogInformation($"Time to create document: {stopwatch.Elapsed.TotalSeconds:0.0}s");

            stopwatch.Start();
            var filename = PdfFileUtility.GetTempPdfFileName($"LargePdf({pageCount:0.0})");
            document.Save(filename);
            stopwatch.Stop();
            PdfSharpLogHost.Logger.LogInformation($"Time to save document: {stopwatch.Elapsed.TotalSeconds:0.0}s");

            stopwatch.Start();
            document = PdfReader.Open(filename);
            stopwatch.Stop();
            PdfSharpLogHost.Logger.LogInformation($"Time to load document: {stopwatch.Elapsed.TotalSeconds:0.0}s");

            stopwatch.Start();
            filename = PdfFileUtility.GetTempPdfFileName($"LargePdf({pageCount})_2nd_");
            document.Save(filename);
            stopwatch.Stop();
            PdfSharpLogHost.Logger.LogInformation($"Time to 2nd save document: {stopwatch.Elapsed.TotalSeconds:0.0}s");
        }
    }
}
