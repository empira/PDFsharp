// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Internal;
using PdfSharp.Logging;
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
        /// //In a web application set the font resolver in Global.asax.
        /// </summary>
        public static IFontResolver? FontResolver
        {
            get => Globals.Global.FontResolver;
            set
            {
                // Cannot remove font resolver.
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                ref var fontResolver = ref Globals.Global.FontResolver;
                try
                {
                    Lock.EnterFontFactory();

                    // Ignore multiple setting of same object.
                    // Can happen in e.g. a web application.
                    if (ReferenceEquals(fontResolver, value))
                    {
                        LogHost.Logger.LogWarning("Setting the same font resolver is ignored.");
                        return;
                    }

                    // Ignore multiple setting of new instance of the same object.
                    // Can happen in e.g. a MAUI application.
                    if (fontResolver != null && ReferenceEquals(fontResolver.GetType(), value.GetType()))
                    {
                        LogHost.Logger.LogWarning("Setting the same font resolver is ignored.");
                        return;
                    }

                    if (FontFactory.HasFontSources)
#if DEBUG
                    {
                        var config = Capabilities.Build.BuildName;

                        var info1 = fontResolver?.ToString();
                        var info2 = value.ToString();
                        var info3 = FontFactory.GetFontCachesState();
                        // When a Unit Tests throws this exception, we are grateful for any piece of information we can get.
                        throw new InvalidOperationException(
                            $"You must not change font resolver after is was once used. Is: {fontResolver?.GetType().Name ?? "<null>"}. New: {value.GetType().Name}. " +
                            $"Info1: {info1}. Info2: {info2}. Config: {config}." +
                            $"Cache: {info3}.");
                    }
#else
                        throw new InvalidOperationException("You must not change font resolver after is was once used.");
#endif

                    fontResolver = value;
                }
                finally { Lock.ExitFontFactory(); }
            }
        }

        /// <summary>
        /// Adds a font resolver. NYI
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
            _ = x;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resets the font resolvers and clears all internal cache.
        /// The font management is set to the same state as it has immediately after loading the PDFsharp library.
        /// </summary>
        /// <remarks>
        /// This function is only useful in unit test scenarios and not intended to be called in application code.
        /// </remarks>
        public static void ResetFontResolvers()
        {
            Globals.Global.FontResolver = null;
            GlyphTypefaceCache.Reset();
            FontFactory.Reset();
        }

        /// <summary>
        /// Gets or sets the default font encoding used for XFont objects where encoding is not explicitly specified.
        /// If it is not set, the default value is PdfFontEncoding.Automatic.
        /// If you are sure your document contains only Windows-1252 characters (see https://en.wikipedia.org/wiki/Windows-1252) 
        /// set default encoding to PdfFontEncoding.WinAnsi.
        /// Must be set only once per app domain.
        /// </summary>
        public static PdfFontEncoding DefaultFontEncoding
        {
            get
            {
                if (!Globals.Global.FontEncodingInitialized)
                    DefaultFontEncoding = PdfFontEncoding.Automatic;
                return Globals.Global.FontEncoding;
            }
            set
            {
                try
                {
                    Lock.EnterFontFactory();
                    if (Globals.Global.FontEncodingInitialized)
                    {
                        // Ignore multiple setting e.g. in a web application.
                        if (Globals.Global.FontEncoding == value)
                            return;
                        throw new InvalidOperationException("Must not change DefaultFontEncoding after is was set once.");
                    }

                    Globals.Global.FontEncoding = value;
                    Globals.Global.FontEncodingInitialized = true;
                }
                finally { Lock.ExitFontFactory(); }
            }
        }
    }
}

namespace PdfSharp.Internal
{
    partial class Globals
    {
        /// <summary>
        /// The globally set font resolver.
        /// </summary>
        public IFontResolver? FontResolver;

        /// <summary>
        /// The font encoding default. Do not change.
        /// </summary>
        public PdfFontEncoding FontEncoding;

        /// <summary>
        /// Is true if FontEncoding was set by user.
        /// </summary>
        public bool FontEncodingInitialized;
    }
}
