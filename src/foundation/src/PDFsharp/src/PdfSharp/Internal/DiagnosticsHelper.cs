// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows;
#endif
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Logging;

namespace PdfSharp.Internal
{
    /// <summary>
    /// A bunch of internal helper functions.
    /// </summary>
    static class DiagnosticsHelper
    {
        public static void HandleNotImplemented(string message, FeatureNotAvailableBehavior behavior)
        {
            // We do not use LoggerMessage here because this function should only be invoked during development.

            //string text = "Not implemented: " + message;
            const string prefix = "Feature not available: {message}";
            //const string category = "FeatureNotAvailable";
            switch (behavior)
            {
                case FeatureNotAvailableBehavior.SilentlyIgnore:
                    // Do nothing.
                    break;

                case FeatureNotAvailableBehavior.LogInformation:
                    PdfSharpLogHost.Logger.LogInformation(PdfSharpEvent.Placeholder, prefix, message);
                    break;

                case FeatureNotAvailableBehavior.LogWarning:
                    PdfSharpLogHost.Logger.LogWarning(PdfSharpEventId.Placeholder, prefix, message);
                    break;

                case FeatureNotAvailableBehavior.LogError:
                    PdfSharpLogHost.Logger.LogError(PdfSharpEventId.Placeholder, prefix, message);
                    break;

                case FeatureNotAvailableBehavior.ThrowException:
                    throw new NotSupportedException($"Feature not available: {message}");

                default:
                    throw new ArgumentOutOfRangeException(nameof(message), "Behavior does not exist.");
            }
        }

        /// <summary>
        /// Indirectly throws NotImplementedException.
        /// Required because PDFsharp Release builds treat warnings as errors and
        /// throwing NotImplementedException may lead to unreachable code which
        /// crashes the build.
        /// </summary>
        public static void ThrowNotImplementedException(string message)
        {
            throw new NotImplementedException(message);
        }

        public static void ThrowNotSupportedException(string message)
        {
            throw new NotSupportedException(message);
        }
    }

    //class Tracing
    //{
    //    [Conditional("DEBUG")]
    //    public void Foo()
    //    { }
    //}

    /// <summary>
    /// Helper class around the Debugger class.
    /// </summary>
    public static class DebugBreak
    {
        /// <summary>
        /// Call Debugger.Break() if a debugger is attached or when always is set to true.
        /// </summary>
        public static void Break(bool always = false)
        {
#if DEBUG
            if (always || Debugger.IsAttached)
                Debugger.Break();
#endif
        }
    }

    /// <summary>
    /// Internal stuff for development of PDFsharp.
    /// </summary>
    public static class FontsDevHelper
    {
        /// <summary>
        /// Creates font and enforces bold/italic simulation.
        /// </summary>
        public static XFont CreateSpecialFont(string familyName, double emSize, XFontStyleEx style,
            XPdfFontOptions pdfOptions, XStyleSimulations styleSimulations)
        {
            return new XFont(familyName, emSize, style, pdfOptions, styleSimulations);
        }

        /// <summary>
        /// Dumps the font caches to a string.
        /// </summary>
        public static string GetFontCachesState()
        {
            return FontFactory.GetFontCachesState();
        }

        /// <summary>
        /// Get stretch and weight from a glyph typeface.
        /// </summary>
        public static (string Stretch, string Weight) TryGetStretchAndWeight(XGlyphTypeface glyphTypeface)
        {
            var bold = glyphTypeface.IsBold ? " (bold)" : "";

            var stretch = XFontStretches.FontStretchFromFaceName(glyphTypeface.FaceName);
            var weight = XFontWeights.FontWeightFromFaceName(glyphTypeface.FaceName);

            return (stretch.ToString(), weight.ToString() + bold);
        }
    }
}
