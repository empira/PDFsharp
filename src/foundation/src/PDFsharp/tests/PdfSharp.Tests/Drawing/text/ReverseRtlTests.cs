// Tests for RTL run reversal in RenderEvents
using FluentAssertions;
using PdfSharp.Events;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
#if CORE
using PdfSharp.Snippets.Font;
using PdfSharp.Fonts;
#endif
using Xunit;

namespace PdfSharp.Tests.Drawing
{
    [Collection("PDFsharp")]
    public class ReverseRtlTests
    {
        public ReverseRtlTests()
        {
            // Intentionally do not set a custom font resolver in tests to avoid test-project-specific
            // dependencies. Tests use default font resolution where available.
        }

        [Theory]
        [InlineData("abc???", "???abc")]
        [InlineData("???abc", "abc???")]
        [InlineData("123 ??? 456", "456 ??? 123")]
        public void OnPrepareTextEvent_ReversesRtlRuns(string input, string expected)
        {
            var events = new RenderEvents();
            var doc = new PdfDocument();
            var font = new XFont("Arial", 10);
            var args = new PrepareTextEventArgs(doc, font, input);

            events.OnPrepareTextEvent(this, args);

            args.Text.Should().Be(expected);
        }
    }
}
