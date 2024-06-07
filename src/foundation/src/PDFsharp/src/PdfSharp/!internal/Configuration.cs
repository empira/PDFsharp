// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

global using System.Diagnostics;
global using PdfSharp.Diagnostics;
global using System.Globalization;
global using static System.FormattableString;

namespace PdfSharp
{
    /// <summary>
    /// Floating-point formatting.
    /// </summary>
    static class Config
    {
#if DEBUG
        public const string SignificantDecimalPlaces0 = "0";    // for testing only
        public const string SignificantDecimalPlaces1 = "0.#";  // for testing only
#endif
        public const string SignificantDecimalPlaces2 = "0.##";
        public const string SignificantDecimalPlaces3 = "0.###";
        public const string SignificantDecimalPlaces4 = "0.####";
        public const string SignificantDecimalPlaces7 = "0.#######";
        public const string SignificantDecimalPlaces10 = "0.##########";
        public const string SignificantDecimalPlaces1Plus9 = "0.0#########";
    }

    static class Const
    {
        /// <summary>
        /// Factor to convert from degree to radian measure.
        /// </summary>
        public const double Deg2Rad = Math.PI / 180;  // = 0.017453292519943295

        /// <summary>
        /// Sine of the angle of 20° to turn a regular font to look oblique. Used for italic simulation.
        /// </summary>
        public const double ItalicSkewAngleSinus = 0.34202014332566873304409961468226;  // = sin(20°)

        /// <summary>
        /// Factor of the em size of a regular font to look bold. Used for bold simulation.
        /// Value of 2% found in original XPS 1.0 documentation.
        /// </summary>
        public const double BoldEmphasis = 0.02;

        // The κ (kappa) for drawing a circle or an ellipse with four Bézier splines, specifying the
        // distance of the influence point from the starting or end point of a spline.
        // Petzold: 4/3 * tan(α / 4)
        // κ := 4/3 * (1 - cos(-π/4)) / sin(π/4)) <=> 4/3 * (sqrt(2) - 1) <=> 4/3 * tan(π/8)
        // ReSharper disable once InconsistentNaming
        public const double κ = 0.5522847498307933984022516322796; 
    }
}
