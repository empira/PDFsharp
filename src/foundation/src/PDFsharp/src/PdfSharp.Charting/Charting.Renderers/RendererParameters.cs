// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the necessary data for chart rendering.
    /// </summary>
    class RendererParameters
    {
        /// <summary>
        /// Initializes a new instance of the RendererParameters class.
        /// </summary>
        public RendererParameters()
        { }

        /// <summary>
        /// Initializes a new instance of the RendererParameters class with the specified graphics and
        /// coordinates.
        /// </summary>
        public RendererParameters(XGraphics gfx, double x, double y, double width, double height)
        {
            Graphics = gfx;
            Box = new XRect(x, y, width, height);
        }

        /// <summary>
        /// Initializes a new instance of the RendererParameters class with the specified graphics and
        /// rectangle.
        /// </summary>
        public RendererParameters(XGraphics gfx, XRect boundingBox)
        {
            Graphics = gfx;
            Box = boundingBox;
        }

        /// <summary>
        /// Gets or sets the graphics object.
        /// </summary>
        public XGraphics Graphics { get; set; } = null!;

        /// <summary>
        /// Gets or sets the item to draw.
        /// </summary>
        public object DrawingItem { get; set; } = null!;

        /// <summary>
        /// Gets or sets the rectangle for the drawing item.
        /// </summary>
        public XRect Box { get; set; }

        /// <summary>
        /// Gets or sets the RendererInfo.
        /// </summary>
        public RendererInfo RendererInfo { get; set; } = null!;
    }
}
