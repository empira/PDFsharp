// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using System.Collections.Concurrent;
using System.Collections.ObjectModel;

// v7.0 TODO review

namespace PdfSharp.Internal.OpenType
{
    /// <summary>
    /// Represents a family of related OpenType fonts.
    /// </summary>
    public class OpenTypeFontFamily
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenTypeFontFamily"/> class.
        /// </summary>
        public OpenTypeFontFamily(string name)
        {
            FamilyName = name;
        }

        public void AddFontFace(OpenTypeGlyphTypeface otGlyphTypefaces)
        {
            // Note that the descriptor may differ from otGlyphTypefaces.OTFontFace.OTDescriptor.
            var descr = otGlyphTypefaces.OTDescriptor;
            var key = KeyHelper.CalcTypefaceKey(descr.OTFontStyle, descr.OTFontWeight, descr.OTFontStretch);
            _glyphTypefaces.TryAdd(key, otGlyphTypefaces);
            _referenceTypeFace = null;  // Reevaluate ReferenceFontFace.
        }

        //public void AddFontFace(OpenTypeGlyphTypeface glyphTypefaces, OpenTypeFontStyle overrideStyle, OpenTypeFontWeight overrideWeight, OpenTypeFontStretch overrideStretch)
        //{
        //    var key = KeyHelper.CalcTypefaceKey(overrideStyle, overrideWeight, overrideStretch);
        //    _glyphTypefaces.TryAdd(key, glyphTypefaces);
        //}

        /// <summary>
        /// Gets a collection of the OpenType glyph typefaces of this family
        /// </summary>
        public IReadOnlyCollection<OpenTypeGlyphTypeface> GetGlyphTypefaces()
        {
            return new ReadOnlyCollection<OpenTypeGlyphTypeface>(_glyphTypefaces.Values.ToArray());
        }

        public OpenTypeGlyphTypeface? ResolveTypeface(OpenTypeFontStyle style, OpenTypeFontWeight weight, OpenTypeFontStretch stretch)
        {
            var key = KeyHelper.CalcTypefaceKey(style, weight, stretch);
            var fontFace = _glyphTypefaces.GetValueOrDefault(key);
            return fontFace;
        }

        public OpenTypeFontFace ReferenceFontFace
        {
            get
            {
                const string normalKey = "0|400|5";
                const string boldKey = "0|700|5";
                const string italicKey = "2|400|5";

                if (_referenceTypeFace == null)
                {
                    if (_glyphTypefaces.Count == 0)
                        throw new InvalidOperationException("An empty font family has no ReferenceFontFace.");

                    var glyphTypeface = _glyphTypefaces.GetValueOrDefault(normalKey) 
                                        ?? _glyphTypefaces.GetValueOrDefault(boldKey)
                                        ?? _glyphTypefaces.GetValueOrDefault(italicKey);
                    _referenceTypeFace = glyphTypeface != null
                        ? glyphTypeface.OTFontFace
                        // HACK: Just return first font.
                        : _glyphTypefaces.Values.ToArray()[0].OTFontFace;
                }
                return _referenceTypeFace;
            }
        }

        /// <summary>
        /// Gets the name of the font family.
        /// </summary>
        public readonly string FamilyName;

        public static OpenTypeFontFamily GetOrCreateFrom(string familyName)
        {
            var openTypeFontFamilyCache = OtGlobals.Global.OTFonts.OpenTypeFontFamilyCache;
            if (openTypeFontFamilyCache.TryGetFontFamily(familyName, out var otFontFamily))
                return otFontFamily;
            otFontFamily = new(familyName);
            openTypeFontFamilyCache.AddFontFamily(otFontFamily);
            return otFontFamily;
        }

        OpenTypeFontFace? _referenceTypeFace;

        // Maps typefaceKey to glyph typefaces.
        // An OpenTypeFontFamily is composed of OpenTypeGlyphTypeface objects (not of OpenTypeTypeface objects)
        // because of style simulation.
        readonly ConcurrentDictionary<string, OpenTypeGlyphTypeface> _glyphTypefaces = new(StringComparer.OrdinalIgnoreCase);
    }
}
