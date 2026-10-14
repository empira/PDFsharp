// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Quality;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.Pdf.Attachments
{
    [Collection("PDFsharp")]
    public class EmbeddedFilesTests_old : IDisposable
    {
        public EmbeddedFilesTests_old()
        {
            PdfSharpCore.ResetAll();
#if CORE
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

        [Fact(Skip = "xxx")]
        public void AddEmbeddedFile()
        {
            var x = new PdfEmbeddedFileParameters((PdfDocument)null!);
            var y = new PdfEmbeddedFileStream((PdfDictionary)null!);
        }
    }
}
