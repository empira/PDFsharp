// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Specifies the calculation rule of the row height.
    /// </summary>
    public enum RowHeightRule
    {
        /// <summary>
        /// Row height must be greater than or equal to the given value.
        /// </summary>
        AtLeast,
        /// <summary>
        /// Row height will be determined automatically.
        /// </summary>
        Auto,
        /// <summary>
        /// Row height must be exactly the given value.
        /// </summary>
        Exactly
    }
}
