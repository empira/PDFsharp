// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;
using PdfSharp.Snippets.Font;
using Xunit;

namespace PdfSharp.Tests.Fonts
{
    [Collection("PDFsharp")]
    public class FontResolverTests : IDisposable
    {
        // Fonts from platform font resolver.
        const string ArialName = "Arial";
        const string TimesName = "Times New Roman";
        const string CourierName = "Courier New";
        const string VerdanaName = "Verdana";
        const string LucidaName = "Lucida Console";
        const string SymbolName = "Symbol";

        // A font that never exists.
        const string DummyFontName = "A-dummy-font";

        // A font that only exists for PFR in GDI or WPF build.
        // ReSharper disable once InconsistentNaming
        const string SegoeUIName = "Segoe UI";

        // A font only exists for a special font resolver
        const string XFilesName = "XFiles";

        public void Dispose()
        {
            // Tear down this test class.
            GlobalFontSettings.ResetFontManagement();
        }


        [Fact]
        public void No_FontResolver_set()
        {
            GlobalFontSettings.ResetFontManagement();

            // No FFR.
            DummyShouldFail();

            // No CFR.
            XFilesShouldFail();
#if CORE
            // Not in PFR.
            SegoeUIShouldFail();

            var arial = new XFont(ArialName, 10, XFontStyleEx.Regular);
            ArialShouldSucceed();
            arial.GlyphTypeface.FaceName.Should().Be("Arial");

            var times = new XFont(TimesName, 10, XFontStyleEx.Regular);
            times.GlyphTypeface.FaceName.Should().Be("Times New Roman");

            var courier = new XFont(CourierName, 10, XFontStyleEx.Regular);
            courier.GlyphTypeface.FaceName.Should().Be("Courier New");

            var verdana = new XFont(VerdanaName, 10, XFontStyleEx.Regular);
            verdana.GlyphTypeface.FaceName.Should().Be("Verdana");

            var lucida = new XFont(LucidaName, 10, XFontStyleEx.Regular);
            lucida.GlyphTypeface.FaceName.Should().Be("Lucida Console");

            var symbol = new XFont(SymbolName, 10, XFontStyleEx.Regular);
            var symbolB = new XFont(SymbolName, 10, XFontStyleEx.Bold);
            var symbolI = new XFont(SymbolName, 10, XFontStyleEx.Italic);
            var symbolBI = new XFont(SymbolName, 10, XFontStyleEx.BoldItalic);
            symbol.GlyphTypeface.FaceName.Should().Be("Symbol");
            symbolB.GlyphTypeface.FaceName.Should().Be("Symbol");
            symbolI.GlyphTypeface.FaceName.Should().Be("Symbol");
            symbolBI.GlyphTypeface.FaceName.Should().Be("Symbol");
            symbol.GlyphTypeface.StyleSimulations.Should().Be(XStyleSimulations.None);
            symbolB.GlyphTypeface.StyleSimulations.Should().Be(XStyleSimulations.BoldSimulation);
            symbolI.GlyphTypeface.StyleSimulations.Should().Be(XStyleSimulations.ItalicSimulation);
            symbolBI.GlyphTypeface.StyleSimulations.Should().Be(XStyleSimulations.BoldItalicSimulation);
#endif
#if GDI
            // Resolved by GDI.
            SegoeUIShouldSucceed();

            var arial = new XFont(ArialName, 10, XFontStyleEx.Regular);
            var times = new XFont(TimesName, 10, XFontStyleEx.Regular);
            var courier = new XFont(CourierName, 10, XFontStyleEx.Regular);
            var verdana = new XFont(VerdanaName, 10, XFontStyleEx.Regular);
            var lucida = new XFont(LucidaName, 10, XFontStyleEx.Italic);
            var symbol = new XFont(SymbolName, 10, XFontStyleEx.Regular);
#endif
#if WPF
            // Resolved by GDI.
            SegoeUIShouldSucceed();

            var arial = new XFont(ArialName, 10, XFontStyleEx.Regular);
            var times = new XFont(TimesName, 10, XFontStyleEx.Regular);
            var courier = new XFont(CourierName, 10, XFontStyleEx.Regular);
            var verdana = new XFont(VerdanaName, 10, XFontStyleEx.Regular);
            var lucida = new XFont(LucidaName, 10, XFontStyleEx.Regular);
            var symbol = new XFont(SymbolName, 10, XFontStyleEx.Regular);
#endif
            var arialCount = FontHelper.CountGlyphs(arial);
            var timesCount = FontHelper.CountGlyphs(times);
            var courierCount = FontHelper.CountGlyphs(courier);
            var verdanaCount = FontHelper.CountGlyphs(verdana);
            var lucidaCount = FontHelper.CountGlyphs(lucida);
            var symbolCount = FontHelper.CountGlyphs(symbol);

            arialCount.Should().BeGreaterThan(3300);
            timesCount.Should().BeGreaterThan(3300);
            courierCount.Should().BeGreaterThan(3100);
            verdanaCount.Should().BeGreaterThan(800);
            lucidaCount.Should().BeGreaterThan(600);
            symbolCount.Should().BeGreaterThan(180);
        }

