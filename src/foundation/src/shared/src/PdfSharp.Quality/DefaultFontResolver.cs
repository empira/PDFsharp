using System.Diagnostics;
using PdfSharp.WPFonts;
using PdfSharp.Fonts;

namespace PdfSharp.Quality
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once InconsistentNaming
    public class DefaultFontResolver_unused : IFontResolver
    {
        public  FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
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
