// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;
using PdfSharp.Snippets.Font;
using System.Runtime.InteropServices;
using Xunit;

namespace PdfSharp.Tests.Fonts
{
    [Collection("PDFsharp")]
    public class UseWindowsFontsTests : IDisposable
    {
        public UseWindowsFontsTests()
        {
            GlobalFontSettings.ResetFontManagement();
        }

        public void Dispose()
        {
            GlobalFontSettings.ResetFontManagement();
        }

        [Fact]
        public void UseWindowsFonts_not_set()
        {
            GlobalFontSettings.ResetFontManagement();
#if CORE
            // Core build fails on any platform.

            // Create a font.
            var fontCreator = () => new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);
            fontCreator.Should().Throw<InvalidOperationException>();

#elif GDI || WPF
            // Create a font.
            var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);
            font.Should().NotBe(null);
#else
#error
#endif
        }

        [Fact]
        public void UseWindowsFontsUnderWindows_set()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
#if CORE
            if (Capabilities.OperatingSystem.IsWindows)
            {
                // Create a font.
                var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);
                font.Should().NotBe(null);
            }
            else
            {
                // Create a font.
                var fontCreator = () => new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);
                fontCreator.Should().Throw<InvalidOperationException>();
            }
#elif GDI || WPF
            // Create a font.
            var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);
            font.Should().NotBe(null);
#else
#error
#endif
        }

        [Fact]
        public void UseWindowsFontsUnderWsl2_set()
        {
            GlobalFontSettings.Reset();
            GlobalFontSettings.UseWindowsFontsUnderWsl2 = true;
#if CORE
            if (Capabilities.OperatingSystem.IsWsl2)
            {
                // Create a font.
                var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);
                font.Should().NotBe(null);
            }
            else
            {
                // Create a font.
                var fontCreator = () => new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);
                fontCreator.Should().Throw<InvalidOperationException>();
            }
#elif GDI || WPF
            // Create a font.
            var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);
            font.Should().NotBe(null);
#else
#error
#endif
        }

        [Fact]
        public void UseWindowsFontsUnderWindows_and_UseWindowsFontsUnderWsl2_set()
        {
            GlobalFontSettings.Reset();
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
            GlobalFontSettings.UseWindowsFontsUnderWsl2 = true;
#if CORE
            if (Capabilities.OperatingSystem.IsWindows ||
                Capabilities.OperatingSystem.IsWsl2)
            {
                // Create a font.
                var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);
                font.Should().NotBe(null);
            }
            else
            {
                // Create a font.
                var fontCreator = () => new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);
                fontCreator.Should().Throw<InvalidOperationException>();
            }
#elif GDI || WPF
            // Create a font.
            var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);
            font.Should().NotBe(null);
#else
#error
#endif
        }
    }
}
