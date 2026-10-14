// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Internal.OpenType;
using PdfSharp.Internal.Threading;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Global table of OpenType font descriptor objects.
    /// </summary>
    class PsFontDescriptorCache
    {
        internal PsFontDescriptorCache(PsGlobals.PsFontStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Gets the FontDescriptor identified by the specified XFont. If no such object 
        /// exists, a new FontDescriptor is created and added to the cache.
        /// </summary>
        public OpenTypeFontDescriptor GetOrCreateDescriptorFor(XFont font)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            font.GlyphTypeface.CheckVersion();

            string fontDescriptorKey = font.GlyphTypeface.Key;
            try
            {
                var cache = OtGlobals.Global.OTFonts.FontDescriptorCache;  // TODO HACK
                Locks.EnterFontManagement();
                if (cache.TryGetValue(fontDescriptorKey, out var descriptor))
                    return descriptor;

                descriptor = new OpenTypeFontDescriptor(fontDescriptorKey,
                    font.GlyphTypeface.OTFontFace,
                    font.GlyphTypeface.FamilyName,
                    font.GlyphTypeface.FaceName);
                cache.TryAdd(fontDescriptorKey, descriptor);
                return descriptor;
            }
            finally { Locks.ExitFontManagement(); }
        }

        public OpenTypeFontDescriptor GetOrCreateDescriptorFor(XGlyphTypeface glyphTypeface)
        {
            glyphTypeface.CheckVersion();

            string fontDescriptorKey = glyphTypeface.Key;
            try
            {
                var cache = OtGlobals.Global.OTFonts.FontDescriptorCache;
                Locks.EnterFontManagement();
                if (cache.TryGetValue(fontDescriptorKey, out var descriptor))
                    return descriptor;

                descriptor = new OpenTypeFontDescriptor(fontDescriptorKey,
                    glyphTypeface.OTFontFace,
                    glyphTypeface.FamilyName,
                    glyphTypeface.FontName, 42);
                cache.TryAdd(fontDescriptorKey, descriptor);
                return descriptor;
            }
            finally { Locks.ExitFontManagement(); }
        }

        /// <summary>
        /// Gets the FontDescriptor identified by the specified FontSelector. If no such object 
        /// exists, a new FontDescriptor is created and added to the stock.
        /// </summary>
        public OpenTypeFontDescriptor GetOrCreateDescriptor(string fontFamilyName, XFontStyleEx style)
        {
            if (String.IsNullOrEmpty(fontFamilyName))
                throw new ArgumentNullException(nameof(fontFamilyName));

            //FontSelector1 selector = new FontSelector1(fontFamilyName, style);
            string fontDescriptorKey = KeyHelper.ComputeFdKey(fontFamilyName, (OTFontStyleHack)style);
            try
            {
                //var cache = Globals.Global.Fonts.FontDescriptorCache;
                var cache = OtGlobals.Global.OTFonts.FontDescriptorCache;
                Locks.EnterFontManagement();
                if (!cache.TryGetValue(fontDescriptorKey, out var descriptor))
                {
                    var font = new XFont(fontFamilyName, 10, style);
                    descriptor = GetOrCreateDescriptorFor(font);
                    // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd because there is not TryAdd in .NET Framework
                    if (cache.ContainsKey(fontDescriptorKey))
                        _ = typeof(int);  // Just a NOP for a break point.
                    else
                        cache.TryAdd(fontDescriptorKey, descriptor);
                }
                return descriptor;
            }
            finally { Locks.ExitFontManagement(); }
        }

        readonly PsGlobals.PsFontStorage _storage;
    }
}
