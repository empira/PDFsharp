// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// Indicates how to retrieve a value from a DocumentObject by calling GetValue.
    /// </summary>
    public enum GV
    {
        /// <summary>
        /// Gets the value for reading and writing. If the value does not exist, it is created.
        /// </summary>
        ReadWrite = 0,

        /// <summary>
        /// Gets the value for reading. If the value does not exist, it is not created.
        /// </summary>
        ReadOnly = 1,

        /// <summary>
        /// Returns null if value is Null or does not exist.
        /// </summary>
        GetNull = 2,
    }
}
