// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace MigraDoc
{
    /// <summary>
    /// UNDER CONSTRUCTION - DO NOT USE.
    /// Capabilities.Fonts.IsAvailable.GlyphToPath
    /// </summary>
    public static class Capabilities
    {
        // Duplicated from PDFsharp. TODO Unify.
        /// <summary>
        /// Defines the action to be taken if a requested feature is not available
        /// in the current build.
        /// </summary>
        public enum FeatureNotAvailableAction  // RENAME ProblemBehavior
        {
            /// <summary>
            /// Do nothing.
            /// </summary>
            DoNothing = 0,

            /// <summary>
            /// The log warning.
            /// </summary>
            LogInformation = 1,

            /// <summary>
            /// The log warning.
            /// </summary>
            LogWarning = 2,

            /// <summary>
            /// The log error.
            /// </summary>
            LogError = 3,

            /// <summary>
            /// The fail with exception.
            /// </summary>
            FailWithException = 4
        }

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
            /// Gets or sets a flag that defines what LastSection does if no section exists.
            /// If false, which is the default value, a new section is created and returned.
            /// If true, no section will be created and null is returned instead.
            /// </summary>
            public static bool DoNotCreateLastSection { get; set; } = false;

            // TODO Space before on new page

            /// <summary>
            /// In RTF a single decimal tabstop in a table is a special case.
            /// In contrast to other tabstops, multiple tabstops in tables and decimal tabstops outside of tables,
            /// the content is here aligned to the tabstop without the need of a tab.
            /// Adding a tab in RTF would move the content to the next (not existing) tabstop and would perhaps
            /// result in an unwanted line break.
            /// Since Version 6.0.0 PDFsharp provides a consistent behavior through PDF and RTF generation
            /// and all tabstop usages. With that change a tab is always needed to reach a tabstop.
            /// To achieve this, the RTF TabStopsRenderer will render an additional left aligned tabstop
            /// at position 0, if TabStops is inside a cell and contains only one tabstop, which is set to
            /// decimal alignment and a position greater than 0.
            /// Set this property to true to reactivate the old behavior without any corrections for
            /// this special case by PDFsharp.
            /// </summary>
            public static bool DoNotUnifyTabStopHandling { get; set; } = false;
        }

        //public static class Action
        //{
        //    /// <summary>
        //    /// Gets or sets the action to be taken when trying to convert glyphs into a graphical path
        //    /// and this feature is currently not available.
        //    /// </summary>
        //    public static FeatureNotAvailableAction GlyphsToPath { get; set; } = FeatureNotAvailableAction.DoNothing;
        //}

        /// <summary>
        /// Compatibility settings for MigraDoc.
        /// </summary>
        public static class Compatibility
        {
            /// <summary>
            /// Gets or sets a flag that specifies if the renderer should silently ignore charts if they are not supported.
            /// Otherwise, an exception will be thrown.
            /// </summary>
            public static FeatureNotAvailableAction ChartsCannotBeRendered { get; set; } = FeatureNotAvailableAction.DoNothing;

            // TODO Barcodes etc.
        }
    }
}
