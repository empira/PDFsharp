// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace PdfSharp.Drawing
{
    enum XFontStretchValue
    {
        // Values taken from WPF.
        UltraCondensed = 1,
        ExtraCondensed = 2,
        Condensed = 3,
        SemiCondensed = 4,
        Normal = 5,
        SemiExpanded = 6,
        Expanded = 7,
        ExtraExpanded = 8,
        UltraExpanded = 9,
    }

    /// <summary>Provides a set of static predefined <see cref="T:System.Windows.XFontStretch" /> values.</summary>
    public static class XFontStretches
    {
        /// <summary>Specifies an ultra-condensed <see cref="T:System.Windows.XFontStretch" />.</summary>
        /// <returns>A value that represents an ultra-condensed <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static XFontStretch UltraCondensed => new XFontStretch(XFontStretchValue.UltraCondensed);

        /// <summary>Specifies an extra-condensed <see cref="T:System.Windows.XFontStretch" />.</summary>
        /// <returns>A value that represents an extra-condensed <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static XFontStretch ExtraCondensed => new XFontStretch(XFontStretchValue.ExtraCondensed);

        /// <summary>Specifies a condensed <see cref="T:System.Windows.XFontStretch" />.</summary>
        /// <returns>A value that represents a condensed <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static XFontStretch Condensed => new XFontStretch(XFontStretchValue.Condensed);

        /// <summary>Specifies a semi-condensed <see cref="T:System.Windows.XFontStretch" />.</summary>
        /// <returns>A value that represents a semi-condensed <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static XFontStretch SemiCondensed => new XFontStretch(XFontStretchValue.SemiCondensed);

        /// <summary>Specifies a normal <see cref="T:System.Windows.XFontStretch" />.</summary>
        /// <returns>A value that represents a normal <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static XFontStretch Normal => new XFontStretch(XFontStretchValue.Normal);

        /// <summary>Specifies a medium <see cref="T:System.Windows.XFontStretch" />.</summary>
        /// <returns>A value that represents a medium <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static XFontStretch Medium => new XFontStretch(XFontStretchValue.Normal);

        /// <summary>Specifies a semi-expanded <see cref="T:System.Windows.XFontStretch" />.</summary>
        /// <returns>A value that represents a semi-expanded <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static XFontStretch SemiExpanded => new XFontStretch(XFontStretchValue.SemiExpanded);

        /// <summary>Specifies an expanded <see cref="T:System.Windows.XFontStretch" />.</summary>
        /// <returns>A value that represents an expanded <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static XFontStretch Expanded => new XFontStretch(XFontStretchValue.ExtraCondensed);

        /// <summary>Specifies an extra-expanded <see cref="T:System.Windows.XFontStretch" />.</summary>
        /// <returns>A value that represents an extra-expanded <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static XFontStretch ExtraExpanded => new XFontStretch(XFontStretchValue.ExtraExpanded);

        /// <summary>Specifies an ultra-expanded <see cref="T:System.Windows.XFontStretch" />.</summary>
        /// <returns>A value that represents an ultra-expanded <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static XFontStretch UltraExpanded => new XFontStretch(XFontStretchValue.UltraExpanded);

        internal static bool XFontStretchStringToKnownStretch(string stretch, IFormatProvider provider, ref XFontStretch xFontStretch)
        {
            switch (stretch.Length)
            {
                case 6:
                    if (stretch.Equals("Normal", StringComparison.OrdinalIgnoreCase))
                    {
                        xFontStretch = Normal;
                        return true;
                    }
                    if (stretch.Equals("Medium", StringComparison.OrdinalIgnoreCase))
                    {
                        xFontStretch = Medium;
                        return true;
                    }
                    break;

                case 8:
                    if (stretch.Equals("Expanded", StringComparison.OrdinalIgnoreCase))
                    {
                        xFontStretch = Expanded;
                        return true;
                    }
                    break;

                case 9:
                    if (stretch.Equals("Condensed", StringComparison.OrdinalIgnoreCase))
                    {
                        xFontStretch = Condensed;
                        return true;
                    }
                    break;

                case 12:
                    if (stretch.Equals("SemiExpanded", StringComparison.OrdinalIgnoreCase))
                    {
                        xFontStretch = SemiExpanded;
                        return true;
                    }
                    break;

                case 13:
                    if (stretch.Equals("SemiCondensed", StringComparison.OrdinalIgnoreCase))
                    {
                        xFontStretch = SemiCondensed;
                        return true;
                    }
                    if (stretch.Equals("ExtraExpanded", StringComparison.OrdinalIgnoreCase))
                    {
                        xFontStretch = ExtraExpanded;
                        return true;
                    }
                    if (stretch.Equals("UltraExpanded", StringComparison.OrdinalIgnoreCase))
                    {
                        xFontStretch = UltraExpanded;
                        return true;
                    }
                    break;

                case 14:
                    if (stretch.Equals("UltraCondensed", StringComparison.OrdinalIgnoreCase))
                    {
                        xFontStretch = UltraCondensed;
                        return true;
                    }
                    if (stretch.Equals("ExtraCondensed", StringComparison.OrdinalIgnoreCase))
                    {
                        xFontStretch = ExtraCondensed;
                        return true;
                    }
                    break;
            }

            if (!Int32.TryParse(stretch, NumberStyles.Integer, provider, out var result))
                return false;
            xFontStretch = XFontStretch.FromOpenTypeStretch(result);
            return true;
        }

        internal static bool XFontStretchToString(XFontStretchValue stretch, out string? convertedValue)
        {
            switch (stretch)
            {
                case XFontStretchValue.UltraCondensed:
                    convertedValue = "UltraCondensed";
                    return true;

                case XFontStretchValue.ExtraCondensed:
                    convertedValue = "ExtraCondensed";
                    return true;

                case XFontStretchValue.Condensed:
                    convertedValue = "Condensed";
                    return true;

                case XFontStretchValue.SemiCondensed:
                    convertedValue = "SemiCondensed";
                    return true;

                case XFontStretchValue.Normal:
                    convertedValue = "Normal";
                    return true;

                case XFontStretchValue.SemiExpanded:
                    convertedValue = "SemiExpanded";
                    return true;

                case XFontStretchValue.Expanded:
                    convertedValue = "Expanded";
                    return true;

                case XFontStretchValue.ExtraExpanded:
                    convertedValue = "ExtraExpanded";
                    return true;

                case XFontStretchValue.UltraExpanded:
                    convertedValue = "UltraExpanded";
                    return true;

                default:
                    convertedValue = null;
                    return false;
            }
        }
    }
}
