// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Abstract base class for formatting information received by calling Format() on a renderer.
    /// </summary>
    public abstract class FormatInfo
    {
        /// <summary>
        /// Indicates that the formatted object is starting.
        /// </summary>
        internal abstract bool IsStarting { get; }

        /// <summary>
        /// Indicates that the formatted object is ending.
        /// </summary>
        internal abstract bool IsEnding { get; }

        /// <summary>
        /// Indicates that the formatted object is complete.
        /// </summary>
        internal abstract bool IsComplete { get; }

        /// <summary>
        /// Indicates that the starting of the element is completed.
        /// </summary>
        internal abstract bool StartingIsComplete { get; }

        /// <summary>
        /// Indicates that the ending of the element is completed.
        /// </summary>
        internal abstract bool EndingIsComplete { get; }

        internal abstract bool IsEmpty { get; }
    }
}
