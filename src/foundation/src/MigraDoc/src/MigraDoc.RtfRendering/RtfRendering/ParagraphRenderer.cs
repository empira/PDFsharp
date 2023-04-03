// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a Paragraph to RTF.
    /// </summary>
    class ParagraphRenderer : StyleAndFormatRenderer
    {
        public ParagraphRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _paragraph = (Paragraph)domObj;
        }

        /// <summary>
        /// Renders the paragraph to RTF.
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true;
            DocumentElements elements = (DocumentRelations.GetParent(_paragraph) as DocumentElements)!;

            _rtfWriter.WriteControl("pard");
            bool isCellParagraph = DocumentRelations.GetParent(elements) is Cell;
            bool isFootnoteParagraph = !isCellParagraph && DocumentRelations.GetParent(elements) is Footnote;

            if (isCellParagraph)
                _rtfWriter.WriteControl("intbl");

            RenderStyleAndFormat();
            if (!_paragraph.Values.Elements.IsValueNullOrEmpty())
                RenderContent();
            EndStyleAndFormatAfterContent();

            if ((!isCellParagraph && !isFootnoteParagraph) || _paragraph != elements.LastObject)
                _rtfWriter.WriteControl("par");
        }

        /// <summary>
        /// Renders the paragraph content to RTF.
        /// </summary>
        void RenderContent()
        {
            DocumentElements elements = (DocumentRelations.GetParent(_paragraph) as DocumentElements)!;
            //First paragraph of a footnote writes the reference symbol:
            if (DocumentRelations.GetParent(elements) is Footnote && _paragraph == elements.First)
            {
                FootnoteRenderer ftntRenderer = new FootnoteRenderer((DocumentRelations.GetParent(elements) as Footnote)!,
                                                                     _docRenderer);
                ftntRenderer.RenderReference();
            }
            foreach (var docObj in _paragraph.Elements)
            {
                if (docObj == _paragraph.Elements.LastObject)
                {
                    if (docObj is Character character)
                    {
                        if (character.SymbolName == SymbolName.LineBreak)
                            continue; //Ignore last linebreak.
                    }
                }

                Debug.Assert(docObj != null, nameof(docObj) + " != null");
                RendererBase rndrr = RendererFactory.CreateRenderer(docObj, _docRenderer);
                //if (rndrr != null)
                rndrr.Render();
            }
        }

        readonly Paragraph _paragraph;
    }
}