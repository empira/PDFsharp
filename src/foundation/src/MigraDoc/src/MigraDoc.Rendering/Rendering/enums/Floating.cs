// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Floating behavior of layout elements.
    /// </summary>
    enum Floating
    {
        /// <summary>
        /// The element floats from top to bottom. This is the default.
        /// </summary>
        TopBottom = 0,

        /// <summary>
        /// The element is ignored.
        /// </summary>
        None,

        // Reserved for future extensions:

        /// <summary>
        /// Reserved for future extensions. The element floats from left to right.
        /// </summary>
        Left,

        /// <summary>
        /// Reserved for future extensions. The element floats from right to left.
        /// </summary>
        Right,

        /// <summary>
        /// Reserved for future extensions.
        /// </summary>
        BothSides,
    }
}
