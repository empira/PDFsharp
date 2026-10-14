// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfSharp.Quality
{
    /// <summary>
    /// Helper class for getting constant values without
    /// getting warnings or unwanted code optimizations.
    /// </summary>
    public static class TrueOrFalse
    {
        /// <summary>
        /// Returns true.
        /// Used in tests for statements like 'if (true)...' without getting
        /// a warning from Visual Studio or ReSharper.
        /// </summary>
        public static bool True => true;

        /// <summary>
        /// Returns false.
        /// Used in tests for statements like 'if (false)...' without getting
        /// a warning from Visual Studio or ReSharper.
        /// </summary>
        public static bool False => false;
    }
}
