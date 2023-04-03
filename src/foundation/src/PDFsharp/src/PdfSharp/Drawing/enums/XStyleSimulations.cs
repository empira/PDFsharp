// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Describes the simulation style of a font.
    /// </summary>
    [Flags]
    public enum XStyleSimulations  // Identical to WpfStyleSimulations.
    {
        /// <summary>
        /// No font style simulation.
        /// </summary>
        None = 0,

        /// <summary>
        /// Bold style simulation.
        /// </summary>
        BoldSimulation = 1,

        /// <summary>
        /// Italic style simulation.
        /// </summary>
        ItalicSimulation = 2,

        /// <summary>
        /// Bold and Italic style simulation.
        /// </summary>
        BoldItalicSimulation = ItalicSimulation | BoldSimulation,
    }
}
