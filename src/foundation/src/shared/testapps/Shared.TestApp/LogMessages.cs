using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.TestApp
{
    public static partial class TestAppLogMessages
    {
        [LoggerMessage(
            EventId = 33,
            //Level = LogLevel.Information,
            Message = "This is a test: `{someText}`")]
        public static partial void TestMessage(
            this ILogger logger,
            LogLevel level, /* Dynamic log level as parameter, rather than defined in attribute. */
            string someText);

        [LoggerMessage(
            EventId = 42,
            Level = LogLevel.Information,
            Message = "This is a test: `{someText}`")]
        public static partial void TestMessage(
            this ILogger logger,
            string someText);

    }
}
