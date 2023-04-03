// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Field information used to fill fields when rendering or formatting.
    /// </summary>
    class FieldInfos
    {
        internal FieldInfos(Dictionary<string, BookmarkInfo> bookmarks)
        {
            _bookmarks = bookmarks;
        }

        internal void AddBookmark(string name)
        {
            if (PhysicalPageNr <= 0)
                return;

            if (_bookmarks.ContainsKey(name))
                _bookmarks.Remove(name);

            if (PhysicalPageNr > 0)
                _bookmarks.Add(name, new BookmarkInfo(PhysicalPageNr, DisplayPageNr));
        }

        internal int GetShownPageNumber(string bookmarkName)
        {
            if (_bookmarks.ContainsKey(bookmarkName))
            {
                var bi = _bookmarks[bookmarkName];
                return bi.ShownPageNumber;
            }
            return -1;
        }

        internal int GetPhysicalPageNumber(string bookmarkName)
        {
            if (_bookmarks.ContainsKey(bookmarkName))
            {
                var bi = _bookmarks[bookmarkName];
                return bi.DisplayPageNumber;
            }
            return -1;
        }

        internal struct BookmarkInfo
        {
            internal BookmarkInfo(int physicalPageNumber, int displayPageNumber)
            {
                DisplayPageNumber = physicalPageNumber;
                ShownPageNumber = displayPageNumber;
            }

            internal readonly int DisplayPageNumber;
            internal readonly int ShownPageNumber;
        }

        readonly Dictionary<string, BookmarkInfo> _bookmarks;
        internal int DisplayPageNr;
        internal int PhysicalPageNr;
        internal int Section;
        internal int SectionPages;
        internal int NumPages;
        internal DateTime Date;
    }
}
