// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Units rendered in RTF output.
    /// </summary>
    public enum RtfUnit
    {
        Twips,
        HalfPts,
        Lines,
        // ReSharper disable InconsistentNaming
        EMU,
        // ReSharper restore InconsistentNaming
        CharUnit100,
        Undefined
    }
}
