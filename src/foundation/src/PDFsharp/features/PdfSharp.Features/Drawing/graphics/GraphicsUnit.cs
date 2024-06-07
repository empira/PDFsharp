// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Events;
using PdfSharp.Quality;

namespace PdfSharp.Features.Drawing
{
    /// <summary>
    /// Drawing in an image. CGI and WPF only.
    /// </summary>
    public class GraphicsUnit : Feature
    {
        /// <summary>
        /// Render snippet with page direction downwards.
        /// </summary>
        public void Downwards()
        {
            RenderSnippetAsPdf(new Snippets.Drawing.GraphicsUnitDownwards(), XUnit.FromPoint(WidthInPoint), XUnit.FromPoint(HeightInPoint), XGraphicsUnit.Presentation, XPageDirection.Downwards);
        }

        /// <summary>
        /// Render snippet with page direction upwards.
        /// </summary>
        public void Upwards()
        {
            RenderSnippetAsPdf(new Snippets.Drawing.GraphicsUnitUpwards(), XUnit.FromPoint(WidthInPoint), XUnit.FromPoint(HeightInPoint), XGraphicsUnit.Presentation, XPageDirection.Downwards);
        }
    }
}
