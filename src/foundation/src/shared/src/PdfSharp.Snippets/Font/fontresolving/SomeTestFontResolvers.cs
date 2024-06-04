// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Fonts;

namespace PdfSharp.Snippets.Font
{
    /// <summary>
    /// Returns null for every invocation.
    /// </summary>
    public class NullFontResolver : IFontResolver
    {
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic) => null;

        public Byte[]? GetFont(string faceName) => [];
    }

    /// <summary>
    /// A font resolver for unit tests that only resolves the font X-Files.
    /// X-Files is used for testing as a substitute font because it hit in the
    /// reader eye when used in a PDF file.
    /// Version 1 does not call the platform font resolver it X-Files cannot
    /// be resolved.
    /// </summary>
    public class TestXFilesFontResolver1 : IFontResolver
    {
        // The family name, the typeface name, the font face name, and the file name.
        const string XFiles = "xfiles";

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            const bool simulateBold = false;
            bool simulateItalic = isItalic;

            if (familyName.Equals(XFiles, StringComparison.OrdinalIgnoreCase))
                return new(XFiles, simulateBold, simulateItalic);

            return null;
        }

        public byte[]? GetFont(string faceName)
        {
            if (faceName.Equals(XFiles, StringComparison.OrdinalIgnoreCase))
                return ExoticFontsDataHelper.XFiles;

            return null;
        }
    }

    /// <summary>
    /// A font resolver for unit tests that only resolves the font XFiles.
    /// Version 2 tries PlatformFontResolver if the font to resolve is not X-Files.
    /// </summary>
    public class TestXFilesFontResolver2 : IFontResolver
    {
        // The family name, the typeface name, the font face name, and the file name.
        const string XFiles = "xfiles";

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            const bool simulateBold = false;
            bool simulateItalic = isItalic;

            if (familyName.Equals(XFiles, StringComparison.OrdinalIgnoreCase))
                return new(XFiles, simulateBold, simulateItalic);

            var result = PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
            return result;
        }

        public byte[]? GetFont(string faceName)
        {
            if (faceName.Equals(XFiles, StringComparison.OrdinalIgnoreCase))
                return ExoticFontsDataHelper.XFiles;

            return null;
        }
    }

    /// <summary>
    /// A font resolver for unit tests that only resolves the font XFiles.
    /// Version 3 throws an exception if font X-Files could not be resolved.
    /// </summary>
    public class TestXFilesFontResolver3 : IFontResolver
    {
        // The family name, the typeface name, the font face name, and the file name.
        const string XFiles = "xfiles";

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            const bool simulateBold = false;
            bool simulateItalic = isItalic;

            if (familyName.Equals(XFiles, StringComparison.OrdinalIgnoreCase))
                return new(XFiles, simulateBold, simulateItalic);

            // Illegally throw an exception.
            throw new ArgumentException("Illegal exception in font resolver.");
        }

        public byte[]? GetFont(string faceName)
        {
            if (faceName.Equals(XFiles, StringComparison.OrdinalIgnoreCase))
                return ExoticFontsDataHelper.XFiles;

            return null;
        }
    }
}
