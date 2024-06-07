// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Charting
{
    /// <summary>
    /// The Pdf-Sharp-Charting-String-Resources.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once IdentifierTypo
    class PSCSR
    {
        internal static string InvalidChartTypeForCombination(ChartType chartType) 
            => Invariant($"ChartType '{chartType}' not valid for combination of charts.");

        internal static string PercentNotSupportedByColumnDataLabel 
            => "Column data label cannot be set to 'Percent'.";

        public static string RenderInfoNotInitialized(Type type)
            => Invariant($"RenderInfo '{type.Name}' is not fully initialized.");
    }
}
