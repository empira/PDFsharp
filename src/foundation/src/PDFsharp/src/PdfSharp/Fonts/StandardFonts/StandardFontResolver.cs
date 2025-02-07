using PdfSharp.Drawing;
using PdfSharp.Fonts.OpenType;
using System.Diagnostics.CodeAnalysis;

namespace PdfSharp.Fonts.StandardFonts
{
    /// <summary>
    /// Resolves the 14 standard-fonts and fonts that were pre-registered
    /// </summary>
    public class StandardFontResolver : IFontResolver
    {
        private readonly Dictionary<string, byte[]> localFonts = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> registeredFontFiles = new(StringComparer.OrdinalIgnoreCase);
        private string? fallbackFontName;
        private readonly Dictionary<string, HashSet<int>> characterSets = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates a new instance of the <see cref="StandardFontResolver"/>
        /// </summary>
        public StandardFontResolver()
        {
            // register character-sets of standard-fonts
            foreach (var fontName in StandardFontData.FontNames)
            {
                var fontData = StandardFontData.GetFontData(fontName)!;
                var fontSource = XFontSource.GetOrCreateFrom(fontData, false);
                var typeFace = new OpenTypeFontFace(fontSource);
                var characterSet = typeFace.cmap.GetSupportedCharacters();
                characterSets[fontName] = characterSet;
            }
        }
        
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
            // get the name stored in the font itself
            var fontSource = XFontSource.GetOrCreateFrom(fontData, false);
            var typeFace = new OpenTypeFontFace(fontSource);
            if (!string.IsNullOrEmpty(typeFace.FullFaceName))
            {
                localFonts[typeFace.FullFaceName] = fontData;
                var characterSet = typeFace.cmap.GetSupportedCharacters();
                characterSets[typeFace.FullFaceName] = characterSet;
            }
        }

        /// <summary>
        /// Registers all (TrueType)-fonts from the specified folder and all sub-folders.<br></br>
        /// In an <see cref="XFont"/>, these fonts may be referenced by their filename or their full face-name.
        /// </summary>
        /// <param name="folderPath">The base path to load fonts from</param>
        /// <returns>The number of fonts that were found</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public int RegisterFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentNullException(nameof(folderPath), "Folder name may not be null or empty");
            if (!Directory.Exists(folderPath))
                throw new ArgumentException($"The folder '{folderPath}' does not exist", nameof(folderPath));

            var fontFiles = Directory.GetFiles(folderPath, "*.ttf", SearchOption.AllDirectories);
            var count = 0;
            foreach (var file in fontFiles)
            {
                try
                {
                    var data = File.ReadAllBytes(file);
                    // create Font-source without caching it
                    var fontSource = XFontSource.GetOrCreateFrom(data, false);
                    var font = new OpenTypeFontFace(fontSource);
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    registeredFontFiles[fileName] = file;
                    registeredFontFiles[font.FullFaceName] = file;

                    var characterSet = font.cmap.GetSupportedCharacters();
                    characterSets[font.FullFaceName] = characterSet;
                    count++;
                }
                catch
                {
                }
            }
            return count;
        }

        /// <summary>
        /// Registers the font with the specified name as the fallback font.<br></br>
        /// The font must be either one of the <see cref="StandardFontNames"/> or one of the pre-registered fonts.
        /// </summary>
        /// <param name="fontName"></param>
        /// <exception cref="ArgumentException"></exception>
        public void RegisterFallbackFont(string fontName)
        {
            if (StandardFontData.IsStandardFont(fontName)
                || localFonts.ContainsKey(fontName)
                || registeredFontFiles.ContainsKey(fontName))
                fallbackFontName = fontName;
            else
                throw new ArgumentException($"'{fontName}' is not one of the standard font names and none of the registered fonts");
        }

        /// <summary>
        /// Tries to find a font suitable for rendering all characters specified in <paramref name="text"/>.<br></br>
        /// The returned font-name can be used to construct a new <see cref="XFont"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontName"></param>
        /// <returns>The name of a font or null if no appropiate font was found</returns>
        /// <remarks>
        /// If no font was found it could mean that the specified text contains characters for different languages.<br></br>
        /// e.g. if the text contains a mix of Arabic and Korean, it is unlikely to find a single font that suits both languages.<br></br>
        /// In this case you could try to split the text into multiple substrings, each one containing only characters for a single language.<br></br>
        /// Then call this function for each substring.
        /// </remarks>
        public bool TryFindAppropiateFont(string text, [MaybeNullWhen(false)] out string fontName)
        {
            fontName = null!;
            if (string.IsNullOrWhiteSpace(text))
                return false;
            // try to find a character-set containing all characters of text
            var kv = characterSets.FirstOrDefault(it => text.All(c => it.Value.Contains(c)));
            fontName = kv.Key;
            return kv.Key != null;
        }

        /// <summary>
        /// Gets the data for the specified font.
        /// </summary>
        /// <param name="faceName">Name of the font</param>
        /// <returns>Font data or null, if no font with the specified name could be found</returns>
        public byte[]? GetFont(string faceName)
        {
            var result = Resolve(faceName, false, false);
            if (result.Item1 == null && !string.IsNullOrEmpty(fallbackFontName))
                result = Resolve(fallbackFontName, false, false);
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
            if (result.Item2 == null && !string.IsNullOrEmpty(fallbackFontName))
                result = Resolve(fallbackFontName, isBold, isItalic);
            return result.Item2;
        }

        private static string MakeLocalName(string fontName, bool isBold, bool isItalic)
        {
            return XGlyphTypeface.ComputeGtfKey(fontName, isBold, isItalic);
        }

        private Tuple<byte[]?, FontResolverInfo?> Resolve(string fontName, bool isBold, bool isItalic)
        {
            var localName = MakeLocalName(fontName, isBold, isItalic);
            if (localFonts.TryGetValue(localName, out var localData) || localFonts.TryGetValue(fontName, out localData))
                return new Tuple<byte[]?, FontResolverInfo?>(localData, new FontResolverInfo(fontName, isBold, isItalic));
            
            if (registeredFontFiles.TryGetValue(fontName, out var fileName))
            {
                var fileData = System.IO.File.ReadAllBytes(fileName);
                return new Tuple<byte[]?, FontResolverInfo?>(fileData, new FontResolverInfo(fontName, isBold, isItalic));
            }
            
            var data = StandardFontData.GetFontData(fontName);
            if (data != null)
            {
                Register(fontName, data, isBold, isItalic);
                return new Tuple<byte[]?, FontResolverInfo?>(data, new FontResolverInfo(fontName, isBold, isItalic));
            }

            return new Tuple<byte[]?, FontResolverInfo?>(null, null);
        }
    }
}
