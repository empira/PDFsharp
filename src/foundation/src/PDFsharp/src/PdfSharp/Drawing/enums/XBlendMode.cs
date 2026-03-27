// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// PDF blend modes as specified in the PDF Reference for the /BM entry
    /// in a graphics state parameter dictionary.
    /// </summary>
    public enum XBlendMode
    {
        /// <summary>
        /// The Normal blend mode. This is the default.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The Multiply blend mode. Multiplies the source and destination colors,
        /// producing a darker result.
        /// </summary>
        Multiply = 1,

        /// <summary>
        /// The Screen blend mode. Complements, multiplies, and complements again,
        /// producing a lighter result.
        /// </summary>
        Screen = 2,

        /// <summary>
        /// The Overlay blend mode. Combines Multiply and Screen depending on
        /// the destination color, preserving highlights and shadows.
        /// </summary>
        Overlay = 3,

        /// <summary>
        /// The Darken blend mode. Selects the darker of the source and destination colors.
        /// </summary>
        Darken = 4,

        /// <summary>
        /// The Lighten blend mode. Selects the lighter of the source and destination colors.
        /// </summary>
        Lighten = 5,

        /// <summary>
        /// The ColorDodge blend mode. Brightens the destination color to reflect
        /// the source color.
        /// </summary>
        ColorDodge = 6,

        /// <summary>
        /// The ColorBurn blend mode. Darkens the destination color to reflect
        /// the source color.
        /// </summary>
        ColorBurn = 7,

        /// <summary>
        /// The HardLight blend mode. Combines Multiply and Screen depending on
        /// the source color, similar to shining a harsh spotlight on the destination.
        /// </summary>
        HardLight = 8,

        /// <summary>
        /// The SoftLight blend mode. Darkens or lightens depending on the source color,
        /// similar to shining a diffused spotlight on the destination.
        /// </summary>
        SoftLight = 9,

        /// <summary>
        /// The Difference blend mode. Subtracts the darker of the two colors from
        /// the lighter color.
        /// </summary>
        Difference = 10,

        /// <summary>
        /// The Exclusion blend mode. Similar to Difference but with lower contrast.
        /// </summary>
        Exclusion = 11,

        /// <summary>
        /// The Hue blend mode. Uses the hue of the source color with the saturation
        /// and luminosity of the destination color.
        /// </summary>
        Hue = 12,

        /// <summary>
        /// The Saturation blend mode. Uses the saturation of the source color with
        /// the hue and luminosity of the destination color.
        /// </summary>
        Saturation = 13,

        /// <summary>
        /// The Color blend mode. Uses the hue and saturation of the source color with
        /// the luminosity of the destination color.
        /// </summary>
        Color = 14,

        /// <summary>
        /// The Luminosity blend mode. Uses the luminosity of the source color with
        /// the hue and saturation of the destination color.
        /// </summary>
        Luminosity = 15
    }
}