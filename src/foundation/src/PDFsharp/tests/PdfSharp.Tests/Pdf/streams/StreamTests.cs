// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using Xunit;
using static PdfSharp.Pdf.PdfDictionary;

namespace PdfSharp.Tests.Pdf
{
    [Collection("PDFsharp")]
    public class StreamTests
    {
        [Fact]
        public void Create_a_raw_stream()
        {
            var document = new PdfDocument();
            var page1 = document.AddPage();

            var dict = new PdfDebugDictionary(document)
            {
                StreamLength = -42 // Illegal value.
            };
            dict.Stream = new PdfDictionary.PdfStream("Hello, World!"u8.ToArray(), dict);

            document.Internals.AddObject(dict);
            document.Catalog.Elements.Add("/SomeRawDict", dict.RequiredReference);

            string filename = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/pdf/BasicStreamTest");
            document.Save(filename);
            //PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        /// <summary>
        /// Creates test files with a manipulated, unfiltered stream.
        /// </summary>
        /// <param name="size">Size of the raw stream.</param>
        /// <param name="reportedLength">Length given in PDF file.</param>
        /// <param name="content">Content of the stream.</param>
        /// <param name="shouldFail">True if created files throws an exception on PdfReader.Open.</param>
        [Theory]
        [InlineData(20, 10, Chars.LF, false)]
        [InlineData(20, 10, 'x', false)]
        [InlineData(6000, 10, Chars.LF, true)] // Should work, but does not yet work with 6.2.0 Preview 2.
        [InlineData(6000, 10, 'X', true)] // Should work, but does not yet work with 6.2.0 Preview 2.
        [InlineData(0, 5555, Chars.NUL, false)]
        [InlineData(5555, 0, Chars.SP, true)] // Should work, but does not yet work with 6.2.0 Preview 2.
        public void Create_and_read_a_raw_stream(int size, int reportedLength, char content, bool shouldFail)
        {
#pragma warning disable CS0162 // Unreachable code detected
            // Set SaveToFile to true if you want to inspect the created files.
            // Otherwise, set SaveToFile to false for better performance.
#if DEBUG
            const bool saveToFile = true;
            //const bool saveToFile = false;
#else
            const bool saveToFile = false;
#endif

            var document = new PdfDocument();
            var page1 = document.AddPage();

            var dict = new PdfDebugDictionary(document)
            {
                StreamLength = reportedLength
            };
            var streamData = new byte[size];
            for (int x = 0; x < size; ++x)
                streamData[x] = (byte)content;

            dict.Stream = new PdfDictionary.PdfStream(streamData, dict);

            document.Internals.AddObject(dict);
            document.Catalog.Elements.Add("/SomeRawDict", dict.RequiredReference);

            string filename = PdfFileUtility.GetTempPdfFullFileName($"PDFsharp/UnitTest/pdf/StreamTests--StreamSize_{size}-Length_{reportedLength}-Contents_{(byte)content}-ShouldFail{shouldFail}");
            // ReSharper disable once RedundantAssignment
            var memoryStream = saveToFile ? null : new MemoryStream();

            if (saveToFile)
            {
                document.Save(filename);
                //PdfFileUtility.ShowDocumentIfDebugging(filename);
            }
            else
            {
                document.Save(memoryStream);
            }

            if (shouldFail)
            {
                // The created file is corrupted and cannot be opened by PDFsharp.
                if (saveToFile)
                {
                    var openDocument = () => PdfReader.Open(filename);
                    openDocument.Should().Throw<ObjectNotAvailableException>();
                }
                else
                {
                    memoryStream.Position = 0;
                    var openDocument = () => PdfReader.Open(memoryStream);
                    openDocument.Should().Throw<ObjectNotAvailableException>();
                }

            }
            else
            {
                // The created file is OK or slightly corrupted and can be opened by PDFsharp.
                PdfDocument openedDocument;
                if (saveToFile)
                {
                    openedDocument = PdfReader.Open(filename);
                }
                else
                {
                    memoryStream.Position = 0;
                    openedDocument = PdfReader.Open(memoryStream);
                }
                openedDocument.PageCount.Should().Be(1);
            }
#pragma warning restore CS0162 // Unreachable code detected
        }

        /// <summary>
        /// Creates test files with a manipulated, flate-encoded stream.
        /// </summary>
        /// <param name="size">Size of the raw stream.</param>
        /// <param name="reportedLength">Length given in PDF file. Set -1 to get length of filtered stream.</param>
        /// <param name="content">Content of the stream. Use Chars.NumberSign to get random data.</param>
        /// <param name="shouldFail">True if created files throws an exception on PdfReader.Open.</param>
        [Theory]
        [InlineData(100, -1, 'x', false)] // -1 means use correct length.
        [InlineData(100, 1, 'x', false)]
        [InlineData(100, 512, 'x', false)]
        [InlineData(100, -1, Chars.NumberSign, false)] // -1 means use correct length.
        [InlineData(100, 1, Chars.NumberSign, true)] // Should work, but does not yet work with 6.2.0 Preview 2.
        [InlineData(100, 512, Chars.NumberSign, false)]
        public void Create_and_read_a_flate_decoded_raw_stream(int size, int reportedLength, char content, bool shouldFail)
        {
#pragma warning disable CS0162 // Unreachable code detected
            // Set SaveToFile to true if you want to inspect the created files.
            // Otherwise, set SaveToFile to false for better performance.
#if DEBUG
            const bool saveToFile = true;
            //const bool saveToFile = false;
#else
            const bool saveToFile = false;
#endif

            var rnd = new Random(17);
            var document = new PdfDocument();
            var page1 = document.AddPage();

            var dict = new PdfDebugDictionary(document)
            {
                StreamLength = reportedLength
            };
            var streamData = new byte[size];
            for (int x = 0; x < size; ++x)
                streamData[x] = content == Chars.NumberSign ? (byte)rnd.Next(255) : (byte)content;

            FlateDecode fd = new FlateDecode();
            byte[] compressed = fd.Encode(streamData, PdfFlateEncodeMode.BestCompression);
            dict.Stream = new PdfDictionary.PdfStream(compressed, dict);
            dict.Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
            if (reportedLength == -1)
                dict.StreamLength = compressed.Length;

            document.Internals.AddObject(dict);
            document.Catalog.Elements.Add("/SomeRawDict", dict.RequiredReference);

            string filename = PdfFileUtility.GetTempPdfFullFileName($"PDFsharp/UnitTest/pdf/FilteredStreamTests-StreamSize_{size}-Length_{reportedLength}-Contents_{(content == Chars.NumberSign ? "RND" : (byte)content)}-ShouldFail{shouldFail}");
            // ReSharper disable once RedundantAssignment
            var memoryStream = saveToFile ? null : new MemoryStream();

            if (saveToFile)
            {
                document.Save(filename);
                //PdfFileUtility.ShowDocumentIfDebugging(filename);
            }
            else
            {
                document.Save(memoryStream);
            }

            if (shouldFail)
            {
                // The created file is corrupted and cannot be opened by PDFsharp.
                if (saveToFile)
                {
                    var openDocument = () => PdfReader.Open(filename);
                    openDocument.Should().Throw<ObjectNotAvailableException>();
                }
                else
                {
                    memoryStream.Position = 0;
                    var openDocument = () => PdfReader.Open(memoryStream);
                    openDocument.Should().Throw<ObjectNotAvailableException>();
                }

            }
            else
            {
                // The created file is OK or slightly corrupted and can be opened by PDFsharp.
                PdfDocument openedDocument;
                if (saveToFile)
                {
                    openedDocument = PdfReader.Open(filename);
                }
                else
                {
                    memoryStream.Position = 0;
                    openedDocument = PdfReader.Open(memoryStream);
                }
                openedDocument.PageCount.Should().Be(1);
            }
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
