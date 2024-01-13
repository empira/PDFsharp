using System.Text;
using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using Xunit;

namespace PdfSharp.Tests
{
    public class CLexerTests
    {
        [Theory]
        [InlineData("q (text) Tj Q ")]  // This works.
        [InlineData("q (text) Tj Q")]   // This works.
        public void Content_Can_Be_Parsed_And_Reconstructed(string contentString)
        {
            var contentBytes = Encoding.UTF8.GetBytes(contentString);

            var sequence = ContentReader.ReadContent(contentBytes);
            using var ms = new MemoryStream();
            var cw = new ContentWriter(ms);
            foreach (var obj in sequence)
            {
                obj.WriteObject(cw);
            }
            var newContent = new PdfContent(new PdfDictionary());
            newContent.CreateStream(ms.ToArray());

            var content = newContent.Stream.ToString();
            // ContentWriter adds a newline after each operator
            newContent.Stream.ToString().Should().Be("q\n(text)Tj\nQ\n");
            // is this intended ? ToString() writes only operator-names but not the operands...
            var s = sequence.ToString();    // result: "qTjQ"
        }

        [Fact]
        public void Content_Can_Be_Manually_Constructed()
        {
            var sequence = new CSequence();
            var op = OpCodes.OperatorFromName("q");
            sequence.Add(op);
            op = OpCodes.OperatorFromName("Tj");
            op.Operands.Add(new CString { CStringType = CStringType.String, Value = "text" });
            sequence.Add(op);
            op = OpCodes.OperatorFromName("Q");
            sequence.Add(op);

            byte[] text = null!;
            using (var ms = new MemoryStream())
            {
                var cw = new ContentWriter(ms);
                foreach (var obj in sequence)
                {
                    obj.WriteObject(cw);
                }
                cw.Close();
                text = ms.ToArray();
            }
            var newContent = new PdfContent(new PdfDictionary());
            newContent.CreateStream(text);

            var text2 = newContent.Stream.ToString();
            // ContentWriter adds a newline after each operator.
            text2.Should().Be("q\n(text)Tj\nQ\n");
        }

        [Theory]
        [InlineData("<7465787420> Tj")]  // This works.
        [InlineData("<746578742> Tj")]   // This had to be fixed.
        public void Can_Parse_Hex_String_With_Odd_Length(string contentString)
        {
            var contentBytes = Encoding.UTF8.GetBytes(contentString);

            var sequence = ContentReader.ReadContent(contentBytes);
            using var ms = new MemoryStream();
            var cw = new ContentWriter(ms);
            foreach (var obj in sequence)
            {
                obj.WriteObject(cw);
            }
            var newContent = new PdfContent(new PdfDictionary());
            newContent.CreateStream(ms.ToArray());

            // ContentWriter adds a newline after each operator.
            newContent.Stream.ToString().Should().Be("(text )Tj\n");
        }
    }
}