        [Fact]
        public void No_FontResolver_set_and_multiple_creations()
        {
            GlobalFontSettings.ResetFontManagement();

            // No FFR.
            DummyShouldFail();

            // No CFR.
            XFilesShouldFail();
#if CORE
            // Not in PFR.
            SegoeUIShouldFail();
#else
            // Resolved by PDF for GDI and WPF.
            SegoeUIShouldSucceed();
#endif
            var arial0 = new XFont(ArialName, 10, XFontStyleEx.Regular);
            var arial = new XFont(ArialName, 10, XFontStyleEx.Regular);
            var times0 = new XFont(TimesName, 10, XFontStyleEx.Regular);
            var times = new XFont(TimesName, 10, XFontStyleEx.Regular);
            var courier0 = new XFont(CourierName, 10, XFontStyleEx.Regular);
            var courier = new XFont(CourierName, 10, XFontStyleEx.Regular);
            var verdana0 = new XFont(VerdanaName, 10, XFontStyleEx.Regular);
            var verdana = new XFont(VerdanaName, 10, XFontStyleEx.Regular);
            var lucida0 = new XFont(LucidaName, 10, XFontStyleEx.Regular);
            var lucida = new XFont(LucidaName, 10, XFontStyleEx.Regular);
            var symbol0 = new XFont(SymbolName, 10, XFontStyleEx.Regular);
            var symbol = new XFont(SymbolName, 10, XFontStyleEx.Regular);

            var arialCount = FontHelper.CountGlyphs(arial);
            var timesCount = FontHelper.CountGlyphs(times);
            var courierCount = FontHelper.CountGlyphs(courier);
            var verdanaCount = FontHelper.CountGlyphs(courier);
            var lucidaCount = FontHelper.CountGlyphs(lucida);
            var symbolCount = FontHelper.CountGlyphs(symbol);

            arialCount.Should().BeGreaterThan(3300);
            timesCount.Should().BeGreaterThan(3300);
            courierCount.Should().BeGreaterThan(3100);
            verdanaCount.Should().BeGreaterThan(3100);
            lucidaCount.Should().BeGreaterThan(600);
            symbolCount.Should().BeGreaterThan(180);
        }

