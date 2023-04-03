using System;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;

namespace Shared.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });

            ILogger logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Example log message");

            LogHost.Factory = loggerFactory;

            LogHost.Logger.LogError("Something went wrong.");

            LogHost.Logger.TestMessage(LogLevel.Critical, "blah");
            LogHost.Logger.TestMessage("blub");

            Console.ReadLine();
        }
    }
}
