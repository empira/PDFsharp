// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the underline type for the font.
    /// </summary>
    public enum Underline
    {
        /// <summary>
        /// No underline.
        /// </summary>
        None,

        /// <summary>
        /// Continuous single underline.
        /// </summary>
        Single,

        /// <summary>
        /// Word underline.
        /// </summary>
        Words,

        /// <summary>
        /// Dotted underline.
        /// </summary>
        Dotted,

        /// <summary>
        /// Dashed underline.
        /// </summary>
        Dash,

        /// <summary>
        /// Dash-dotted underline.
        /// </summary>
        DotDash,

        /// <summary>
        /// Dash-dot-dotted underline.
        /// </summary>
        DotDotDash,

        /* --- unsupported ---
          Double          = 3,
          Thick           = 6,
          Wavy            = 11,
          WavyHeavy       = 27,
          DottedHeavy     = 20,
          DashHeavy       = 23,
          DotDashHeavy    = 25,
          DotDotDashHeavy = 26,
          DashLong        = 39,
          DashLongHeavy   = 55,
          WavyDouble      = 43
        */
    }
}
