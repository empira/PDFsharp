// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Index to the three HeaderFooter objects of a HeadersFooters collection.
    /// </summary>
    public enum HeaderFooterIndex
    {
        /// <summary>
        /// Header or footer which is primarily used.
        /// </summary>
        Primary = 0,

        /// <summary>
        /// Header or footer for the first page of the section.
        /// </summary>
        FirstPage = 1,

        /// <summary>
        /// Header or footer for the even pages of the section.
        /// </summary>
        EvenPage = 2
    }
}
