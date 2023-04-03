// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows;
#endif
using System.Diagnostics;
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
        public static void HandleNotImplemented(string message)
        {
            string text = "Not implemented: " + message;
            switch (Diagnostics.NotImplementedBehavior)
            {
                case NotImplementedBehavior.DoNothing:
                    break;

                case NotImplementedBehavior.Log:
                    LogHost.CreateLogger<NotImplementedBehavior>().LogWarning(message);
                    break;

                case NotImplementedBehavior.LogError:
                    LogHost.CreateLogger<NotImplementedBehavior>().LogError(message);
                    break;

                case NotImplementedBehavior.Throw:
                    throw new NotSupportedException(text);

                default:
                    throw new ArgumentOutOfRangeException();
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
        /// Call Debugger.Break() if a debugger is attached.
        /// </summary>
        public static void Break()
        {
            Break(false);
        }

        /// <summary>
        /// Call Debugger.Break() if a debugger is attached or when always is set to true.
        /// </summary>
        public static void Break(bool always)
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
    }
}
