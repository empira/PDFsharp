// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// Re- Sharper disable RedundantNameQualifier

using System.Runtime.InteropServices;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Used in Core build only if no custom FontResolver and no FallbackFontResolver set
    /// and UseWindowsFontsUnderWindows or UseWindowsFontsUnderWsl2 is set.
    /// </summary>
    class WindowsPlatformFontResolver : IFontResolver
    {
        // This path is carved in stone. Even Microsoft hard-coded it in the source code of WPF.
        const string WindowsFontsPath = @"C:\Windows\Fonts";
        const string WindowsFontsPathUnderWsl2 = "/mnt/c/Windows/Fonts";

        class TypefaceInfo
        {
            public string FontFaceName { get; init; }

            public FontSimulation Simulation { get; init; }

            public string WindowsFileName { get; init; }

            internal TypefaceInfo(
                string fontFaceName,
                FontSimulation fontSimulation,
                string windowsFileName)
            {
                FontFaceName = fontFaceName;
                Simulation = fontSimulation;
                WindowsFileName = windowsFileName;
            }

            [Flags]
            public enum FontSimulation
            {
                None = 0,
                Bold = 1,
                Italic = 2,
                Both = Bold | Italic
            }
        }

        static readonly List<TypefaceInfo> TypefaceInfos =
        [
            // ReSharper disable StringLiteralTypo

            new("Arial", TypefaceInfo.FontSimulation.None, "arial"),
            new("Arial Black", TypefaceInfo.FontSimulation.None, "ariblk"),
            new("Arial Bold", TypefaceInfo.FontSimulation.None, "arialbd"),
            new("Arial Italic", TypefaceInfo.FontSimulation.None, "ariali"),
            new("Arial Bold Italic", TypefaceInfo.FontSimulation.None, "arialbi"),

            new("Times New Roman", TypefaceInfo.FontSimulation.None, "times"),
            new("Times New Roman Bold", TypefaceInfo.FontSimulation.None, "timesbd"),
            new("Times New Roman Italic", TypefaceInfo.FontSimulation.None, "timesi"),
            new("Times New Roman Bold Italic", TypefaceInfo.FontSimulation.None, "timesbi"),

            new("Courier New", TypefaceInfo.FontSimulation.None, "cour"),
            new("Courier New Bold", TypefaceInfo.FontSimulation.None, "courbd"),
            new("Courier New Italic", TypefaceInfo.FontSimulation.None, "couri"),
            new("Courier New Bold Italic", TypefaceInfo.FontSimulation.None, "courbi"),

            new("Verdana", TypefaceInfo.FontSimulation.None, "verdana"),
            new("Verdana Bold", TypefaceInfo.FontSimulation.None, "verdanab"),
            new("Verdana Italic", TypefaceInfo.FontSimulation.None, "verdanai"),
            new("Verdana Bold Italic", TypefaceInfo.FontSimulation.None, "verdanaz"),

            new("Lucida Console", TypefaceInfo.FontSimulation.None, "lucon"),
            new("Lucida Console Bold", TypefaceInfo.FontSimulation.Bold, "lucon"),
            new("Lucida Console Italic", TypefaceInfo.FontSimulation.Italic, "lucon"),
            new("Lucida Console Bold Italic", TypefaceInfo.FontSimulation.Both, "lucon"),

            new("Symbol", TypefaceInfo.FontSimulation.None, "symbol"),
            new("Symbol Bold", TypefaceInfo.FontSimulation.Bold, "symbol"),
            new("Symbol Italic", TypefaceInfo.FontSimulation.Italic, "symbol"),
            new("Symbol Bold Italic", TypefaceInfo.FontSimulation.Both, "symbol"),

            // ReSharper restore StringLiteralTypo
        ];

        // Returns a PlatformFontResolverInfo.
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var typefaces = TypefaceInfos.Where(f => f.FontFaceName.StartsWith(familyName, StringComparison.OrdinalIgnoreCase));

            if (isBold)
                typefaces = typefaces.Where(f => f.FontFaceName.Contains("bold", StringComparison.OrdinalIgnoreCase));

            if (isItalic)
                typefaces = typefaces.Where(f => f.FontFaceName.Contains("italic", StringComparison.OrdinalIgnoreCase));

            var family = typefaces.FirstOrDefault();

            if (family is not null)
                return new PlatformFontResolverInfo(family.WindowsFileName,
                    (family.Simulation & TypefaceInfo.FontSimulation.Bold) != 0,
                    (family.Simulation & TypefaceInfo.FontSimulation.Italic) != 0);

            return null;
        }

        public byte[]? GetFont(string faceName)
        {
            // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract because it can be null.
            _fontsPath ??= Capabilities.OperatingSystem.IsWindows
                ? WindowsFontsPath
                : WindowsFontsPathUnderWsl2;

            var filepath = Path.Combine(_fontsPath, faceName + ".ttf");
            if (File.Exists(filepath))
                return File.ReadAllBytes(filepath);
            return null;
        }
        static string _fontsPath = null!;
    }
}
