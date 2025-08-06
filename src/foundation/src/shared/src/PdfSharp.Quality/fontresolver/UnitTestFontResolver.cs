// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO;
using PdfSharp.Fonts;

namespace PdfSharp.Quality
{
    /// <summary>
    /// The font resolver that provides fonts needed by the unit tests on any platform.
    /// The typeface names are case-sensitive by design.
    /// </summary>
    public class UnitTestFontResolver : IFontResolver
    {
        /// <summary>
        /// The minimum assets version required.
        /// </summary>
        public const int RequiredAssets = 1027;

        /// <summary>
        /// Name of the Emoji font supported by this font resolver.
        /// </summary>
        public const string EmojiFont = "Segoe UI Emoji";

        /// <summary>
        /// Name of the Arial font supported by this font resolver.
        /// </summary>
        public const string ArialFont = "Arial";

        /// <summary>
        /// Name of the Courier New font supported by this font resolver.
        /// </summary>
        public const string CourierFont = "Courier New";

        /// <summary>
        /// Name of the Lucida Console font supported by this font resolver.
        /// </summary>
        public const string LucidaFont = "Lucida Console";

        /// <summary>
        /// Name of the Symbol font supported by this font resolver.
        /// </summary>
        public const string SymbolFont = "Symbol";

        /// <summary>
        /// Name of the Times New Roman font supported by this font resolver.
        /// </summary>
        public const string TimesFont = "Times New Roman";

        /// <summary>
        /// Name of the Verdana font supported by this font resolver.
        /// </summary>
        public const string VerdanaFont = "Verdana";

        /// <summary>
        /// Name of the Wingdings font supported by this font resolver.
        /// </summary>
        public const string WingdingsFont = "Wingdings";

        // The face names.
        const string Arial = "arial";
        const string ArialB = "arialbd";
        const string ArialI = "ariali";
        const string ArialBI = "arialbi";
        const string Cour = "cour";
        const string CourB = "courbd";
        const string CourI = "couri";
        const string CourBI = "courbi";
        const string Times = "times";
        const string TimesB = "timesbd";
        const string TimesI = "timesi";
        const string TimesBI = "timesbi";
        const string Verdana = "verdana";
        const string VerdanaB = "verdanab";
        const string VerdanaI = "verdanai";
        const string VerdanaBI = "verdanaz";

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

                case ArialFont:
                    if (bold && italic)
                        return new FontResolverInfo(ArialBI);
                    if (bold)
                        return new FontResolverInfo(ArialB);
                    if (italic)
                        return new FontResolverInfo(ArialI);
                    return new FontResolverInfo(Arial);

                case CourierFont:
                    if (bold && italic)
                        return new FontResolverInfo(CourBI);
                    if (bold)
                        return new FontResolverInfo(CourB);
                    if (italic)
                        return new FontResolverInfo(CourI);
                    return new FontResolverInfo(Cour);

                case LucidaFont:
                    return new FontResolverInfo(LucidaFont);

                case SymbolFont:
                    return new FontResolverInfo(SymbolFont);

                case TimesFont:
                    if (bold && italic)
                        return new FontResolverInfo(TimesBI);
                    if (bold)
                        return new FontResolverInfo(TimesB);
                    if (italic)
                        return new FontResolverInfo(TimesI);
                    return new FontResolverInfo(Times);

                case VerdanaFont:
                    if (bold && italic)
                        return new FontResolverInfo(VerdanaBI);
                    if (bold)
                        return new FontResolverInfo(VerdanaB);
                    if (italic)
                        return new FontResolverInfo(VerdanaI);
                    return new FontResolverInfo(Verdana);

                case WingdingsFont:
                    return new FontResolverInfo(WingdingsFont);
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
                    {
                        var fontFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/fonts/SegoeUI");
                        var fontFile = Path.Combine(fontFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), "seguiemj.ttf");
                        var data = File.ReadAllBytes(fontFile);
                        return data;
                    }

                case Arial:
                case ArialB:
                case ArialI:
                case ArialBI:
                    {
                        var fontFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/fonts/Arial");
                        var fontFile = Path.Combine(fontFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), $"{faceName}.ttf");
                        var data = File.ReadAllBytes(fontFile);
                        return data;
                    }

                case Cour:
                case CourB:
                case CourI:
                case CourBI:
                    {
                        var fontFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/fonts/CourierNew");
                        var fontFile = Path.Combine(fontFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), $"{faceName}.ttf");
                        var data = File.ReadAllBytes(fontFile);
                        return data;
                    }

                case LucidaFont:
                {
                    var fontFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/fonts/LucidaConsole");
                    var fontFile = Path.Combine(fontFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), "lucon.ttf");
                    var data = File.ReadAllBytes(fontFile);
                    return data;
                }

                case SymbolFont:
                {
                    var fontFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/fonts/Symbol");
                    var fontFile = Path.Combine(fontFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), "symbol.ttf");
                    var data = File.ReadAllBytes(fontFile);
                    return data;
                }

                case Times:
                case TimesB:
                case TimesI:
                case TimesBI:
                    {
                        var fontFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/fonts/TimesNewRoman");
                        var fontFile = Path.Combine(fontFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), $"{faceName}.ttf");
                        var data = File.ReadAllBytes(fontFile);
                        return data;
                    }

                case Verdana:
                case VerdanaB:
                case VerdanaI:
                case VerdanaBI:
                    {
                        var fontFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/fonts/Verdana");
                        var fontFile = Path.Combine(fontFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), $"{faceName}.ttf");
                        var data = File.ReadAllBytes(fontFile);
                        return data;
                    }

                case WingdingsFont:
                {
                    var fontFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/fonts/Wingdings");
                    var fontFile = Path.Combine(fontFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), "wingding.ttf");
                    var data = File.ReadAllBytes(fontFile);
                    return data;
                }
            }
            return null;
        }
    }
}
