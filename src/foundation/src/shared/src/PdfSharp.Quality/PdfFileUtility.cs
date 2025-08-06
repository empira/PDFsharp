// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using PdfSharp.Pdf;

namespace PdfSharp.Quality
{
    /// <summary>
    /// Static helper functions for file IO.
    /// These functions are intended for unit tests and samples in solution code only.
    /// </summary>
    public static class PdfFileUtility
    {
        /// <summary>
        /// Creates a temporary name of a PDF file with the pattern '{namePrefix}-{WIN|WSL|LNX|...}-{...uuid...}_temp.pdf'.
        /// The name ends with '_temp.pdf' to make it easy to delete it using the pattern '*_temp.pdf'.
        /// No file is created by this function. The name contains 10 hex characters to make it unique.
        /// It is not tested whether the file exists, because we have no path here.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static string GetTempPdfFileName(string? namePrefix, bool addInfo = true)
        {
            return IOUtility.GetTempFileName(namePrefix, "pdf", addInfo);
        }

        /// <summary>
        /// Creates a temporary file and returns the full name. The name pattern is '.../temp/.../{namePrefix}-{WIN|WSL|LNX|...}-{...uuid...}_temp.pdf'.
        /// The namePrefix may contain a sub-path relative to the temp directory.
        /// The name ends with '_temp.pdf' to make it easy to delete it using the pattern '*_temp.pdf'.
        /// The file is created and immediately closed. That ensures the returned full file name can be used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static string GetTempPdfFullFileName(string? namePrefix, bool addInfo = true)
        {
            return IOUtility.GetTempFullFileName(namePrefix, "pdf", addInfo);
        }

        /// <summary>
        /// Finds the latest PDF temporary file in the specified folder, including sub-folders, or null,
        /// if no such file exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        public static string? FindLatestPdfTempFile(string? name, string path, bool recursive = false)
        {
            return IOUtility.FindLatestTempFile(name, path, "pdf", recursive);
        }

        /// <summary>
        /// Save the specified document and returns the path.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="addInfo"></param>
        public static string SaveDocument(PdfDocument doc, string name, bool addInfo = true)
        {
            var pdfFilename = GetTempPdfFullFileName(name, addInfo);
            doc.Save(pdfFilename);
            return pdfFilename;
        }

        /// <summary>
        /// Save the specified document and shows it in a PDF viewer application.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="addInfo"></param>
        public static string SaveAndShowDocument(PdfDocument doc, string name, bool addInfo = true)
        {
            var pdfFilename = GetTempPdfFullFileName(name, addInfo);
            doc.Save(pdfFilename);
            ShowDocument(pdfFilename);
            return pdfFilename;
        }

        /// <summary>
        /// Save the specified document and shows it in a PDF viewer application only if the current program
        /// is debugged.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="addInfo"></param>
        public static string SaveAndShowDocumentIfDebugging(PdfDocument doc, string name, bool addInfo = true)
        {
            var pdfFilename = GetTempPdfFullFileName(name, addInfo);
            doc.Save(pdfFilename);
            ShowDocumentIfDebugging(pdfFilename);
            return pdfFilename;
        }

        /// <summary>
        /// Shows the specified document in a PDF viewer application.
        /// </summary>
        /// <param name="pdfFilename">The PDF filename.</param>
        // Renamed from ViewDocument to ShowDocument. I discussed that with ChatGPT.
        // It explains that the latter expresses more what the function really does.
        public static void ShowDocument(string pdfFilename)
        {
            if (Capabilities.OperatingSystem.IsWindows)
            {
                var startInfo = new ProcessStartInfo(pdfFilename)
                {
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            else if (Capabilities.OperatingSystem.IsWsl2)
            {
                CopyFile(pdfFilename);
            }
            else if (Capabilities.OperatingSystem.IsLinux)
            {
                var startInfo = new ProcessStartInfo(pdfFilename)
                {
                    //UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            else
            {
                throw new NotImplementedException("What OS?");
            }

            static void CopyFile(string pdfFilename)
            {
                try
                {
                    var dir = IOUtility.GetViewerWatchDirectory();
                    Debug.Assert(dir != null);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    var filename = Path.GetFileName(pdfFilename);
                    var destination = Path.Combine(dir, filename);
                    File.Copy(pdfFilename, destination);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
        }

        /// <summary>
        /// Shows the specified document in a PDF viewer application only if the current program
        /// is debugged. 
        /// </summary>
        /// <param name="pdfFilename">The PDF filename.</param>
        public static void ShowDocumentIfDebugging(string pdfFilename)
        {
            if (Debugger.IsAttached)
            {
                ShowDocument(pdfFilename);
            }
        }
    }
}
