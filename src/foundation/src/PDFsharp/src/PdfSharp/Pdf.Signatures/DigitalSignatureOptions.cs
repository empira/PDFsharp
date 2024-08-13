// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// Sets the options of the DigitalSignatureHandler class.
    /// </summary>
    public class DigitalSignatureOptions()
    {
        /// <summary>
        /// Gets or sets the appearance handler that draws the visual representation of the signature in the PDF.
        /// </summary>
        public IAnnotationAppearanceHandler? AppearanceHandler { get; init; }

        /// <summary>
        /// Gets or sets a string associated with the signature.
        /// </summary>
        public string ContactInfo { get; init; } = "";

        /// <summary>
        /// Gets or sets a string associated with the signature.
        /// </summary>
        public string Location { get; init; } = "";

        /// <summary>
        /// Gets or sets a string associated with the signature.
        /// </summary>
        public string Reason { get; init; } = "";

        /// <summary>
        /// Gets or sets the name of the application used to sign the document.
        /// </summary>
        public string AppName { get; init; } = "PDFsharp http://www.pdfsharp.net";

        /// <summary>
        /// The location of the visual representation on the selected page.
        /// </summary>
        public XRect Rectangle { get; init; }

        /// <summary>
        /// The page index, zero-based, of the page showing the signature.
        /// </summary>
        public int PageIndex { get; init; }
    }
}
