// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a Style to RTF.
    /// </summary>
    class StyleRenderer : RendererBase
    {
        internal StyleRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _style = (Style)domObj;
            _styles = (Styles)DocumentRelations.GetParent(_style)!;
        }

        internal override void Render()
        {
            _rtfWriter.StartContent();
            int currIdx = _styles.GetIndex(_style.Name);
            RendererBase rndrr;
            if (_style.Type == StyleType.Character)
            {
                _rtfWriter.WriteControlWithStar("cs", currIdx);
                _rtfWriter.WriteControl("additive");
                rndrr = RendererFactory.CreateRenderer(_style.Font, _docRenderer);
            }
            else
            {
                _rtfWriter.WriteControl("s", currIdx);
                rndrr = RendererFactory.CreateRenderer(_style.ParagraphFormat, _docRenderer);
            }
            if (_style.BaseStyle != "")
            {
                int bsIdx = _styles.GetIndex(_style.BaseStyle);
                _rtfWriter.WriteControl("sbasedon", bsIdx);
            }
            rndrr.Render();
            _rtfWriter.WriteText(_style.Name);
            _rtfWriter.WriteSeparator();
            _rtfWriter.EndContent();
        }

        readonly Style _style;
        readonly Styles _styles;
    }
}
