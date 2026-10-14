// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Quality;

#pragma warning disable CS1591 // Internal class

namespace PdfSharp.Features.IO
{
    public class Info
    {
        public static void ReadPdfInfo()
        {
            var path = PathHelper.FindPath("internal\\PDFsharp", "assets\\PDFs\\HelloPDF.pdf");

            PdfFileFormatter.FormatDocument(path);
        }
    }
}