        [Fact]
        public void No_FontResolver_set_and_multiple_creations2()
        {
            GlobalFontSettings.ResetFontManagement();

            var arial0 = new XFont(ArialName, 10, XFontStyleEx.Regular);
            var arial1 = new XFont(ArialName, 10, XFontStyleEx.Italic);
            var arial = new XFont(ArialName, 10, XFontStyleEx.Bold);
            arial0.GlyphTypeface.FaceName.Should().Be("Arial");
            arial1.GlyphTypeface.FaceName.Should().Be("Arial Italic");
            arial.GlyphTypeface.FaceName.Should().Be("Arial Bold");

            var times0 = new XFont(TimesName, 10, XFontStyleEx.Regular);
            var times1 = new XFont(TimesName, 10, XFontStyleEx.Bold);
            var times = new XFont(TimesName, 10, XFontStyleEx.Italic);
            times0.GlyphTypeface.FaceName.Should().Be("Times New Roman");
            times1.GlyphTypeface.FaceName.Should().Be("Times New Roman Bold");
            times.GlyphTypeface.DisplayName.Should().Be("Times New Roman Italic");

            var courier0 = new XFont(CourierName, 10, XFontStyleEx.Bold);
            var courier1 = new XFont(CourierName, 10, XFontStyleEx.Bold);
            var courier = new XFont(CourierName, 10, XFontStyleEx.BoldItalic);
            courier0.GlyphTypeface.FaceName.Should().Be("Courier New Bold");
            courier1.GlyphTypeface.FaceName.Should().Be("Courier New Bold");
            courier.GlyphTypeface.FaceName.Should().Be("Courier New Bold Italic");

            var verdana0 = new XFont(VerdanaName, 10, XFontStyleEx.Bold);
            var verdana1 = new XFont(VerdanaName, 10, XFontStyleEx.Bold);
            var verdana = new XFont(VerdanaName, 10, XFontStyleEx.BoldItalic);
            verdana0.GlyphTypeface.FaceName.Should().Be("Verdana Bold");
            verdana1.GlyphTypeface.FaceName.Should().Be("Verdana Bold");
            verdana.GlyphTypeface.FaceName.Should().Be("Verdana Bold Italic");

            var lucida0 = new XFont(LucidaName, 10, XFontStyleEx.Regular);
            var lucida1 = new XFont(LucidaName, 10, XFontStyleEx.Italic);
            var lucida = new XFont(LucidaName, 10, XFontStyleEx.Italic);
            lucida0.GlyphTypeface.FaceName.Should().Be("Lucida Console");
            lucida1.GlyphTypeface.FaceName.Should().Be("Lucida Console");
            lucida.GlyphTypeface.FaceName.Should().Be("Lucida Console");

            var symbol0 = new XFont(SymbolName, 10, XFontStyleEx.Italic);
            var symbol1 = new XFont(SymbolName, 10, XFontStyleEx.Bold);
            var symbol = new XFont(SymbolName, 10, XFontStyleEx.BoldItalic);
            symbol0.GlyphTypeface.FaceName.Should().Be("Symbol");
            symbol0.IsSymbolFont.Should().BeTrue();
            symbol0.GlyphTypeface.StyleSimulations.Should().Be(XStyleSimulations.ItalicSimulation);
            symbol1.GlyphTypeface.FaceName.Should().Be("Symbol");
            symbol1.GlyphTypeface.StyleSimulations.Should().Be(XStyleSimulations.BoldSimulation);
            symbol.GlyphTypeface.FaceName.Should().Be("Symbol");
            symbol.GlyphTypeface.StyleSimulations.Should().Be(XStyleSimulations.BoldItalicSimulation);

            var arialCount = FontHelper.CountGlyphs(arial);
            var timesCount = FontHelper.CountGlyphs(times);
            var courierCount = FontHelper.CountGlyphs(courier);
            var verdanaCount = FontHelper.CountGlyphs(verdana);
            var lucidaCount = FontHelper.CountGlyphs(lucida);
            var symbolCount = FontHelper.CountGlyphs(symbol);

            arialCount.Should().BeGreaterThan(3300);
            timesCount.Should().BeGreaterThan(2900);
            courierCount.Should().BeGreaterThan(2400);
            verdanaCount.Should().BeGreaterThan(800);
            lucidaCount.Should().BeGreaterThan(600);
            symbolCount.Should().BeGreaterThan(180);
        }

        [Fact]
        public void Test_FailSaveFontResolver_as_fallback_font_resolver()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FallbackFontResolver = new FailsafeFontResolver();

            // Resolved in FFR.
            DummyShouldBeReplacedBy("Segoe WP");

