// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Pdf.Forms;
using PdfSharp.Snippets.Fonts.Text;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.Structs
{
    [Collection("PDFsharp")]
    public class NameTests
    {
        [Fact]
        public void Test_Name_ASCII_escaping()
        {
            const string ascii = "/!\"#$%&'()*+,_./:;<=>?[\\]^_`{|}~";

            // Name
            {
                var name = Name.FromCanonicalName(ascii);
                var literal = name.LiteralValue;
                Name.FromLiteralName(literal).Value.Should().Be(ascii);
            }

            // PdfName
            {
                new PdfName().Should().Be(PdfName.Empty);

                var name = new PdfName(ascii);
                var literal = name.Name.LiteralValue;
                new PdfName(Name.FromLiteralName(literal)).Value.Should().Be(ascii);
            }
        }

        [Fact]
        public void Test_Name_constructor()
        {
            // Default constructor.
            {
                var name = new Name();
                name.LiteralValue.Should().Be("/");
                name.Value.Should().Be("/");
            }

            // Canonical name constructor.
            {
                var name = new Name("/Name");
                name.LiteralValue.Should().Be("/Name");
                name.Value.Should().Be("/Name");

                name = new Name("/@Name_123");
                name.LiteralValue.Should().Be("/@Name_123");
                name.Value.Should().Be("/@Name_123");

                name = new Name("/Ä");
            }
        }

        [Fact]
        public void Test_Name_creation()
        {
            // FromLiteralName
            {
                var name = Name.FromLiteralName("/@Simple_Name");
                name.LiteralValue.Should().Be("/@Simple_Name");
                name.Value.Should().Be("/@Simple_Name");

                name = Name.FromLiteralName("/Lime#20Green");
                name.LiteralValue.Should().Be("/Lime#20Green");
                name.Value.Should().Be("/Lime Green");

                name = Name.FromLiteralName("/#41");
                name.LiteralValue.Should().Be("/#41");
                name.Value.Should().Be("/A");
            }

            // FromCanonicalName
            {
                var name = Name.FromCanonicalName("/@Simple_Name");
                name.LiteralValue.Should().Be("/@Simple_Name");
                name.Value.Should().Be("/@Simple_Name");

                name = Name.FromCanonicalName("/Lime#20Green");
                name.LiteralValue.Should().Be("/Lime#2320Green");
                name.Value.Should().Be("/Lime#20Green");

                name = Name.FromCanonicalName("////");
                name.LiteralValue.Should().Be("/#2F#2F#2F");
                name.Value.Should().Be("////");

                name = Name.FromCanonicalName("/#41");
                name.LiteralValue.Should().Be("/#2341");
                name.Value.Should().Be("/#41");
            }
        }

        [Fact]
        public void Test_Name_UTF8()
        {
            // FromLiteralName
            {
                // An invalid literal name is reevaluated
                var name = Name.FromLiteralName("/Umlauts-#41#C3#96#C3#9C#C3#A4#C3#B6#C3#BC#C3#9F-AÖÜäöüß");
                name.LiteralValue.Should().Be("/Umlauts-A#C3#96#C3#9C#C3#A4#C3#B6#C3#BC#C3#9F-A#C3#96#C3#9C#C3#A4#C3#B6#C3#BC#C3#9F");
                name.Value.Should().Be("/Umlauts-AÖÜäöüß-AÖÜäöüß");
            }

            // FromCanonicalName
            {
                var name = Name.FromCanonicalName("/UmlautsÄÖÜäöüß");
                name.LiteralValue.Should().Be("/Umlauts#C3#84#C3#96#C3#9C#C3#A4#C3#B6#C3#BC#C3#9F");
                name.Value.Should().Be("/UmlautsÄÖÜäöüß");

                name = Name.FromCanonicalName(Name.MakeName(ShortTestTexts.GoodMorning_Korean));
                var l = name.LiteralValue;
                var c = name.Value;
                c.Should().Be(Name.FromLiteralName(l).Value);
            }
        }

        [Fact]
        public void Test_Name_Enums()
        {
            var name = Name.FromEnum(PdfFormFieldFlags.DoNotSpellCheckChoiceField);
            name.Value.Should().Be("/DoNotSpellCheckChoiceField");
        }

        [Fact]
        public void Test_ill_formatted_Names()
        {
            // Ill formatted but legal.
            {
                // Invalid hex values.
                var name = Name.FromLiteralName("/Name#4");
                name.LiteralValue.Should().Be("/Name#40");
                name.Value.Should().Be("/Name@");
            }

            // Ill formatted and illegal.
            {
                var name = Name.FromLiteralName("/Name#0");
                name.LiteralValue.Should().Be("/Name");
                name.Value.Should().Be("/Name");

                name = Name.FromLiteralName("/Name#");
                name.LiteralValue.Should().Be("/Name");
                name.Value.Should().Be("/Name");

                name = Name.FromLiteralName("/Name#XY");
                name.LiteralValue.Should().Be("/Name");
                name.Value.Should().Be("/Name");

                name = Name.FromLiteralName("/Name#0X");
                name.LiteralValue.Should().Be("/Name");
                name.Value.Should().Be("/Name");

                name = Name.FromLiteralName("/Name#00XY");
                name.LiteralValue.Should().Be("/NameXY");
                name.Value.Should().Be("/NameXY");

                name = Name.FromLiteralName("/Name#XYAB");
                name.LiteralValue.Should().Be("/NameAB");
                name.Value.Should().Be("/NameAB");

                name = Name.FromLiteralName("/Name#0XYAB");
                name.LiteralValue.Should().Be("/NameYAB");
                name.Value.Should().Be("/NameYAB");
            }
        }

        [Fact]
        public void Test_Name_comparison()
        {
            var ar = String.Compare("A", "A", StringComparison.Ordinal);
            var br = String.Compare("A", "B", StringComparison.Ordinal);
            var cr = String.Compare("B", "A", StringComparison.Ordinal);
            var dr = String.Compare("A", null, StringComparison.Ordinal);
            var er = String.Compare(null, "B", StringComparison.Ordinal);
            var fr = String.Compare(null, null, StringComparison.Ordinal);

            // Name
            {
                var comp = Name.Comparer;
                var an = comp.Compare(new("/A"), new("/A"));
                an.Should().Be(ar);
                var bn = comp.Compare(new("/A"), new("/B"));
                bn.Should().Be(br);
                var cn = comp.Compare(new("/B"), new("/A"));
                cn.Should().Be(cr);
                var dn = comp.Compare(new("/A"), null);
                dn.Should().Be(dr);
                var en = comp.Compare(null, new("/B"));
                en.Should().Be(er);
                var fn = comp.Compare(null, null);
                fn.Should().Be(fr);
            }

            // PdfName
            {
                var comp = Name.Comparer;
                var an = comp.Compare(new("/A"), new("/A"));
                an.Should().Be(ar);
                var bn = comp.Compare(new("/A"), new("/B"));
                bn.Should().Be(br);
                var cn = comp.Compare(new("/B"), new("/A"));
                cn.Should().Be(cr);
                var dn = comp.Compare(new("/A"), null);
                dn.Should().Be(dr);
                var en = comp.Compare(null, new("/B"));
                en.Should().Be(er);
                var fn = comp.Compare(null, null);
                fn.Should().Be(fr);
            }
        }
    }
}
