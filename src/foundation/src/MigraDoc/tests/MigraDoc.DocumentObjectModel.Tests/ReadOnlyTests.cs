// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.IO;
using MigraDoc.DocumentObjectModel.Tests.Helper;
using Xunit;
using FluentAssertions;

namespace MigraDoc.DocumentObjectModel.Tests
{
    [Collection("PDFsharp")]
    public class ReadOnlyTests
    {
        [Theory]
        [MemberData(nameof(TestHelper.TestData), MemberType = typeof(TestHelper))]
        public void Test_Clone(TestHelper.TestDocument testDocument)
        {
            // Create test document and make snapshot.
            var doc = TestHelper.CreateTestDocument(testDocument);
            var snapshot1 = new DocumentObjectSnapshot(doc);

            // Run the method which shall be tested to not change the document.
            // If this Test fails, check which Property has not the expected value and set breakpoints
            // while these lines are executed to examine if e.g. the property is accessed and created accidentally.
            var docClone = doc.Clone();

            // Create second snapshot and compare both snapshots.
            var snapshot2 = new DocumentObjectSnapshot(docClone);
            snapshot2.CompareTo(snapshot1);
        }

        [Theory]
        [MemberData(nameof(TestHelper.TestData), MemberType = typeof(TestHelper))]
        public void Test_WriteAndReadMdddl(TestHelper.TestDocument testDocument)
        {
            // Create test document and make snapshot.
            var doc = TestHelper.CreateTestDocument(testDocument);
            var snapshot1 = new DocumentObjectSnapshot(doc);

            // Run the method which shall be tested to not change the document.
            // If this Test fails, check which Property has not the expected value and set breakpoints
            // while these lines are executed to examine if e.g. the property is accessed and created accidentally.
            // ReSharper disable once UnusedVariable
            var mdddl = DdlWriter.WriteToString(doc);

            // Create second snapshot and compare both snapshots.
            var snapshot2 = new DocumentObjectSnapshot(doc);
            snapshot2.CompareTo(snapshot1);

            // Read Mdddl and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);

            // Create another snapshot and compare with the first.
            var snapshot3 = new DocumentObjectSnapshot(docRead);
            snapshot3.CompareTo(snapshot1);
        }

        [Theory]
        [MemberData(nameof(TestHelper.TestData), MemberType = typeof(TestHelper))]
        public void Test_GetElementsRecursively(TestHelper.TestDocument testDocument)
        {
            // Create test document and make snapshot.
            var doc = TestHelper.CreateTestDocument(testDocument);
            var snapshot1 = new DocumentObjectSnapshot(doc);

            // Run the method which shall be tested to not change the document.
            // If this Test fails, check which Property has not the expected value and set breakpoints
            // while these lines are executed to examine if e.g. the property is accessed and created accidentally.
            var elements = doc.GetElementsRecursively(true);
            // ReSharper disable once UnusedVariable
            var elementCount = elements.Count(); // Here GetElementsRecursively() is executed.

            // Create second snapshot and compare both snapshots.
            var snapshot2 = new DocumentObjectSnapshot(doc);
            snapshot2.CompareTo(snapshot1);
        }

        [Fact]
        public void Test_GetText()
        {
            // Create test document and make snapshot.
            var doc = new Document();
            var sec = doc.AddSection();
            var table = sec.AddTable();
            table.AddColumn(Unit.FromCentimeter(10));
            var row = table.AddRow();
            var cell = row[0];

            var p = TestHelper.CreateParagraphWithFormattedText();
            cell.Add(p);

            foreach (var p1 in TestHelper.CreateParagraphsWithStyles())
                cell.Add(p1);

            foreach (var p1 in TestHelper.CreateParagraphsWithOwnStyles(doc))
                cell.Add(p1);

            var snapshot1 = new DocumentObjectSnapshot(doc);

            // Run the method which shall be tested to not change the document.
            // If this Test fails, check which Property has not the expected value and set breakpoints
            // while these lines are executed to examine if e.g. the property is accessed and created accidentally.
            // ReSharper disable once UnusedVariable
            var text = cell.GetText();

            // Create second snapshot and compare both snapshots.
            var snapshot2 = new DocumentObjectSnapshot(doc);
            snapshot2.CompareTo(snapshot1);
        }

        [Fact]
        public void Test_GetUsedFormatValue()
        {
            // Create test document and make snapshot.
            var doc = new Document();

            var styles = doc.Styles;
            var style = styles[StyleNames.Heading1];
            style!.Font.Size = 20;
            style.Font.Color = Colors.Red;
            style.ParagraphFormat.LeftIndent = Unit.FromCentimeter(1);
            style = styles[StyleNames.Heading2];
            style!.Font.Size = 18;
            style.Font.Color = Colors.Blue;
            style.ParagraphFormat.LeftIndent = Unit.FromCentimeter(2);
            style = styles[StyleNames.Heading3];
            style!.Font.Size = 16;
            style.ParagraphFormat.LeftIndent = Unit.FromCentimeter(3);

            var sec = doc.AddSection();
            var p = sec.AddParagraph("Test", StyleNames.Heading3);
            p.Format.Font.Size = 14;

            var snapshot1 = new DocumentObjectSnapshot(doc);

            // Run the method which shall be tested to not change the document.
            // If this Test fails, check which Property has not the expected value and set breakpoints
            // while these lines are executed to examine if e.g. the property is accessed and created accidentally.
            var fontSize = p.GetUsedFormatValue(font => font.Size);
            var leftIndent = p.GetUsedFormatValue(format => format.LeftIndent);
            var fontColor = p.GetUsedFormatValue(format => format.Color, color => color == Color.Empty, Color.Empty);

            fontSize.Point.Should().Be(14);
            leftIndent.Centimeter.Should().Be(3);
            fontColor.Should().Be(Colors.Blue);

            // Create second snapshot and compare both snapshots.
            var snapshot2 = new DocumentObjectSnapshot(doc);
            snapshot2.CompareTo(snapshot1);
        }
    }
}
