// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using PdfSharp.Fonts;

namespace PdfSharp.Snippets.Font
{

    // BUG HACK

    /// <summary>
    /// Maps font requests for a Segoe WP, Segoe UI, or any other font to a bunch of 6 specific font files. These 6 fonts are embedded as resources in the WPFonts assembly.
    /// </summary>
    public class SegoeUiFontResolver : IFontResolver
    {
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            familyName = familyName.Replace("UI", "WP").Replace("ui", "WP");
            var info = _segoeWpFontResolver.ResolveTypeface(familyName, isBold, isItalic)
                      ?? _segoeWpFontResolver.ResolveTypeface(SegoeWpFontResolver.FamilyNames.SegoeWP, isBold, isItalic);

            return info;
        }

        public byte[]? GetFont(string faceName)
        {
            return _segoeWpFontResolver.GetFont(faceName);
        }

        readonly SegoeWpFontResolver _segoeWpFontResolver = new();
    }
}
