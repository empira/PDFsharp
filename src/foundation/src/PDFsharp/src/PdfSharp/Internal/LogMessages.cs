// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

namespace PdfSharp.Internal
{
    /// <summary>
    /// Logger messages.
    /// </summary>
    public static partial class PdfSharpLogMessages
    {
        /// <summary>
        /// The ARGB value 0 is treated as the empty color.
        /// </summary>
        [LoggerMessage(
            //EventId = 42,
            Message = "Could not open socket to `{hostName}`")]
        public static partial void SomeTest(
            this ILogger logger,
            LogLevel level,
            string hostName);


        // What to log?

       

        // TODO remove all Console.WriteLine calls.
    }
}
