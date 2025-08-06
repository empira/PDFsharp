// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

using Font = MigraDoc.DocumentObjectModel.Font;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a ParagraphFormat to RTF.
    /// </summary>
    class ParagraphFormatRenderer : RendererBase
    {
        /// <summary>
        /// Initializes a new instance of the Paragraph Renderer class.
        /// </summary>
        internal ParagraphFormatRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _format = (ParagraphFormat)domObj;
            Debug.Assert(_format != null);
        }

        /// <summary>
        /// Renders a ParagraphFormat object.
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true; //unfortunately has to be true always.

            Translate("Alignment", "q");
            Translate("SpaceBefore", "sb");
            Translate("SpaceAfter", "sa");

            TranslateBool("WidowControl", "widctlpar", "nowidctlpar", false);
            Translate("PageBreakBefore", "pagebb");
            Translate("KeepTogether", "keep");
            Translate("KeepWithNext", "keepn");
            Translate("FirstLineIndent", "fi");

            Translate("LeftIndent", "li");
            Translate("LeftIndent", "lin");

            Translate("RightIndent", "ri");
            Translate("RightIndent", "rin");

            var ol = GetValueAsIntended("OutlineLevel");
            if (ol != null && ((OutlineLevel)ol) != OutlineLevel.BodyText)
                Translate("OutlineLevel", "outlinelevel");

            Unit lineSpc = (Unit)GetValueAsIntended("LineSpacing")!;
            LineSpacingRule lineSpcRule = (LineSpacingRule)GetValueAsIntended("LineSpacingRule")!;

            switch (lineSpcRule)
            {
                case LineSpacingRule.Exactly: // A bit strange, but follows the RTF specification:
                    _rtfWriter.WriteControl("sl", ToTwips(-lineSpc.Point));
                    break;

                case LineSpacingRule.AtLeast:
                    Translate("LineSpacing", "sl");
                    break;

                case LineSpacingRule.Multiple:
                    _rtfWriter.WriteControl("sl", ToRtfUnit(lineSpc, RtfUnit.Lines));
                    break;

                case LineSpacingRule.Double:
                    _rtfWriter.WriteControl("sl", 480); // equals 12 * 2 * 20 (Standard line height * 2 in twips)
                    break;

                case LineSpacingRule.OnePtFive:
                    _rtfWriter.WriteControl("sl", 360); // equals 12 * 1.5 * 20 (Standard lineheight * 1.5 in twips)
                    break;
            }
            Translate("LineSpacingRule", "slmult");
            var shad = GetValueAsIntended("Shading");
            if (shad != null)
                new ShadingRenderer((DocumentObject)shad, _docRenderer).Render();

            var font = GetValueAsIntended("Font");
            if (font != null)
                RendererFactory.CreateRenderer((Font)font, _docRenderer).Render();

            var brdrs = GetValueAsIntended("Borders");
            if (brdrs != null)
            {
                BordersRenderer brdrsRndrr = new BordersRenderer((Borders)brdrs, _docRenderer);
                brdrsRndrr.ParentFormat = _format;
                brdrsRndrr.Render();
            }

            var tabStops = GetValueAsIntended("TabStops");
            if (tabStops != null)
                RendererFactory.CreateRenderer((TabStops)tabStops, _docRenderer).Render();

            // TODO_OLD: ListInfo is still under construction.
            var listInfo = GetValueAsIntended("ListInfo");
            if (listInfo != null)
            {
                int nr = ListInfoOverrideRenderer.GetListNumber((ListInfo)listInfo);
                if (nr > 0)
                    _rtfWriter.WriteControl("ls", nr);
            }
        }

        readonly ParagraphFormat _format;
    }
}
