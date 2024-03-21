// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Runtime.InteropServices;
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;
using GdiPrivateFontCollection = System.Drawing.Text.PrivateFontCollection;
#endif
#if WPF
using WpfFonts = System.Windows.Media.Fonts;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfTypeface = System.Windows.Media.Typeface;
using WpfGlyphTypeface = System.Windows.Media.GlyphTypeface;
#endif
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;

namespace PdfSharp.Drawing
{
    ///<summary>
    /// Makes fonts that are not installed on the system available within the current application domain.<br/>
    /// </summary>
    public sealed class XPrivateFontCollection
    {
        // This one is global and can only grow. It is not possible to remove fonts that have been added.

        /// <summary>
        /// Initializes a new instance of the <see cref="XPrivateFontCollection"/> class.
        /// </summary>
        XPrivateFontCollection()
        {
            // HACK: Use one global PrivateFontCollection in GDI+
        }

#if GDI
        //internal PrivateFontCollection PrivateFontCollection
        //{
        //  get { return privateFontCollection; }
        //  set { privateFontCollection = value; }
        //}

        GdiPrivateFontCollection GetPrivateFontCollection()
        {
            // Create only if really needed.
            if (_privateFontCollection == null)
                _privateFontCollection = new GdiPrivateFontCollection();
            return _privateFontCollection;
        }

        // PrivateFontCollection of GDI+
        GdiPrivateFontCollection? _privateFontCollection;
#endif

        /// <summary>
        /// Gets the global font collection.
        /// </summary>
        internal static XPrivateFontCollection Singleton { get; } = new();

        //internal static XPrivateFontCollection Singleton => _singleton;
        //internal static XPrivateFontCollection _singleton = new XPrivateFontCollection();

#if GDI
        /// <summary>
        /// Adds the font data to the font collections.
        /// </summary>
        [Obsolete("Use Add(Stream stream)")]
        public void AddFont(byte[] data, string familyName)
        {
            if (String.IsNullOrEmpty(familyName))
                throw new ArgumentNullException("familyName");

            //if (glyphTypeface == null)
            //  throw new ArgumentNullException("glyphTypeface");

            // Add to GDI+ PrivateFontCollection
            int length = data.Length;

            // Copy data without unsafe code 
            IntPtr ip = Marshal.AllocCoTaskMem(length);
            Marshal.Copy(data, 0, ip, length);
            GetPrivateFontCollection().AddMemoryFont(ip, length);
            // Do not free the memory here, AddMemoryFont stores a pointer, not a copy!
            //Marshal.FreeCoTaskMem(ip);
            //privateFonts.Add(glyphTypeface);
        }
#endif

        /// <summary>
        /// Adds the specified font data to the global PrivateFontCollection.
        /// Family name and style are automatically retrieved from the font.
        /// </summary>
#if GDI
        [Obsolete("Use Add(Stream stream)")]
#else
        [Obsolete("Use the GDI build of PDFsharp and use Add(Stream stream)")]
#endif
        public static void AddFont(string filename)
        {
            throw new NotImplementedException();
            //XGlyphTypeface glyphTypeface = new XGlyphTypeface(filename);
            //Global.AddGlyphTypeface(glyphTypeface);
        }

#if GDI
        /// <summary>
        /// Adds the specified font data to the global PrivateFontCollection.
        /// Family name and style are automatically retrieved from the font.
        /// </summary>
        [Obsolete("Use Add(stream).")]
        public static void AddFont(Stream stream)
        {
            Add(stream);
        }

        /// <summary>
        /// Adds the specified font data to the global PrivateFontCollection.
        /// Family name and style are automatically retrieved from the font.
        /// </summary>
        public static void Add(Stream stream)
        {
            int length = (int)stream.Length;
            byte[] bytes = new byte[length];
            stream.Read(bytes, 0, length);
            Add(bytes);
        }

        /// <summary>
        /// Adds the specified font data to the global PrivateFontCollection.
        /// Family name and style are automatically retrieved from the font.
        /// </summary>
        public static void Add(byte[] font)
        {
            IntPtr unmanagedPointer = Marshal.AllocCoTaskMem(font.Length);
            Marshal.Copy(font, 0, unmanagedPointer, font.Length);
            Singleton.GetPrivateFontCollection().AddMemoryFont(unmanagedPointer, font.Length);
            // Do not free the memory here, AddMemoryFont stores a pointer, not a copy!
            //Marshal.FreeCoTaskMem(ip);

            XFontSource fontSource = XFontSource.GetOrCreateFrom(font);

            string familyName = fontSource.FontName;

            if (familyName.EndsWith(" Regular", StringComparison.OrdinalIgnoreCase))
                familyName = familyName.Substring(0, familyName.Length - 8);

            bool bold = fontSource.FontFace.os2.IsBold;
            bool italic = fontSource.FontFace.os2.IsItalic;
            IncompetentlyMakeAHackToFixAProblemYouWouldNeverHaveIfYouUsedAFontResolver(fontSource, ref familyName, ref bold, ref italic);
            string key = MakeKey(familyName, bold, italic);
            Singleton._fontSources.Add(key, fontSource);

            string typefaceKey = XGlyphTypeface.ComputeGtfKey(familyName, bold, italic);
            FontFactory.CacheExistingFontSourceWithNewTypefaceKey(typefaceKey, fontSource);
        }

