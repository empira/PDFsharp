// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the type of the Border object and therefore its position.
    /// </summary>
    public enum BorderType
    {
        Top,
        Left,
        Bottom,
        Right,
        [Obsolete("Not used in MigraDoc 1.2")]
        Horizontal,  // Not used in MigraDoc 1.2.
        [Obsolete("Not used in MigraDoc 1.2")]
        Vertical,    // Not used in MigraDoc 1.2.
        DiagonalDown,
        DiagonalUp
    }
}
