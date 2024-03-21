// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Logging;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member because it is UNDER CONSTRUCTION.

namespace PdfSharp
{
    /// <summary>
    /// Defines the action to be taken if a requested feature is not available
    /// in the current build.
    /// </summary>
    public enum FeatureNotAvailableBehavior
    {
        /// <summary>
        /// Silently ignore the parser error.
        /// </summary>
        SilentlyIgnore = 0,

        /// <summary>
        /// Log an information.
        /// </summary>
        LogInformation = 1,

        /// <summary>
        /// Log a  warning.
        /// </summary>
        LogWarning = 2,

        /// <summary>
        /// Log an  error.
        /// </summary>
        LogError = 3,

        /// <summary>
        /// Throw a parser exception.
        /// </summary>
        ThrowException = 4,
    }

    /// <summary>
    /// UNDER CONSTRUCTION
    /// Capabilities.Fonts.IsAvailable.GlyphToPath
    /// </summary>
    public static class Capabilities
    {
        static Capabilities()
        {
#if DEBUG
            var x = Capabilities.IsAvailable.GlyphsToPathFrom(new XFontFamily("test"));

            Capabilities.Action.GlyphsToPath = FeatureNotAvailableBehavior.ThrowException;
            x = Capabilities.Build.IsGdiBuild;
#endif
        }

        /// <summary>
        /// Resets the capabilities settings to the values they have immediately after loading the PDFsharp library.
        /// </summary>
        /// <remarks>
        /// This function is only useful in unit test scenarios and not intended to be called in application code.
        /// </remarks>
        public static void ResetAll()
        {
            LogHost.Logger.LogInformation("All PDFsharp capability settings are about to be reset.");

            Action.GlyphsToPath = FeatureNotAvailableBehavior.SilentlyIgnore;
        }

        /// <summary>
        /// Access to information about the current PDFsharp build via fluent API.
        /// </summary>
        public static class Build
        {
            /// <summary>
            /// Gets the name of the PDFsharp build.
            /// Can be 'CORE', 'GDI', or 'WPF'
            /// </summary>
            public static string BuildName
#if CORE
                => "CORE";
#elif GDI
                => "GDI";
#elif WPF
                => "WPF";
            //#elif MAUI
            //                => "MAUI";
#else
                => "<unkown>";  // Cannot happen.
#endif

            /// <summary>
            /// Gets a value indicating whether this instance is PDFsharp CORE build.
            /// </summary>
            public static bool IsCoreBuild
#if CORE
                => true;
#else
                => false;
#endif

            /// <summary>
            /// Gets a value indicating whether this instance is PDFsharp GDI+ build.
            /// </summary>
            public static bool IsGdiBuild
#if GDI
                => true;
#else
                => false;
#endif

            /// <summary>
            /// Gets a value indicating whether this instance is PDFsharp WPF build.
            /// </summary>
            public static bool IsWpfBuild
#if WPF
                => true;
#else
                => false;
#endif

            /// <summary>
            /// Gets a 3-character abbreviation preceded with a dash of the current
            /// build flavor system.
            /// Valid return values are '-core', '-gdi', '-wpf', or '-xxx'
            /// if the platform is not known.
            /// </summary>
            public static string BuildTag
#if CORE
                => "-core";
#elif GDI
                => "-gdi";
#elif WPF
                => "-wpf";
#else 
                => "-xxx";
#endif
        }

        /// <summary>
        /// Access to information about the currently running operating system.
        /// The functionality supersede functions that are partially not available
        /// in .NET Framework / Standard.
        /// </summary>
        public static class OperatingSystem
        {
            /// <summary>
            /// Indicates whether the current application is running on Windows.
            /// </summary>
            public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;

            /// <summary>
            /// Indicates whether the current application is running on Linux.
            /// </summary>
            public static bool IsLinux => Environment.OSVersion.Platform == PlatformID.Unix;

            /// <summary>
            /// Indicates whether the current application is running on WSL2.
            /// If IsWsl2 is true, IsLinux also is true.
            /// </summary>
            public static bool IsWsl2
            {
                get
                {
                    if (IsLinux)
                    {
                        // The source code of WPF contains hard-coded "C:\Windows".
                        // So this directory is caved in stone forever.
                        return Directory.Exists("/mnt/c/Windows");
                    }
                    return false;
                }
            }

