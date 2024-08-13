// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member because it is for internal use only.

namespace PdfSharp.Logging
{
    /// <summary>
    /// Defines the logging event ids of PDFsharp.
    /// </summary>
    public static class PdfSharpEventId  // draft...
    {
        public const int DocumentCreated = StartId + 1;
        public const int DocumentSaved = StartId + 2;
        public const int PageCreated = StartId + 3;
        public const int PageAdded = StartId + 4;
        public const int GraphicsCreated = StartId + 5;
        public const int FontCreated = StartId + 6;

        public const int PdfReaderIssue = StartId + 10;

        internal const int Placeholder = StartId + 1234;
        const int StartId = 50000;
    };

    public static class PdfSharpEventName
    {
        public const string DocumentCreated = "Document created";
        public const string DocumentSaved = "Document saved";
        public const string PageCreated = "Page created";
        public const string PageAdded = "Page creation2";
        public const string GraphicsCreated = "Graphics created";
        public const string FontCreated = "Font created";

        public const string PdfReaderIssue = "PDF reader issue";
    }

    public static class PdfSharpEvent
    {
        public static EventId DocumentCreate = new(PdfSharpEventId.DocumentCreated, PdfSharpEventName.DocumentCreated);
        public static EventId DocumentSaved = new(PdfSharpEventId.DocumentSaved, PdfSharpEventName.DocumentSaved);
        public static EventId PageCreate = new(PdfSharpEventId.PageCreated, PdfSharpEventName.PageCreated);
        public static EventId PageAdded = new(PdfSharpEventId.PageAdded, PdfSharpEventName.PageAdded);
        public static EventId FontCreate = new(PdfSharpEventId.FontCreated, PdfSharpEventName.FontCreated);

        public static EventId PdfReaderIssue = new(PdfSharpEventId.PdfReaderIssue, PdfSharpEventName.PdfReaderIssue);

        public static EventId Placeholder = new(999999, "Placeholder");
    }

    /// <summary>
    /// Defines the logging high performance messages of PDFsharp.
    /// </summary>
    public static partial class LogMessages
    {
#pragma warning disable SYSLIB1006

        [LoggerMessage(
            Level = LogLevel.Information,
            EventId = PdfSharpEventId.DocumentCreated,
            EventName = PdfSharpEventName.DocumentCreated,
            Message = "New PDF document '{DocumentName}' created.")]
        public static partial void PdfDocumentCreated(this ILogger logger,
            string? documentName);

        [LoggerMessage(
            Level = LogLevel.Information,
            EventId = PdfSharpEventId.DocumentSaved,
            EventName = PdfSharpEventName.DocumentSaved,
            Message = "PDF document '{documentName}' saved.")]
        public static partial void PdfDocumentSaved(this ILogger logger,
            string? documentName);

        [LoggerMessage(
            Level = LogLevel.Information,
            EventId = PdfSharpEventId.PageCreated,
            EventName = PdfSharpEventName.PageCreated,
            Message = "New PDF page added to document '{documentName}'.")]
        public static partial void NewPdfPageCreated(this ILogger logger,
            string? documentName);

        [LoggerMessage(
            Level = LogLevel.Information,
            EventId = PdfSharpEventId.PageAdded,
            EventName = PdfSharpEventName.PageAdded,
            Message = "Existing PDF page added to document '{documentName}'.")]
        public static partial void ExistingPdfPageAdded(this ILogger logger,
            string? documentName);

        [LoggerMessage(
            Level = LogLevel.Information,
            EventId = PdfSharpEventId.GraphicsCreated,
            EventName = PdfSharpEventName.GraphicsCreated,
            Message = "New XGraphics created from '{source}'.")]
        public static partial void XGraphicsCreated(this ILogger logger,
            string? source);

        //[LoggerMessage(EventId = 23, EventName = "hallo", Level = LogLevel.Warning, Message = "This is a warning: `{someText}`")]
        //public static partial void WarningMessage(this ILogger logger, string someText);
    }

    // TODO remove all Console.WriteLine calls.

#if true_
    class LogTestCode
    {
        void FooBar()
        {
            //var ss = PSEventId.Test1;
            //PdfSharpLogHost.Logger.LogError(ss, "message");
            LoggerMessage.Define<char>(LogLevel.Critical, )
        }
    }
#endif
}
