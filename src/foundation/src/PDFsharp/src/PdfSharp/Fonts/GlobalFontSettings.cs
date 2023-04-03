// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Fonts.OpenType;
using PdfSharp.Internal;
using PdfSharp.Pdf;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Provides functionality to specify information about the handling of fonts in the current application domain.
    /// </summary>
    public static class GlobalFontSettings
    {
        /// <summary>
        /// The name of the default font.
        /// </summary>
        public const string DefaultFontName = "PlatformDefault";

        /// <summary>
        /// Gets or sets the global font resolver for the current application domain.
        /// This static function must be called only once and before any font operation was executed by PDFsharp.
        /// If this is not easily to obtain, e.g. because your code is running on a web server, you must provide the
        /// same instance of your font resolver in every subsequent setting of this property.
        /// In a web application set the font resolver in Global.asax.
        /// </summary>
        public static IFontResolver? FontResolver
        {
            get => _fontResolver;
            set
            {
                // Cannot remove font resolver.
                if (value == null)
                    throw new ArgumentNullException();

                try
                {
                    Lock.EnterFontFactory();
                    // Ignore multiple setting e.g. in a web application.
                    if (ReferenceEquals(_fontResolver, value))
                        return;

                    if (FontFactory.HasFontSources)
#if DEBUG
                    {
#if CORE
                        string Config = "Core.";
#elif GDI
                        string Config = "GDI.";
#elif WPF
                        string Config = "WPF.";
#else
                        string Config = "<Unknown config>.";
#endif
                        var info1 = _fontResolver?.ToString();
                        var info2 = value.ToString();
                        var info3 = FontFactory.GetFontCachesState();
                        // When a Unit Tests throws this exception, we are grateful for any piece of information we can get.
                        throw new InvalidOperationException(
                            $"You must not change font resolver after is was once used. Is: {_fontResolver?.GetType().Name ?? "<null>"}. New: {value.GetType().Name}. "+
                            $"Info1: {info1}. Info2: {info2}. Config: {Config}." +
                            $"Cache: {info3}.");
                    }
#else
                        throw new InvalidOperationException("You must not change font resolver after is was once used.");
#endif

                        _fontResolver = value;
                }
                finally { Lock.ExitFontFactory(); }
            }
        }

        static IFontResolver? _fontResolver;

        /// <summary>
        /// Adds a font resolver. NXI
        /// </summary>
        /// <param name="fontResolver">The font resolver.</param>
        [Obsolete("Not yet implemented.")]
        public static void AddFontResolver(IFontResolverMarker fontResolver)
        {
            int x = fontResolver switch
            {
                IFontResolver => 7,
                IFontResolver2 => 8,
                _ => throw new ArgumentException("Not a valid font resolver.")

            };
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resets the font resolvers and clears all internal cache.
        /// Intended to be used in unit tests only.
        /// </summary>
        public static void ResetFontResolvers()
        {
            _fontResolver = null;
            GlyphTypefaceCache.Reset();
            FontFactory.Reset();
        }

        /// <summary>
        /// Gets or sets the default font encoding used for XFont objects where encoding is not explicitly specified.
        /// If it is not set, the default value is PdfFontEncoding.Unicode.
        /// If you are sure your document contains only Windows-1252 characters (see https://en.wikipedia.org/wiki/Windows-1252) 
        /// set default encoding to PdfFontEncoding.Windows1252.
        /// Must be set only once per app domain.
        /// </summary>
        public static PdfFontEncoding DefaultFontEncoding
        {
            get
            {
                if (!_fontEncodingInitialized)
                    DefaultFontEncoding = PdfFontEncoding.Unicode;
                return _fontEncoding;
            }
            set
            {
                try
                {
                    Lock.EnterFontFactory();
                    if (_fontEncodingInitialized)
                    {
                        // Ignore multiple setting e.g. in a web application.
                        if (_fontEncoding == value)
                            return;
                        throw new InvalidOperationException("Must not change DefaultFontEncoding after is was set once.");
                    }

                    _fontEncoding = value;
                    _fontEncodingInitialized = true;
                }
                finally { Lock.ExitFontFactory(); }
            }
        }

        static PdfFontEncoding _fontEncoding;
        static bool _fontEncodingInitialized;
    }
}