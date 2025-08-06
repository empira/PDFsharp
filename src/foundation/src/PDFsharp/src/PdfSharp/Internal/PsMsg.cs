// PDFsharp - A .NET library for processing PDF
// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

namespace PdfSharp.Internal
{
    /// <summary>
    /// PDFsharp message.
    /// </summary>
    readonly struct PsMsg(PsMsgId id, string message)
    {
        public PsMsgId Id { get; init; } = id;

        public string Message { get; init; } = message;

        public EventId EventId => new((int)Id, EventName);

        public string EventName => Id.ToString();
    }
}