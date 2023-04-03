// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Specifies the position of a shape. Values are used for both LeftPosition and TopPosition.
    /// </summary>
    public enum ShapePosition
    {
        /// <summary>
        /// Undefined position.
        /// </summary>
        Undefined,

        /// <summary>
        /// Left-aligned position.
        /// </summary>
        Left,

        /// <summary>
        /// Right-aligned position.
        /// </summary>
        Right,

        /// <summary>
        /// Centered position.
        /// </summary>
        Center,

        /// <summary>
        /// Top-aligned position.
        /// </summary>
        Top,

        /// <summary>
        /// Bottom-aligned position.
        /// </summary>
        Bottom,

        /// <summary>
        /// Used with mirrored margins: left-aligned on right page and right-aligned on left page.
        /// </summary>
        Inside,

        /// <summary>
        /// Used with mirrored margins: left-aligned on left page and right-aligned on right page.
        /// </summary>
        Outside
    }
}
