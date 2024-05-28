// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;

namespace Shared.TestApp
{
    partial class Program
    {
        static void Main( /*string[] args*/)
        {
            var frameworkDescription = RuntimeInformation.FrameworkDescription;
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    //.AddFilter("Microsoft", LogLevel.Warning)
                    //.AddFilter("System", LogLevel.Warning)
                    //.AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddFilter("", LogLevel.Debug)
                    //.AddFilter("PDFsharp", LogLevel.Critical)
                    .AddConsole();
            });

            ILogger logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Example log message 1");

            LogHost.Logger.LogInformation("Example log message 2");

            LogHost.Factory = loggerFactory;

            LogHost.Logger.LogError("Something went wrong.");

            LogHost.Logger.TestMessage(LogLevel.Critical, "blah");
            LogHost.Logger.TestMessage("di-blub");
            LogHost.Logger.TestMessage("------------------------------------------------------------------------------");



            var tempFileName = PdfFileUtility.GetTempPdfFullFileName("tests");

            //document.Save(tempFileName);

            // Call some developer specific test code from a file not in the repo.
            // Implement your code in ProgramEx.cs in partial class Program.
            var test = typeof(Program).GetMethod("Test", BindingFlags.Static| BindingFlags.NonPublic);
            if (test != null)
            {
                test.Invoke(null, null);
            }

            //PdfFileUtility.ShowDocumentIfDebugging(tempFileName);
        }
    }
}
