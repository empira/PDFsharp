// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Base class for all renderers used to draw gridlines.
    /// </summary>
    abstract class GridlinesRenderer : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the GridlinesRenderer class with the specified renderer parameters.
        /// </summary>
        internal GridlinesRenderer(RendererParameters parms) : base(parms)
        { }
    }
}