            // Resolved in FFR.
            XFilesShouldBeReplacedBy("Segoe WP");
#if CORE
            // Resolved in FFR.
            SegoeUIShouldBeReplacedBy("Segoe WP");
#else
            // Resolved by PFF for GDI and WPF.
            SegoeUIShouldSucceed();
#endif
            // Platform font resolver creates Arial.
            var arial = new XFont(ArialName, 10, XFontStyleEx.Regular);
            arial.GlyphTypeface.FaceName.Should().Be("Arial");
#if CORE
            // Fallback font resolver creates Segoe WP.
            var dummy = new XFont("Dummy", 10, XFontStyleEx.Regular);
            dummy.GlyphTypeface.FaceName.Should().Be("Segoe WP");
#endif
#if GDI
            // Fallback font resolver creates Segoe WP.
            var dummy = new XFont("Dummy", 10, XFontStyleEx.Regular);
            dummy.GlyphTypeface.FaceName.Should().Be("Segoe WP");
#endif
#if WPF
            // Fallback font resolver creates Segoe WP.
            var dummy = new XFont("Dummy", 10, XFontStyleEx.Regular);
            dummy.GlyphTypeface.FaceName.Should().Be("Segoe WP");
#endif
        }

        [Fact]
        public void TestXFilesFontResolver1_used_as_main_font_resolver()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new TestXFilesFontResolver1();

            // No FFR.
            DummyShouldFail();

            // Resolved in CFR.
            XFilesShouldSucceed();

            // PFR not called by CFR.
            SegoeUIShouldFail();

            // Arial fails because a font resolver is set and platform font resolver is not called.
            Func<XFont> createArial = () => new XFont(ArialName, 10, XFontStyleEx.Regular);
            createArial.Should().Throw<InvalidOperationException>();

            var xfilesR = new XFont(XFilesName, 10, XFontStyleEx.Regular);
            var xfilesB = new XFont(XFilesName, 10, XFontStyleEx.Bold);
            var xfilesI = new XFont(XFilesName, 10, XFontStyleEx.Italic);
            var xfilesBI = new XFont(XFilesName, 10, XFontStyleEx.BoldItalic);
            xfilesR.GlyphTypeface.FaceName.Should().Be("X-Files");

            var xfilesCount = FontHelper.CountGlyphs(xfilesR);

            xfilesCount.Should().Be(94);
        }

        [Fact]
        public void TestXFilesFontResolver1_used_as_main_font_resolver_with_fallback()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new TestXFilesFontResolver1();
            GlobalFontSettings.FallbackFontResolver = new FailsafeFontResolver();

            // Resolved in FFR.
            var xxx = new XFont(DummyFontName, 10, XFontStyleEx.Regular);

            DummyShouldBeReplacedBy("Segoe WP");

            // Resolved in CFR.
            XFilesShouldSucceed();

            // PFR not called by CFR.
            SegoeUIShouldBeReplacedBy("Segoe WP");

            // Arial fails not no because of fallback font resolver
            // but was resolved as Segoe WP.
            var arial = new XFont(ArialName, 10, XFontStyleEx.Regular);
            arial.GlyphTypeface.FaceName.Should().Be("Segoe WP");

            // X-Files resolved by font resolver.
            var xfiles = new XFont(XFilesName, 10, XFontStyleEx.Regular);
            xfiles.GlyphTypeface.FaceName.Should().Be("X-Files");
        }

        [Fact]
        public void TestXFilesFontResolver2_used_as_main_font_resolver()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new TestXFilesFontResolver2();

            // No FFR.
            DummyShouldFail();

            // Resolved in CFR.
            XFilesShouldSucceed();

#if CORE
            // PFR called from CFR but cannot resolve.
            SegoeUIShouldFail();
#else
            // PFR called from CFR and resolved by GDI / WPF PFR.
            SegoeUIShouldSucceed();
