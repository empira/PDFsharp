// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

using Font = MigraDoc.DocumentObjectModel.Font;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Base class to render objects that have a style and a format attribute (currently cells, paragraphs).
    /// </summary>
    abstract class StyleAndFormatRenderer : RendererBase
    {
        internal StyleAndFormatRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        { }

        /// <summary>
        /// Renders format and style. Always call EndFormatAndStyleAfterContent() after the content was written.
        /// </summary>
        protected void RenderStyleAndFormat()
        {
            var styleName = GetValueAsIntended("Style");
            var parStyleName = styleName;
            Debug.Assert(styleName != null, nameof(styleName) + " != null");
            var style = _docRenderer.Document.Styles[(string)styleName];
            _hasCharacterStyle = false;
            if (style != null)
            {
                if (style.Type == StyleType.Character)
                {
                    _hasCharacterStyle = true;
                    parStyleName = "Normal";
                }
            }
            else
                parStyleName = null;

            if (parStyleName != null)
                _rtfWriter.WriteControl("s", _docRenderer.GetStyleIndex((string)parStyleName));

            var frmt = GetValueAsIntended("Format") as ParagraphFormat;
            Debug.Assert(frmt != null, nameof(frmt) + " != null");
            RendererFactory.CreateRenderer(frmt, _docRenderer).Render();
            _rtfWriter.WriteControl("brdrbtw");// Should separate border, but does not work.
            if (_hasCharacterStyle)
            {
                _rtfWriter.StartContent();
                _rtfWriter.WriteControlWithStar("cs", _docRenderer.GetStyleIndex((string)styleName));
                var font = GetValueAsIntended("Format.Font");
                if (font != null)
                    new FontRenderer(((Font)font), _docRenderer).Render();
            }
        }


        /// <summary>
        /// Ends the format and style rendering. Always paired with RenderStyleAndFormat().
        /// </summary>
        protected void EndStyleAndFormatAfterContent()
        {
            if (_hasCharacterStyle)
            {
                _rtfWriter.EndContent();
            }
        }

        bool _hasCharacterStyle;
    }
}
