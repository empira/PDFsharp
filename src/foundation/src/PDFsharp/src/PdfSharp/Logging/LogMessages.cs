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
        public const int DocumentCreate = StartId + 0;
        public const int PageCreate = StartId + 1;
        public const int GraphicsCreate = StartId + 2;
        public const int FontCreate = StartId + 3;

        public const int PdfReaderIssue = StartId + 10;

        internal const int Placeholder = StartId + 1234;
        const int StartId = 50000;
    };

    public static class PdfSharpEventName
    {
        public const string DocumentCreate = "Document creation";
        public const string PageCreate = "Page creation";
        public const string GraphicsCreate = "Graphics creation";
        public const string FontCreate = "Font creation";

        public const string PdfReaderIssue = "PDF reader issue";
    }

    public static class PdfSharpEvent
    {
        public static EventId DocumentCreate = new(PdfSharpEventId.DocumentCreate, PdfSharpEventName.DocumentCreate);
        public static EventId PageCreate = new(PdfSharpEventId.PageCreate, PdfSharpEventName.PageCreate);
        public static EventId FontCreate = new(PdfSharpEventId.FontCreate, PdfSharpEventName.FontCreate);

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
            EventId = PdfSharpEventId.DocumentCreate,
            EventName = PdfSharpEventName.DocumentCreate,
            Message = "New PDF document '{DocumentName}' created.")]
        public static partial void PdfDocumentCreated(this ILogger logger, 
            string? documentName);

        [LoggerMessage(
            Level = LogLevel.Information,
            EventId = PdfSharpEventId.DocumentCreate,
            EventName = PdfSharpEventName.DocumentCreate,
            Message = "PDF document '{documentName}' saved.")]
        public static partial void PdfDocumentSaved(this ILogger logger,
            string? documentName);

        [LoggerMessage(
            Level = LogLevel.Information,
            EventId = PdfSharpEventId.PageCreate,
            EventName = PdfSharpEventName.PageCreate,
            Message = "New PDF page added to document '{documentName}'.")]
        public static partial void NewPdfPageAdded(this ILogger logger,
            string? documentName);

        [LoggerMessage(
            Level = LogLevel.Information,
            EventId = PdfSharpEventId.PageCreate,
            EventName = PdfSharpEventName.PageCreate,
            Message = "Existing PDF page added to document '{documentName}'.")]
        public static partial void ExistingPdfPageAdded(this ILogger logger,
            string? documentName);

        [LoggerMessage(
            Level = LogLevel.Information,
            EventId = PdfSharpEventId.GraphicsCreate,
            EventName = PdfSharpEventName.GraphicsCreate,
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
