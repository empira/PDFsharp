using FluentAssertions;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Quality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MigraDoc.Tests.Fonts
{
    [Collection("PDFsharp")]
    public class FontsTests
    {

        [Fact]
        public void Test_font_caching()
        {
            PredefinedFontsAndChars.Reset();
            try
            {
                PdfSharpCore.ResetAll();

#if CORE
                GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif

                var m = typeof(DocumentRenderer).Assembly.GetType("MigraDoc.Rendering.FontHandler")!
                    .GetMethod("FontToXFont", BindingFlags.Static | BindingFlags.NonPublic)!;

                var fonts = new[] { new Font("Arial") { Size = 9.0 }, new Font("Arial") { Size = 6.5 } };
                var tears = 0;

                Parallel.For(0, 200_000, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i =>
                {
                    var font = fonts[i % 2];
                    var got = (XFont)m.Invoke(null, [font])!;
                    if (Math.Abs(got.Size - font.Size.Point) > 0.001)
                        Interlocked.Increment(ref tears);
                });

                tears.Should().Be(0, "XFont size must always match Font size");
            }
            finally
            {
                PredefinedFontsAndChars.Reset();
                PdfSharpCore.ResetAll();
            }
        }
    }
}
