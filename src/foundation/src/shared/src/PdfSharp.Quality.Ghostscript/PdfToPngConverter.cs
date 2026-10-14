// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using System.Runtime.InteropServices;

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Quality.Ghostscript
{
    /// <summary>
    /// </summary>
    public class PdfToPngConverter
    {
        // Add support for Linux when needed.
        // Add more options when needed.

#if false
        // Sample code:

            var pdf = @"D:\PDFsharp\Specs\ISO_32000-2_2020(en).pdf";
            var png = @"D:\page34.png";
            var png2 = @"D:\page%d.png";
            var png3 = @"D:\page%04d.png";

            var pdfConvert = new PdfToPngConverter(pdf, 150, true);
            pdfConvert.ConvertPages(png, 33);
            pdfConvert.ConvertPages(png2, 34, 3);
            pdfConvert.ConvertPages(png3, 37, 3);
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfToPngConverter"/> class.
        /// </summary>
        /// <param name="pdf">The PDF file to be rendered.</param>
        /// <param name="dpi">The DPI setting of the generated image files.</param>
        /// <param name="fontFolder">The font folder used to supply TTF files for the conversion.</param>
        public PdfToPngConverter(string pdf, int dpi, string? fontFolder = null)
        {
            Pdf = pdf;
            Dpi = dpi;
            FontFolder = fontFolder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfToPngConverter"/> class.
        /// </summary>
        /// <param name="pdf">The PDF file to be rendered.</param>
        /// <param name="dpi">The DPI setting of the generated image files.</param>
        /// <param name="useWindowsFontFolder">If set to <c>true</c> the Windows fonts folder will be
        /// used to supply TTF fonts if running under Windows. If false or not running under Windows,
        /// no font folder will be used.</param>
        public PdfToPngConverter(string pdf, int dpi, bool useWindowsFontFolder)
        {
            Pdf = pdf;
            Dpi = dpi;
            if (useWindowsFontFolder && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FontFolder = Environment.ExpandEnvironmentVariables("%windir%") + "/fonts";
            }
        }

        public bool IsInstalled()
        {
            var toolExe = FindTool(false);
            return !String.IsNullOrEmpty(toolExe);
        }

        /// <summary>
        /// Sets the timeout in seconds.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Timeout must be in the range 1 to {Int32.MaxValue / 1000} seconds.</exception>
        public void SetTimeout(int timeout)
        {
            if (timeout is < 1 or > Int32.MaxValue / 1000)
                throw new ArgumentOutOfRangeException($"Timeout must be in the range 1 to {Int32.MaxValue / 1000} seconds.");
            Timeout = timeout;
        }

        /// <summary>
        /// Converts a range of pages. If more than 1 page is to be converted, include a placeholder
        /// in the filename.
        /// "%d" is replaced by the page number without leading zeros.
        /// "%3d" is replaced by the page number padded with leading blanks to always get 3 characters.
        /// "%03d" is replaced by the page number padded with leading zeros to always get 3 digits.
        /// Note that automatic page numbering always begins at 1. When rendering pages 10 through 12,
        /// the files will be numbered 1, 2, and 3.
        /// </summary>
        /// <param name="outputFileName">Name of the output file.</param>
        /// <param name="startPage">The index of the start page, zero-based.</param>
        /// <param name="pageCount">The page count.</param>
        /// <exception cref="System.Exception">Processing failed with exit code {result.ExitCode}.\n" +
        ///                                     $"Output: {result.Output}\nError: {result.Error}</exception>
        /// <exception cref="System.InvalidOperationException">Processing fails if conversion tool is
        /// not installed</exception>
        public void ConvertPages(string outputFileName, int startPage = 0, int pageCount = 1)
        {
            var result = PdfPagesToPng(Pdf, outputFileName, startPage + 1, pageCount, Dpi, FontFolder, Timeout);
            if (result.ExitCode != 0)
            {
                throw new Exception($"Processing failed with exit code {result.ExitCode}.\n" +
                                    $"Output: {result.Output}\nError: {result.Error}");
            }
        }

        static ProcessResult PdfPagesToPng(string pdf, string outputFile, int page, int pageCount, int dpi, string? fontFolder, int timeout)
        {
            var toolExe = FindTool();

            // Optional parameters.
            // -sFONTPATH: Path to folder with TTF files.
            var options = String.IsNullOrEmpty(fontFolder) ?
                "" :
                $"-sFONTPATH={fontFolder}";

            // List of pages to be printed. We support a single page or a single range.
            var pages = pageCount > 1 ?
                $"{page}-{page + pageCount - 1}" :
                page.ToString();

            var si = new ProcessStartInfo(toolExe);
            si.Arguments = $"-dNOPAUSE -dBATCH -r{dpi} -sDEVICE=png16m -sOutputFile=\"{outputFile}\" -sPageList={pages} {options} \"{pdf}\"";
            si.UseShellExecute = false;
            si.CreateNoWindow = true;
            si.RedirectStandardError = true;
            si.RedirectStandardOutput = true;
            var proc = Process.Start(si);
            if (proc is not null)
            {
                proc.WaitForExit(timeout * 1000); // Use Async when we encounter problems.
                var result = new ProcessResult(proc);
                return result;
            }

            return default;
        }

        private static string FindTool(bool throwOnError = true)
        {
#if true
            var toolFolder = IOUtility.GetAssetsPath("tools/pdf-to-png");
            if (toolFolder == null)
                throw new InvalidOperationException("Copy tools to asset folder \"tools/pdf-to-png\". See README.md for further information.");
            var toolExe = toolFolder + "/gswin64c.exe"; // Linux: different application name.
#else
            var toolFolder = AssetsHelper.GetAssetsFolder();
            if (toolFolder == null)
                throw new InvalidOperationException("Copy tools to asset folder.");
            toolFolder = Path.Combine(toolFolder, "tools\\pdf-to-png");
            var toolExe = toolFolder + "\\gswin64c.exe"; // Linux: different application name.
#endif
            if (!File.Exists(toolExe))
            {
                if (throwOnError)
                    throw new InvalidOperationException("Copy tools to asset folder.");
                return null!;
            }

            return toolExe;
        }

        struct ProcessResult
        {
            internal ProcessResult(Process process)
            {
                ExitCode = process.ExitCode;
                Output = process.StandardOutput.ReadToEnd();
                Error = process.StandardError.ReadToEnd();
            }

            public int ExitCode;
            public string Output;
            public string Error;
        }
        /// <summary>
        /// Gets the PDF file name.
        /// </summary>
        public string Pdf { get; }

        /// <summary>
        /// Gets the DPI.
        /// </summary>
        public int Dpi { get; }

        /// <summary>
        /// Gets the timeout for conversion processes in seconds.
        /// </summary>
        public int Timeout { get; private set; } = 30;

        /// <summary>
        /// Gets the font folder.
        /// </summary>
        public string? FontFolder { get; }
    }
}
