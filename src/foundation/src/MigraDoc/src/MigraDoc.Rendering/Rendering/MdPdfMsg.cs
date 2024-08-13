// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// MigraDoc PDF renderer message.
    /// </summary>
    readonly struct MdPdfMsg(MdPdfMsgId id, string message)
    {
        public MdPdfMsgId Id { get; init; } = id;

        public string Message { get; init; } = message;

        public EventId EventId => new((int)Id, EventName);

        public string EventName => Id.ToString();
    }
}
