// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;
using PdfSharp.Quality;

#pragma warning disable 1591
namespace PdfSharp.Features.Pdf
{
    public class LinkAnnotations
    {
        public static void MergeDocumentsWithLinkAnnotations()
        {
            var linkMasterPath = PathHelper.FindPath("internal\\PDFsharp", "assets\\Word\\LinksRoot.pdf");
            var linkInsertPath = PathHelper.FindPath("internal\\PDFsharp", "assets\\Word\\LinksInsert.pdf");

            var masterDoc = PdfReader.Open(linkMasterPath, PdfDocumentOpenMode.Modify);
            var insertDoc = PdfReader.Open(linkInsertPath, PdfDocumentOpenMode.Import);

            //masterDoc.Pages.Insert(1, insertDoc.Pages[0]);
            //masterDoc.Pages.Insert(2, insertDoc.Pages[1]);

            masterDoc.Pages.InsertRange(1, insertDoc, 0, 2);

            var filename = PdfFileUtility.GetTempPdfFileName("LinkAnnotations");
            masterDoc.Save(filename);
        }
    }
}
