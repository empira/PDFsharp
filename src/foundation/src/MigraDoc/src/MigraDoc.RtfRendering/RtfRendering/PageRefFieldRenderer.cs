// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a PageRefField to RTF.
    /// </summary>
    class PageRefFieldRenderer : NumericFieldRendererBase
    {
        internal PageRefFieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _pageRefField = (PageRefField)domObj;
        }

        /// <summary>
        /// Renders PageRefField to RTF.
        /// </summary>
        internal override void Render()
        {
            StartField();
            _rtfWriter.WriteText("PAGEREF ");
            _rtfWriter.WriteText(BookmarkFieldRenderer.MakeValidBookmarkName(_pageRefField.Name));
            TranslateFormat();
            EndField();
        }

        readonly PageRefField _pageRefField;
    }
}
