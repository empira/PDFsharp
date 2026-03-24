// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using FluentAssertions;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using Xunit;

// TODO REMOVED
#if false
namespace PdfSharp.Tests.IO
{
    [Collection("PDFsharp")]
    public class CLexerTests
    {
        [Theory]
        [InlineData("q (text) Tj Q ")]  // This works.
        [InlineData("q (text) Tj Q")]   // This works.
        public void Content_Can_Be_Parsed_And_Reconstructed(string contentString)
        {
            var contentBytes = Encoding.UTF8.GetBytes(contentString);

            var sequence = ContentReader.ReadContent(contentBytes);
            var cw = new ContentWriter(new ContentWriterOptions());
            foreach (var obj in sequence)
            {
                obj.WriteObject(cw);
            }

            // ContentWriter adds a newline after each operator
            cw.ToString().Should().Be("q\n(text)Tj\nQ\n");

            // Is this intended? ToString() writes only operator-names but not the operands...
            var s = sequence.ToString();    // result: "q Tj Q"
        }

        [Fact]
        public void Content_Can_Be_Manually_Constructed()
        {
            var sequence = new CSequence();
            var op = OpCodes.OperatorFromName("q");
            sequence.Add(op);
            op = OpCodes.OperatorFromName("Tj");
            op.Operands.Add(new CString("text"));
            sequence.Add(op);
            op = OpCodes.OperatorFromName("Q");
            sequence.Add(op);

            var cw = new ContentWriter(new ContentWriterOptions());
            foreach (var obj in sequence)
            {
                obj.WriteObject(cw);
            }

            // ContentWriter adds a newline after each operator.
            cw.ToString().Should().Be("q\n(text)Tj\nQ\n");
        }

        [Theory]
        [InlineData("<7465787420> Tj")]  // This works.
        [InlineData("<746578742> Tj")]   // This had to be fixed (if the final digit of a hex string is missing,
                                                     // it shall be assumed to be 0 according to the pdf reference).
        public void Can_Parse_Hex_String_With_Odd_Length(string contentString)
        {
            var contentBytes = Encoding.UTF8.GetBytes(contentString);

            var sequence = ContentReader.ReadContent(contentBytes);
            var cw = new ContentWriter(new ContentWriterOptions());
            foreach (var obj in sequence)
            {
                obj.WriteObject(cw); // B/UG:
                                     // A hex string read by ContentReader was converted to a CStringType.String CString before.
                                     // Now, it remains a CStringType.HexString CString, to be able to write content as it was read.
                                     // Is other code affected by this change? Provide a way to get the CStringType.String value?
            }

            // ContentWriter adds a newline after each operator.
            cw.ToString().Should().Be("<7465787420>Tj\n");
        }
    }
}
#endif