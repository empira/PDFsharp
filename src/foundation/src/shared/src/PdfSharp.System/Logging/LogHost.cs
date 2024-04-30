// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PdfSharp.Logging
{
    /// <summary>
    /// Defines the logging categories of PDFsharp.
    /// </summary>
    public static class LogCategory
    {
        /// <summary>
        /// Default category for standard logger.
        /// </summary>
        public const string PdfSharp = "PDFsharp";
    }

    /// <summary>
    /// Provides a single global logger factory used for logging in PDFsharp.
    /// </summary>
    public static class LogHost
    {
        /// <summary>
        /// Gets or sets the current global logger factory singleton for PDFsharp.
        /// Every logger used in PDFsharp code is created by this factory.
        /// You can change the logger factory at any one time you want.
        /// If no factory is provided the NullLoggerFactory is used as the default.
        /// </summary>
        public static ILoggerFactory Factory
        {
            get => _loggerFactory ??= new NullLoggerFactory();
            set
            {
                _loggerFactory = value;
                _logger = default;
            }
        }
        static ILoggerFactory? _loggerFactory;

        /// <summary>
        /// Gets the global PDFsharp default logger.
        /// </summary>
        public static ILogger Logger => _logger ??= CreateLogger(LogCategory.PdfSharp);
        static ILogger? _logger;

        /// <summary>
        /// Creates a logger with a given category name.
        /// </summary>
        public static ILogger CreateLogger(string name) => Factory.CreateLogger(name);

        /// <summary>
        /// Creates a logger with the full name of the given type as category name.
        /// </summary>
        public static ILogger<T> CreateLogger<T>() => Factory.CreateLogger<T>();

        /// <summary>
        /// Resets the logging host to the state it has immediately after loading the PDFsharp library.
        /// </summary>
        /// <remarks>
        /// This function is only useful in unit test scenarios and not intended to be called from application code.
        /// </remarks>
        public static void ResetLogging()
        {
            _loggerFactory = default;
            _logger = default;
        }
    }
}
