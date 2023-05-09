// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the symbol or kind of numbering of the list.
    /// </summary>
    public enum ListType
    {
        // The UI of Microsoft Word provides nine list types. In practical use, three list levels should be enough, as a deeper nesting impedes readability.
        /// <summary>
        /// Bullet list, level 1.
        /// </summary>
        BulletList1,

        /// <summary>
        /// Bullet list, level 2.
        /// </summary>
        BulletList2,

        /// <summary>
        /// Bullet list, level 3.
        /// </summary>
        BulletList3,

        /// <summary>
        /// Numbered list, level 1.
        /// </summary>
        NumberList1,

        /// <summary>
        /// Numbered list, level 2.
        /// </summary>
        NumberList2,

        /// <summary>
        /// Numbered list, level 3.
        /// </summary>
        NumberList3
    }
}
