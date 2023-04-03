using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;

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
