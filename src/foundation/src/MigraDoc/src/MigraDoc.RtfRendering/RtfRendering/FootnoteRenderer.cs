// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Diagnostics;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders a footnote to RTF.
    /// </summary>
    class FootnoteRenderer : RendererBase
    {
        public FootnoteRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _footnote = (Footnote)domObj;
        }

        /// <summary>
        /// Renders a footnote to RTF.
        /// </summary>
        internal override void Render()
        {
            RenderReference();
            RenderContent();
        }

        /// <summary>
        /// Renders the footnote’s reference symbol.
        /// </summary>
        internal void RenderReference()
        {
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("super");
            if (_footnote.Values.Reference.IsValueNullOrEmpty())
                _rtfWriter.WriteControl("chftn");
            else
                _rtfWriter.WriteText(_footnote.Reference);
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Renders the footnote’s content.
        /// </summary>
        void RenderContent()
        {
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("footnote");
            foreach (var obj in _footnote.Elements)
            {
                RendererBase rndrr = RendererFactory.CreateRenderer(obj!, _docRenderer);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (rndrr != null)
                    rndrr.Render();
            }
            _rtfWriter.EndContent();
        }

        readonly Footnote _footnote;
    }
}
