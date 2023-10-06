// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;

namespace PdfSharp.Drawing
{
    enum FontWeightValue
    {
        // Values taken from WPF.
        Thin = 100,
        ExtraLight = 200,
        Light = 300,
        Normal = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        ExtraBold = 800,
        Black = 900,
        ExtraBlack = 950,
    }

    /// <summary>
    /// Defines a set of static predefined XFontWeight values.
    /// </summary>
    public static class XFontWeights
    {
        internal static bool FontWeightStringToKnownWeight(string s, IFormatProvider provider, ref XFontWeight fontWeight)
        {
            switch (s.ToLowerInvariant())
            {
                case "thin":
                    fontWeight = Thin;
                    return true;

                case "extralight":
                    fontWeight = ExtraLight;
                    return true;

                case "ultralight":
                    fontWeight = UltraLight;
                    return true;

                case "light":
                    fontWeight = Light;
                    return true;

                case "normal":
                    fontWeight = Normal;
                    return true;

                case "regular":
                    fontWeight = Regular;
                    return true;

                case "medium":
                    fontWeight = Medium;
                    return true;

                case "semibold":
                    fontWeight = SemiBold;
                    return true;

                case "demibold":
                    fontWeight = DemiBold;
                    return true;

                case "bold":
                    fontWeight = Bold;
                    return true;

                case "extrabold":
                    fontWeight = ExtraBold;
                    return true;

                case "ultrabold":
                    fontWeight = UltraBold;
                    return true;

                case "heavy":
                    fontWeight = Heavy;
                    return true;

                case "black":
                    fontWeight = Black;
                    return true;

                case "extrablack":
                    fontWeight = ExtraBlack;
                    return true;

                case "ultrablack":
                    fontWeight = UltraBlack;
                    return true;
            }

            if (Int32.TryParse(s, NumberStyles.Integer, provider, out var weight))
            {
                fontWeight = new XFontWeight(weight);
                return true;
            }
            return false;
        }

        internal static bool FontWeightToString(int weight, /*[NotNullWhen(true)]*/ out string? convertedValue)
        {
            switch (weight)
            {
                case (int)FontWeightValue.Thin:
                    convertedValue = "Thin";
                    return true;

                case (int)FontWeightValue.ExtraLight:
                    convertedValue = "ExtraLight";
                    return true;

                case (int)FontWeightValue.Light:
                    convertedValue = "Light";
                    return true;

                case (int)FontWeightValue.Normal:
                    convertedValue = "Normal";
                    return true;

                case (int)FontWeightValue.Medium:
                    convertedValue = "Medium";
                    return true;

                case (int)FontWeightValue.SemiBold:
                    convertedValue = "SemiBold";
                    return true;

                case (int)FontWeightValue.Bold:
                    convertedValue = "Bold";
                    return true;

                case (int)FontWeightValue.ExtraBold:
                    convertedValue = "ExtraBold";
                    return true;

                case (int)FontWeightValue.Black:
                    convertedValue = "Black";
                    return true;

                case (int)FontWeightValue.ExtraBlack:
                    convertedValue = "ExtraBlack";
                    return true;
            }
            convertedValue = null;
            return false;
        }

        /// <summary>
        /// Specifies a "Thin" font weight.
        /// </summary>
        public static XFontWeight Thin => new XFontWeight((int)FontWeightValue.Thin);

        /// <summary>
        /// Specifies an "ExtraLight" font weight.
        /// </summary>
        public static XFontWeight ExtraLight => new XFontWeight((int)FontWeightValue.ExtraLight);

        /// <summary>
        /// Specifies an "UltraLight" font weight.
        /// </summary>
        public static XFontWeight UltraLight => new XFontWeight((int)FontWeightValue.ExtraLight);

        /// <summary>
        /// Specifies a "Light" font weight.
        /// </summary>
        public static XFontWeight Light => new XFontWeight((int)FontWeightValue.Light);

        /// <summary>
        /// Specifies a "Normal" font weight.
        /// </summary>
        public static XFontWeight Normal => new XFontWeight((int)FontWeightValue.Normal);

        /// <summary>
        /// Specifies a "Regular" font weight.
        /// </summary>
        public static XFontWeight Regular => new XFontWeight((int)FontWeightValue.Normal);

        /// <summary>
        /// Specifies a "Medium" font weight.
        /// </summary>
        public static XFontWeight Medium => new XFontWeight((int)FontWeightValue.Medium);

        /// <summary>
        /// Specifies a "SemiBold" font weight.
        /// </summary>
        public static XFontWeight SemiBold => new XFontWeight((int)FontWeightValue.SemiBold);

        /// <summary>
        /// Specifies a "DemiBold" font weight.
        /// </summary>
        public static XFontWeight DemiBold => new XFontWeight((int)FontWeightValue.SemiBold);

        /// <summary>
        /// Specifies a "Bold" font weight.
        /// </summary>
        public static XFontWeight Bold => new XFontWeight((int)FontWeightValue.Bold);

        /// <summary>
        /// Specifies a "ExtraBold" font weight.
        /// </summary>
        public static XFontWeight ExtraBold => new XFontWeight((int)FontWeightValue.ExtraBold);

        /// <summary>
        /// Specifies a "UltraBold" font weight.
        /// </summary>
        public static XFontWeight UltraBold => new XFontWeight((int)FontWeightValue.ExtraBold);

        /// <summary>
        /// Specifies a "Heavy" font weight.
        /// </summary>
        public static XFontWeight Heavy => new XFontWeight((int)FontWeightValue.Black);

        /// <summary>
        /// Specifies a "Black" font weight.
        /// </summary>
        public static XFontWeight Black => new XFontWeight((int)FontWeightValue.Black);

        /// <summary>
        /// Specifies a "ExtraBlack" font weight.
        /// </summary>
        public static XFontWeight ExtraBlack => new XFontWeight((int)FontWeightValue.ExtraBlack);

        /// <summary>
        /// Specifies a "UltraBlack" font weight.
        /// </summary>
        public static XFontWeight UltraBlack => new XFontWeight((int)FontWeightValue.ExtraBlack);
    }
}
