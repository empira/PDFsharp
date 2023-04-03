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
        /// TODO Default
        /// </summary>
        TopBottom = 0,

        /// <summary>
        /// The element is ignored.
        /// </summary>
        None,

        // Served for future extensions:
        
        /// <summary>
        /// TODO
        /// </summary>
        Left,
        
        /// <summary>
        /// TODO
        /// </summary>
        Right,

        /// <summary>
        /// TODO
        /// </summary>
        BothSides,
    }
}