#endif
            // Arial fails not because custom font resolver calls
            // platform font resolver.
            var arial = new XFont(ArialName, 10, XFontStyleEx.Regular);
            arial.GlyphTypeface.FaceName.Should().Be("Arial");

            // X-Files resolved by font resolver.
            var xfiles = new XFont(XFilesName, 10, XFontStyleEx.Regular);
            xfiles.GlyphTypeface.FaceName.Should().Be("X-Files");

#if CORE
            // Dummy fails because it cannot be resolved.
            Func<XFont> createDummy = () => new XFont("Dummy", 10, XFontStyleEx.Regular);
            createDummy.Should().Throw<InvalidOperationException>();
#endif
#if GDI
            // Dummy resolved by platform font resolver and creates Microsoft Sans Serif.
            //var dummy = new XFont("Dummy", 10, XFontStyleEx.Regular);
            //dummy.GlyphTypeface.FaceName.Should().Be("Microsoft Sans Serif");
            Func<XFont> createDummy = () => new XFont("Dummy", 10, XFontStyleEx.Regular);
            createDummy.Should().Throw<InvalidOperationException>();
#endif
#if WPF
            //// Dummy resolved by platform font resolver and creates Microsoft Sans Serif.
            //var dummy = new XFont("Dummy", 10, XFontStyleEx.Regular);
            //dummy.GlyphTypeface.FaceName.Should().Be("Microsoft Sans Serif");
            // Dummy fails because it cannot be resolved.
            Func<XFont> createDummy = () => new XFont("Dummy", 10, XFontStyleEx.Regular);
            createDummy.Should().Throw<InvalidOperationException>();
#endif
        }

        [Fact]
        public void TestXFilesFontResolver2_used_as_main_font_resolver_with_fallback()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new TestXFilesFontResolver2();
            GlobalFontSettings.FallbackFontResolver = new FailsafeFontResolver();

            // Resolved in FFR.
            DummyShouldBeReplacedBy("Segoe WP");

            // Resolved in CFR.
            XFilesShouldSucceed();
#if CORE
            // PFR called CFT but cannot resolve Segoe UI so it is resolved by FFR.
            SegoeUIShouldBeReplacedBy("Segoe WP");
#else
            // PFR called and resolved by GDI / WPF PFR.
            SegoeUIShouldSucceed();
#endif
            // Arial fails not because custom font resolver calls
            // platform font resolver.
            var arial = new XFont(ArialName, 10, XFontStyleEx.Regular);
            arial.GlyphTypeface.FaceName.Should().Be("Arial");

            // X-Files resolved by font resolver.
            var xfiles = new XFont(XFilesName, 10, XFontStyleEx.Regular);
            xfiles.GlyphTypeface.FaceName.Should().Be("X-Files");

            // Dummy resolved by fallback font resolver and creates Segoe WP.
            var dummy = new XFont("Dummy", 10, XFontStyleEx.Regular);
#if CORE
            dummy.GlyphTypeface.FaceName.Should().Be("Segoe WP");
#endif
#if GDI
            // PDFsharp ignores the fact that GDI resolve every unknown font with 'Microsoft Sans Serif'.
            //dummy.GlyphTypeface.FaceName.Should().Be("Microsoft Sans Serif");
            dummy.GlyphTypeface.FaceName.Should().Be("Segoe WP");
#endif
#if WPF
            dummy.GlyphTypeface.FaceName.Should().Be("Segoe WP");
