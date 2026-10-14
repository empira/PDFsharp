// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Internal;

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// PDFsharp cryptography message.
    /// </summary>
    readonly struct PsCryptoMsg(PsCryptoMsgId id, string message) : IErrorMessageInfo
    {

        int IErrorMessageInfo.Id => (int)Id;

        public PsCryptoMsgId Id { get; init; } = id;

        public string Name { get; init; } = "Crypto" + id;

        public string Message { get; init; } = message;

        public EventId EventId => new((int)Id, EventName);

        public string EventName => Id.ToString();
    }

    readonly struct PsCryptoMsg2<T>(T id, string message) : IErrorMessageInfo where T : Enum
    {
        // I’m frustrated. After 25 year of programming C# I was not able to
        // cast Id into integer and need to ask ChatGPT to tell me the correct way.
        int IErrorMessageInfo.Id => (int)(object)Id;

        public T Id { get; init; } = id;

        public string Name { get; init; } = "Crypto" + id;

        public string Message { get; init; } = message;

        public EventId EventId => new((int)(object)Id, EventName);

        public string EventName => Id.ToString();
    }
}