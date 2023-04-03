// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the style of the line of the Border object.
    /// </summary>
    public enum BorderStyle
    {
        /// <summary>
        /// No border.
        /// </summary>
        None,

        /// <summary>
        /// A single solid line.
        /// </summary>
        Single,

        /// <summary>
        /// A dotted line.
        /// </summary>
        Dot,

        /// <summary>
        /// A dashed line (small gaps).
        /// </summary>
        DashSmallGap,

        /// <summary>
        /// A dashed line (large gaps).
        /// </summary>
        DashLargeGap,

        /// <summary>
        /// Alternating dashes and dots.
        /// </summary>
        DashDot,

        /// <summary>
        /// A dash followed by two dots.
        /// </summary>
        DashDotDot,

        /* --- unsupported ---
          Double                = 7,
          Triple                = 8,
          ThinThickSmallGap     = 9,
          ThickThinSmallGap     = 10,
          ThinThickThinSmallGap = 11,
          ThinThickMedGap       = 12,
          ThickThinMedGap       = 13,
          ThinThickThinMedGap   = 14,
          ThinThickLargeGap     = 15,
          ThickThinLargeGap     = 16,
          ThinThickThinLargeGap = 17,
          SingleWavy            = 18,
          DoubleWavy            = 19,
          DashDotStroked        = 20,
          Emboss3D              = 21,
          Engrave3D             = 22,
          LineStyleOutset       = 23,
          LineStyleInset        = 24
        */
    }
}