#endif
        }

        [Fact]
        public void TestXFilesFontResolver2_used_as_fallback()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new TestXFilesFontResolver1();
            GlobalFontSettings.FallbackFontResolver = new TestXFilesFontResolver2();

            // X-Files resolved by font resolver.
            var xfiles = new XFont(XFilesName, 10, XFontStyleEx.Regular);
            xfiles.GlyphTypeface.FaceName.Should().Be("X-Files");

            // Arial fails because fallback font resolver calls platform font resolver,
            // which is illegal.
            Func<XFont> createDummy = () => new XFont(ArialName, 10, XFontStyleEx.Regular);
            createDummy.Should().Throw<InvalidOperationException>();

            // Not resolved by CFR and FFR failed because it tries illegally call PFR.
            DummyShouldFail();
        }

        [Fact]
        public void TestXFilesFontResolver3_used_as_main_font_resolver()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new TestXFilesFontResolver3();
            //GlobalFontSettings.FallbackFontResolver = new TestXFilesFontResolver2();

            // Resolved by CFR.
            XFilesShouldSucceed();

            // CFR throws exception.
            ArialShouldFail();

            // CFR throws exception.
            DummyShouldFail();
        }

        [Fact]
        public void TestXFilesFontResolver3_used_as_main_font_resolver_with_fallback()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new TestXFilesFontResolver3();
            GlobalFontSettings.FallbackFontResolver = new FailsafeFontResolver();

            // Resolved by CFR.
            XFilesShouldSucceed();

            // CFR throws exception and FFR resolves.
            ArialShouldBeReplacedBy("Segoe WP");

            // CFR throws exception and FFR resolves.
            DummyShouldBeReplacedBy("Segoe WP");
        }

        [Fact]
        public void Test_test()
        {
            GlobalFontSettings.ResetFontManagement();
            //GlobalFontSettings.FontResolver = new TestXFilesFontResolver1();
            //GlobalFontSettings.FallbackFontResolver = new TestXFilesFontResolver2();

            // No failsafe font resolver.
            DummyShouldFail();
        }

        // -----------------------------------------------------------------------------------------

        void ArialShouldSucceed(XFontStyleEx style = XFontStyleEx.Regular)
        {
            EnsureSuccess(ArialName, "Arial", style);
        }

        void ArialShouldFail(XFontStyleEx style = XFontStyleEx.Regular)
        {
            EnsureFail(ArialName, style);
        }

        void ArialShouldBeReplacedBy(string faceName, XFontStyleEx style = XFontStyleEx.Regular)
        {
            EnsureSuccess(ArialName, faceName, style);
        }

        void DummyShouldFail(XFontStyleEx style = XFontStyleEx.Regular)
        {
            EnsureFail("Dummy", style);
        }

        void DummyShouldBeReplacedBy(string faceName, XFontStyleEx style = XFontStyleEx.Regular)
        {
            EnsureSuccess("Dummy", faceName, style);
        }

        // ReSharper disable once InconsistentNaming
        void SegoeUIShouldSucceed(XFontStyleEx style = XFontStyleEx.Regular)
        {
            EnsureSuccess(SegoeUIName, "Segoe UI", style);
        }

        // ReSharper disable once InconsistentNaming
        void SegoeUIShouldFail(XFontStyleEx style = XFontStyleEx.Regular)
        {
            EnsureFail(SegoeUIName, style);
        }

        // ReSharper disable once InconsistentNaming
        void SegoeUIShouldBeReplacedBy(string faceName, XFontStyleEx style = XFontStyleEx.Regular)
        {
            EnsureSuccess(SegoeUIName, faceName, style);
        }

        void XFilesShouldSucceed(XFontStyleEx style = XFontStyleEx.Regular)
        {
            EnsureSuccess(XFilesName, "X-Files", style);
        }

        void XFilesShouldFail(XFontStyleEx style = XFontStyleEx.Regular)
        {
            EnsureFail(XFilesName, style);
        }
        void XFilesShouldBeReplacedBy(string faceName, XFontStyleEx style = XFontStyleEx.Regular)
        {
            EnsureSuccess(XFilesName, faceName, style);
        }

        void EnsureSuccess(string inputFaceName, string resolvedFaceName, XFontStyleEx style = XFontStyleEx.Regular)
        {
            var font = new XFont(inputFaceName, 10, style);
            font.GlyphTypeface.FaceName.Should().Be(resolvedFaceName);
        }

        void EnsureFail(string inputFaceName, XFontStyleEx style = XFontStyleEx.Regular)
        {
            var font = () => new XFont(inputFaceName, 10, style);
            font.Should().Throw<InvalidOperationException>();
        }
    }
}
