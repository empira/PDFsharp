// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a PageBreak to RTF.
    /// </summary>
    class PageBreakRenderer : RendererBase
    {
        internal PageBreakRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
        }

        /// <summary>
        /// Renders a pagebreak to RTF.
        /// </summary>
        internal override void Render()
        {
            _rtfWriter.WriteControl("page");
        }
    }
}
