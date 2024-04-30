// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Fonts;

namespace PdfSharp.Snippets.Font
{
    /// <summary>
    /// Maps font requests for a SegoeWP font to a set of 6 specific font files.
    /// These 6 fonts faces are embedded as resources in the WPFonts assembly.
    /// </summary>
    public class SegoeWpFontResolver : IFontResolver
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// The font family names that can be used in the constructor of XFont.
        /// Used in the first parameter of ResolveTypeface.
        /// Family names are given in lower case because the implementation of
        /// SegoeWpFontResolver ignores case.
        /// </summary>
        public static class FamilyNames
        {
            // This implementation considers each font face as its own family.
            public const string SegoeWPLight = "segoe wp light";
            public const string SegoeWPSemilight = "segoe wp semilight";
            public const string SegoeWP = "segoe wp";
            public const string SegoeWPSemibold = "segoe wp semibold";
            public const string SegoeWPBold = "segoe wp bold";
            public const string SegoeWPBlack = "segoe wp black";
        }

        /// <summary>
        /// The internal names that uniquely identify the font faces.
        /// Used in the first parameter of the FontResolverInfo constructor.
        /// </summary>
        static class FaceNames
        {
            // Used in the first parameter of the FontResolverInfo constructor.
            public const string SegoeWPLight = "SegoeWPLight";
            public const string SegoeWPSemilight = "SegoeWPSemilight";
            public const string SegoeWP = "SegoeWP";
            public const string SegoeWPSemibold = "SegoeWPSemibold";
            public const string SegoeWPBold = "SegoeWPBold";
            public const string SegoeWPBlack = "SegoeWPBlack";
        }

        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Selects a physical font face based on the specified information
        /// of a required typeface.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="isBold">Set to <c>true</c> when a bold font face
        ///  is required.</param>
        /// <param name="isItalic">Set to <c>true</c> when an italic font face 
        /// is required.</param>
        /// <returns>
        /// Information about the physical font, or null if the request cannot be satisfied.
        /// </returns>
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Note: PDFsharp calls ResolveTypeface only once for each unique combination
            // of familyName, isBold, and isItalic.

            string lowercaseFamilyName = familyName.ToLowerInvariant();

            // Looking for a Segoe WP font?
            if (lowercaseFamilyName.StartsWith("segoe wp", StringComparison.Ordinal))
            {
                // Bold simulation is not yet implemented in PDFsharp.
                const bool simulateBold = false;

                string faceName;

                // In this sample family names are case-sensitive. You can relax this
                // in your own implementation and make them case-insensitive.
                switch (lowercaseFamilyName)
                {
                    case FamilyNames.SegoeWPLight:
                        // Just for demonstration use 'Semilight' if bold is requested.
                        if (isBold)
                            goto case FamilyNames.SegoeWPSemilight;
                        faceName = FaceNames.SegoeWPLight;
                        break;

                    case FamilyNames.SegoeWPSemilight:
                        // Do not care about bold for semilight.
                        faceName = FaceNames.SegoeWPSemilight;
                        break;

                    case FamilyNames.SegoeWP:
                        // Use font 'Bold' if bold is requested.
                        if (isBold)
                            goto UseSegoeWPBold;
                        faceName = FaceNames.SegoeWP;
                        break;

                    case FamilyNames.SegoeWPSemibold:
                        // Do not care about bold for semibold.
                        faceName = FaceNames.SegoeWPSemibold;
                        break;

                    case FamilyNames.SegoeWPBold:
                        // Just for demonstration use font 'Black' if bold is requested.
                        if (isBold)
                            goto case FamilyNames.SegoeWPBlack;
                        UseSegoeWPBold:
                        faceName = FaceNames.SegoeWPBold;
                        break;

                    case FamilyNames.SegoeWPBlack:
                        // Do not care about bold for black.
                        faceName = FaceNames.SegoeWPBlack;
                        break;

                    default:
                        return null;
                }

                // Tell PDFsharp the effective face name and whether italic should be
                // simulated. Since Segoe WP typefaces do not contain any italic font
                // always simulate italic if it is requested.
                return new FontResolverInfo(faceName, simulateBold, isItalic);
            }

            // Return null means that the typeface cannot be resolved and PDFsharp forwards
            // the typeface request depending on PDFsharp build flavor and operating system.
            // Alternatively forward call to PlatformFontResolver.
            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }

        /// <summary>
        /// Gets the bytes of a physical font face with specified face name.
        /// </summary>
        /// <param name="faceName">A face name previously retrieved by ResolveTypeface.</param>
        /// <returns>
        /// The bits of the font.
        /// </returns>
        public byte[]? GetFont(string faceName)
        {
            // Note: PDFsharp never calls GetFont twice with the same face name.
            // Note: If a typeface is resolved by the PlatformFontResolver.ResolveTypeface
            //       you never come here.

            // Return the bytes of a font.
            return faceName switch
            {
                FaceNames.SegoeWPLight => PdfSharp.WPFonts.FontDataHelper.SegoeWPLight,
                FaceNames.SegoeWPSemilight => PdfSharp.WPFonts.FontDataHelper.SegoeWPSemilight,
                FaceNames.SegoeWP => PdfSharp.WPFonts.FontDataHelper.SegoeWP,
                FaceNames.SegoeWPSemibold => PdfSharp.WPFonts.FontDataHelper.SegoeWPSemibold,
                FaceNames.SegoeWPBold => PdfSharp.WPFonts.FontDataHelper.SegoeWPBold,
                FaceNames.SegoeWPBlack => PdfSharp.WPFonts.FontDataHelper.SegoeWPBlack,
                // PDFsharp never calls GetFont with a face name that was not returned
                // by ResolveTypeface.
                _ => null  // Never comes here, but compiler needs it to be happy.
            };
        }
    }
}
