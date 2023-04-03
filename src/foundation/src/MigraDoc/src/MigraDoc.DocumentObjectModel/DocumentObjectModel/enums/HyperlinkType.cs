// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the target of the hyperlink.
    /// </summary>
    public enum HyperlinkType
    {
        /// <summary>
        /// Targets a position in the document. Same as 'Bookmark'.
        /// </summary>
        Local = 0,

        /// <summary>
        /// Targets a position in the document. Same as 'Local'.
        /// </summary>
        Bookmark = Local,

        /// <summary>
        /// Targets a position in another PDF document.
        /// This is only supported in PDF. In RTF the other document is opened, but the target position is not moved to.
        /// </summary>
        ExternalBookmark,

        /// <summary>
        /// Targets a position in an embedded document in this or another root PDF document.
        /// This is only supported in PDF.
        /// </summary>
        EmbeddedDocument,

        /// <summary>
        /// Targets a resource on the Internet or network. Same as 'Url'.
        /// </summary>
        Web,

        /// <summary>
        /// Targets a resource on the Internet or network. Same as 'Web'.
        /// </summary>
        Url = Web,

        /// <summary>
        /// Targets a physical file.
        /// </summary>
        File
    }
}
