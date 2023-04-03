// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Determines the parts of a page to be rendered.
    /// </summary>
    [Flags]
    public enum PageRenderOptions
    {
        /// <summary>
        /// Renders nothing (creates an empty page).
        /// </summary>
        None = 0,

        /// <summary>
        /// Renders Headers.
        /// </summary>
        RenderHeader = 1,

        /// <summary>
        /// Renders Footers.
        /// </summary>
        RenderFooter = 2,

        /// <summary>
        /// Renders Content.
        /// </summary>
        RenderContent = 4,

        /// <summary>
        /// Renders PDF background pages.
        /// </summary>
        RenderPdfBackground = 8,

        /// <summary>
        /// Renders PDF content pages.
        /// </summary>
        RenderPdfContent = 16,

        /// <summary>
        /// Renders all.
        /// </summary>
        All = RenderHeader | RenderFooter | RenderContent | RenderPdfBackground | RenderPdfContent,

        /// <summary>
        /// Creates not even an empty page.
        /// </summary>
        RemovePage = 32
    }
}
