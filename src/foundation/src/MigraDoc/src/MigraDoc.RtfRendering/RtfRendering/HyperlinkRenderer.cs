// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using System.IO;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Diagnostics;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders a Hyperlink to RTF.
    /// </summary>
    class HyperlinkRenderer : RendererBase
    {
        internal HyperlinkRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _hyperlink = (Hyperlink)domObj;
        }

        /// <summary>
        /// Renders a hyperlink to RTF.
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true;
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("field");
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("fldinst", true);
            _rtfWriter.WriteText("HYPERLINK ");
            string name = _hyperlink.Filename;
            if (_hyperlink.Values.Type is null || _hyperlink.Type == HyperlinkType.Local)
            {
                name = BookmarkFieldRenderer.MakeValidBookmarkName(_hyperlink.BookmarkName);
                _rtfWriter.WriteText(@"\l ");
            }
            else if (_hyperlink.Type == HyperlinkType.File || _hyperlink.Type == HyperlinkType.ExternalBookmark) // Open at least the document for external bookmarks (in PDF: Links to external named destinations).
            {
                string workingDirectory = _docRenderer.WorkingDirectory;
                if (!workingDirectory.IsValueNullOrEmpty())
                    name = Path.Combine(_docRenderer.WorkingDirectory, name);

                name = name.Replace(@"\", @"\\");
            }

            _rtfWriter.WriteText("\"" + name + "\"");
            _rtfWriter.EndContent();
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("fldrslt");
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("cs", _docRenderer.GetStyleIndex("Hyperlink"));

            FontRenderer fontRenderer = new FontRenderer(_hyperlink.Font, _docRenderer);
            fontRenderer.Render();

            if (!_hyperlink.Values.Elements.IsValueNullOrEmpty())
            {
                foreach (var domObj in _hyperlink.Elements)
                {
                    Debug.Assert(domObj != null, nameof(domObj) + " != null");
                    RendererFactory.CreateRenderer(domObj, _docRenderer).Render();
                }
            }
            _rtfWriter.EndContent();
            _rtfWriter.EndContent();
            _rtfWriter.EndContent();
        }

        readonly Hyperlink _hyperlink;
    }
}