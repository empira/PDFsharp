// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using Xunit;

namespace PdfSharp.Tests.Pdf
{
    [Collection("PDFsharp")]
    public class StringParserTests
    {
        [Fact]
        public void Test_Panose_string()
        {
            var doc = new PdfDocument();
            doc.Pages.Add();

            var catalog = doc.Catalog;
            var dict = new PdfDictionary(doc, true);
            catalog.Elements.Add("/TestStuff", dict);

            // Sample from the specs.
            dict.Elements.Add("/Panose1", new PdfDebugItem("<01 05 02 02 03 00 00 00 00 00 00 00>"));

            // "< 0 0 2 b 6 6 3 8 4 2 2 4>" comes from a bug report.
            // According to the specs (PDF 2.0: 7.3.4.3  Hexadecimal strings / Page 27)
            // white space shall be ignored:
            //   “Each pair of hexadecimal digits defines one byte of the string. White-space characters
            //    shall be ignored.”
            // "< 0 0 2 b 6 6 3 8 4 2 2 4>" => "<002b66384224>"
            // is a string of 6 bytes.
            // However, the string comes with a /Panose entry and is expected to be 12 bytes.
            // That is:
            // "< 0 0 2 b 6 6 3 8 4 2 2 4>" => "<00 00 02 0b 06 06 03 08 04 02 02 04>" or "<0000020b0606030804020204>"
            // Therefore PDFsharp now interprets a single hex character "x" as "0x".
            dict.Elements.Add("/Panose2", new PdfDebugItem("< 0 0 2 b 6 6 3 8 4 2 2 4>"));

            dict.Elements.Add("/Panose3", new PdfDebugItem("< 0 0 2 b 6 6 3 8 4 2 2 4 >"));
            dict.Elements.Add("/Panose4", new PdfDebugItem("< 00 00 02 0b 06 06 03 08 04 02 02 04 >"));

            var filename = PdfFileUtility.GetTempPdfFullFileName("Parsers/" + nameof(Test_Panose_string));
            doc.Options.Layout = PdfWriterLayout.Verbose;
            doc.Save(filename);

            var doc2 = PdfReader.Open(filename, PdfDocumentOpenMode.Modify);
            var stuff = doc2.Catalog.Elements.GetRequiredDictionary("/TestStuff");
            var p1 = stuff.Elements.GetString("/Panose1");
            var p2 = stuff.Elements.GetString("/Panose2");
            var p3 = stuff.Elements.GetString("/Panose3");
            var p4 = stuff.Elements.GetString("/Panose4");

            p1.Length.Should().Be(12);
            p2.Length.Should().Be(12);
            p2.Should().Be(p3);
            p2.Should().Be(p4);

            filename = PdfFileUtility.GetTempPdfFullFileName("Parsers/" + nameof(Test_Panose_string) + "#2");
            doc2.Options.Layout = PdfWriterLayout.Verbose;
            doc2.Save(filename);

            // PDFsharp wrote the following code in the first place:
            //
            // /Panose1 <010502020300000000000000>
            // /Panose2 <0000020B0606030804020240>
            // /Panose3 <0000020B0606030804020204>
            // /Panose4 <0000020B0606030804020204>
            // 
            // Note that the last "4" in /Panose2 goes to "40".
            // To fix this I add a hack to ScanHexadecimalString in case
            // the hex-string is presumable a 12 byte Panose style code.
        }
    }
}
