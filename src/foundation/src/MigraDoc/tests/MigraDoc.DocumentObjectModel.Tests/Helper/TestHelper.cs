using FluentAssertions;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;

namespace MigraDoc.DocumentObjectModel.Tests.Helper
{
    public class TestHelper
    {
        public static void InitializeFontResolver()
        {
            GlobalFontSettings.FontResolver ??= SnippetsFontResolver.Get();
        }

        //[Obsolete("Not needed when using SnippetsFontResolver")]
        public static void InitializeFontResolverWithSegoeWpAsDefault(Document doc)
        {
            InitializeFontResolver();

            var style = doc.Styles[StyleNames.Normal];
            style.Should().NotBeNull();
            //style!.Font.Name = "segoe wp";
        }

        public static void RemoveStyles(Document doc)
        {
            for (var i = doc.Styles.Count - 1; i >= 0; i--)
            {
                var style = doc.Styles[i];

                if (style.Name is StyleNames.DefaultParagraphFont or StyleNames.Normal)
                    continue;

                doc.Styles.RemoveObjectAt(i);
            }
        }

        public static IEnumerable<Paragraph> CreateParagraphsWithStyles()
        {
            var p = new Paragraph { Style = StyleNames.Heading1 };
            p.AddText("Heading 1");
            yield return p;

            p = new Paragraph { Style = StyleNames.Heading2 };
            p.AddText("Heading 2");
            yield return p;
        }

        public static IEnumerable<Paragraph> CreateParagraphsWithOwnStyles(Document doc)
        {
            var styles = doc.Styles;

            const string ownStyle = "OwnStyle";
            if (styles.GetIndex(ownStyle) < 0)
            {
                var style = styles.AddStyle(ownStyle, StyleNames.Normal);
                style.Font.Color = Colors.DarkRed;
            }

            const string ownStyle2 = "OwnStyle2";
            if (styles.GetIndex(ownStyle2) < 0)
            {
                var style = styles.AddStyle(ownStyle2, ownStyle);
                style.ParagraphFormat.Font.Underline = Underline.Single;
            }

            var p = new Paragraph { Style = ownStyle };
            p.AddText("Test Paragraph OwnStyle in dark red");
            yield return p;

            p = new Paragraph { Style = ownStyle2 };
            p.AddText("Test Paragraph OwnStyle2 in dark red and underlined");
            yield return p;
        }

        public static Paragraph CreateParagraphWithFormattedText()
        {
            var p = new Paragraph();
            p.AddText("Test Paragraph with ");

            var ft = p.AddFormattedText("bold");
            ft.Bold = true;
            p.AddText(", ");

            ft = p.AddFormattedText("red");
            ft.Color = Colors.Red;
            p.AddText(", ");

            ft = p.AddFormattedText("big");
            ft.Size = Unit.FromPoint(20);
            p.AddText(", ");

            ft = p.AddFormattedText("superscript");
            ft.Superscript = true;
            p.AddText(", ");

            ft = p.AddFormattedText("Courier New");
            ft.FontName = "Courier New";

            p.AddText(" formatted text and a character (");
            p.AddCharacter(SymbolName.Euro);
            p.AddText(").");

            return p;
        }

        public static Table CreateSimpleTable()
        {
            var table = new Table();

            table.AddColumn(Unit.FromCentimeter(1));
            table.AddColumn(Unit.FromCentimeter(2));

            var row = table.AddRow();
            row[0].AddParagraph("A1");
            row[1].AddParagraph("B1");

            row = table.AddRow();
            row[0].AddParagraph("A2");
            row[1].AddParagraph("B2");

            return table;
        }

