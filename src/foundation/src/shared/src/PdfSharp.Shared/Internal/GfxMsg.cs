// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591 // Because this is preview code.
// It should not be public.

using Microsoft.Extensions.Logging;

namespace PdfSharp.Internal
{
    /// <summary>
    /// PDFsharp Graphics message.
    /// </summary>
    readonly struct GfxMsg(SyMsgId id, string message) : IErrorMessageInfo
    {
        int IErrorMessageInfo.Id => (int)Id;

        public string Name => "Gfx-" + Id;

        public SyMsgId Id { get; init; } = id;

        public string Message { get; init; } = message;

        public EventId EventId => new((int)Id, EventName);

        public string EventName => Id.ToString();
    }
}
