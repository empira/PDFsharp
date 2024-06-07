// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

namespace PdfSharp.Logging
{
    /// <summary>
    /// Defines the logging categories of PDFsharp.
    /// </summary>
    public static class PdfSharpLogCategory
    {
        /// <summary>
        /// Logger category for creating or saving documents, adding or removing pages,
        /// and other document level specific action.s
        /// </summary>
        public const string DocumentProcessing = "Document processing";

        /// <summary>
        /// Logger category for processing bitmap images.
        /// </summary>
        public const string ImageProcessing = "Image processing";

        /// <summary>
        /// Logger category for creating XFont objects.
        /// </summary>
        public const string FontManagement = "Font management";

        /// <summary>
        /// Logger category for reading PDF documents.
        /// </summary>
        public const string PdfReading = "PDF reading";
    }

    /// <summary>
    /// Provides a single host for logging in PDFsharp.
    /// The logger factory is taken from LogHost.
    /// </summary>
    public static class PdfSharpLogHost
    {
        /// <summary>
        /// Gets the general PDFsharp logger.
        /// This the same you get from LogHost.Logger.
        /// </summary>
        public static ILogger Logger => LogHost.Logger;

        #region Specific logger

        /// <summary>
        /// Gets the global PDFsharp font management logger.
        /// </summary>
        public static ILogger DocumentProcessingLogger
        {
            get
            {
                // We do not need lock, because even creating two loggers has no negative effects.
                //lock (typeof(MigraDocLogHost)) // Keep for documenting that it is by design not to lock.
                {
                    CheckFactoryHasChanged();
                    return _documentProcessingLogger ??= LogHost.CreateLogger(PdfSharpLogCategory.DocumentProcessing);
                }
            }
        }
        static ILogger? _documentProcessingLogger;

        /// <summary>
        /// Gets the global PDFsharp image processing logger.
        /// </summary>
        public static ILogger ImageProcessingLogger
        {
            get
            {
                //lock (typeof(MigraDocLogHost)) // Keep for documenting that it is by design not to lock.
                {
                    CheckFactoryHasChanged();
                    return _imageProcessingLogger ??= LogHost.CreateLogger(PdfSharpLogCategory.ImageProcessing);
                }
            }
        }
        static ILogger? _imageProcessingLogger;

        /// <summary>
        /// Gets the global PDFsharp font management logger.
        /// </summary>
        public static ILogger FontManagementLogger
        {
            get
            {
                //lock (typeof(MigraDocLogHost)) // Keep for documenting that it is by design not to lock.
                {
                    CheckFactoryHasChanged();
                    return _fontManagementLogger ??= LogHost.CreateLogger(PdfSharpLogCategory.FontManagement);
                }
            }
        }
        static ILogger? _fontManagementLogger;

        /// <summary>
        /// Gets the global PDFsharp document reading logger.
        /// </summary>
        public static ILogger PdfReadingLogger
        {
            get
            {
                //lock (typeof(MigraDocLogHost)) // Keep for documenting that it is by design not to lock.
                {
                    CheckFactoryHasChanged();
                    return _pdfReadingLogger ??= LogHost.CreateLogger(PdfSharpLogCategory.PdfReading);
                }
            }
        }
        static ILogger? _pdfReadingLogger;
        #endregion

        static void CheckFactoryHasChanged()
        {
            // Sync with LogHost factory.
            if (!ReferenceEquals(_factory, LogHost.Factory))
            {
                ResetLogging();
                _factory = LogHost.Factory;
            }
        }
        static ILoggerFactory? _factory;

        /// <summary>
        /// Resets all loggers after an update of global logging factory.
        /// </summary>
        internal static void ResetLogging()
        {
            _factory = default;
            _documentProcessingLogger = default;
            _imageProcessingLogger = default;
            _fontManagementLogger = default;
            _pdfReadingLogger = default;
        }
    }
}
