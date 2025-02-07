using PdfSharp.Pdf.AcroForms;

namespace PdfSharp.Fonts.StandardFonts
{
    /// <summary>
    /// Provides the data for the 14 Standard-Fonts defined in the PDF-specification.<br></br>
    /// Pdf spec. 1.7, Chapter 9.6.2.2: Standard Type 1 Fonts (Standard 14 Fonts)<br></br>
    /// Mainly intended for existing documents which use one or more of the standard-fonts without embedding them.<br></br>
    /// Most often used in fillable forms, i.e. a <see cref="PdfAcroForm"/>
    /// </summary>
    public static class StandardFontData
    {
        /// <summary>
        /// Gets the data for the specified font
        /// </summary>
        /// <param name="fontName">Name of the font. A leading slash is automatically stripped</param>
        /// <returns>Font-data or null if a font with the specified name could not be found</returns>
        public static byte[]? GetFontData(string fontName)
        {
            if (string.IsNullOrWhiteSpace(fontName))
                return null;

            // if the name comes from a resource-dictionary...
            if (fontName.StartsWith("/"))
                fontName = fontName.TrimStart('/');

            if (fontData.TryGetValue(fontName, out var data))
                return data;
            return null;
        }

        /// <summary>
        /// Indicates, whether the specified <paramref name="fontName"/> is one of the 14 Standard-Fonts
        /// </summary>
        /// <param name="fontName">Font-name to check. A leading slash it automatically stripped</param>
        /// <returns>true, if <paramref name="fontName"/> is one of the 14 Standard-Fonts, otherwise false</returns>
        public static bool IsStandardFont(string fontName)
        {
            if (string.IsNullOrWhiteSpace(fontName))
                return false;
            return FontNames.Contains(fontName.TrimStart('/'));
        }

        /// <summary>
        /// Get the names of the supported standard-fonts
        /// </summary>
        public static IEnumerable<string> FontNames => fontData.Keys;

        private static readonly Dictionary<string, byte[]> fontData = new()
        {
            { StandardFontNames.Courier, FontResources.NimbusMonoPS_Regular },
            { StandardFontNames.CourierBold, FontResources.NimbusMonoPS_Bold },
            { StandardFontNames.CourierItalic, FontResources.NimbusMonoPS_Italic },
            { StandardFontNames.CourierBoldItalic, FontResources.NimbusMonoPS_BoldItalic },

            { StandardFontNames.Helvetica, FontResources.NimbusSans_Regular },
            { StandardFontNames.HelveticaBold, FontResources.NimbusSans_Bold },
            { StandardFontNames.HelveticaItalic, FontResources.NimbusSans_Italic },
            { StandardFontNames.HelveticaBoldItalic, FontResources.NimbusSans_BoldItalic },

            { StandardFontNames.Times, FontResources.NimbusRoman_Regular },
            { StandardFontNames.TimesBold, FontResources.NimbusRoman_Bold },
            { StandardFontNames.TimesItalic, FontResources.NimbusRoman_Italic },
            { StandardFontNames.TimesBoldItalic, FontResources.NimbusRoman_BoldItalic },

            { StandardFontNames.ZapfDingbats, FontResources.D050000L },
            { StandardFontNames.Symbol, FontResources.StandardSymbolsPS }
        };
    }
}
