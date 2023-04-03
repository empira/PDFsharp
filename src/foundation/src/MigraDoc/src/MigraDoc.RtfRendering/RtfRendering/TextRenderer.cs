// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Diagnostics;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders Text to RTF.
    /// </summary>
    class TextRenderer : RendererBase
    {
        internal TextRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _text = (Text)domObj;
        }

        /// <summary>
        /// Renders text to RTF.
        /// </summary>
        internal override void Render()
        {
            if (!_text.Values.Content.IsValueNullOrEmpty())
                _rtfWriter.WriteText(_text.Content);
        }

        readonly Text _text;
    }
}
