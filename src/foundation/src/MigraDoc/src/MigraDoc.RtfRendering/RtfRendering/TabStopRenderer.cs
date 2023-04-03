// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a TabStop to RTF.
    /// </summary>
    class TabStopRenderer : RendererBase
    {
        internal TabStopRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _tabStop = (TabStop)domObj;
            Debug.Assert(_tabStop != null);
        }

        /// <summary>
        /// Renders a TabStop to RTF.
        /// </summary>
        internal override void Render()
        {
            Translate("Alignment", "tq");
            if (_tabStop.Values.Leader is not null && _tabStop.Leader != TabLeader.Spaces)
                Translate("Leader", "tl");

            Translate("Position", "tx");
        }

        readonly TabStop _tabStop;
    }
}
