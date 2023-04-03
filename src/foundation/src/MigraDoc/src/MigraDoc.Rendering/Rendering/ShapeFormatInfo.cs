// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Format information for all shapes.
    /// </summary>
    class ShapeFormatInfo : FormatInfo
    {
        internal override bool IsStarting => Fits;

        internal override bool IsEnding => Fits;

        internal override bool IsComplete => Fits;

        /// <summary>
        /// Indicates that the starting of the element is completed.
        /// </summary>
        internal override bool StartingIsComplete => Fits;

        /// <summary>
        /// Indicates that the ending of the element is completed.
        /// </summary>
        internal override bool EndingIsComplete => Fits;

        internal override bool IsEmpty => !Fits;

        internal bool Fits;
    }
}
