// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591 // Because this is preview code.
// It should not be public.

using Microsoft.Extensions.Logging;

namespace PdfSharp.Internal
{
    /// <summary>
    /// Provides information about an error.
    /// </summary>
    public interface IErrorMessageInfo
    {
        /// <summary>
        /// The integer value of the error enum.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The name of the error enum.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The event ID for logging.
        /// </summary>
        public EventId EventId { get; }

        /// <summary>
        /// The name of the event.
        /// </summary>
        public string EventName { get; }
    }

    /// <summary>
    /// (PDFsharp) System message.
    /// </summary>
    readonly struct SyMsg(SyMsgId id, string message) : IErrorMessageInfo
    {
        int IErrorMessageInfo.Id => (int)Id;

        public string Name => "Sys-" + Id;

        public SyMsgId Id { get; init; } = id;

        public string Message { get; init; } = message;

        public EventId EventId => new((int)Id, EventName);

        public string EventName => Id.ToString();
    }
}
