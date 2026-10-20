// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Filters;
using Xunit;

namespace PdfSharp.Tests.Filters
{
    [Collection("PDFsharp")]
    public class FilteringTests
    {
        static readonly byte[] ContentStream = Encoding.ASCII.GetBytes("0 0 1 RG 4 w 72 72 m 540 720 l S\n");

        [Fact]
        public void Decode_filter_array_with_null_decode_parms_entry()
        {
            // "/Filter [/FlateDecode] /DecodeParms [null]": a null entry means the filter uses its default parameters.
            var encoded = new FlateDecode().Encode(ContentStream);

            var decoded = Filtering.Decode(encoded, new PdfArray(new PdfName("/FlateDecode")), new PdfArray(PdfNull.Value));

            decoded.Should().Equal(ContentStream);
        }

        [Fact]
        public void Decode_filter_array_with_null_decode_parms()
        {
            var encoded = new FlateDecode().Encode(ContentStream);

            var decoded = Filtering.Decode(encoded, new PdfArray(new PdfName("/FlateDecode")), PdfNull.Value);

            decoded.Should().Equal(ContentStream);
        }

        [Fact]
        public void Draw_page_whose_content_stream_has_null_decode_parms_entry()
        {
            // DocuSign writes page content streams this way.
            using var stream = new MemoryStream(CreatePdfWithNullDecodeParmsEntry());
            using var form = XPdfForm.FromStream(stream);
            var document = new PdfDocument();
            using var gfx = XGraphics.FromPdfPage(document.AddPage());

            gfx.Invoking(g => g.DrawImage(form, 0, 0)).Should().NotThrow();
        }

        static byte[] CreatePdfWithNullDecodeParmsEntry()
        {
            var content = new FlateDecode().Encode(ContentStream);
            var objects = new[]
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << >> /Contents 4 0 R >>",
                $"<< /Filter [/FlateDecode] /DecodeParms [null] /Length {content.Length} >>\nstream\n"
            };

            using var pdf = new MemoryStream();
            void Write(string text) => pdf.Write(Encoding.ASCII.GetBytes(text), 0, text.Length);

            Write("%PDF-1.7\n");
            var offsets = new long[objects.Length];
            for (var i = 0; i < objects.Length; i++)
            {
                offsets[i] = pdf.Position;
                Write($"{i + 1} 0 obj\n{objects[i]}");
                if (i == objects.Length - 1)
                {
                    pdf.Write(content, 0, content.Length);
                    Write("\nendstream");
                }
                Write("\nendobj\n");
            }

            var xref = pdf.Position;
            Write($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
            foreach (var offset in offsets)
                Write($"{offset:D10} 00000 n \n");
            Write($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");

            return pdf.ToArray();
        }
    }
}
