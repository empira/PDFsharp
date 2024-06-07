// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Drawing.Pdf;

namespace PdfSharp.UniversalAccessibility
{
    /// <summary>
    /// Helper class containing methods that are called on XGraphics object’s XGraphicsPdfRenderer.
    /// </summary>
    public static class PdfRendererExtensions
    {
        /// <summary>
        /// Activate Text mode for Universal Accessibility.
        /// </summary>
        public static void BeginTextMode(XGraphics gfx)
        {
            if (gfx._renderer is not XGraphicsPdfRenderer renderer)
                throw new InvalidOperationException("Current renderer must be an XGraphicsPdfRenderer.");

            // BeginPage() must be executed before first BeginTextMode.
            renderer.BeginPage();
            renderer.BeginTextMode();
        }

        /// <summary>
        /// Activate Graphic mode for Universal Accessibility.
        /// </summary>
        public static void BeginGraphicMode(XGraphics gfx)
        {
            if (gfx._renderer is not XGraphicsPdfRenderer renderer)
                throw new InvalidOperationException("Current renderer must be an XGraphicsPdfRenderer.");

            // BeginPage must be executed before first BeginGraphicMode.
            renderer.BeginPage();
            renderer.BeginGraphicMode();
        }

        /// <summary>
        /// Determine if renderer is in Text mode or Graphic mode.
        /// </summary>
        public static bool IsInTextMode(XGraphics gfx)
        {
            if (gfx._renderer is not XGraphicsPdfRenderer renderer)
                throw new InvalidOperationException("Current renderer must be an XGraphicsPdfRenderer.");

            return renderer.IsInTextMode();
        }
    }
}
