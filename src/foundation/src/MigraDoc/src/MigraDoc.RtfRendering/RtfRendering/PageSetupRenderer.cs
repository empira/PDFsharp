// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

using Orientation = MigraDoc.DocumentObjectModel.Orientation;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a PageSetup to RTF.
    /// </summary>
    class PageSetupRenderer : RendererBase
    {
        internal PageSetupRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _pageSetup = (PageSetup)domObj;
        }

        /// <summary>
        /// Render a PageSetup object to RTF.
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true;
            var obj = GetValueAsIntended("DifferentFirstPageHeaderFooter");
            if (obj != null && (bool)obj)
                _rtfWriter.WriteControl("titlepg");

            obj = GetValueAsIntended("Orientation");
            if (obj != null && (Orientation)obj == Orientation.Landscape)
                _rtfWriter.WriteControl("lndscpsxn");
            RenderPageSize(obj == null ? Orientation.Portrait : (Orientation)obj);
            RenderPageDistances();
            RenderSectionStart();
        }

        /// <summary>
        /// Renders attributes related to the page margins and header footer distances.
        /// </summary>
        void RenderPageDistances()
        {
            Translate("LeftMargin", "marglsxn");
            Translate("RightMargin", "margrsxn");
            Translate("TopMargin", "margtsxn");
            Translate("BottomMargin", "margbsxn");

            Translate("MirrorMargins", "margmirsxn");
            Translate("HeaderDistance", "headery");
            Translate("FooterDistance", "footery");
        }

        /// <summary>
        /// Renders attributes related to the section start behavior.
        /// </summary>
        void RenderSectionStart()
        {
            Translate("SectionStart", "sbk");

            var obj = GetValueAsIntended("StartingNumber");
            if (obj != null && (int)obj >= 0)
            {
                //"pgnrestart" needs to be written here so that the starting page number is used.
                _rtfWriter.WriteControl("pgnrestart");
                _rtfWriter.WriteControl("pgnstarts", (int)obj);
            }
        }
        /// <summary>
        /// Renders the page size, taking into account Orientation, PageFormat and PageWidth / PageHeight.
        /// </summary>
        void RenderPageSize(Orientation orient)
        {
            if (orient == Orientation.Landscape)
            {
                RenderUnit("pghsxn", _pageSetup.PageWidth);
                RenderUnit("pgwsxn", _pageSetup.PageHeight);
            }
            else
            {
                RenderUnit("pghsxn", _pageSetup.PageHeight);
                RenderUnit("pgwsxn", _pageSetup.PageWidth);
            }
        }

        readonly PageSetup _pageSetup;
    }
}