        static void IncompetentlyMakeAHackToFixAProblemYouWouldNeverHaveIfYouUsedAFontResolver(XFontSource fontSource,
            ref string familyName, ref bool bold, ref bool italic)
        {
            const string regularSuffix = " Regular";
            const string boldSuffix = " Bold";
            const string italicSuffix = " Italic";
            const string boldItalicSuffix = " Bold Italic";
            const string italicBoldSuffix = " Italic Bold";

            if (familyName.EndsWith(regularSuffix, StringComparison.OrdinalIgnoreCase))
            {
                familyName = familyName.Substring(0, familyName.Length - regularSuffix.Length);
                Debug.Assert(!bold && !italic);
                bold = italic = false;
            }
            else if (familyName.EndsWith(boldItalicSuffix, StringComparison.OrdinalIgnoreCase) || familyName.EndsWith(italicBoldSuffix, StringComparison.OrdinalIgnoreCase))
            {
                familyName = familyName.Substring(0, familyName.Length - boldItalicSuffix.Length);
                Debug.Assert(bold && italic);
                bold = italic = true;
            }
            else if (familyName.EndsWith(boldSuffix, StringComparison.OrdinalIgnoreCase))
            {
                familyName = familyName.Substring(0, familyName.Length - boldSuffix.Length);
                Debug.Assert(bold && !italic);
                bold = true;
                italic = false;
            }
            else if (familyName.EndsWith(italicSuffix, StringComparison.OrdinalIgnoreCase))
            {
                familyName = familyName.Substring(0, familyName.Length - italicSuffix.Length);
                Debug.Assert(!bold && italic);
                bold = false;
                italic = true;
            }
            else
            {
                Debug.Assert(!bold && !italic);
                bold = false;
                italic = false;
            }
        }
#endif

        /// <summary>
        /// Adds the specified font data to the global PrivateFontCollection.
        /// Family name and style are automatically retrieved from the font.
        /// </summary>
#if GDI
        [Obsolete("Use Add(Stream stream)")]
#else
        [Obsolete("Use the GDI build of PDFsharp and use Add(Stream stream)")]
#endif
        public static void AddFont(Stream stream, string facename)
        {
            throw new NotImplementedException();
            //XGlyphTypeface glyphTypeface = new XGlyphTypeface(stream, facename);
            //Global.AddGlyphTypeface(glyphTypeface);
        }

        //        /// <summary>
        //        /// Adds XGlyphTypeface to internal collection.
        //        /// Family name and style are automatically retrieved from the font.
        //        /// </summary>
        //        void AddGlyphTypeface(XGlyphTypeface glyphTypeface)
        //        {
        //            string name = MakeName(glyphTypeface);
        //            if (_typefaces.ContainsKey(name))
        //                throw new InvalidOperationException(PSSR.FontAlreadyAdded(glyphTypeface.DisplayName));

        //            _typefaces.Add(name, glyphTypeface);
        //            //Debug.WriteLine("Font added: " + name);

        //#if GDI
        //            // Add to GDI+ PrivateFontCollection singleton.
        //            byte[] data = glyphTypeface.FontFace.FontSource.Bytes;
        //            int length = data.Length;

        //            IntPtr ip = Marshal.AllocCoTaskMem(length);
        //            Marshal.Copy(data, 0, ip, length);
        //            _privateFontCollection.AddMemoryFont(ip, length);
        //            // Do not free the memory here, AddMemoryFont stores a pointer, not a copy!
        //            // Marshal.FreeCoTaskMem(ip);
        //#endif

