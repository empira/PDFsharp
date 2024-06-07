// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using System.Globalization;
using System.IO;
using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.IO
{
    [Collection("PDFsharp")]
    public class ReaderTests
    {
        [Fact]
        public void Read_empty_file()
        {
            var data = Array.Empty<byte>();
            using var stream = new MemoryStream(data);

            // A file with 0 bytes is not valid and an exception should occur.
            // ReSharper disable once AccessToDisposedClosure
            Action open = () => PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
            open.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Read_invalid_file()
        {
            var data = new byte[32];
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(33 + i);

            using var stream = new MemoryStream(data);

            // A file with 32 random bytes is not valid and an exception should occur.
            // ReSharper disable once AccessToDisposedClosure
            Action open = () => PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
            open.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Write_and_read_test()
        {
            var doc = new PdfDocument();
            doc.AddPage();
            doc.Info.Author = "empira";
            doc.Info.Title = "Test";

            using var stream = new MemoryStream();
            doc.Save(stream, false);
            stream.Position = 0;
            stream.Length.Should().BeGreaterThan(0);

            var doc2 = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);

            using var stream2 = new MemoryStream();
            doc2.Save(stream2, false);
            stream2.Position = 0;
            stream2.Length.Should().BeGreaterThan(0);

            var doc3 = PdfReader.Open(stream2, PdfDocumentOpenMode.Modify);

            using var stream3 = new MemoryStream();
            doc3.Save(stream3, false);
            stream3.Position = 0;
            stream3.Length.Should().BeGreaterThan(0);

            doc3.Info.Author.Should().Be("empira");
            doc3.Info.Title.Should().Be("Test");
        }

        [Fact]
        public void Read_reals_and_integers_test()
        {
            // Checks Lexer::ScanNumber with various number formats.
            var doc = new PdfDocument();
            var page = doc.AddPage();
            doc.Info.Author = "empira";
            doc.Info.Title = "Test";

            // Now add many different numbers for testing.
            var dict = new PdfDictionary(doc);
            var array = new PdfArray(doc);
            dict.Elements["/empiraPlayground"] = array;
            array.Elements.Add(new PdfLiteral("-32768"));
            array.Elements.Add(new PdfLiteral("-2147483648")); // Int32
            array.Elements.Add(new PdfLiteral("-2147483649")); // Int64
            array.Elements.Add(new PdfLiteral("-2147483648.0")); // Real
            array.Elements.Add(new PdfLiteral("-2147483649.0"));
            array.Elements.Add(new PdfLiteral("-2147483648."));
            array.Elements.Add(new PdfLiteral("-2147483649."));
            array.Elements.Add(new PdfLiteral(".123"));
            array.Elements.Add(new PdfLiteral("-.456"));
            array.Elements.Add(new PdfLiteral("-2147483648.123"));
            array.Elements.Add(new PdfLiteral("-2147483649.123"));
            array.Elements.Add(new PdfLiteral("123.4567890123456789"));
            array.Elements.Add(new PdfLiteral("1234567890.1234567890"));
            // This line causes an error message in Adobe Reader. array.Elements.Add(new PdfLiteral("12345678901234567890.12345678901234567890"));
            array.Elements.Add(new PdfLiteral("-9223372036854775808")); // Int64
            doc.Internals.AddObject(dict);
            doc.Internals.Catalog.Elements["/empiraPlayground"] = PdfInternals.GetReference(dict);

            page.Resources.Elements["/NumberTest"] = array;

            //array = new PdfArray(doc);
            //array.Elements.Add(new PdfInteger(-32768));
            //array.Elements.Add(new PdfIntegerObject(-32768));
            //array.Elements.Add(new PdfReal(17));
            //array.Elements.Add(new PdfRealObject(17));
            //array.Elements.Add(new PdfReal(17.7));
            //array.Elements.Add(new PdfRealObject(17.7));
            //array.Elements.Add(new PdfString("Foo"));
            //array.Elements.Add(new PdfStringObject("Foo_äöüß", PdfStringEncoding.PDFDocEncoding));
            //array.Elements.Add(new PdfStringObject("Foo_äöüß", PdfStringEncoding.WinAnsiEncoding));
            //array.Elements.Add(new PdfStringObject("Foo_äöüß", PdfStringEncoding.Unicode));
            //page.Resources.Elements["/ReferenceTest"] = array;

            using var stream = new MemoryStream();
            doc.Save(stream, false);
            stream.Position = 0;
            stream.Length.Should().BeGreaterThan(0);

            var doc2 = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);

            // Now check if the numbers are correct.
            page = doc2.Pages[0];
            var array2 = page.Resources.Elements["/NumberTest"] as PdfArray;
            Debug.Assert(array2 != null, nameof(array2) + " != null");
            for (int x = 0; x < array2!.Elements.Count; ++x)
            {
                var item1 = array.Elements[x];
                var item2 = array2.Elements[x];
                var num2 = Double.Parse(item1.ToString()!, CultureInfo.InvariantCulture);
                if (item2 is PdfInteger intValue)
                {
                    num2.Should().Be(intValue.Value);
                }
                else if (item2 is PdfLongInteger longValue)
                {
                    num2.Should().Be(longValue.Value);
                }
                else if (item2 is PdfReal realValue)
                {
                    num2.Should().Be(realValue.Value);
                }
                else
                {
                    1.Should().Be(2, "Should not come here!");
                }
            }

            using var stream2 = new MemoryStream();
            doc2.Save(stream2, false);
            stream2.Position = 0;
            stream2.Length.Should().BeGreaterThan(0);

            var doc3 = PdfReader.Open(stream2, PdfDocumentOpenMode.Modify);

            using var stream3 = new MemoryStream();
            doc3.Save(stream3, false);
            stream3.Position = 0;
            stream3.Length.Should().BeGreaterThan(0);

            doc3.Info.Author.Should().Be("empira");
            doc3.Info.Title.Should().Be("Test");
        }

        [Fact]
        public void Custom_properties_test()
        {
            // Checks custom properties with non-ASCII chars.
            var doc = new PdfDocument();
            var page = doc.AddPage();
            doc.Info.Author = "empira";
            doc.Info.Title = "Test";
#if true
            var properties = doc.Info.Elements;
            properties.SetString("/Test", "Test");
            properties.SetString("/UmlautTestÄÖÜäöüß", "UmlautTestÄÖÜäöüß");
            properties.SetString("/ÖÇŞİĞÜüğişçö", "ÖÇŞİĞÜüğişçö");
            properties.SetString("/فثسف", "فثسف");
            properties.SetString(@"/é£½€¨´", @"é£½€¨´");
            properties.SetString("/A#20B", "A#20B");
            properties.SetString("/A B", "A B");
            properties.SetString("/A()B", "A()B");
            properties.SetString("/A<>B", "A<>B");
            properties.SetString("/A<<>>B", "A<<>>B");
            properties.SetString("/AÀÁÂÃÄÅÆÇÈÉÊËB", "AÀÁÂÃÄÅÆÇÈÉÊËB");
            properties.SetString("/🖕 🦄 🦂 🍇 🍆 ☕ 🚂 🛸 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒", "🖕 🦄 🦂 🍇 🍆 ☕ 🚂 🛸 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒");
            properties.SetString("/✓✔✅🐛👌🆗", "✓✔✅🐛👌🆗");
#endif
            using var stream = new MemoryStream();
            doc.Save(stream, false);
            stream.Position = 0;
            stream.Length.Should().BeGreaterThan(0);

            var doc2 = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);

            using var stream2 = new MemoryStream();
            doc2.Save(stream2, false);
            stream2.Position = 0;
            stream2.Length.Should().BeGreaterThan(0);

            var doc3 = PdfReader.Open(stream2, PdfDocumentOpenMode.Modify);

            using var stream3 = new MemoryStream();
            doc3.Save(stream3, false);
            stream3.Position = 0;
            stream3.Length.Should().BeGreaterThan(0);

            doc3.Info.Author.Should().Be("empira");
            doc3.Info.Title.Should().Be("Test");

            var doc4 = PdfReader.Open(stream3, PdfDocumentOpenMode.Modify);
            var filename = PdfFileUtility.GetTempPdfFileName("Custom_properties");
            doc4.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }
    }
}
