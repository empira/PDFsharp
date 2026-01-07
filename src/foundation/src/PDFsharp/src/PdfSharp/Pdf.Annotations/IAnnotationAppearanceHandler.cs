// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Draws the visual representation of an AcroForm element.
    /// </summary>
    public interface IAnnotationAppearanceHandler  // kann man Annotation generell selber malen?
    {
        /// <summary>
        /// Draws the visual representation of an AcroForm element.
        /// </summary>
        /// <param name="gfx"></param>
        /// <param name="rect"></param>
        void DrawAppearance(XGraphics gfx, XRect rect);
    }
}
