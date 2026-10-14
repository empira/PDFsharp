// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using System.Globalization;
using Microsoft.Extensions.Logging;
using PdfSharp.Internal;
using PdfSharp.Logging;
using Fixed = System.Int32;
//using FWord = System.Int16;
//using UFWord = System.UInt16;

#pragma warning disable 0649

namespace PdfSharp.Internal.OpenType
{
    /// <summary>
    /// Represents an OpenType glyph typeface.
    /// </summary>
    //[DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public sealed class OpenTypeGlyphTypeface
    {
        /// <summary>
        /// </summary>
        public OpenTypeGlyphTypeface(OpenTypeFontFace otFontFace, OpenTypeFontDescriptor? otDescriptor = null,
            bool isBoldSimulated = false, bool isObliqueSimulated = false, string? key = null)
        {
            otDescriptor ??= otFontFace.OTDescriptor;
            key ??= KeyHelper.CalcGlyphTypefaceKey(otDescriptor.FamilyName, otDescriptor.FaceName,
                otDescriptor.OTFontStyle, otDescriptor.OTFontWeight, otDescriptor.OTFontStretch,
                isBoldSimulated, isObliqueSimulated);

            Key = key;
            OTFontFace = otFontFace;
            OTDescriptor = otDescriptor;
            IsBoldSimulated = isBoldSimulated;
            IsObliqueSimulated = isObliqueSimulated;
        }

        public static OpenTypeGlyphTypeface GetOrCreate(OpenTypeFontSource otFontSource,
            bool isBoldSimulated, bool isObliqueSimulated)
        {
            var otFontFace = otFontSource.OTFontFace;
            var otDescriptor = otFontFace.OTDescriptor;

            var key = KeyHelper.CalcGlyphTypefaceKey(otDescriptor.FamilyName, otDescriptor.FaceName,
                otDescriptor.OTFontStyle, otDescriptor.OTFontWeight, otDescriptor.OTFontStretch,
                isBoldSimulated, isObliqueSimulated);

            var openTypeGlyphTypefaceCache = OtGlobals.Global.OTFonts.OpenTypeGlyphTypefaceCache;
            if (!openTypeGlyphTypefaceCache.TryGetGlyphTypeface(key, out var otGlyphTypeface))
            {
                otGlyphTypeface = new(otFontFace, otFontFace.OTDescriptor, isBoldSimulated, isObliqueSimulated, key);
                openTypeGlyphTypefaceCache.TryAddGlyphTypeface(otGlyphTypeface);
            }
            return otGlyphTypeface;
        }

        public OpenTypeGlyphTypeface CreateVariant(bool isBoldSimulated, bool isObliqueSimulated)
        {
            IsBoldSimulated = true;
            var variant = (OpenTypeGlyphTypeface)MemberwiseClone();
            variant.IsBoldSimulated = isBoldSimulated;
            variant.IsObliqueSimulated = isObliqueSimulated;
            var descr = variant.OTDescriptor;
            variant.Key = KeyHelper.CalcGlyphTypefaceKey(descr.FamilyName, descr.FaceName,
                descr.OTFontStyle, descr.OTFontWeight, descr.OTFontStretch,
                isBoldSimulated, isObliqueSimulated);
            return variant;
        }

        public string Key { get; private set; }

        public OpenTypeFontFace OTFontFace { get; private set; }

        public OpenTypeFontDescriptor OTDescriptor { get; private set; }

        public bool IsBoldSimulated { get; private set; }

        public bool IsObliqueSimulated { get; private set; }
    }
}
