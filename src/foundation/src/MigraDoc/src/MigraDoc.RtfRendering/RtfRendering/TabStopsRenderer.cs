// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
#if WPF
using TabAlignment = MigraDoc.DocumentObjectModel.TabAlignment;
#endif

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
            // Special handling in RTF, if DoNotUnifyTabStopHandling backward compatibility capability setting is not enabled:
            // For single decimal tabstops inside of tables add an additional left aligned tabstop at position 0.
            // Without this, in that special case no tab is required to reach this tabstop in RTF.
            // With this special handling, a consistent behavior is achieved through PDF and RTF generation and all tabstop usages.
            if (!Capabilities.BackwardCompatibility.DoNotUnifyTabStopHandling)
            {
                if (_tabStops.Count == 1)
                {
                    var tabStop = _tabStops[0];
                    if (tabStop.Alignment == TabAlignment.Decimal && tabStop.Position > Unit.Zero)
                    {
                        var cell = DocumentRelations.GetParentOfType(_tabStops, typeof(Cell));
                        if (cell != null)
                        {
                            var additionalRtfTabStop = new TabStop(Unit.Zero)
                            {
                                Alignment = TabAlignment.Left
                            };
                            RendererFactory.CreateRenderer(additionalRtfTabStop, _docRenderer).Render();
                        }
                    }
                }
            }

            foreach (var tabStop in _tabStops)
                RendererFactory.CreateRenderer(tabStop, _docRenderer).Render();
        }

        readonly TabStops _tabStops;
    }
}
