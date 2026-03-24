//// PDFsharp - A .NET library for processing PDF
//// See the LICENSE file in the solution root for more information.

using System.Globalization;

#if DEBUG || true  // I don’t compile this effin’ crap into a Release build.

namespace PdfSharp.Diagnostics
{
    /// <summary>
    /// Try to fix a ReSharper issue.
    /// </summary>
    public static class DebuggerDisplayHelper
    {
        // As a German I use a German version of Windows, Office, etc. Visual Studio, all developer
        // tools etc. are en-US. During debugging, I want to see decimal points and not decimal commas
        // as common in Germany. Therefore, I write e.g. the struct Point like this:
        //
        //   [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
        //   public struct Point : ...
        //   {
        //     ...
        //     string DebuggerDisplay => Invariant($"Point=({X} {Y})");
        //   }
        //
        // So far, so good. But during a debug session ReSharper says:
        //
        // "Evaluation of method PdfSharp.Graphics.Point.get_DebuggerDisplay requires reading field System.Globalization.CultureInfo._numInfo, which is not available in this context."
        //
        // What???

        static DebuggerDisplayHelper()
        {
            // Initialize InvariantCulture with CultureInfo.InvariantCulture.
            _ = InvariantCulture;
        }

        /// <summary>
        /// Prevent ReSharper from saying:
        /// Evaluation of method {MyType}.get_DebuggerDisplay requires reading field System.Globalization.CultureInfo._numInfo, which is not available in this context.
        /// </summary>
        public static void FixReSharperBugInConnectionWithCultureInfo_numInfo()
        {
            // Just ensures that the class constructor is executed.
        }

        /// <summary>
        /// Number format DebuggerDisplay.
        /// </summary>
        public const string DebugNumberFormat = "0.########;-0.########;0";

        /// <summary>
        /// Get my own copy of InvariantCulture that ReSharper can use.
        /// </summary>
        public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
    }
}
#endif
