using System.Diagnostics;

namespace PdfSharp.TestHelper
{
    public static class PdfFileHelper
    {
        public static string CreateTempFileName(string prefix, string extension = "pdf")
        {
            // ReSharper disable once StringLiteralTypo
            return $"{CreateTempFileNameWithoutExtension(prefix)}.{extension}";
        }

        public static string CreateTempFileNameWithoutExtension(string prefix)
        {
            // ReSharper disable once StringLiteralTypo
            return $"{prefix}-{Guid.NewGuid().ToString("N").ToUpperInvariant()}_tempfile";
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
