using System;
using System.Diagnostics;

namespace PdfSharp.TestHelper
{
    public static class PdfFileHelper
    {
        public static string CreateTempFileName(string suffix)
        {
            // ReSharper disable once StringLiteralTypo
            return $"{suffix}-{Guid.NewGuid().ToString("N").ToUpperInvariant()}_tempfile.pdf";
        }

        public static void StartPdfViewer(string filename)
        {
            var startInfo = new ProcessStartInfo(filename) { UseShellExecute = true };
            Process.Start(startInfo);
        }

        public static void StartPdfViewerIfDebugging(string filename)
        {
            if (Debugger.IsAttached)
            {
                StartPdfViewer(filename);
            }
        }
    }
}
