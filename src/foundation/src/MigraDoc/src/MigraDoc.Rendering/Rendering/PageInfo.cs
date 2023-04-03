// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Provides information necessary to render the page.
    /// </summary>
    public class PageInfo
    {
        internal PageInfo(XUnit width, XUnit height, PageOrientation orientation)
        {
            Width = width;
            Height = height;
            Orientation = orientation;
        }

        /// <summary>
        /// Gets the width of the described page as specified in Document.PageSetup, i.e. the orientation
        /// is not taken into account.
        /// </summary>
        public XUnit Width { get; }

        /// <summary>
        /// Gets the height of the described page as specified in Document.PageSetup, i.e. the orientation
        /// is not taken into account.
        /// </summary>
        public XUnit Height { get; }

        /// <summary>
        /// Gets the orientation of the described page as specified in Document.PageSetup.
        /// The value has no influence on the properties Width or Height, i.e. if the result is PageOrientation.Landscape
        /// you must exchange the values of Width or Height to get the real page size.
        /// </summary>
        public PageOrientation Orientation { get; }
    }
}
