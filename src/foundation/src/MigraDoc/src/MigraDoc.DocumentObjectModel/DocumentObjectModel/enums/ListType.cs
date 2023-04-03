// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the symbol or kind of numbering of the list.
    /// </summary>
    public enum ListType
    {
        // The UI of Microsoft Word provides nine list types. In practical use, three list levels should be enough, as a deeper nesting impedes readability.
        BulletList1,
        BulletList2,
        BulletList3,
        NumberList1,
        NumberList2,
        NumberList3
    }
}
