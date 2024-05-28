// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;

namespace MigraDoc.Logging
{
    /// <summary>
    /// Defines the logging categories of MigraDoc.
    /// </summary>
    public static class MigraDocLogCategory
    {
        /// <summary>
        /// Logger category for general MigraDoc matters.
        /// </summary>
        public const string MigraDoc = "MigraDoc";

        /// <summary>
        /// Logger category for the MigraDoc Document Object Model (MD DOM).
        /// </summary>
        public const string DocumentModel = "MigraDoc DOM";

        /// <summary>
        /// Logger category for PDF rendering.
        /// </summary>
        public const string PdfRendering = "MigraDoc PDF rendering";

        /// <summary>
        /// Logger category for RTF Rendering.
        /// </summary>
        public const string RtfRendering = "MigraDoc RTF rendering";
    }

    /// <summary>
    /// Provides a single host for logging in MigraDoc.
    /// The logger factory is taken from LogHost.
    /// </summary>
    public static class MigraDocLogHost
    {
        /// <summary>
        /// Gets the global MigraDoc logger.
        /// </summary>
        public static ILogger MigraDocLogger
        {
            get
            {
                // We do not need lock, because even creating two loggers has no negative effects.
                //lock (typeof(MigraDocLogHost)) // Keep for documenting that it is by design not to lock.
                {
                    CheckFactoryHasChanged();
                    return _migraDocLogger ??= LogHost.CreateLogger(MigraDocLogCategory.MigraDoc);
                }
            }
        }
        static ILogger? _migraDocLogger;

        #region Specific logger

        /// <summary>
        /// Gets the global MigraDoc document object model logger.
        /// </summary>
        public static ILogger DocumentModelLogger
        {
            get
            {
                //lock (typeof(MigraDocLogHost)) // Keep for documenting that it is by design not to lock.
                {
                    CheckFactoryHasChanged();
                    return _documentModelLogger ??= LogHost.CreateLogger(MigraDocLogCategory.DocumentModel);
                }
            }
        }
        static ILogger? _documentModelLogger;

        /// <summary>
        /// Gets the global MigraDoc PDF rendering logger.
        /// </summary>
        public static ILogger PdfRenderingLogger
        {
            get
            {
                //lock (typeof(MigraDocLogHost)) // Keep for documenting that it is by design not to lock.
                {
                    CheckFactoryHasChanged();
                    return _pdfRenderingLogger ??= LogHost.CreateLogger(MigraDocLogCategory.PdfRendering);
                }
            }
        }
        static ILogger? _pdfRenderingLogger;

        /// <summary>
        /// Gets the global MigraDoc RTF rendering logger.
        /// </summary>
        public static ILogger RtfRenderingLogger
        {
            get
            {
                //lock (typeof(MigraDocLogHost)) // Keep for documenting that it is by design not to lock.
                {
                    CheckFactoryHasChanged();
                    return _rtfRenderingLogger ??= LogHost.CreateLogger(MigraDocLogCategory.RtfRendering);
                }
            }
        }
        static ILogger? _rtfRenderingLogger;

        #endregion

        static void CheckFactoryHasChanged()
        {
            // Sync with LogHot factory.
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
            _migraDocLogger = default;
            _documentModelLogger = default;
            _pdfRenderingLogger = default;
            _rtfRenderingLogger = default;
        }
    }
}
