// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace PdfSharp.TestHelper
{
    /// <summary>
    /// A Logger which saves entries in memory for further inspection.
    /// </summary>
    public class MemoryLogger : ILogger
    {
        public MemoryLogger(string name, MemoryLoggerConfiguration config)
        {
            _config = config;
            Name = name;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, String> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var logEntry = new LogEntry
            {
                LogLevel = logLevel,
                EventId = eventId,
                Message = formatter(state, exception)
            };

            _logEntries.Add(logEntry);
        }

        /// <summary>
        /// Gets all log entries for further inspection.
        /// </summary>
        public IEnumerable<LogEntry> GetLogEntries() => _logEntries;

        /// <summary>
        /// Clears all log entries.
        /// </summary>
        public void Clear() => _logEntries.Clear();

        public Boolean IsEnabled(LogLevel logLevel)
        {
            var level = _config.LogLevel;
            return level != LogLevel.None && logLevel >= level;
        }

        public IDisposable BeginScope<TState>(TState state) => default!;

        public string Name { get; }

        readonly MemoryLoggerConfiguration _config;

        readonly List<LogEntry> _logEntries = [];

        public class LogEntry
        {
            public LogLevel LogLevel;

            public EventId EventId;

            public String? Message;
        }
    }

    /// <summary>
    /// A LoggerProvider to create a MemoryLogger which saves entries in memory for further inspection.
    /// </summary>
    public class MemoryLoggerProvider : ILoggerProvider
    {
        public MemoryLoggerProvider(LogLevel logLevel)
        {
            _config = new MemoryLoggerConfiguration
            {
                LogLevel = logLevel
            };
        }
        
        public ILogger CreateLogger(string categoryName = "") =>
            _loggers.GetOrAdd(categoryName, name => new MemoryLogger(name, _config));

        /// <summary>
        /// Gets all loggers for further inspection.
        /// </summary>
        public IReadOnlyDictionary<string, MemoryLogger> GetLoggers() => _loggers;

        /// <summary>
        /// Gets the logger for the given category for further inspection.
        /// </summary>
        public MemoryLogger? GetLogger(string categoryName)
        {
            _loggers.TryGetValue(categoryName, out var logger);
            return logger;
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        MemoryLoggerConfiguration _config;
        readonly ConcurrentDictionary<string, MemoryLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class MemoryLoggerConfiguration
    {
        public LogLevel LogLevel { get; set; }
    }
}
