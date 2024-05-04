using PdfSharp.Fonts;

namespace PdfSharp.Tests
{
    /// <summary>
    /// Used to resolve application-specific fonts
    /// </summary>
    public class ApplicationFontResolver : IFontResolver
    {
        private readonly Dictionary<string, byte[]> localFonts = [];
        private readonly List<string> searchPaths = [];

        /// <summary>
        /// Registers a new font
        /// </summary>
        /// <param name="fontName">The name of the font</param>
        /// <param name="fontData">The font-data</param>
        /// <param name="isBold">Specifies, whether the font is bold</param>
        /// <param name="isItalic">Specifies, whether the font is italic</param>
        public void Register(string fontName, byte[] fontData, bool isBold = false, bool isItalic = false)
        {
            var localName = MakeLocalName(fontName, isBold, isItalic);
            localFonts[localName] = fontData;
        }

        /// <summary>
        /// Registers a path in the file-system where the resolver should look for fonts
        /// </summary>
        /// <param name="path"></param>
        public void RegisterSearchPath(string path)
        {
            if (!searchPaths.Contains(path))
            {
                searchPaths.Add(path);
            }
        }

        /// <summary>
        /// Gets the data for the specified font.
        /// </summary>
        /// <param name="faceName">Name of the font</param>
        /// <returns>Font data or null, if no font with the specified name could be found</returns>
        public byte[]? GetFont(string faceName)
        {
            var result = Resolve(faceName, false, false);
            return result.Item1;
        }

        /// <summary>
        /// Get a <see cref="FontResolverInfo"/> for the specified font
        /// </summary>
        /// <param name="familyName">Name of the font</param>
        /// <param name="isBold"></param>
        /// <param name="isItalic"></param>
        /// <returns>A <see cref="FontResolverInfo"/> or null, if no font with the specified name could be found</returns>
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var result = Resolve(familyName, isBold, isItalic);
            return result.Item2;
        }

        private byte[]? GetFontFromFile(string faceName)
        {
            foreach (var fontLocation in searchPaths)
            {
                var filepath = Path.Combine(fontLocation, faceName + ".ttf");
                if (File.Exists(filepath))
                    return File.ReadAllBytes(filepath);
            }

            return null;
        }

        private static string MakeLocalName(string fontName, bool isBold, bool isItalic)
        {
            var localName = fontName;
            if (isBold || isItalic)
            {
                localName += "+";
                if (isBold)
                    localName += "b";
                if (isItalic)
                    localName += "i";
            }
            return localName;
        }

        private Tuple<byte[]?, FontResolverInfo?> Resolve(string fontName, bool isBold, bool isItalic)
        {
            var localName = MakeLocalName(fontName, isBold, isItalic);
            if (localFonts.TryGetValue(localName, out var localData))
                return new Tuple<byte[]?, FontResolverInfo?>(localData, new FontResolverInfo(fontName));

            localData = GetFontFromFile(fontName);
            if (localData != null)
                return new Tuple<byte[]?, FontResolverInfo?>(localData, new FontResolverInfo(fontName));

            return new Tuple<byte[]?, FontResolverInfo?>(null, null);
        }
    }
}
