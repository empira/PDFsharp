// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Alignment of layout elements.
    /// </summary>
    enum ElementAlignment
    {
        /// <summary>
        /// Element is aligned near. This is the default.
        /// </summary>
        Near = 0,

        /// <summary>
        /// Element is center-aligned.
        /// </summary>
        Center,

        /// <summary>
        /// Element is far-aligned.
        /// </summary>
        Far,

        /// <summary>
        /// Element is inside-aligned.
        /// </summary>
        Inside,

        /// <summary>
        /// Element is outside-aligned.
        /// </summary>
        Outside
    }
}
