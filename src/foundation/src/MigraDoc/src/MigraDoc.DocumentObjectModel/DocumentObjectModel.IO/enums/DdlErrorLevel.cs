// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// Specifies the severity of a DDL reader diagnostic.
    /// </summary>
    public enum DdlErrorLevel
    {
        /// <summary>
        /// An unknown severity.
        /// </summary>
        None,

        /// <summary>
        /// An information diagnostic.
        /// </summary>
        Info,

        /// <summary>
        /// A warning or suggestive diagnostic.
        /// </summary>
        Warning,

        /// <summary>
        /// An error diagnostic.
        /// </summary>
        Error,
    }
}
