// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the underline type for the font.
    /// </summary>
    public enum Underline
    {
        None,
        Single,
        Words,
        Dotted,
        Dash,
        DotDash,
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
