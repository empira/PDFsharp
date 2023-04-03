// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a TabStops collection to RTF.
    /// </summary>
    class TabStopsRenderer : RendererBase
    {
        internal TabStopsRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _tabStops = (TabStops)domObj;
            Debug.Assert(_tabStops != null);
        }

        /// <summary>
        /// Renders a TabStops collection to RTF.
        /// </summary>
        internal override void Render()
        {
            foreach (TabStop tabStop in _tabStops)
                RendererFactory.CreateRenderer(tabStop, _docRenderer).Render();
        }

        readonly TabStops _tabStops;
    }
}
