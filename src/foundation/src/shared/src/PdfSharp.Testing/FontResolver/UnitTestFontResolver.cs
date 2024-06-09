// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Fonts;
using PdfSharp.Quality;

namespace PdfSharp.FontResolver
{
    /// <summary>
    /// The font resolver that provides fonts needed by the unit tests which are not available from the PlatformFontResolver.
    /// </summary>
    public class UnitTestFontResolver : IFontResolver
    {
        /// <summary>
        /// The minimum assets version required.
        /// </summary>
        public const int RequiredAssets = 1009;

        /// <summary>
        /// Name of the Emoji font supported by this font resolver.
        /// </summary>
        public const string EmojiFont = "Segoe UI Emoji";

        /// <summary>
        /// Creates a new instance of UnitTestFontResolver.
        /// </summary>
        public UnitTestFontResolver()
        {
            IOUtility.EnsureAssetsVersion(RequiredAssets);
        }

        /// <summary>
        /// Converts specified information about a required typeface into a specific font.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="bold">Set to <c>true</c> when a bold font face is required.</param>
        /// <param name="italic">Set to <c>true</c> when an italic font face is required.</param>
        /// <returns>Information about the physical font, or null if the request cannot be satisfied.</returns>
        public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        {
            switch (familyName)
            {
                case EmojiFont:
                    return new FontResolverInfo(EmojiFont);
            }

            return PlatformFontResolver.ResolveTypeface(familyName, bold, italic);
        }

        /// <summary>
        /// Gets the bytes of a physical font with specified face name.
        /// </summary>
        /// <param name="faceName">A face name previously retrieved by ResolveTypeface.</param>
        public Byte[]? GetFont(string faceName)
        {
            switch (faceName)
            {
                case EmojiFont:
                    var fontFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/fonts/SegoeUI");
                    var emojiFile = Path.Combine(fontFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), "seguiemj.ttf");
                    var data = File.ReadAllBytes(emojiFile);
                    return data;
            }
            return null;
        }
    }
}
