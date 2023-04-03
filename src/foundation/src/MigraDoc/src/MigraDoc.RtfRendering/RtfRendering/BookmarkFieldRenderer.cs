// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.Text;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a Bookmark to RTF.
    /// </summary>
    class BookmarkFieldRenderer : RendererBase
    {
        public BookmarkFieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _bookmark = (BookmarkField)domObj;
        }
        /// <summary>
        /// Renders a Bookmark.
        /// </summary>
        internal override void Render()
        {
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("bkmkstart", true);
            string name = MakeValidBookmarkName(_bookmark.Name);
            _rtfWriter.WriteText(name);
            _rtfWriter.EndContent();

            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("bkmkend", true);
            _rtfWriter.WriteText(name);
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Gets a valid bookmark name for RTF by the given original name.
        /// </summary>
        internal static string MakeValidBookmarkName(string originalName)
        {
            //Bookmarks (at least in Word) have the following limitations:
            //1. First character must be a letter (umlauts und ß are allowed)
            //2. All further characters must be letters, numbers or underscores. 
            //   For example, '-' is NOT allowed).
            StringBuilder strBuilder = new StringBuilder(originalName.Length);
            if (!Char.IsLetter(originalName[0]))
                strBuilder.Append("BM__");

            for (int idx = 0; idx < originalName.Length; ++idx)
            {
                char ch = originalName[idx];
                strBuilder.Append(Char.IsLetterOrDigit(ch) ? ch : '_');
            }
            return strBuilder.ToString();
        }

        readonly BookmarkField _bookmark;
    }
}
