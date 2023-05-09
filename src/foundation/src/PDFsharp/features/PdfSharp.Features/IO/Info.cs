// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;

#pragma warning disable 1591
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