        public static Table CreateComplexTable()
        {
            var table = new Table();

            table.AddColumn(Unit.FromCentimeter(1));
            table.AddColumn(Unit.FromCentimeter(2));
            table.AddColumn(Unit.FromCentimeter(3));

            var row = table.AddRow();
            row.HeadingFormat = true;

            row[0].AddParagraph("Heading A");

            row[1].AddParagraph("Heading B");

            var cell = row[2];
            cell.Style = StyleNames.Heading3;
            cell.AddParagraph("Heading C");

            row = table.AddRow();
            row.Shading.Color = Colors.Cyan;
            cell = row[0];
            cell.MergeRight = 1;
            cell.AddParagraph("A1 + B1");
            row[2].AddParagraph("C1");

            row = table.AddRow();
            cell = row[0];
            cell.MergeDown = 1;
            cell.AddParagraph("A2 + A3");

            cell = row[1];
            cell.Borders.Bottom.Width = Unit.FromPoint(2);
            cell.AddParagraph("B2");

            cell = row[2];
            cell.Borders.Width = Unit.FromPoint(2);
            cell.AddParagraph("C2");

            row = table.AddRow();
            row.Shading.Color = Colors.Cyan;
            cell = row[1];
            cell.VerticalAlignment = VerticalAlignment.Center;
            cell.AddParagraph("B3");

            cell = row[2];
            cell.Format.Alignment = ParagraphAlignment.Justify;
            cell.AddParagraph("C3");

            row = table.AddRow();
            row.KeepWith = 1;
            row[0].AddParagraph("A4");

            row[1].AddParagraph("B4");

            cell = row[2];
            cell.Format.TabStops.AddTabStop(Unit.FromCentimeter(1), TabAlignment.Decimal);
            var p = cell.AddParagraph();
            p.AddTab();
            p.AddText("C4");

            row = table.AddRow();
            row.Shading.Color = Colors.Cyan;
            row.KeepWith = 1;
            row[0].AddParagraph("A5");

            row[1].AddParagraph("B5");

            cell = row[2];
            p = CreateParagraphWithFormattedText();
            cell.Add(p);

            return table;
        }

        public static IEnumerable<Section> CreateComplexSections(Document doc)
        {
            var sec = new Section();
            sec.Headers.Primary.AddParagraph("PrimaryHeader");

            var p = CreateParagraphWithFormattedText();
            sec.Add(p);

            var table = CreateSimpleTable();
            sec.Add(table);

            yield return sec;

            sec = new Section();
            sec.Footers.FirstPage.AddParagraph("FirstPageFooter");

            foreach (var p1 in CreateParagraphsWithStyles())
                sec.Add(p1);

            table = CreateComplexTable();
            sec.Add(table);

            foreach (var p1 in CreateParagraphsWithOwnStyles(doc))
                sec.Add(p1);

            yield return sec;

            sec = new Section();
            sec.AddParagraph("Test Paragraph");

            yield return sec;
        }

        public static Document CreateTestDocument(TestDocument testDocument)
        {
            var doc = new Document();

            switch (testDocument)
            {
                case TestDocument.Empty:
                    break;

                case TestDocument.Minimal:
                    {
                        var sec = doc.AddSection();
                        sec.AddParagraph("Test Paragraph");
                        break;
                    }
                case TestDocument.Header:
                    {
                        var sec = doc.AddSection();
                        sec.AddParagraph("Test Paragraph");
                        sec.Headers.Primary.AddParagraph("PrimaryHeader");
                        break;
                    }
                case TestDocument.Styles:
                    {
                        var sec = doc.AddSection();
                        foreach (var p in CreateParagraphsWithStyles())
                            sec.Add(p);
                        sec.AddParagraph("Test Paragraph");
                        break;
                    }
                case TestDocument.OwnStyles:
                    {
                        var sec = doc.AddSection();
                        foreach (var p in CreateParagraphsWithOwnStyles(doc))
                            sec.Add(p);
                        break;
                    }
                case TestDocument.FormattedText:
                    {
                        var sec = doc.AddSection();
                        var p = CreateParagraphWithFormattedText();
                        sec.Add(p);
                        break;
                    }
                case TestDocument.SimpleTable:
                    {
                        var sec = doc.AddSection();
                        var table = CreateSimpleTable();
                        sec.Add(table);
                        break;
                    }
                case TestDocument.ComplexTable:
                    {
                        var sec = doc.AddSection();
                        var table = CreateComplexTable();
                        sec.Add(table);
                        break;
                    }
                case TestDocument.ComplexDocument:
                    {
                        foreach (var sec in CreateComplexSections(doc))
                            doc.Add(sec);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(testDocument), testDocument, null);
            }

            return doc;
        }

        public static IEnumerable<object[]> TestData => TestDocuments.Select(x => new object[] { x });

        public static IEnumerable<TestDocument> TestDocuments
        {
            get
            {
                yield return TestDocument.Empty;
                yield return TestDocument.Minimal;
                yield return TestDocument.Header;
                yield return TestDocument.Styles;
                yield return TestDocument.OwnStyles;
                yield return TestDocument.FormattedText;
                yield return TestDocument.SimpleTable;
                yield return TestDocument.ComplexTable;
                yield return TestDocument.ComplexDocument;
            }
        }

        public enum TestDocument
        {
            Empty,
            Minimal,
            Header,
            Styles,
            OwnStyles,
            FormattedText,
            SimpleTable,
            ComplexTable,
            ComplexDocument
        }
    }
}
