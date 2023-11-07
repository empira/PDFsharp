// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.Signatures
{
    public class PdfSignatureOptions
    {
        public IAnnotationAppearanceHandler AppearanceHandler { get; set; }
        public string ContactInfo { get; set; }
        public string Location { get; set; }
        public string Reason { get; set; }
        public XRect Rectangle { get; set; }
    }
}
