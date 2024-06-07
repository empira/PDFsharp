// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Fonts.Internal;
using PdfSharp.Internal;
#if GDI
using GdiFontFamily = System.Drawing.FontFamily;
#endif
#if WPF
using WpfFontFamily = System.Windows.Media.FontFamily;
#endif

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertPropertyToExpressionBody

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Internal implementation class of XFontFamily.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public sealed class FontFamilyInternal
    {
        // Implementation Notes
        // FontFamilyInternal implements an XFontFamily.
        //
        // * Each XFontFamily object is just a handle to its FontFamilyInternal singleton.
        //
        // * A FontFamilyInternal is uniquely identified by its name. It
        //    is not possible to use two different fonts that have the same
        //    family name.

        FontFamilyInternal(string familyName, bool createPlatformObjects)
        {
            _sourceName = _name = familyName;
#if GDI
            if (createPlatformObjects)
            {
                try
                {
                    _gdiFontFamily = new GdiFontFamily(familyName);
                    _name = _gdiFontFamily.Name;
                }
                catch
                {
                    // Do nothing.
                }
            }
#endif
#if WPF
            if (createPlatformObjects)
            {
                try
                {
                    _wpfFontFamily = new WpfFontFamily(familyName);
                    _name = _wpfFontFamily.FamilyNames[FontHelper.XmlLanguageEnUs];
                }
                catch
                {
                    // Do nothing.
                }
            }
#endif
        }

#if GDI
        FontFamilyInternal(GdiFontFamily gdiFontFamily)
        {
            _sourceName = _name = gdiFontFamily.Name;
            _gdiFontFamily = gdiFontFamily;
#if WPF
            // Hybrid build only.
            _wpfFontFamily = new WpfFontFamily(gdiFontFamily.Name);
#endif
        }
#endif

#if WPF
        FontFamilyInternal(WpfFontFamily wpfFontFamily)
        {
            _sourceName = wpfFontFamily.Source;
            _name = wpfFontFamily.FamilyNames[FontHelper.XmlLanguageEnUs];
            _wpfFontFamily = wpfFontFamily;
#if GDI
            // Hybrid build only.
            _gdiFontFamily = new GdiFontFamily(_sourceName);
#endif
        }
#endif

        internal static FontFamilyInternal GetOrCreateFromName(string familyName, bool createPlatformObject)
        {
            try
            {
                Lock.EnterFontFactory();
                var family = FontFamilyCache.GetFamilyByName(familyName);
                if (family == null)
                {
                    family = new FontFamilyInternal(familyName, createPlatformObject);
                    family = FontFamilyCache.CacheOrGetFontFamily(family);
                }
                return family;
            }
            finally { Lock.ExitFontFactory(); }
        }

#if GDI
        internal static FontFamilyInternal GetOrCreateFromGdi(GdiFontFamily gdiFontFamily)
        {
            try
            {
                Lock.EnterFontFactory();
                FontFamilyInternal fontFamily = new FontFamilyInternal(gdiFontFamily);
                fontFamily = FontFamilyCache.CacheOrGetFontFamily(fontFamily);
                return fontFamily;
            }
            finally { Lock.ExitFontFactory(); }
        }
#endif

#if WPF
        internal static FontFamilyInternal GetOrCreateFromWpf(WpfFontFamily wpfFontFamily)
        {
            FontFamilyInternal fontFamily = new FontFamilyInternal(wpfFontFamily);
            fontFamily = FontFamilyCache.CacheOrGetFontFamily(fontFamily);
            return fontFamily;
        }
#endif

        /// <summary>
        /// Gets the family name this family was originally created with.
        /// </summary>
        public string SourceName => _sourceName;

        readonly string _sourceName;

        /// <summary>
        /// Gets the name that uniquely identifies this font family.
        /// </summary>
        public string Name => _name;

        readonly string _name;

#if GDI
        /// <summary>
        /// Gets the underlying GDI+ font family object.
        /// Is null if the font was created by a font resolver.
        /// </summary>
        public GdiFontFamily? GdiFamily => _gdiFontFamily;

        readonly GdiFontFamily? _gdiFontFamily;
#endif

#if WPF
        /// <summary>
        /// Gets the underlying WPF font family object.
        /// Is null if the font was created by a font resolver.
        /// </summary>
        public WpfFontFamily WpfFamily => _wpfFontFamily;

        readonly WpfFontFamily _wpfFontFamily = null!;
#endif

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        internal string DebuggerDisplay
            => String.Format(CultureInfo.InvariantCulture, "FontFamily: '{0}'", Name);
    }
}
