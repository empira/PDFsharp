// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using System.Numerics;
using Xunit;

namespace PdfSharp.Tests.IO
{
    /// <summary>
    /// Most Lexer function are tested implicitly by correctly parsing PDF files.
    /// We only test the non-trivial functions.
    /// </summary>
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

        [Fact]
        public void ScanNumberTests()
        {
            var lexer = CreateLexer("123");
            var symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Integer);
            lexer.TokenToInteger.Should().Be(123);

            lexer = CreateLexer("5000000000");
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.LongInteger);
            lexer.TokenToLongInteger.Should().Be(5_000_000_000);

            // Int32.MaxValue must be Integer
            lexer = CreateLexer(Invariant($"{Int32.MaxValue}"));
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Integer);
            lexer.TokenToInteger.Should().Be(Int32.MaxValue);

            // Int32.MaxValue must be Integer, even with leading zeros.
            lexer = CreateLexer(Invariant($"000000{Int32.MaxValue}"));
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Integer);
            lexer.TokenToInteger.Should().Be(Int32.MaxValue);

            // Int32.MaxValue + 1  must be LongInteger
            lexer = CreateLexer(Invariant($"{Int32.MaxValue + 1L}"));
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.LongInteger);
            lexer.TokenToLongInteger.Should().Be(Int32.MaxValue + 1L);

            // Int32.MinValue must be Integer
            lexer = CreateLexer(Invariant($"{Int32.MinValue}"));
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Integer);
            lexer.TokenToInteger.Should().Be(Int32.MinValue);

            // Int32.MinValue - 1  must be LongInteger
            lexer = CreateLexer(Invariant($"{Int32.MinValue - 1L}"));
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.LongInteger);
            lexer.TokenToLongInteger.Should().Be(Int32.MinValue - 1L);

            // Int64.MaxValue must be LongInteger
            lexer = CreateLexer(Invariant($"{Int64.MaxValue}"));
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.LongInteger);
            lexer.TokenToLongInteger.Should().Be(Int64.MaxValue);

            // Int64.MaxValue + 1  must be Real
            lexer = CreateLexer(Invariant($"{(BigInteger)1 + Int64.MaxValue}"));
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Real);
            lexer.TokenToReal.Should().Be((double)((BigInteger)1 + Int64.MaxValue));

            // Int64.MinValue must be LongInteger
            lexer = CreateLexer(Invariant($"{Int64.MinValue}"));
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.LongInteger);
            lexer.TokenToLongInteger.Should().Be(Int64.MinValue);

            // Int64.MinValue - 1  must be Real
            lexer = CreateLexer(Invariant($"{(BigInteger)Int64.MinValue - 1}"));
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Real);
            lexer.TokenToReal.Should().Be((double)((BigInteger)Int64.MinValue - 1));

            // From https://github.com/empira/PDFsharp/issues/223
            lexer = CreateLexer("00000000000001588776");
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Integer);
            lexer.TokenToInteger.Should().Be(00000000000001588776);

            // Real literals

            lexer = CreateLexer("100.124");
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Real);
            lexer.TokenToReal.Should().Be(100.124);

            lexer = CreateLexer("123.");
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Real);
            lexer.TokenToReal.Should().Be(123);

            lexer = CreateLexer("123.00000000000000000000000000000");
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Real);
            lexer.TokenToReal.Should().Be(123);


            lexer = CreateLexer(Invariant($"{Int64.MaxValue - 42}."));
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Real);
            lexer.TokenToReal.Should().Be(Int64.MaxValue - 42);

            lexer = CreateLexer(Invariant($"{Int64.MaxValue - 42}.00"));
            symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Real);
            lexer.TokenToReal.Should().Be(Int64.MaxValue - 42);
        }

        [Theory]
        [InlineData("42", 42, true, true)]
        [InlineData("2147483647", 2147483647, true, true)]  // int32.MaxValue is 2147483647.
        [InlineData("-2147483648", -2147483648, true, true)]
        public void Scan_Integer_Tests(string text, int value, bool testAsLong, bool testAsReal)
        {
            var lexer = CreateLexer(text);
            var symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Integer);
            lexer.TokenToInteger.Should().Be(value);
            if (testAsLong)
                lexer.TokenToLongInteger.Should().Be(value);
            if (testAsReal)
                lexer.TokenToReal.Should().Be(value);
        }

        [Theory]
        [InlineData("9223372036854775807", 9223372036854775807L, true, true)]  // UInt64.MaxValue is 9223372036854775807L.
        [InlineData("-9223372036854775808", -9223372036854775808L, true, true)]
        [InlineData("2147483648", 2147483648, true, true)]  // int32.MaxValue is 2147483647.
        [InlineData("-2147483649", -2147483649, true, true)]
        public void Scan_LongInteger_Tests(string text, long value, bool testAsInteger, bool testAsReal)
        {
            var lexer = CreateLexer(text);
            var symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.LongInteger);
            lexer.TokenToLongInteger.Should().Be(value);
            if (testAsInteger)
            {
                var getInt = () => lexer.TokenToInteger;
                getInt.Should().Throw<InvalidOperationException>();
            }
            if (testAsReal)
                lexer.TokenToReal.Should().Be(value);
        }

        [Theory]
        [InlineData("42.0", 42, true, true)]
        [InlineData("42.17", 42.17, true, true)]
        [InlineData("42.12345678", 42.12345678, true, true)]
        [InlineData("-42.0", -42, true, true)]
        [InlineData("-42.17", -42.17, true, true)]
        [InlineData("-42.12345678", -42.12345678, true, true)]
        [InlineData("9223372036854775808", 9223372036854775808, true, true)]  // UInt64.MaxValue is 9223372036854775807L.
        [InlineData("-9223372036854775809", -9223372036854775809d, true, true)]
        [InlineData("9223372036854775807.0", 9223372036854775807L, true, true)]  // UInt64.MaxValue is 9223372036854775807L.
        [InlineData("-9223372036854775808.0", -9223372036854775808L, true, true)]
        [InlineData("2147483648.0", 2147483648, true, true)]  // int32.MaxValue is 2147483647.
        [InlineData("-2147483649.0", -2147483649, true, true)]
        public void Scan_Real_Tests(string text, double value, bool testAsInteger, bool testAsLong)
        {
            var lexer = CreateLexer(text);
            var symbol = lexer.ScanNumber(false);
            symbol.Should().Be(Symbol.Real);
            lexer.TokenToReal.Should().Be(value);
            if (testAsInteger)
            {
                var getInt = () => lexer.TokenToInteger;
                getInt.Should().Throw<InvalidOperationException>();
            }
            if (testAsLong)
            {
                var getInt = () => lexer.TokenToLongInteger;
                getInt.Should().Throw<InvalidOperationException>();
            }
        }

        public void Scan_ObjRef_Tests(string text, (int, int) objID/*, bool testAsLong, bool testAsReal*/)
        {

        }

        Lexer CreateLexer(string text)
        {
            var pdfString = new PdfString(text, PdfStringEncoding.RawEncoding);
            var bytes = pdfString.GetRawBytes();
            var stream = new MemoryStream(bytes);
            return new(stream, null);
        }
    }
}
