// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders headers and footers to RTF.
    /// </summary>
    class HeadersFootersRenderer : RendererBase
    {
        internal HeadersFootersRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _headersFooters = (HeadersFooters)domObj;
        }

        /// <summary>
        /// Renders a section's headers and footers to RTF.
        /// </summary>
        /* The MigraDoc DOM page setup properties, the MigraDoc DOM headers and the
         * RTF Controls For left, right and first header/footer 
         * and the RTF flag titlepg are related as follows.
         * 
         * |  First | OddEvn | | Left  | Right | First |TitlePg |
         * ------------------------------------------------------
         * |  True  | True   | | EvnPg | Prim  | First |  True  |
         * |  False | False  | | Prim  | Prim  |   -   |  False |
         * |  True  | False  | | Prim  | Prim  | First |  True  |
         * |  False | True   | | EvnPg | Prim  |   -   |  False |
        */
        internal override void Render()
        {
            _useEffectiveValue = true;
            var obj = GetValueAsIntended("Primary");
            if (obj != null)
                RenderHeaderFooter((HeaderFooter)obj, HeaderFooterIndex.Primary);

            obj = GetValueAsIntended("FirstPage");
            if (obj != null)
                RenderHeaderFooter((HeaderFooter)obj, HeaderFooterIndex.FirstPage);

            obj = GetValueAsIntended("EvenPage");
            if (obj != null)
                RenderHeaderFooter((HeaderFooter)obj, HeaderFooterIndex.EvenPage);
        }

        /// <summary>
        /// Renders a single header footer.
        /// </summary>
        void RenderHeaderFooter(HeaderFooter hdrFtr, HeaderFooterIndex renderAs)
        {
            HeaderFooterRenderer hfr = new HeaderFooterRenderer(hdrFtr, _docRenderer);
            hfr.PageSetup = _pageSetup;
            hfr.RenderAs = renderAs;
            hfr.Render();
        }

        /// <summary>
        /// Sets the PageSetup (It stems from the section the HeadersFooters are used in).
        /// Caution: This PageSetup might differ from the one the "parent" section's got
        /// for inheritance reasons.
        /// </summary>
        internal PageSetup PageSetup
        {
            set
            {
                _pageSetup = value;
            }
        }
        
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        HeadersFooters _headersFooters;
        PageSetup _pageSetup = null!;
    }
}
