// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Fonts;

namespace PdfSharp.Snippets.Font
{
    /// <summary>
    /// This font resolver maps each request to a valid font face of the SegoeWP fonts.
    /// </summary>
    public class FailsafeFontResolver : IFontResolver
    {
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string typefaceName = 
                $"{familyName}{(isBold ? " bold" : "")}{(isItalic ? " italic" : "")}";

            // Use either SegoeWP or SegoeWPBold.
            var result = SegoeWpFontResolver.ResolveTypeface(
                isBold 
                    ? SegoeWpFontResolver.FamilyNames.SegoeWPBold 
                    : SegoeWpFontResolver.FamilyNames.SegoeWP,
                false, isItalic);

            Debug.Assert(result != null);

            // No use of LoggerMessages here because this code is only for
            // demonstration purposes.
            PdfSharpLogHost.FontManagementLogger.LogWarning(
                $"{typefaceName} was substituted by a SegoeWP font.");

            return result;
        }

        public byte[]? GetFont(string faceName)
        {
            PdfSharpLogHost.FontManagementLogger.LogInformation($"Get font for '{faceName}'.");

            return SegoeWpFontResolver.GetFont(faceName);
        }

        static readonly SegoeWpFontResolver SegoeWpFontResolver = new();
    }
}
