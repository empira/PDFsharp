// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// Logger messages.
    /// </summary>
    public static partial class MigraDocLogMessages
    {
        /// <summary>
        /// The ARGB value 0 is treated as the empty color.
        /// </summary>
        [LoggerMessage(
            //EventId = AppLogEventIds.MDDOM,
            Message = "ARGB value 0 creates Color.Empty.")]
        public static partial void ArgbValueIsConsideredEmptyColor(
            this ILogger logger,
            LogLevel level);

        // What to log?

        // Image not found (not loadable, not found, ...)
        // Font not found under Linux (use Linux substitution)
        // Information/Trace-level for e.g. TabStop inheritance, style inheritance
        // Differences in RTF vs Word (decimal tab)
        // Performance optimization

        // TODO_OLD Use logging instead of Con/sole.WriteLine.
        // TODO_OLD remove all Con/sole.WriteLine calls.
    }
}
