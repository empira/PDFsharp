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
        /// The name of the default font. This name is obsolete and must not be used anymore.
        /// </summary>
        [Obsolete("DefaultFontName is deprecated. Do not use it anymore.")]
        public const string DefaultFontName_ = "PlatformDefault";

        /// <summary>
        /// Gets or sets the custom font resolver for the current application.
        /// This static function must be called only once and before any font operation was executed by PDFsharp.
        /// If this is not easily to obtain, e.g. because your code is running on a web server, you must provide the
        /// same instance of your font resolver in every subsequent setting of this property.
        /// </summary>
        public static IFontResolver? FontResolver
        {
            get => Globals.Global.Fonts.FontResolver;
            set
            {
                // Cannot remove font resolver.
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "You cannot remove the font resolver.");

                ref var fontResolver = ref Globals.Global.Fonts.FontResolver;
                try
                {
                    Lock.EnterFontFactory();
                    SetFontResolver(value, ref fontResolver);
                }
                finally { Lock.ExitFontFactory(); }
            }
        }

        /// <summary>
        /// Gets or sets the fallback font resolver for the current application.
        /// This static function must be called only once and before any font operation was executed by PDFsharp.
        /// If this is not easily to obtain, e.g. because your code is running on a web server, you must provide the
        /// same instance of your font resolver in every subsequent setting of this property.
        /// </summary>
        public static IFontResolver? FallbackFontResolver
        {
            get => Globals.Global.Fonts.FallbackFontResolver;
            set
            {
                // Cannot remove font resolver.
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "You cannot remove the fallback font resolver.");

                ref var fontResolver = ref Globals.Global.Fonts.FallbackFontResolver;
                try
                {
                    Lock.EnterFontFactory();
                    SetFontResolver(value, ref fontResolver);
                }
                finally { Lock.ExitFontFactory(); }
            }
        }

        static void SetFontResolver(IFontResolver value, ref IFontResolver? location)
        {
            // Ignore multiple setting of same object.
            // Can happen in e.g. a web application.
            if (ReferenceEquals(value, location))
            {
                PdfSharpLogHost.Logger.LogWarning("Setting the same font resolver twice is ignored.");
                return;
            }

            // Ignore multiple setting of new instance of the same object.
            // Can happen in e.g. a MAUI application.
            if (location != null && ReferenceEquals(value.GetType(), location.GetType()))
            {
                PdfSharpLogHost.Logger.LogWarning("Setting another instance of the same type of a font resolver is ignored.");
                return;
            }

            if (FontFactory.HasFontSources)
            {
#if DEBUG
                var config = Capabilities.Build.BuildName;

                var info1 = value?.ToString();
                var info2 = location?.ToString();
                var info3 = FontFactory.GetFontCachesState();
                // When a Unit Tests throws this exception, we are grateful for any piece of information we can get.
                throw new InvalidOperationException(
                    $"You must not change font resolver after is was once used. New: {value?.GetType().Name ?? "<null>"}. Is: {location?.GetType().Name ?? "<null>"}. " +
                    $"Info1: {info1}. Info2: {info2}. Config: {config}." +
                    $"Cache: {info3}.");
#else
                throw new InvalidOperationException("You must not change font resolver after is was once used.");
#endif
            }

            location = value;
        }

        /// <summary>
        /// Adds a font resolver. NYI
        /// </summary>
        /// <param name="fontResolver">The font resolver.</param>
        [Obsolete("Not yet implemented.")]
        /*public*/
        static void AddFontResolver(IFontResolverMarker fontResolver)
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
        internal static void ResetAll(bool calledFromResetFontManagement = false)
        {
            if (calledFromResetFontManagement)
                PdfSharpLogHost.Logger.LogInformation("PDFsharp font management is about to be reset.");

            Globals.Global.Fonts.FontResolver = null;
            Globals.Global.Fonts.FallbackFontResolver = null;
            Globals.Global.Fonts.UseWindowsFontsUnderWindows = null;
            Globals.Global.Fonts.UseWindowsFontsUnderWsl2 = null;
            GlyphTypefaceCache.Reset();
            FontDescriptorCache.Reset();
            FontFactory.Reset();
            FontFamilyCache.Reset();
            OpenTypeFontFaceCache.Reset();
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
                if (!Globals.Global.Fonts.FontEncodingInitialized)
                    DefaultFontEncoding = PdfFontEncoding.Automatic;
                return Globals.Global.Fonts.FontEncoding;
            }
            set
            {
                try
                {
                    Lock.EnterFontFactory();
                    if (Globals.Global.Fonts.FontEncodingInitialized)
                    {
                        // Ignore multiple setting e.g. in a web application.
                        if (Globals.Global.Fonts.FontEncoding == value)
                            return;
                        throw new InvalidOperationException("Must not change DefaultFontEncoding after it was set once.");
                    }

                    Globals.Global.Fonts.FontEncoding = value;
                    Globals.Global.Fonts.FontEncodingInitialized = true;
                }
                finally { Lock.ExitFontFactory(); }
            }
        }

