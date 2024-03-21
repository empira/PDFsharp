// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace PdfSharp.Logging
{
    /// <summary>
    /// Defines the logging categories of PDFsharp.
    /// UNDER CONSTRUCTION.
    /// </summary>
    public static class LogCategory
    {
        public const string PdfSharp = "PDFsharp";
        public const string DocumentCreation = "DocumentCreation";
        public const string FontManagement = "FontResolving";
        public const string FontResolving = "FontResolving";
        public const string ImageCreation = "ImageCreation";
        public const string PdfReading = "PdfReading";

        public const string MigraDoc = "MigraDoc";
    }

    /// <summary>
    /// Provides a single global logger factory used for logging in PDFsharp.
    /// </summary>
    public static class LogHost
    {
        /// <summary>
        /// Gets or sets the current global logger factory for PDFsharp.
        /// Every logger used in PDFsharp code is created by this factory.
        /// You can change the logger factory at any one time.
        /// If no factory is provided the NullLoggerFactory is used as the default.
        /// </summary>
        public static ILoggerFactory Factory
        {
            get => _loggerFactory ??= new NullLoggerFactory();
            set
            {
                _loggerFactory = value;
                _logger = default;
                _fontManagementLogger = default;
            }
        }
        static ILoggerFactory _loggerFactory = default!;

        /// <summary>
        /// Gets the global PDFsharp logger.
        /// </summary>
        public static ILogger Logger => _logger ??= CreateLogger(LogCategory.PdfSharp);
        static ILogger? _logger;

        #region Specific logger

        /// <summary>
        /// Gets the global PDFsharp font management logger.
        /// </summary>
        public static ILogger FontManagementLogger => _fontManagementLogger ??= CreateLogger(LogCategory.FontManagement);
        static ILogger? _fontManagementLogger;

        #endregion

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
        /// This function is only useful in unit test scenarios and not intended to be called in application code.
        /// </remarks>
        public static void ResetLogging()
        {
            _loggerFactory = default!;
            _logger = default!;
        }
    }
}
