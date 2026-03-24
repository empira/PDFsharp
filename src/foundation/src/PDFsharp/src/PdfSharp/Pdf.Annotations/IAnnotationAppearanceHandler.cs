// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

// v7.0.0 Review

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Draws the visual representation of a form element.
    /// </summary>
    public interface IAnnotationAppearanceHandler  // TODO This is BS
    // TODO THHO4STLA Make it better.
    {
        /// <summary>
        /// Draws the visual representation of a form element.
        /// </summary>
        /// <param name="gfx"></param>
        /// <param name="rect"></param>
        void DrawAppearance(XGraphics gfx, XRect rect);
    }
}