            /// <summary>
            /// Gets a 3-character abbreviation of the current operating system.
            /// Valid return values are 'WIN', 'WSL', 'LNX', 'OSX',
            /// or 'xxx' if the platform is not known.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            public static string OSAbbreviation
            {
                get
                {
                    return Environment.OSVersion.Platform switch
                    {
                        PlatformID.Win32NT => "WIN",
                        PlatformID.Unix => IsWsl2 ? "WSL" : "LNX",
                        PlatformID.MacOSX => "OSX",
                        // IOS, MOS???
                        _ => "XXX"
                    };
                }
            }

            // Also needed...
            // IsUI if we run tests on Azure?
            /*public*/
            // ReSharper disable once InconsistentNaming
            private static bool IsLinuxWithUI => false;
        }

        /// <summary>
        /// Access to feature availability information via fluent API.
        /// </summary>
        public static class IsAvailable
        {
            // Converting the outline of the glyphs of a string into a graphical path is possible,
            // not very difficult to implement, but work that must be done for something presumably
            // nobody really needs. Therefore, it is not available in a CORE build and if the font
            // comes from a font resolver.

            /// <summary>
            /// Gets a value indicating whether XPath.AddString is available in this build of PDFsharp.
            /// It is always false in CORE build. It is true for GDI and WPF builds if the font did not come from a FontResolver.
            /// </summary>
            /// <param name="family">The font family.</param>
            public static bool GlyphsToPathFrom(XFontFamily family)
            {
#if CORE
                return false;
#elif GDI
                return family.GdiFamily != null!;
#elif WPF
                return family.WpfFamily != null!;
#endif
            }

            // - Create XGraphics from image
            // - Font not available
            // - 
        }

        /// <summary>
        /// Access to action information with fluent API.
        /// </summary>
        public static class Action
        {
            /// <summary>
            /// Gets or sets the action to be taken when trying to convert glyphs into a graphical path
            /// and this feature is currently not supported.
            /// </summary>
            public static FeatureNotAvailableBehavior GlyphsToPath { get; set; } = FeatureNotAvailableBehavior.LogError;

            /// <summary>
            /// Gets or sets the action to be taken when a not implemented path operation was invoked.
            /// Currently, AddPie, AddClosedCurve, and AddPath are not implemented.
            /// </summary>
            public static FeatureNotAvailableBehavior PathOperations { get; set; } = FeatureNotAvailableBehavior.LogInformation;
        }

        /// <summary>
        /// Access to compatibility features with fluent API.
        /// </summary>
        public static class Compatibility
        {
            /// <summary>
            /// Gets or sets a flag that defines how cryptographic exceptions should be handled that occur while decrypting objects of an encrypted document.
            /// If false, occurring exceptions will be rethrown and PDFsharp will only open correctly encrypted documents.
            /// If true, occurring exceptions will be caught and only logged for information purposes.
            /// This way PDFsharp will be able to load documents with unencrypted contents that should be encrypted due to the settings of the file.
            /// </summary>
            public static bool IgnoreErrorsOnDecryption { get; set; } = true;  // Define a behavior.
        }

        public static class Features
        {
            public static class Font
            {
                // ReSharper disable once MemberHidesStaticFromOuterClass
                public static class IsAvailable
                {
                    public static bool GlyphsToPathFrom(XFontFamily family) => false;

                    public static bool HasFontResolver(XFontFamily family) => false;
                }

                // ReSharper disable once MemberHidesStaticFromOuterClass
                public static class Action
                {
                    public static FeatureNotAvailableBehavior GlyphsToPath { get; set; } = FeatureNotAvailableBehavior.SilentlyIgnore;
                }
            }

            public static class Images
            {
                // ReSharper disable once MemberHidesStaticFromOuterClass
                public static class IsAvailable
                {
                    public static bool GlyphsToPathFrom(XFontFamily family) => false;

                    public static bool HasFontResolver(XFontFamily family) => false;
                }

                // ReSharper disable once MemberHidesStaticFromOuterClass
                public static class Action
                {
                    public static FeatureNotAvailableBehavior GlyphsToPath { get; set; } = FeatureNotAvailableBehavior.SilentlyIgnore;
                }

            }

            public static class PdfFiles
            {
                // ReSharper disable once MemberHidesStaticFromOuterClass
                public static class IsAvailable
                {
                    public static bool GlyphsToPathFrom(XFontFamily family) => false;

                    public static bool HasFontResolver(XFontFamily family) => false;
                }

                // ReSharper disable once MemberHidesStaticFromOuterClass
                public static class Action
                {
                    public static FeatureNotAvailableBehavior GlyphsToPath { get; set; } = FeatureNotAvailableBehavior.SilentlyIgnore;
                }

            }

        }
    }
}
