// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Pdf.IO;
using PdfSharp.Diagnostics;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Quality;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.IO
{
    [Collection("PDFsharp")]
    public class ContentReaderTests : IDisposable
    {
        public ContentReaderTests()
        {
            PdfSharpCore.ResetAll();
#if CORE_
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

        [Fact]
        public void Read_HelloWorld()
        {
            const int requiredAssets = 1032;
            IOUtility.EnsureAssetsVersion(requiredAssets);

            const string pdf = @"archives/samples-1.5/PDFs/HelloWorld.pdf";
            var testFile = IOUtility.GetAssetsPath(pdf)!;

            var doc = PdfReader.Open(testFile, PdfDocumentOpenMode.Import);

            doc.PageCount.Should().Be(1);
            doc.Pages.Count.Should().Be(1);
            var page = doc.Pages[0];
            var contents = ContentReader.ReadContent(page);
            int comments = 0, names = 0, operators = 0, sequences = 0, strings = 0, arrays = 0, integers = 0, reals = 0;
            var sb = new StringBuilder();
            foreach (var item in contents)
            {
                string info = item.GetType().Name switch
                {
                    nameof(CComment) => $"Comment {++comments}",
                    nameof(CName) => $"Name {++names}",
                    nameof(COperator) => $"Operator {++operators}: {((COperator)item).Name} with {((COperator)item).Operands.Count} operands",
                    nameof(CSequence) => $"Sequence {++sequences}",
                    nameof(CString) => $"String {++strings}",
                    nameof(CArray) => $"Array {++arrays}",
                    nameof(CInteger) => $"Integer {++integers}",
                    nameof(CReal) => $"Real {++reals}",
                    _ => throw new NotImplementedException($"Type {item.GetType().Name} was unexpected.")
                };
                sb.AppendLine(info);
            }
            var infos = sb.ToString();
            comments.Should().Be(0);
            names.Should().Be(0);
            operators.Should().Be(12);
            sequences.Should().Be(0);
            strings.Should().Be(0);
            arrays.Should().Be(0);
            integers.Should().Be(0);
            reals.Should().Be(0);
        }
    }
}
