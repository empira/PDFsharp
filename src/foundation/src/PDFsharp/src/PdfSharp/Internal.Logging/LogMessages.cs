using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Pdf;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member because it is for internal use only.

namespace PdfSharp.Internal.Logging
{
    /*

       This code is under construction may change.
     
     */

    /// <summary>
    /// Defines the logging event ids of PDFsharp.
    /// UNDER CONSTRUCTION.
    /// </summary>
    public static class PdfSharpEventId
    {
        public const int Placeholder = 50000;

        public const int FontCreate = 50010;
        public const int Test1 = 50100;
        public const int Test2 = 50101;
        public const int Test3 = 50102;



        //public static EventId FontCreate = new(1000, "Error trying to insert a record in database.");
        //public static EventId Test1 = new(1000, "Test 1 log event.");
        //public static EventId Test2 = new(1000, "Test 2 log event.");
        //public static EventId Test3 = new(1000, "Test 3 log event.");
        //public const EventId Placeholder  = new(1000, "(placeholder)");
    };

    /// <summary>
    /// Defines the logging high performance messages of PDFsharp.
    /// UNDER CONSTRUCTION.
    /// </summary>
    public static partial class LogMessages
    {
        // Event ids not yet used.
#pragma warning disable SYSLIB1006

        [LoggerMessage(
            EventId = PdfSharpEventId.Placeholder,
            Level = LogLevel.Information,
            Message = "New PDF document '{documentName}' created.")]
        public static partial void PdfDocumentCreated
            (this ILogger logger, string documentName);

        [LoggerMessage(
            EventId = PdfSharpEventId.Test1,
            Level = LogLevel.Information,
            Message = "PDF document '{documentName}' saved.")]
        public static partial void PdfDocumentSaved
            (this ILogger logger, string documentName);

        [LoggerMessage(
            EventId = PdfSharpEventId.Placeholder + 78,
            Level = LogLevel.Information,
            Message = "New PDF page added to document '{documentName}'.")]
        public static partial void NewPdfPageAdded
            (this ILogger logger, string? documentName);

        [LoggerMessage(
            //EventId = PdfSharpEventId.Placeholder + 78,
            Level = LogLevel.Information,
            Message = "Existing PDF page added to document '{documentName}'.")]
        public static partial void ExistingPdfPageAdded
            (this ILogger logger, string? documentName);

        [LoggerMessage(
            EventId = PdfSharpEventId.Placeholder + 78,
            Level = LogLevel.Information,
            Message = "New XGraphics created from {source}.")]
        public static partial void XGraphicsCreated
            (this ILogger logger, string source);







        [LoggerMessage(EventId = 43, Level = LogLevel.Warning, Message = "This is a warning: `{someText}`")]
        public static partial void WarningMessage(this ILogger logger, string someText);
    }


}
