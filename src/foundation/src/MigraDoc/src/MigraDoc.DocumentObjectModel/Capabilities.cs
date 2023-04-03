// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace MigraDoc
{

    /// <summary>
    /// UNDER CONSTRUCTION #RENAME Capabilities
    /// Capabilities.Fonts.IsAvailable.GlyphToPath
    /// </summary>
    public static class Capabilities
    {
        //static Capabilities()
        //{
        //    //var x = IsAvailable.GlyphsToPathFrom(new XFontFamily("test"));

        //    //Action.GlyphsToPath = FeatureNotAvailableAction.FailWithException;
        //    //x = Build.IsGdiBuild;
        //}

        /// <summary>
        /// Access to backward compatibility information via fluent API.
        /// </summary>

        public static class BackwardCompatibility
        {
            /// <summary>
            /// Gets or sets a flag that defines what new Color(0) do.
            /// If false, which is the default value, the RGB color black with opacity 0 is created.
            /// If true, Color.Empty is created for backward compatibility to older code using
            /// the MigraDoc Document Object Model.
            /// </summary>
            public static bool TreatArgbZeroAsEmptyColor { get; set; } = false;

            /// <summary>
            /// NYI
            /// </summary>
            public static bool DoNotCreateLastParagraph { get; set; } = false;

            /// <summary>
            /// NYI
            /// </summary>
            public static bool DoNotCreateLastTable { get; set; } = false;

            /// <summary>
            /// NYI
            /// </summary>
            public static bool DoNotCreateLastSection { get; set; } = false;


            // TODO Space before on new page
        }

        //public static class Action
        //{
        //    /// <summary>
        //    /// Gets or sets the action to be taken when trying to convert glyphs into a graphical path
        //    /// and this feature is currently not available.
        //    /// </summary>
        //    public static FeatureNotAvailableAction GlyphsToPath { get; set; } = FeatureNotAvailableAction.DoNothing;
        //}
    }
}
