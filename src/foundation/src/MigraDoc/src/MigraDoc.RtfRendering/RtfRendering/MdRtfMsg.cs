// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

namespace MigraDoc.RtfRendering
{
    readonly struct MdRtfMsg(MdRtfMsgId id, string message)
    {
        public MdRtfMsgId Id { get; } = id;

        public string Message { get; } = message;

        public EventId EventId => new((int)Id, EventName);

        public string EventName => Id.ToString();
    }
}
