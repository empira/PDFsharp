// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Diagnostics;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders formatted text to RTF.
    /// </summary>
    class FormattedTextRenderer : RendererBase
    {
        internal FormattedTextRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _formattedText = (FormattedText)domObj;
        }

        /// <summary>
        /// Renders a formatted text to RTF.
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true;

            _rtfWriter.StartContent();
            RenderStyleAndFont();
            foreach (var docObj in _formattedText.Elements)
                RendererFactory.CreateRenderer(docObj!, _docRenderer).Render();

            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Renders the style if it is a character style and the font of the formatted text.
        /// </summary>
        void RenderStyleAndFont()
        {
            bool hasCharacterStyle = false;
            if (!_formattedText.Values.Style.IsValueNullOrEmpty())
            {
                var style = _formattedText!.Document!.Styles[_formattedText.Style];
                if (style != null && style.Type == StyleType.Character)
                    hasCharacterStyle = true;
            }
            var font = GetValueAsIntended("Font");
            if (font != null)
            {
                if (hasCharacterStyle)
                    _rtfWriter.WriteControlWithStar("cs", _docRenderer.GetStyleIndex(_formattedText.Style));

                RendererFactory.CreateRenderer(_formattedText.Font, _docRenderer).Render();
            }
        }

        readonly FormattedText _formattedText;
    }
}
