// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// MigraDoc DOM message.
    /// </summary>
    readonly struct MdDomMsg(MdDomMsgId id, string message)
    {
        public MdDomMsgId Id { get; init; } = id;

        public string Message { get; init; } = message;

        public EventId EventId => new((int)Id, EventName);

        public string EventName => Id.ToString();
    }
}
