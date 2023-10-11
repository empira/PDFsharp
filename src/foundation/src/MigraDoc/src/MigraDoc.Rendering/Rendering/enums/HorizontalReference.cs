// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Horizontal reference point of alignment.
    /// </summary>
    enum HorizontalReference
    {
        /// <summary>
        /// Horizontal reference is the area boundary.
        /// </summary>
        AreaBoundary = 0, // Default

        /// <summary>
        /// Horizontal reference is the page margin.
        /// </summary>
        PageMargin,

        /// <summary>
        /// Horizontal reference is the page.
        /// </summary>
        Page
    }
}
