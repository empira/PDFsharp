// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp
{
    /// <summary>
    /// Defines the action to be taken if a requested feature is not available
    /// in the current build.
    /// </summary>
    public enum FeatureNotAvailableAction
    {
        /// <summary>
        /// Do Nothing.
        /// </summary>
        DoNothing = 0,

        /// <summary>
        /// The fail with exception
        /// </summary>
        FailWithException = 1,

        /// <summary>
        /// The log warning
        /// </summary>
        LogWarning = 2,

        /// <summary>
        /// The log error
        /// </summary>
        LogError = 3
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

            Capabilities.Action.GlyphsToPath = FeatureNotAvailableAction.FailWithException;
            x = Capabilities.Build.IsGdiBuild;
#endif
        }

        /// <summary>
        /// Access to build information via fluent API.
        /// </summary>
        public static class Build
        {
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
        }

        /// <summary>
        /// Access to feature availability information via fluent API.
        /// </summary>
        public static class IsAvailable
        {
            // Converting the outline of the glyphs of a string into a graphical path is possible,
            // not very difficult to implement, but work that must be done for something presumably
            // nobody really needs. Therefore it is not available in a CORE build and if the font
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
            public static FeatureNotAvailableAction GlyphsToPath { get; set; } = FeatureNotAvailableAction.DoNothing;
        }
    }
}
