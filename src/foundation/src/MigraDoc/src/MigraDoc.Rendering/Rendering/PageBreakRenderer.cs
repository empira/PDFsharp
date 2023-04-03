// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders a page break to an XGraphics object.
    /// </summary>
    class PageBreakRenderer : Renderer
    {
        /// <summary>
        /// Initializes a ParagraphRenderer object for formatting.
        /// </summary>
        /// <param name="gfx">The XGraphics object to do measurements on.</param>
        /// <param name="pageBreak">The page break.</param>
        /// <param name="fieldInfos">The field infos.</param>
        internal PageBreakRenderer(XGraphics gfx, PageBreak pageBreak, FieldInfos? fieldInfos)
            : base(gfx, pageBreak, fieldInfos)
        {
            _pageBreak = pageBreak;
        }

        /// <summary>
        /// Initializes a ParagraphRenderer object for rendering.
        /// </summary>
        /// <param name="gfx">The XGraphics object to render on.</param>
        /// <param name="renderInfo">The render info object containing information necessary for rendering.</param>
        /// <param name="fieldInfos">The field infos.</param>
        internal PageBreakRenderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos? fieldInfos)
            : base(gfx, renderInfo, fieldInfos)
        {
            _renderInfo = renderInfo;
        }

        internal override void Format(Area area, FormatInfo? previousFormatInfo)
        {
            var pbRenderInfo = new PageBreakRenderInfo
            {
                FormatInfo = new PageBreakFormatInfo()
            };
            _renderInfo = pbRenderInfo;

            pbRenderInfo.LayoutInfo.PageBreakBefore = true;
            pbRenderInfo.LayoutInfo.ContentArea = new Rectangle(area.Y, area.Y, 0, 0);
            pbRenderInfo.DocumentObject = _pageBreak;
        }

        internal override void Render()
        {
            // Nothing to do here.
        }

        internal override LayoutInfo InitialLayoutInfo
        {
            get
            {
                var layoutInfo = new LayoutInfo();
                layoutInfo.PageBreakBefore = true;
                return layoutInfo;
            }
        }

        readonly PageBreak _pageBreak = null!;
    }
}