        //#if WPF
        //#endif
        //        }

#if WPF
        /// <summary>
        /// Initializes a new instance of the FontFamily class from the specified font family name and an optional base uniform resource identifier (URI) value.
        /// Sample: Add(new Uri("pack://application:,,,/"), "./myFonts/#FontFamilyName");)
        /// </summary>
        /// <param name="baseUri">Specifies the base URI that is used to resolve familyName.</param>
        /// <param name="familyName">The family name or names that comprise the new FontFamily. Multiple family names should be separated by commas.</param>
        public static void Add(Uri baseUri, string familyName)
        {
            Uri uri = new Uri("pack://application:,,,/");

            // TODO: What means 'Multiple family names should be separated by commas.'?
            // does not work

            if (String.IsNullOrEmpty(familyName))
                throw new ArgumentNullException(nameof(familyName));

            if (familyName.Contains(","))
                throw new NotImplementedException("Only one family name is supported.");

            // Family name starts right of '#'.
#if NET6_0_OR_GREATER
            int idxHash = familyName.IndexOf('#', StringComparison.Ordinal);
#else
            int idxHash = familyName.IndexOf('#'); // Searching for '#' works fine even without "StringComparison.Ordinal".
#endif
            if (idxHash < 0)
                throw new ArgumentException("Family name must contain a '#'. Example './#MyFontFamilyName'", nameof(familyName));

            string key = familyName.Substring(idxHash + 1);
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("familyName has invalid format.");

            if (Singleton._fontFamilies.ContainsKey(key))
                throw new ArgumentException("An entry with the specified family name already exists.");

#if true
#if DEBUG_
            foreach (WpfFontFamily fontFamily1 in WpfFonts.GetFontFamilies(baseUri, familyName))
            {
                ICollection<WpfTypeface> wpfTypefaces = fontFamily1.GetTypefaces();
                wpfTypefaces.GetType();
            }
#endif
            // Create WPF font family.
            WpfFontFamily fontFamily = new WpfFontFamily(baseUri, familyName);
            //System.Windows.Media.FontFamily  x;
            // Required for new Uri("pack://application:,,,/")
            // ReSharper disable once ObjectCreationAsStatement
            //            new System.Windows.Application();

#else
            System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily(familyName);
#endif

            // Check whether font data really exists.
            ICollection<WpfTypeface> list = fontFamily.GetTypefaces();
            foreach (WpfTypeface typeFace in list)
            {
                Debug.WriteLine($"{familyName}, {typeFace.FaceNames[FontHelper.XmlLanguageEnUs]}, {typeFace.Style}, {typeFace.Weight}, {typeFace.Stretch}");
                if (!typeFace.TryGetGlyphTypeface(out var glyphTypeface))
                {
                    Debug.WriteLine("    Glyph typeface does not exist.");
                    //throw new ArgumentException("Font with the specified family name does not exist.");
                }
            }

            Singleton._fontFamilies.Add(key, fontFamily);
        }
#endif

        //internal static XGlyphTypeface TryGetXGlyphTypeface(string familyName, XFontStyleEx style)
        //{
        //    string name = MakeName(familyName, style);

        //    XGlyphTypeface typeface;
        //    _global._typefaces.TryGetValue(name, out typeface);
        //    return typeface;
        //}

#if CORE
        internal static XTypeface? TryCreateTypeface(string name, XFontStyleEx style, out XFontFamily? fontFamily)
        {
            if (Singleton._fontFamilies.TryGetValue(name, out fontFamily))
            {
                var typeface = FontHelper.CreateTypeface(fontFamily, style);
                return typeface;
            }
            return null;
        }
#endif

#if GDI
        internal static GdiFont? TryCreateFont(string name, double size, GdiFontStyle style, out XFontSource? fontSource)
        {
            fontSource = null;
            try
            {
                var pfc = Singleton._privateFontCollection;
                if (pfc == null)
                    return null;
#if true
                string key = MakeKey(name, (XFontStyleEx)style);
                if (Singleton._fontSources.TryGetValue(key, out fontSource))
                {
                    GdiFont font = new GdiFont(name, (float)size, style, GraphicsUnit.World);
#if DEBUG_
                    Debug.Assert(StringComparer.OrdinalIgnoreCase.Compare(name, font.Name) == 0);
                    Debug.Assert(font.Bold == ((style & GdiFontStyle.Bold) != 0));
                    Debug.Assert(font.Italic == ((style & GdiFontStyle.Italic) != 0));
#endif
                    return font;
                }
                return null;
#else
                foreach (GdiFontFamily family in pfc.Families)
                {
                    if (string.Compare(family.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        GdiFont font = new GdiFont(family, (float)size, style, GraphicsUnit.World);
                        if (string.Compare(font.Name, name, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            // Style simulation is not implemented in GDI+.
                            // Use WPF build.
                        }
                        return font;
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                // Ignore exception and return null.
                Debug.WriteLine(ex.ToString());
            }
            return null;
        }
#endif

#if WPF
            internal static WpfTypeface TryCreateTypeface(string name, XFontStyleEx style, out WpfFontFamily fontFamily)
        {
            if (Singleton._fontFamilies.TryGetValue(name, out fontFamily!)) // NRT HACK
            {
                WpfTypeface typeface = FontHelper.CreateTypeface(fontFamily, style);
                return typeface;
            }
            return default!;
        }
#endif

        static string MakeKey(string familyName, XFontStyleEx style)
            => MakeKey(familyName, (style & XFontStyleEx.Bold) != 0, (style & XFontStyleEx.Italic) != 0);

        static string MakeKey(string familyName, bool bold, bool italic)
            => familyName + "#" + (bold ? "b" : "") + (italic ? "i" : "");

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        readonly Dictionary<string, XGlyphTypeface> _typefaces_unused = new();
#if CORE
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        readonly Dictionary<string, XFontFamily> _fontFamilies = new(StringComparer.OrdinalIgnoreCase);
#endif
#if GDI
        //List<XGlyphTypeface> privateFonts = new List<XGlyphTypeface>();
        readonly Dictionary<string, XFontSource> _fontSources = new Dictionary<string, XFontSource>(StringComparer.OrdinalIgnoreCase);
#endif
#if WPF
        readonly Dictionary<string, WpfFontFamily> _fontFamilies = new Dictionary<string, WpfFontFamily>(StringComparer.OrdinalIgnoreCase);
#endif
    }
}
