// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using System.Collections.Concurrent;

namespace PdfSharp.Internal.OpenType
{
    /// <summary>
    /// A hack to bootstrap the interaction of FontFamily, Typeface, and FontFace.
    /// </summary>
    public class OpenTypeFontRegistry
    {
        // Example
        // Windows comes with 10 Arial font files. 4 times Arial (regular, bold, italic, and bold italic),
        // 4 times Arial Narrow (also 4 faces), Arial Black, and Arial Rounded. Windows groups them into two
        // families. The first family contains the first 9 faces and the second family only Arial Rounded. WPF
        // does it // the same way. A look at the source code of WPF 3.0 reveals that this is done with a lot
        // of specialized knowledge about fonts. E.g. WPF knows the meaning of 'narrow' in Arial Narrow and
        // removed it from the family name. In Arial Narrow the stretch entry in the OS/2 table is correct.
        // But in Arial Black the weight value in the OS/2 table is 400 as it is for a regular with. WPF again
        // 'knows' the meaning of Black, removes it from the family name and corrects the wrong weight value
        // to 900.
        // For details see https://assets.pdfsharp.net/docs/WPF%20Font%20Selection%20Model.pdf
        // PDFsharp does not use these heuristics. It creates 4 families: Arial (4 faces), Arial Narrow (4 faces), 
        // Arial Black (1 face), and Arial Rounded (1 face).
        enum State  // not yet used
        {
            Created,
            Initializing,
            Ready
        }

        public OpenTypeFontRegistry()
        { }

        public OpenTypeGlyphTypeface RegisterFont(OpenTypeFontSource fontSource)
        {
            var fontFace = fontSource.OTFontFace;
            return RegisterFont(fontFace);
        }

        public OpenTypeGlyphTypeface RegisterFont(OpenTypeFontFace fontFace, string? overrideFamilyName = null, string? overrideFaceName = null,
            OpenTypeFontStyle? overrideStyle = null, OpenTypeFontWeight? overrideWeight = null, OpenTypeFontStretch? overrideStretch = null)
        {
            if (_state == State.Created)
                _state = State.Initializing;
            if (_state == State.Ready)
                throw new InvalidOperationException("Cannot add fonts after registry is initialized.");

            var fontSource = fontFace.OTFontSource;
            Debug.Assert(ReferenceEquals(fontSource.OTFontFace, fontFace));

            // We clone the descriptor because some values may be overridden in a GlyphTypeface.
            var descr = fontFace.OTDescriptor.Clone();
            //var key = fontSource.FontFaceKey;
            //var fontFace = OpenTypeGlobals.Global.OTFonts.FontFaceCache[key];

            descr.FamilyName = overrideFamilyName ?? fontFace.OTDescriptor.FamilyName;
            descr.FaceName = overrideFaceName ?? fontFace.OTDescriptor.FaceName;
            descr.OTFontStyle = overrideStyle ?? fontFace.OTDescriptor.OTFontStyle;
            descr.OTFontWeight = overrideWeight ?? fontFace.OTDescriptor.OTFontWeight;
            descr.OTFontStretch = overrideStretch ?? fontFace.OTDescriptor.OTFontStretch;

            //if (!String.IsNullOrEmpty(overrideFamilyName))
            //{
            //    familyName = overrideFamilyName;
            //    fontFace.OTDescriptor.FamilyName = familyName;
            //}

            //if (!String.IsNullOrEmpty(overrideFaceName))
            //{
            //    faceName = overrideFaceName;
            //    fontFace.OTDescriptor.FaceName = faceName;
            //}

            // Get or create appropriate font family.
            if (!_fontFamilies.TryGetValue(descr.FamilyName, out var otFontFamily))
            {
                otFontFamily = new(descr.FamilyName);
                _fontFamilies.TryAdd(otFontFamily.FamilyName, otFontFamily);
            }

            var otGlyphTypeface = new OpenTypeGlyphTypeface(fontFace, descr);
            _glyphTypefaces.TryAdd(otGlyphTypeface.Key, otGlyphTypeface);
            otFontFamily.AddFontFace(otGlyphTypeface/*, style, weight, stretch*/);

            return otGlyphTypeface;
            //// Add font to family with alternate family name.
            //if (altFamilyName != null && String.Compare(altFamilyName, familyName, StringComparison.OrdinalIgnoreCase) != 0)
            //{
            //    if (_fontFamilies.TryGetValue(altFamilyName, out family))
            //    {
            //        family.AddFontFace(fontFace, style, weight, stretch);
            //    }
            //    else
            //    {
            //        family = new(altFamilyName);
            //        family.AddFontFace(fontFace, style, weight, stretch);
            //        _fontFamilies.TryAdd(altFamilyName, family);
            //    }
            //}
        }

        // TODO
        public OpenTypeGlyphTypeface RegisterGlyphTypeface(OpenTypeGlyphTypeface otGlyphTypeface,
            bool isBoldSimulated, bool isItalicSimulated)
        {
            var variant = otGlyphTypeface.CreateVariant(isBoldSimulated, isItalicSimulated);

            // Get or create appropriate font family.
            if (!_fontFamilies.TryGetValue(variant.OTDescriptor.FamilyName, out var otFontFamily))
            {
                otFontFamily = new(variant.OTDescriptor.FamilyName);
                _fontFamilies.TryAdd(otFontFamily.FamilyName, otFontFamily);
            }

            //   var otGlyphTypeface = new OpenTypeGlyphTypeface(fontFace, descr);
            _glyphTypefaces.TryAdd(otGlyphTypeface.Key, otGlyphTypeface);
            otFontFamily.AddFontFace(otGlyphTypeface/*, style, weight, stretch*/);

            return variant;
        }

        public ICollection<OpenTypeFontFamily> GetFamilies()
        {
            var families = _fontFamilies.Values;
            return families;
        }

        public OpenTypeFontFamily? GetFamily(string familyName)
        {
            _fontFamilies.TryGetValue(familyName, out var fontFamily);
            return fontFamily;
        }

        public OpenTypeGlyphTypeface? ResolveTypeface(string familyName, OpenTypeFontStyle style, OpenTypeFontWeight weight, OpenTypeFontStretch stretch)
        {
            //OpenTypeFontFace? fontFace;
            if (_fontFamilies.TryGetValue(familyName, out var family))
            {
                var glyphTypeface = family.ResolveTypeface(style, weight, stretch);
                return glyphTypeface;
            }

            //if (_defaultFamily != null)
            //{
            //    var glyphTypeface = _defaultFamily.ResolveTypeface(style, weight, stretch);
            //    return glyphTypeface;
            //}
            return null;
        }

        //OpenTypeFontFamily? _defaultFamily;

        State _state;

        readonly ConcurrentDictionary<string, OpenTypeGlyphTypeface> _glyphTypefaces = new(StringComparer.OrdinalIgnoreCase);

        readonly ConcurrentDictionary<string, OpenTypeFontFamily> _fontFamilies = new(StringComparer.OrdinalIgnoreCase);
    }
}