#if CORE
        /// <summary>
        /// Gets or sets a value that defines what to do if the Core build of PDFsharp runs under Windows.
        /// If true, PDFsharp uses the build-in WindowsPlatformFontResolver to resolve some standards fonts like Arial or Times New Roman if
        /// the code runs under Windows. If false, which is default, you must provide your own custom font resolver.
        /// We recommend to use always a custom font resolver for a PDFsharp Core build.
        /// </summary>
        public static bool UseWindowsFontsUnderWindows
        {
            get
            {
                // If not opted-in we do not use Windows fonts anymore.
                if (Globals.Global.Fonts.UseWindowsFontsUnderWindows == null)
                    UseWindowsFontsUnderWindows = false;
                return Globals.Global.Fonts.UseWindowsFontsUnderWindows ?? false;
            }
            set
            {
                try
                {
                    Lock.EnterFontFactory();
                    if (Globals.Global.Fonts.UseWindowsFontsUnderWindows.HasValue)
                    {
                        // Ignore multiple setting e.g. in a web application.
                        if (Globals.Global.Fonts.UseWindowsFontsUnderWindows == value)
                            return;
                        throw new InvalidOperationException("Must not change UseWindowsFontsUnderWindows after it was once set or got.");
                    }

                    Globals.Global.Fonts.UseWindowsFontsUnderWindows = value;
                }
                finally { Lock.ExitFontFactory(); }
            }
        }

        /// <summary>
        /// Gets or sets a value that defines what to do if the Core build of PDFsharp runs under WSL2.
        /// If true, PDFsharp uses the build-in WindowsPlatformFontResolver to resolve some standards fonts like Arial or Times New Roman if
        /// the code runs under WSL2. If false, which is default, you must provide your own custom font resolver.
        /// We recommend to use always a custom font resolver for a PDFsharp Core build.
        /// </summary>
        public static bool UseWindowsFontsUnderWsl2
        {
            get
            {
                // If not opted-in we do not use Windows fonts anymore.
                if (Globals.Global.Fonts.UseWindowsFontsUnderWsl2 == null)
                    UseWindowsFontsUnderWsl2 = false;
                return Globals.Global.Fonts.UseWindowsFontsUnderWsl2 ?? false;
            }
            set
            {
                try
                {
                    Lock.EnterFontFactory();
                    if (Globals.Global.Fonts.UseWindowsFontsUnderWsl2.HasValue)
                    {
                        // Ignore multiple setting e.g. in a web application.
                        if (Globals.Global.Fonts.UseWindowsFontsUnderWsl2 == value)
                            return;
                        throw new InvalidOperationException("Must not change UseWindowsFontsUnderWsl2 after it was once set or got.");
                    }

                    Globals.Global.Fonts.UseWindowsFontsUnderWsl2 = value;
                }
                finally { Lock.ExitFontFactory(); }
            }
        }
#elif GDI || WPF
        /// <summary>
        /// This property has no effect under a GDI or WPF build of PDFsharp.
        /// </summary>
        public static bool UseWindowsFontsUnderWindows
        {
            get => true;
            set
            {
                if (value == false)
                    PdfSharpLogHost.Logger.LogWarning("Setting UseWindowsFontsUnderWindows to false makes no sense and is ignored.");
            }
        }

        /// <summary>
        /// This property has no effect under a GDI or WPF build of PDFsharp.
        /// </summary>
        public static bool UseWindowsFontsUnderWsl2
        {
            get => false; // We are always under Windows.
            set
            {
                if (value)
                    PdfSharpLogHost.Logger.LogWarning("Setting UseWindowsFontsUnderWsl2 to true makes no sense under Windows and is ignored.");
            }
        }
#else
#error Should not come here
#endif

        /// <summary>
        /// Shortcut for PdfSharpCore.ResetFontManagement.
        /// </summary>
        public static void ResetFontManagement() => PdfSharpCore.ResetFontManagement();

        internal static void Reset()
        {
            // Reset font resolvers.
            Globals.Global.Fonts.FontResolver = null;
            Globals.Global.Fonts.FallbackFontResolver = null;

            // Reset mode of PlatformFontResolver.
            Globals.Global.Fonts.UseWindowsFontsUnderWindows = null;
            Globals.Global.Fonts.UseWindowsFontsUnderWsl2 = null;
        }
    }
}

namespace PdfSharp.Internal
{
    partial class Globals
    {
        partial class FontStorage
        {
            /// <summary>
            /// The globally set custom font resolver.
            /// </summary>
            public IFontResolver? FontResolver;

            /// <summary>
            /// The globally set fallback font resolver.
            /// </summary>
            public IFontResolver? FallbackFontResolver;

            /// <summary>
            /// The font encoding default. Do not change.
            /// </summary>
            public PdfFontEncoding FontEncoding;

            /// <summary>
            /// Is true if FontEncoding was set by user code.
            /// </summary>
            public bool FontEncodingInitialized;

            /// <summary>
            /// If true PDFsharp uses some Windows fonts like Arial, Times New Roman from
            /// C:\Windows\Fonts if the code runs under Windows.
            /// </summary>
            public bool? UseWindowsFontsUnderWindows;

            /// <summary>
            /// If true PDFsharp uses some Windows fonts like Arial, Times New Roman from
            /// /mnt/c/Windows/Fonts if the code runs under WSL2.
            /// </summary>
            public bool? UseWindowsFontsUnderWsl2;
        }
    }
}
