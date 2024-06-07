// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Internals;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders a header or footer to RTF.
    /// </summary>
    class HeaderFooterRenderer : RendererBase
    {
        internal HeaderFooterRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _headerFooter = (HeaderFooter)domObj;
        }

        /// <summary>
        /// Renders a single Header or Footer.
        /// </summary>
        internal override void Render()
        {
            List<string> hdrFtrCtrls = GetHeaderFooterControls();
            foreach (string hdrFtrCtrl in hdrFtrCtrls)
            {
                _rtfWriter.StartContent();
                _rtfWriter.WriteControl(hdrFtrCtrl);
                //REM: Could be optimized by storing rendering results in a strings.
                foreach (var docObj in _headerFooter.Elements)
                {
                    var renderer = RendererFactory.CreateRenderer(docObj!, _docRenderer);
                    renderer.Render();
                }
                _rtfWriter.EndContent();
            }
        }

        /// <summary>
        /// Gets a collection of RTF header/footer control words the HeaderFooter is rendered in. (e.g. the primary header might be rendered into the rtf controls headerl and headerr for left and right pages.)
        /// </summary>
        List<string> GetHeaderFooterControls()
        {
            List<string> retColl = new List<string>();
            bool differentFirst = true == _pageSetup.Values.DifferentFirstPageHeaderFooter;
            bool oddEven = true == _pageSetup.Values.OddAndEvenPagesHeaderFooter;
            string ctrlBase = _headerFooter.IsHeader ? "header" : "footer";
            if (_renderAs == HeaderFooterIndex.FirstPage)
            {
                if (differentFirst)
                    retColl.Add(ctrlBase + "f");
            }
            else if (_renderAs == HeaderFooterIndex.EvenPage)
            {
                if (oddEven)
                    retColl.Add(ctrlBase + "l");
            }
            else if (_renderAs == HeaderFooterIndex.Primary) //clearly
            {
                retColl.Add(ctrlBase + "r");
                if (!oddEven)
                    retColl.Add(ctrlBase + "l");
            }
            return retColl;
        }

        /// <summary>
        /// Sets the PageSetup (it stems from the section the currently HeaderFooter is used in). Caution: This PageSetup might differ from the one the "parent" section’s got for inheritance reasons. This value is set by the HeadersFootersRenderer.
        /// </summary>
        internal PageSetup PageSetup
        {
            set { _pageSetup = value; }
        }

        /// <summary>
        /// Sets the HeaderFooterIndex (Primary, Even FirstPage) the rendered HeaderFooter shall represent. This value is set by the HeadersFootersRenderer.
        /// </summary>
        internal HeaderFooterIndex RenderAs
        {
            set { _renderAs = value; }
        }

        HeaderFooterIndex _renderAs;
        PageSetup _pageSetup = null!;
        readonly HeaderFooter _headerFooter;
    }
}