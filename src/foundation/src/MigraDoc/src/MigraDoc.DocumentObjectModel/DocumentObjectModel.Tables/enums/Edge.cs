// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Combinable flags to set Borders using the SetEdge function.
    /// </summary>
    [Flags]
    public enum Edge
    {
        Top = 0x0001,
        Left = 0x0002,
        Bottom = 0x0004,
        Right = 0x0008,
        Horizontal = 0x0010,
        Vertical = 0x0020,
        DiagonalDown = 0x0040,
        DiagonalUp = 0x0080,
        Box = Top | Left | Bottom | Right,
        Interior = Horizontal | Vertical,
        Cross = DiagonalDown | DiagonalUp,
    }
}
