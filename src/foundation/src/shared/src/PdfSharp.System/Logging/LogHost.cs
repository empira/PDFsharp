// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

namespace PdfSharp.Logging
{
    /// <summary>
    /// HACK to use logging without a hosted app.
    /// </summary>
    public static class LogHost
    {
        //static LogHost()
        //{
        //    //TheLogger = LoggerFactory.Create(builder => { }).CreateLogger("Test");
        //}

        /// <summary>
        /// Gets or sets the logger factory for PDFsharp.
        /// </summary>
        public static ILoggerFactory Factory
        {
            get
            {
                return _loggerFactory ??= LoggerFactory.Create(
                    builder =>
                    {
                        builder
                            //.l
                            .AddConsole();
                    });
            }
            set => _loggerFactory = value;
        }
        static ILoggerFactory? _loggerFactory;

        /// <summary>
        /// Gets the global PDFsharp logger.
        /// </summary>
        public static ILogger Logger
        {
            get => _logger ??= CreateLogger();
            private set => _logger = value;
        }
        static ILogger _logger = default!;

        /// <summary>
        /// Creates a logger without a category.
        /// </summary>
        public static ILogger CreateLogger() => Factory.CreateLogger("default category");

        /// <summary>
        /// Creates a logger with the full name of the given type as category.
        /// </summary>
        public static ILogger<T> CreateLogger<T>() => Factory.CreateLogger<T>();
    }
}
