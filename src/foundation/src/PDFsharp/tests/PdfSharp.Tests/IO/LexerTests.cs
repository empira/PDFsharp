// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.IO
{
    [Collection("PDFsharp")]
    public class LexerTests
    {
        [Fact]
        public void ReverseSolidusTests()
        {
            // The string we set in PDFsharp.
            const string creator = @"PDFsharp (\PDFsharp library)";
            // How it looks in PDF.
            const string creatorWritten = @"PDFsharp \(\\PDFsharp library\)";
            // What we replace it with to get a superfluous reverse solidus.
            const string creatorReplaced = @"PDFsharp \(x\PDFsharp library\)";
            // What we expect PDFsharp to read back.
            const string creatorExpected = "PDFsharp (xPDFsharp library)";

            var doc = new PdfDocument();
            doc.AddPage();

            // Test with memory stream.
            using var stream = new MemoryStream();

            doc.Info.Creator = creator;

            doc.Save(stream);
            stream.Position = 0;

#if true_
            // Create file to inspect what was written.
            var filename = PdfFileUtility.GetTempPdfFullFileName("PdfSharp/Lexer/ReverseSolidusTests-Before");
            using (var filestream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                stream.CopyTo(filestream);
            }
#endif

            var bytes = stream.GetBuffer();
            var encoding = PdfEncoders.WinAnsiEncoding;
            var text = encoding.GetString(bytes, 0, (int)stream.Length);

            // Verify the written creator.
            var idx = text.IndexOf(creatorWritten, StringComparison.Ordinal);
            idx.Should().BeGreaterThan(8);

            // Manipulate text to get "\P" instead of "\\P".
#if NET6_0_OR_GREATER
            var replacementText = text.Replace(creatorWritten, creatorReplaced, StringComparison.InvariantCulture);
#else
            var replacementText = text.Replace(creatorWritten, creatorReplaced);
#endif
            var modifiedBytes = encoding.GetBytes(replacementText);

            using var modifiedStream = new MemoryStream(modifiedBytes);
            var modifiedDocument = PdfReader.Open(modifiedStream);

#if true_
            // Create file to inspect what was written.
            var filename2 = PdfFileUtility.GetTempPdfFullFileName("PdfSharp/Lexer/ReverseSolidusTests-After");
            modifiedStream.Position = 0;
            using (var filestream2 = new FileStream(filename2, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                modifiedStream.CopyTo(filestream2);
            }
#endif

            modifiedDocument.Info.Creator.Should().Be(creatorExpected);
        }
    }
}
