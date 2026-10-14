// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Attachments;
using PdfSharp.Pdf.Signatures;
using Xunit;

namespace PdfSharp.Tests.Internal
{
    [Collection("PDFsharp")]
    public class ErrorMessages
    {
        [Fact]
        public void Test_error_messages()
        {
            //EmbeddedFilesManager.AddFile2();

            var msg1 = new PsCryptoMsg(PsCryptoMsgId.SampleMessage1, "some text");

            var msg2 = new PsCryptoMsg2<PsCryptoMsgId>(PsCryptoMsgId.SampleMessage1, "some more text");
            PsCryptoMsgId id1 = msg2.Id;
            int i2d = ((IErrorMessageInfo)msg2).Id;
        }
    }
}
