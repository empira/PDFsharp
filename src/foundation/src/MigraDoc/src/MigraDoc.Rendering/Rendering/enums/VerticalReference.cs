// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    enum VerticalReference
    {
        /// <summary>
        /// Vertical reference is the previous element.
        /// </summary>
        PreviousElement = 0, // Default

        /// <summary>
        /// Vertical reference is the area boundary.
        /// </summary>
        AreaBoundary,

        /// <summary>
        /// Vertical reference is the page margin.
        /// </summary>
        PageMargin,

        /// <summary>
        /// Vertical reference is page.
        /// </summary>
        Page
    }
}
