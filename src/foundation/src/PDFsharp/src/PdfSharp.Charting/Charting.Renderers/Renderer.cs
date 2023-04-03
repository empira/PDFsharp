// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Base class of all renderers.
    /// </summary>
    abstract class Renderer
    {
        /// <summary>
        /// Initializes a new instance of the Renderer class with the specified renderer parameters.
        /// </summary>
        internal Renderer(RendererParameters rendererParms)
        {
            _rendererParms = rendererParms;
        }

        /// <summary>
        /// Derived renderer should return an initialized and renderer-specific rendererInfo,
        /// e. g. XAxisRenderer returns an new instance of AxisRendererInfo class.
        /// </summary>
        internal virtual RendererInfo? Init()
            => null;

        /// <summary>
        /// Layouts and calculates the space used by the renderer's drawing item.
        /// </summary>
        internal virtual void Format()
        {
            // Nothing to do.
        }

        /// <summary>
        /// Draws the item.
        /// </summary>
        internal abstract void Draw();

        /// <summary>
        /// Holds all necessary rendering information.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected RendererParameters _rendererParms;
    }
}
