// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a Section to RTF.
    /// </summary>
    class SectionRenderer : RendererBase
    {
        internal SectionRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _section = (Section)domObj;
        }

        /// <summary>
        /// Renders a section to RTF.
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true;

            var secs = DocumentRelations.GetParent(_section) as Sections;
            Debug.Assert(secs != null, nameof(secs) + " != null");
            if (_section != secs.First)
            {
                _rtfWriter.WriteControl("pard");
                _rtfWriter.WriteControl("sect");
            }
            _rtfWriter.WriteControl("sectd");

            //Rendering some footnote attributes:
            _docRenderer.RenderSectionProperties();

            var pageStp = _section.PageSetup;
            //if (pageStp != null)
                RendererFactory.CreateRenderer(pageStp, _docRenderer).Render();

            var hdrs = GetValueAsIntended("Headers");
            if (hdrs != null)
            {
                HeadersFootersRenderer hfr = new((hdrs as HeadersFooters)!, _docRenderer);
                // PageSetup has to be set here, because HeaderFooter could be from a different section than PageSetup.
                hfr.PageSetup = pageStp;
                hfr.Render();
            }

            var ftrs = GetValueAsIntended("Footers");
            if (ftrs != null)
            {
                HeadersFootersRenderer hfr = new((ftrs as HeadersFooters)!, _docRenderer);
                hfr.PageSetup = (PageSetup)pageStp!;
                hfr.Render();
            }

            if (!_section.Values.Elements.IsValueNullOrEmpty())
            {
                foreach (var docObj in _section.Elements)
                {
                    var rndrr = RendererFactory.CreateRenderer(docObj!, _docRenderer);
                    //if (rndrr != null)
                    rndrr.Render();
                }
            }
        }

        readonly Section _section;
    }
}
