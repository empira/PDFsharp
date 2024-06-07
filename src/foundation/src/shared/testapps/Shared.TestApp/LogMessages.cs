// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.TestApp
{
    public class Ids
    {
        public const int IdXxxx = 7000;
        public const int IdYyyy = 7010;

    };
    
    public static partial class TestAppLogMessages
    {
        [LoggerMessage(EventId = Ids.IdYyyy, Level = LogLevel.Warning, Message = "This is a warning: `{someText}`")]
        public static partial void WarningMessage(this ILogger logger, string someText);
    }
    
    
    public static partial class TestAppLogMessages
    {
        [LoggerMessage(
            EventId = Ids.IdXxxx,
            //Level = LogLevel.Information,
            Message = "This is a test: `{someText}`")]
        public static partial void TestMessage(
            this ILogger logger,
            LogLevel level, /* Dynamic log level as parameter, rather than defined in attribute. */
            string someText);

        [LoggerMessage(
            EventId = 45, //new EventId(234),
            Level = LogLevel.Information,
            Message = "This is a test: `{someText}`")]
        public static partial void TestMessage(
            this ILogger logger,
            string someText);

    }
}
