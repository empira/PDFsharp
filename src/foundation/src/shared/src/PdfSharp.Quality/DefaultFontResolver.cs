// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.WPFonts;
using PdfSharp.Fonts;

namespace PdfSharp.Quality
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="PdfSharp.Fonts.IFontResolver" />
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once InconsistentNaming    
    public class DefaultFontResolver_unused : IFontResolver
    {
        /// <summary>
        /// Converts specified information about a required typeface into a specific font.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="isBold">Set to <c>true</c> when a bold fontface is required.</param>
        /// <param name="isItalic">Set to <c>true</c> when an italic fontface is required.</param>
        /// <returns>
        /// Information about the physical font, or null if the request cannot be satisfied.
        /// </returns>
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var name = familyName.ToLower();

#if DEBUG
            //if (name == "segoe ui symbol")
            //    Debug ger.Break();
#endif

            switch (name)
            {
                case "segoe wp light":
                    return new FontResolverInfo("SegoeWPLight#");

                case "segoe wp semilight":
                    return new FontResolverInfo("SegoeWPSemilight#");

                case "segoe wp":
                    if (isBold)
                    {
                        return new FontResolverInfo("SegoeWP#b");
                    }
                    return new FontResolverInfo("SegoeWP#");

                case "segoe wp semibold":
                    return new FontResolverInfo("SegoeWPSemiBold#");

                case "segoe wp black":
                    return new FontResolverInfo("SegoeWPBlack#");
            }
            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }

        /// <summary>
        /// Gets the bytes of a physical font with specified face name.
        /// </summary>
        /// <param name="faceName">A face name previously retrieved by ResolveTypeface.</param>
        public byte[] GetFont(string faceName)
        {
            return faceName switch
            {
                "SegoeWPLight#" => FontDataHelper.SegoeWPLight,
                "SegoeWPSemilight#" => FontDataHelper.SegoeWPSemilight,
                "SegoeWP#" => FontDataHelper.SegoeWP,
                "SegoeWP#b" => FontDataHelper.SegoeWPBold,
                "SegoeWPSemiBold#" => FontDataHelper.SegoeWPSemibold,
                "SegoeWPBlack#" => FontDataHelper.SegoeWPBlack,
                _ => throw new ArgumentException($"'{faceName}' is not a known face name.")
            };
        }
    }
}
