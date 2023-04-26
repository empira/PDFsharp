// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Units rendered in RTF output.
    /// </summary>
    public enum RtfUnit
    {
        /// <summary>
        /// A Twip is 1/20 point or 1/1440 inch.
        /// </summary>
        Twips,
        /// <summary>
        /// A HalfPt is 1/2 point.
        /// </summary>
        HalfPts,
        /// <summary>
        /// A Line is 1/240 point.
        /// </summary>
        Lines,
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// An English-Metric-Unit or EMU is 1/12700 point.
        /// </summary>
        EMU,
        // ReSharper restore InconsistentNaming
        /// <summary>
        /// A CharUnit100 is 1/100 pica.
        /// </summary>
        CharUnit100,
        /// <summary>
        /// Undefined unit.
        /// </summary>
        Undefined
    }
}
