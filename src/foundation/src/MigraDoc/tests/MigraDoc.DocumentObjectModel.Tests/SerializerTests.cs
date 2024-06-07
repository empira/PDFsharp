// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.Rendering;
using MigraDoc.DocumentObjectModel.IO;
using MigraDoc.DocumentObjectModel.Tests.Helper;
using Xunit;
using FluentAssertions;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;

namespace MigraDoc.DocumentObjectModel.Tests
{
    [Collection("PDFsharp")]
    public class SerializerTests
    {
        [Fact]
        public void Test_WriteToString()
        {
            var doc = new Document();

            var s = DdlWriter.WriteToString(doc);

            s.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Test_OptimizedTabSymbol_AddText()
        {
            var doc = new Document();
            TestHelper.RemoveStyles(doc);
            doc.AddSection().AddParagraph().AddText("\t");

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().Contain("\\section");
            mdddl.Should().Contain("\\tab");
        }

        [Fact]
        public void Test_OptimizedTabSymbol_AddTab()
        {
            var doc = new Document();
            TestHelper.RemoveStyles(doc);

            doc.AddSection().AddParagraph().AddTab();

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().Contain("\\section");
            mdddl.Should().Contain("\\tab");
        }

        [Fact]
        public void Test_OptimizedTabSymbol_InsertCharacter()
        {
            var doc = new Document();
            TestHelper.RemoveStyles(doc);

            doc.AddSection().AddParagraph().Elements.InsertObject(0, (DocumentObject)Character.Tab.Clone());

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().Contain("\\section");
            mdddl.Should().Contain("\\tab");
        }

        [Theory]
        [InlineData("0")]
        [InlineData("23.45mm")]
        [InlineData("1.345cm")]
        [InlineData("0.678in")]
        [InlineData("12.34pc")]
        [InlineData("12.34pt")]
        public void Test_WriteAndReadMdddl_ValueTypeDescriptor(string value)
        {
            var doc = new Document();

            var styleName = "TestStyle";
            doc.AddStyle(styleName, StyleNames.Normal);

            var subject = Unit.Parse(value);
            subject.Should().NotBe(Unit.Empty).And.NotBe(Unit.Empty);

            // Set document style and write MDDDL.
            var style = doc.Styles[styleName];
            style.Should().NotBeNull();
            style!.Document.Should().NotBeNull();
            style!.Document!.DefaultTabStop = subject;

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().NotBeNullOrEmpty();

            // Change value in doc.
            style.Document.DefaultTabStop = "0.1mm";
            style.Document.DefaultTabStop.Should().NotBe(subject);

            // Read MDDDL and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);

            var styleRead = docRead.Styles[styleName];
            styleRead.Should().NotBeNull();
            styleRead!.Document!.DefaultTabStop.Should().Be(subject);
        }

        [Fact]
        public void Test_WriteAndReadMdddl_ReferenceTypeDescriptor()
        {
            const int testIterations = 3;
            var subject = "";

            var doc = new Document();

            for (var i = 0; i < testIterations; i++)
            {
                subject += Guid.NewGuid().ToString();

                // Set document comment and write MDDDL.
                doc.ImagePath = subject;

                var mdddl = DdlWriter.WriteToString(doc);
                mdddl.Should().NotBeNullOrEmpty();

                // Change value in doc.
                doc.ImagePath = "";
                doc.ImagePath.Should().NotBe(subject);

                // Read MDDDL and check if desired value is restored correctly.
                var docRead = DdlReader.DocumentFromString(mdddl);

                docRead.ImagePath.Should().NotBeNullOrEmpty();
                docRead.ImagePath.Should().Be(subject);
            }
        }

        [Fact]
        public void Test_WriteAndReadMdddl_DocumentObjectDescriptor()
        {
            // This test should fail if Test_WriteAndReadMdddl_ReferenceTypeDescriptor failed.

            const int testIterations = 3;
            var doc = new Document();

            for (var i = 0; i < testIterations; i++)
            {
                // Set document info and write MDDDL.
                var subjectAuthor = doc.Info.Author = Guid.NewGuid().ToString();

                var mdddl = DdlWriter.WriteToString(doc);
                mdddl.Should().NotBeNullOrEmpty();

                // Change values in doc.
                doc.Info.Author = "";

                // Read MDDDL and check if desired values are restored correctly.
                var docRead = DdlReader.DocumentFromString(mdddl);

                docRead.Info.Author.Should().NotBeNullOrEmpty();
                docRead.Info.Author.Should().Be(subjectAuthor);
            }
        }

        [Fact]
        public void Test_WriteAndReadMdddl_DocumentObjectCollectionDescriptor()
        {
            // This test should fail if Test_WriteAndReadMdddl_ReferenceTypeDescriptor failed.
            // This test should fail if Test_WriteAndReadMdddl_DocumentObjectDescriptor failed.

            const int testIterations = 3;
            var doc = new Document();

            for (var i = 0; i < testIterations; i++)
            {
                // Add desired value to doc and write MDDDL.
                var styleName = Guid.NewGuid().ToString();

                doc.AddStyle(styleName, StyleNames.Normal);
                doc.Styles[styleName].Should().NotBeNull();

                var mdddl = DdlWriter.WriteToString(doc);
                mdddl.Should().NotBeNullOrEmpty();

                // Remove value from doc.
                doc.Styles.RemoveObjectAt(doc.Styles.GetIndex(styleName));
                doc.Styles[styleName].Should().BeNull();

                // Read MDDDL and check if desired value is restored correctly.
                var docRead = DdlReader.DocumentFromString(mdddl);

                docRead.Styles[styleName].Should().NotBeNull();
            }
        }

        [Fact]
        public void Test_WriteAndReadMdddl_String_and_DocumentObject()
        {
            PdfSharpCore.ResetAll();
            try
            {
                GlobalFontSettings.FontResolver = new SegoeWpFontResolver();
                const string desiredValue = "segoe wp bold";
                const string changedValue = "segoe wp light";
                const string defaultValue = "";

                var doc = new Document();

                var styleName = "TestStyle";
                doc.AddStyle(styleName, StyleNames.Normal);
                var style = doc.Styles[styleName];
                style.Should().NotBeNull();

                // Set desired value in doc and write MDDDL.
                style!.ParagraphFormat.Font.Name = desiredValue;
                style.ParagraphFormat.Values.Font.Should().NotBeNull();
                style.ParagraphFormat.Font.Values.Name.Should().Be(desiredValue);
                style.ParagraphFormat.Font.Name.Should().Be(desiredValue);

                var mdddl = DdlWriter.WriteToString(doc);
                mdddl.Should().NotBeNullOrEmpty();

                // Change value in doc.
                style.ParagraphFormat.Font.Name = changedValue;
                style.ParagraphFormat.Font.Values.Name.Should().Be(changedValue);
                style.ParagraphFormat.Font.Name.Should().Be(changedValue);

                // Read Mdddl and check if desired value is restored correctly.
                var docRead = DdlReader.DocumentFromString(mdddl);

                var styleRead = docRead.Styles[styleName];
                styleRead.Should().NotBeNull();
                styleRead!.ParagraphFormat.Font.Name.Should().Be(desiredValue);

                // Assign Style to paragraph and check that the paragraph uses default value (and null in Values).
                var section = docRead.AddSection();
                var p = section.AddParagraph("Test", styleName);
                p.Format.Values.Font.Should().BeNull();
                p.Format.Font.Name.Should().Be(defaultValue);

                // Initiate flattening and check that the paragraph uses the desired value.
                var docReadRenderer = new PdfDocumentRenderer
                {
                    Document = docRead
                };
                docReadRenderer.PrepareRenderPages();
                p.Format.Values.Font.Should().NotBeNull();
                p.Format.Values.Font!.Name.Should().Be(desiredValue);
                p.Format.Font.Name.Should().Be(desiredValue);
            }
            finally
            {
                PdfSharpCore.ResetAll();
            }
        }

        [Fact]
        public void Test_WriteAndReadMdddl_String_and_DocumentObject_Inherited()
        {
            PdfSharpCore.ResetAll();
            try
            {
                GlobalFontSettings.FontResolver = new SegoeWpFontResolver();
                const string defaultValue = "";
                const string baseValue = "segoe wp bold";
                const string changedValue = "segoe wp light";

                var doc = new Document();

                var baseStyleName = "TestStyleBase";
                var baseStyle = doc.AddStyle(baseStyleName, StyleNames.Normal);
                baseStyle.ParagraphFormat.Font.Name = baseValue;

                var styleName = "TestStyle";
                doc.AddStyle(styleName, baseStyleName);

                // Set desired value in doc and write MDDDL.
                var style = doc.Styles[styleName];
                style.Should().NotBeNull();
                style!.ParagraphFormat.Values.Font = null;
                style.ParagraphFormat.Values.Font.Should().Be(null);
                style.ParagraphFormat.Font.Name.Should().Be(defaultValue);

                var mdddl = DdlWriter.WriteToString(doc);
                mdddl.Should().NotBeNullOrEmpty();

                // Change value in doc.
                style.ParagraphFormat.Font.Name = changedValue;
                style.ParagraphFormat.Values.Font.Should().NotBeNull();
                style.ParagraphFormat.Values.Font!.Name.Should().Be(changedValue);
                style.ParagraphFormat.Font.Name.Should().Be(changedValue);

                // Read Mdddl and check if desired value is restored correctly.
                var docRead = DdlReader.DocumentFromString(mdddl);

                var baseStyleRead = docRead.Styles[baseStyleName];
                baseStyleRead.Should().NotBeNull();
                baseStyleRead!.ParagraphFormat.Font.Name.Should().Be(baseValue);

                var styleRead = docRead.Styles[styleName];
                styleRead.Should().NotBeNull();
                styleRead!.ParagraphFormat.Values.Font.Should().Be(null);
                styleRead.ParagraphFormat.Font.Name.Should().Be(defaultValue);

                // Assign Style to paragraph and check that the paragraph uses default value (and null in Values).
                var section = docRead.AddSection();
                var p = section.AddParagraph("Test", styleName);
                p.Format.Values.Font.Should().Be(null);
                p.Format.Font.Name.Should().Be(defaultValue);

                // Initiate flattening and check that the paragraph uses base value.
                var docReadRenderer = new PdfDocumentRenderer
                {
                    Document = docRead
                };
                docReadRenderer.PrepareRenderPages();
                p.Format.Values.Font.Should().NotBeNull();
                p.Format.Values.Font!.Name.Should().Be(baseValue);
                p.Format.Font.Name.Should().Be(baseValue);
            }
            finally
            {
                PdfSharpCore.ResetAll();
            }
        }

        [Fact]
        public void Test_WriteAndReadMdddl_String_and_DocumentObject_Override_Inherited()
        {
            PdfSharpCore.ResetAll();
            try
            {
                GlobalFontSettings.FontResolver = new SegoeWpFontResolver();
                const string defaultValue = "";
                const string baseValue = "segoe wp bold";
                const string desiredValue = "segoe wp black";
                const string changedValue = "segoe wp light";

                var doc = new Document();

                var baseStyleName = "TestStyleBase";
                var baseStyle = doc.AddStyle(baseStyleName, StyleNames.Normal);
                baseStyle.ParagraphFormat.Font.Name = baseValue;

                var styleName = "TestStyle";
                doc.AddStyle(styleName, baseStyleName);

                // Set desired value in doc and write MDDDL.
                var style = doc.Styles[styleName];
                style.Should().NotBeNull();
                style!.ParagraphFormat.Font.Name = desiredValue;
                style.ParagraphFormat.Values.Font.Should().NotBeNull();
                style.ParagraphFormat.Values.Font!.Name.Should().Be(desiredValue);
                style.ParagraphFormat.Font.Name.Should().Be(desiredValue);

                var mdddl = DdlWriter.WriteToString(doc);
                mdddl.Should().NotBeNullOrEmpty();

                // Change value in doc.
                style.ParagraphFormat.Font.Name = changedValue;
                style.ParagraphFormat.Values.Font.Name.Should().Be(changedValue);
                style.ParagraphFormat.Font.Name.Should().Be(changedValue);

                // Read Mdddl and check if desired value is restored correctly.
                var docRead = DdlReader.DocumentFromString(mdddl);

                var baseStyleRead = docRead.Styles[baseStyleName];
                baseStyleRead.Should().NotBeNull();
                baseStyleRead!.ParagraphFormat.Font.Name.Should().Be(baseValue);

                var styleRead = docRead.Styles[styleName];
                styleRead.Should().NotBeNull();

                styleRead!.ParagraphFormat.Values.Font.Should().NotBeNull();
                styleRead.ParagraphFormat.Values.Font!.Name.Should().Be(desiredValue);
                styleRead.ParagraphFormat.Font.Name.Should().Be(desiredValue);

                // Assign Style to paragraph and check that the paragraph uses default value (and null in Values).
                var section = docRead.AddSection();
                var p = section.AddParagraph("Test", styleName);
                p.Format.Values.Font.Should().Be(null);
                p.Format.Font.Name.Should().Be(defaultValue);

                // Initiate flattening and check that the paragraph uses the desired value.
                var docReadRenderer = new PdfDocumentRenderer
                {
                    Document = docRead
                };
                docReadRenderer.PrepareRenderPages();
                p.Format.Values.Font.Should().NotBeNull();
                p.Format.Values.Font!.Name.Should().Be(desiredValue);
                p.Format.Font.Name.Should().Be(desiredValue);
            }
            finally
            {
                PdfSharpCore.ResetAll();
            }
        }

        [Fact]
        public void Test_WriteAndReadMdddl_Bool()
        {
            const bool defaultValue = false;

            var doc = new Document();

            var styleName = "TestStyle";
            doc.AddStyle(styleName, StyleNames.Normal);
            var style = doc.Styles[styleName];
            style.Should().NotBeNull();

            for (var i = 0; i <= 1; i++)
            {
                var desiredValue = i > 0;

                // Set desired value in doc and write MDDDL.
                style!.ParagraphFormat.KeepTogether = desiredValue;
                style.ParagraphFormat.Values.KeepTogether.Should().Be(desiredValue);
                style.ParagraphFormat.KeepTogether.Should().Be(desiredValue);

                var mdddl = DdlWriter.WriteToString(doc);

                // Change value in doc.
                style.ParagraphFormat.KeepTogether = !desiredValue;
                style.ParagraphFormat.KeepTogether.Should().NotBe(desiredValue);

                // Read MDDDL and check if desired value is restored correctly.
                var docRead = DdlReader.DocumentFromString(mdddl);

                var styleRead = docRead.Styles[styleName];
                styleRead.Should().NotBeNull();
                styleRead!.ParagraphFormat.KeepTogether.Should().Be(desiredValue);

                // Assign style to paragraph and check that the paragraph uses default value (and null in Values)
                var section = docRead.AddSection();
                var p = section.AddParagraph("Test", styleName);
                p.Format.Values.KeepTogether.Should().BeNull();
                p.Format.KeepTogether.Should().Be(defaultValue);

                // Initiate flattening and check that the paragraph uses the desired value.
                var docReadRenderer = new PdfDocumentRenderer
                {
                    Document = docRead
                };
                docReadRenderer.PrepareRenderPages();
                p.Format.Values.KeepTogether.Should().Be(desiredValue);
                p.Format.KeepTogether.Should().Be(desiredValue);
            }
        }

        [Fact]
        public void Test_WriteAndReadMdddl_Bool_Inherited()
        {
            const bool defaultValue = false;
            const bool baseValue = true;
            const bool changedValue = false;

            var doc = new Document();

            var baseStyleName = "TestStyleBase";
            var baseStyle = doc.AddStyle(baseStyleName, StyleNames.Normal);
            baseStyle.ParagraphFormat.KeepTogether = baseValue;

            var styleName = "TestStyle";
            doc.AddStyle(styleName, baseStyleName);

            // Set desired value in doc and write MDDDL.
            var style = doc.Styles[styleName];
            style.Should().NotBeNull();
            style!.ParagraphFormat.Values.KeepTogether = null;
            style.ParagraphFormat.Values.KeepTogether.Should().Be(null);
            style.ParagraphFormat.KeepTogether.Should().Be(defaultValue);

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().NotBeNullOrEmpty();

            // Change value in doc.
            style.ParagraphFormat.KeepTogether = changedValue;
            style.ParagraphFormat.Values.KeepTogether.Should().Be(changedValue);
            style.ParagraphFormat.KeepTogether.Should().Be(changedValue);

            // Read Mdddl and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);

            var baseStyleRead = docRead.Styles[baseStyleName];
            baseStyleRead.Should().NotBeNull();
            baseStyleRead!.ParagraphFormat.KeepTogether.Should().Be(baseValue);

            var styleRead = docRead.Styles[styleName];
            styleRead.Should().NotBeNull();
            styleRead!.ParagraphFormat.Values.KeepTogether.Should().Be(null);
            styleRead.ParagraphFormat.KeepTogether.Should().Be(defaultValue);

            // Assign Style to paragraph and check that the paragraph uses default value (and null in Values).
            var section = docRead.AddSection();
            var p = section.AddParagraph("Test", styleName);
            p.Format.Values.KeepTogether.Should().Be(null);
            p.Format.KeepTogether.Should().Be(defaultValue);

            // Initiate flattening and check that the paragraph uses base value.
            var docReadRenderer = new PdfDocumentRenderer
            {
                Document = docRead
            };
            docReadRenderer.PrepareRenderPages();
            p.Format.Values.KeepTogether.Should().Be(baseValue);
            p.Format.KeepTogether.Should().Be(baseValue);
        }

        [Fact]
        public void Test_WriteAndReadMdddl_Bool_Override_Inherited()
        {
            const bool defaultValue = false;
            const bool baseValue = false;
            const bool desiredValue = true;
            const bool changedValue = false;

            var doc = new Document();

            var baseStyleName = "TestStyleBase";
            var baseStyle = doc.AddStyle(baseStyleName, StyleNames.Normal);
            baseStyle.ParagraphFormat.KeepTogether = baseValue;

            var styleName = "TestStyle";
            doc.AddStyle(styleName, baseStyleName);

            // Set desired value in doc and write MDDDL.
            var style = doc.Styles[styleName];
            style.Should().NotBeNull();
            style!.ParagraphFormat.Values.KeepTogether = desiredValue;
            style.ParagraphFormat.Values.KeepTogether.Should().Be(desiredValue);
            style.ParagraphFormat.KeepTogether.Should().Be(desiredValue);

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().NotBeNullOrEmpty();

            // Change value in doc.
            style.ParagraphFormat.KeepTogether = changedValue;
            style.ParagraphFormat.Values.KeepTogether.Should().Be(changedValue);
            style.ParagraphFormat.KeepTogether.Should().Be(changedValue);

            // Read Mdddl and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);

            var baseStyleRead = docRead.Styles[baseStyleName];
            baseStyleRead.Should().NotBeNull();
            baseStyleRead!.ParagraphFormat.KeepTogether.Should().Be(baseValue);

            var styleRead = docRead.Styles[styleName];
            styleRead.Should().NotBeNull();

            styleRead!.ParagraphFormat.Values.KeepTogether.Should().Be(desiredValue);
            styleRead.ParagraphFormat.KeepTogether.Should().Be(desiredValue);

            // Assign Style to paragraph and check that the paragraph uses default value (and null in Values).
            var section = docRead.AddSection();
            var p = section.AddParagraph("Test", styleName);
            p.Format.Values.KeepTogether.Should().Be(null);
            p.Format.KeepTogether.Should().Be(defaultValue);

            // Initiate flattening and check that the paragraph uses the desired value.
            var docReadRenderer = new PdfDocumentRenderer
            {
                Document = docRead
            };
            docReadRenderer.PrepareRenderPages();
            p.Format.Values.KeepTogether.Should().Be(desiredValue);
            p.Format.KeepTogether.Should().Be(desiredValue);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("23.45mm")]
        [InlineData("1.345cm")]
        [InlineData("0.678in")]
        [InlineData("12.34pc")]
        [InlineData("12.34pt")]
        public void Test_WriteAndReadMdddl_Unit(string desiredValueString)
        {
            var defaultValue = Unit.Empty;
            var changedValue = Unit.Zero;
            var desiredValue = Unit.Parse(desiredValueString);
            desiredValue.Should().NotBe(Unit.Empty).And.NotBe(Unit.Empty);

            var doc = new Document();

            var styleName = "TestStyle";
            doc.AddStyle(styleName, StyleNames.Normal);
            var style = doc.Styles[styleName];
            style.Should().NotBeNull();

            // Set desired value in doc and write MDDDL.
            style!.ParagraphFormat.FirstLineIndent = desiredValue;
            style.ParagraphFormat.Values.FirstLineIndent.Should().Be(desiredValue);
            style.ParagraphFormat.FirstLineIndent.Should().Be(desiredValue);

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().NotBeNullOrEmpty();

            // Change value in doc.
            style.ParagraphFormat.FirstLineIndent = changedValue;
            style.ParagraphFormat.Values.FirstLineIndent.Should().Be(changedValue);
            style.ParagraphFormat.FirstLineIndent.Should().Be(changedValue);

            // Read Mdddl and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);

            var styleRead = docRead.Styles[styleName];
            styleRead.Should().NotBeNull();
            if (!desiredValue.IsNull && desiredValue.Value == 0) // "0" as default is not written to MDDDL and is in consequence Unit.Empty after reading.
                styleRead!.ParagraphFormat.FirstLineIndent.Should().Be(Unit.Empty);
            else
                styleRead!.ParagraphFormat.FirstLineIndent.Should().Be(desiredValue);

            // Assign Style to paragraph and check that the paragraph uses default value (and null in Values).
            var section = docRead.AddSection();
            var p = section.AddParagraph("Test", styleName);
            p.Format.Values.FirstLineIndent.Should().Be(null);
            p.Format.FirstLineIndent.Should().Be(defaultValue);

            // Initiate flattening and check that the paragraph uses the desired value.
            var docReadRenderer = new PdfDocumentRenderer
            {
                Document = docRead
            };
            docReadRenderer.PrepareRenderPages();
            p.Format.Values.FirstLineIndent.Should().Be(desiredValue);
            p.Format.FirstLineIndent.Should().Be(desiredValue);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("23.45mm")]
        [InlineData("1.345cm")]
        [InlineData("0.678in")]
        [InlineData("12.34pc")]
        [InlineData("12.34pt")]
        public void Test_WriteAndReadMdddl_Unit_Inherited(string baseValueString)
        {
            var defaultValue = Unit.Empty;
            var baseValue = Unit.Parse(baseValueString);
            baseValue.Should().NotBe(Unit.Empty).And.NotBe(Unit.Empty);
            var changedValue = Unit.Zero;

            var doc = new Document();

            var baseStyleName = "TestStyleBase";
            var baseStyle = doc.AddStyle(baseStyleName, StyleNames.Normal);
            baseStyle.ParagraphFormat.FirstLineIndent = baseValue;

            var styleName = "TestStyle";
            doc.AddStyle(styleName, baseStyleName);

            // Set desired value in doc and write MDDDL.
            var style = doc.Styles[styleName];
            style.Should().NotBeNull();
            style!.ParagraphFormat.Values.FirstLineIndent = null;
            style.ParagraphFormat.Values.FirstLineIndent.Should().Be(null);
            style.ParagraphFormat.FirstLineIndent.Should().Be(defaultValue);

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().NotBeNullOrEmpty();

            // Change value in doc.
            style.ParagraphFormat.FirstLineIndent = changedValue;
            style.ParagraphFormat.Values.FirstLineIndent.Should().Be(changedValue);
            style.ParagraphFormat.FirstLineIndent.Should().Be(changedValue);

            // Read Mdddl and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);

            var baseStyleRead = docRead.Styles[baseStyleName];
            baseStyleRead.Should().NotBeNull();
            if (!baseValue.IsNull && baseValue.Value == 0) // "0" as default is not written to MDDDL and is in consequence Unit.Empty after reading.
                baseStyleRead!.ParagraphFormat.FirstLineIndent.Should().Be(Unit.Empty);
            else
                baseStyleRead!.ParagraphFormat.FirstLineIndent.Should().Be(baseValue);

            var styleRead = docRead.Styles[styleName];
            styleRead.Should().NotBeNull();
            styleRead!.ParagraphFormat.Values.FirstLineIndent.Should().Be(null);
            styleRead.ParagraphFormat.FirstLineIndent.Should().Be(defaultValue);

            // Assign Style to paragraph and check that the paragraph uses default value (and null in Values).
            var section = docRead.AddSection();
            var p = section.AddParagraph("Test", styleName);
            p.Format.Values.FirstLineIndent.Should().Be(null);
            p.Format.FirstLineIndent.Should().Be(defaultValue);

            // Initiate flattening and check that the paragraph uses base value.
            var docReadRenderer = new PdfDocumentRenderer
            {
                Document = docRead
            };
            docReadRenderer.PrepareRenderPages();
            p.Format.Values.FirstLineIndent.Should().Be(baseValue);
            p.Format.FirstLineIndent.Should().Be(baseValue);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("23.45mm")]
        [InlineData("1.345cm")]
        [InlineData("0.678in")]
        [InlineData("12.34pc")]
        [InlineData("12.34pt")]
        public void Test_WriteAndReadMdddl_Unit_Override_Inherited(string desiredValueString)
        {
            var defaultValue = Unit.Empty;
            var baseValue = Unit.Empty;
            var desiredValue = Unit.Parse(desiredValueString);
            var changedValue = Unit.Parse("32.1mm");

            var doc = new Document();

            var baseStyleName = "TestStyleBase";
            var baseStyle = doc.AddStyle(baseStyleName, StyleNames.Normal);
            baseStyle.ParagraphFormat.FirstLineIndent = baseValue;

            var styleName = "TestStyle";
            doc.AddStyle(styleName, baseStyleName);

            // Set desired value in doc and write MDDDL.
            var style = doc.Styles[styleName];
            style.Should().NotBeNull();
            style!.ParagraphFormat.FirstLineIndent = desiredValue;
            style.ParagraphFormat.Values.FirstLineIndent.Should().Be(desiredValue);
            style.ParagraphFormat.FirstLineIndent.Should().Be(desiredValue);

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().NotBeNullOrEmpty();

            // Change value in doc.
            style.ParagraphFormat.FirstLineIndent = changedValue;
            style.ParagraphFormat.Values.FirstLineIndent.Should().Be(changedValue);
            style.ParagraphFormat.FirstLineIndent.Should().Be(changedValue);

            // Read Mdddl and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);

            var baseStyleRead = docRead.Styles[baseStyleName];
            baseStyleRead.Should().NotBeNull();
            baseStyleRead!.ParagraphFormat.FirstLineIndent.Should().Be(baseValue);

            var styleRead = docRead.Styles[styleName];
            styleRead.Should().NotBeNull();

            styleRead!.ParagraphFormat.Values.FirstLineIndent.Should().Be(desiredValue);
            styleRead.ParagraphFormat.FirstLineIndent.Should().Be(desiredValue);

            // Assign Style to paragraph and check that the paragraph uses default value (and null in Values).
            var section = docRead.AddSection();
            var p = section.AddParagraph("Test", styleName);
            p.Format.Values.FirstLineIndent.Should().Be(null);
            p.Format.FirstLineIndent.Should().Be(defaultValue);

            // Initiate flattening and check that the paragraph uses the desired value.
            var docReadRenderer = new PdfDocumentRenderer
            {
                Document = docRead
            };
            docReadRenderer.PrepareRenderPages();
            p.Format.Values.FirstLineIndent.Should().Be(desiredValue);
            p.Format.FirstLineIndent.Should().Be(desiredValue);
        }

        [Fact]
        public void Test_WriteAndReadMdddl_Enum()
        {
            var defaultValue = ParagraphAlignment.Left;
            var changedValue = ParagraphAlignment.Center;

            var doc = new Document();

            var styleName = "TestStyle";
            doc.AddStyle(styleName, StyleNames.Normal);
            var style = doc.Styles[styleName];
            style.Should().NotBeNull();

#if NET6_0_OR_GREATER
            foreach (var desiredValue in Enum.GetValues<ParagraphAlignment>())
#else
            foreach (ParagraphAlignment desiredValue in Enum.GetValues(typeof(ParagraphAlignment)))
#endif
            {
                // Set desired value in doc and write MDDDL.
                style!.ParagraphFormat.Alignment = desiredValue;
                style.ParagraphFormat.Alignment.Should().Be(desiredValue);

                var mdddl = DdlWriter.WriteToString(doc);
                mdddl.Should().NotBeNullOrEmpty();

                // Change value in doc.
                style.ParagraphFormat.Alignment = changedValue;
                style.ParagraphFormat.Values.Alignment.Should().Be(changedValue);
                style.ParagraphFormat.Alignment.Should().Be(changedValue);

                // Read Mdddl and check if desired value is restored correctly.
                var docRead = DdlReader.DocumentFromString(mdddl);

                var styleRead = docRead.Styles[styleName];
                styleRead.Should().NotBeNull();
                styleRead!.ParagraphFormat.Alignment.Should().Be(desiredValue);

                // Assign Style to paragraph and check that the paragraph uses default value (and null in Values).
                var section = docRead.AddSection();
                var p = section.AddParagraph("Test", styleName);
                p.Format.Values.Alignment.Should().Be(null);
                p.Format.Alignment.Should().Be(defaultValue);

                // Initiate flattening and check that the paragraph uses the desired value.
                var docReadRenderer = new PdfDocumentRenderer
                {
                    Document = docRead
                };
                docReadRenderer.PrepareRenderPages();
                p.Format.Values.Alignment.Should().Be(desiredValue);
                p.Format.Alignment.Should().Be(desiredValue);
            }
        }

        [Fact]
        public void Test_WriteAndReadMdddl_Enum_Inherited()
        {
            var defaultValue = ParagraphAlignment.Left;
            var baseValue = ParagraphAlignment.Right;
            var changedValue = ParagraphAlignment.Center;

            var doc = new Document();

            var baseStyleName = "TestStyleBase";
            var baseStyle = doc.AddStyle(baseStyleName, StyleNames.Normal);
            baseStyle.ParagraphFormat.Alignment = baseValue;

            var styleName = "TestStyle";
            doc.AddStyle(styleName, baseStyleName);

            // Set desired value in doc and write MDDDL.
            var style = doc.Styles[styleName];
            style.Should().NotBeNull();
            style!.ParagraphFormat.Values.Alignment = null;
            style.ParagraphFormat.Values.Alignment.Should().Be(null);
            style.ParagraphFormat.Alignment.Should().Be(defaultValue);

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().NotBeNullOrEmpty();

            // Change value in doc.
            style.ParagraphFormat.Alignment = changedValue;
            style.ParagraphFormat.Values.Alignment.Should().Be(changedValue);
            style.ParagraphFormat.Alignment.Should().Be(changedValue);

            // Read Mdddl and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);

            var baseStyleRead = docRead.Styles[baseStyleName];
            baseStyleRead.Should().NotBeNull();
            baseStyleRead!.ParagraphFormat.Alignment.Should().Be(baseValue);

            var styleRead = docRead.Styles[styleName];
            styleRead.Should().NotBeNull();
            styleRead!.ParagraphFormat.Values.Alignment.Should().Be(null);
            styleRead.ParagraphFormat.Alignment.Should().Be(defaultValue);

            // Assign Style to paragraph and check that the paragraph uses default value (and null in Values).
            var section = docRead.AddSection();
            var p = section.AddParagraph("Test", styleName);
            p.Format.Values.Alignment.Should().Be(null);
            p.Format.Alignment.Should().Be(defaultValue);

            // Initiate flattening and check that the paragraph uses base value.
            var docReadRenderer = new PdfDocumentRenderer
            {
                Document = docRead
            };
            docReadRenderer.PrepareRenderPages();
            p.Format.Values.Alignment.Should().Be(baseValue);
            p.Format.Alignment.Should().Be(baseValue);
        }

        [Fact]
        public void Test_WriteAndReadMdddl_Enum_Override_Inherited()
        {
            var defaultValue = ParagraphAlignment.Left;
            var baseValue = ParagraphAlignment.Right;
            var desiredValue = ParagraphAlignment.Justify;
            var changedValue = ParagraphAlignment.Center;

            var doc = new Document();

            var baseStyleName = "TestStyleBase";
            var baseStyle = doc.AddStyle(baseStyleName, StyleNames.Normal);
            baseStyle.ParagraphFormat.Alignment = baseValue;

            var styleName = "TestStyle";
            doc.AddStyle(styleName, baseStyleName);

            // Set desired value in doc and write MDDDL.
            var style = doc.Styles[styleName];
            style.Should().NotBeNull();
            style!.ParagraphFormat.Alignment = desiredValue;
            style.ParagraphFormat.Values.Alignment.Should().Be(desiredValue);
            style.ParagraphFormat.Alignment.Should().Be(desiredValue);

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().NotBeNullOrEmpty();

            // Change value in doc.
            style.ParagraphFormat.Alignment = changedValue;
            style.ParagraphFormat.Values.Alignment.Should().Be(changedValue);
            style.ParagraphFormat.Alignment.Should().Be(changedValue);

            // Read Mdddl and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);

            var baseStyleRead = docRead.Styles[baseStyleName];
            baseStyleRead.Should().NotBeNull();
            baseStyleRead!.ParagraphFormat.Alignment.Should().Be(baseValue);

            var styleRead = docRead.Styles[styleName];
            styleRead.Should().NotBeNull();

            styleRead!.ParagraphFormat.Values.Alignment.Should().Be(desiredValue);
            styleRead.ParagraphFormat.Alignment.Should().Be(desiredValue);

            // Assign Style to paragraph and check that the paragraph uses default value (and null in Values).
            var section = docRead.AddSection();
            var p = section.AddParagraph("Test", styleName);
            p.Format.Values.Alignment.Should().Be(null);
            p.Format.Alignment.Should().Be(defaultValue);

            // Initiate flattening and check that the paragraph uses the desired value.
            var docReadRenderer = new PdfDocumentRenderer
            {
                Document = docRead
            };
            docReadRenderer.PrepareRenderPages();
            p.Format.Values.Alignment.Should().Be(desiredValue);
            p.Format.Alignment.Should().Be(desiredValue);
        }

        [Fact]
        public void Test_WriteAndReadMdddl_Style_Inheritance()
        {
            PdfSharpCore.ResetAll();
            try
            {
                GlobalFontSettings.FontResolver = new SegoeWpFontResolver();

                var desiredAlignment = ParagraphAlignment.Right;
                var desiredBorderRightWidth = Unit.Parse("1pt");
                var desiredFirstLineIndent = Unit.Parse("1cm");
                var desiredFontBold = true;
                var desiredFontItalic = true;
                var desiredFontName = "segoe wp bold";
                var desiredLineSpacing = Unit.Parse("2.5mm");
                var desiredLineSpacingRule = LineSpacingRule.AtLeast;
                var desiredShadingColor = Color.Parse("#123456");
                var desiredSpaceAfter = Unit.Parse("0.97in");

                var baseAlignment = ParagraphAlignment.Center;
                var baseBorderRightWidth = Unit.Parse("1.5pt");
                var baseFirstLineIndent = Unit.Parse("1.5cm");
                var baseFontBold = true;
                var baseFontItalic = false;
                var baseFontName = "segoe ui";
                var baseLineSpacing = Unit.Parse("3.75mm");
                var baseLineSpacingRule = LineSpacingRule.Exactly;
                var baseShadingColor = Color.Parse("#654321");
                var baseSpaceAfter = Unit.Parse("1.45in");
                var baseLeftIndent = Unit.Parse("17mm");
                var baseRightIndent = Unit.Parse("34mm");

                var changedAlignment = ParagraphAlignment.Justify;
                var changedBorderRightWidth = Unit.Parse("2pt");
                var changedFirstLineIndent = Unit.Parse("2cm");
                var changedFontBold = false;
                var changedFontItalic = false;
                var changedFontName = "segoe wp light";
                var changedLineSpacing = Unit.Parse("5mm");
                var changedLineSpacingRule = LineSpacingRule.Double;
                var changedShadingColor = Color.Parse("#ABCDEF");
                var changedSpaceAfter = Unit.Parse("1.94in");

                var defaultAlignment = ParagraphAlignment.Left;
                var defaultBorderRightWidth = Unit.Empty;
                var defaultFirstLineIndent = Unit.Empty;
                var defaultFontBold = false;
                var defaultFontItalic = false;
                var defaultFontName = "";
                var defaultLineSpacing = Unit.Empty;
                var defaultLineSpacingRule = LineSpacingRule.Single;
                var defaultShadingColor = Color.Empty;
                var defaultSpaceAfter = Unit.Empty;
                var defaultLeftIndent = Unit.Empty;
                var defaultRightIndent = Unit.Empty;

                var doc = new Document();

                var baseStyleName = "TestStyleBase";
                var baseStyle = doc.AddStyle(baseStyleName, StyleNames.Normal);

                baseStyle.ParagraphFormat.Alignment = baseAlignment;
                baseStyle.ParagraphFormat.Borders = new Borders { Right = new Border { Width = baseBorderRightWidth } };
                baseStyle.ParagraphFormat.FirstLineIndent = baseFirstLineIndent;
                baseStyle.ParagraphFormat.Font.Bold = baseFontBold;
                baseStyle.ParagraphFormat.Font.Italic = baseFontItalic;
                baseStyle.ParagraphFormat.Font.Name = baseFontName;
                baseStyle.ParagraphFormat.LineSpacing = baseLineSpacing;
                baseStyle.ParagraphFormat.LineSpacingRule = baseLineSpacingRule;
                baseStyle.ParagraphFormat.Shading = new Shading { Color = baseShadingColor };
                baseStyle.ParagraphFormat.SpaceAfter = baseSpaceAfter;
                baseStyle.ParagraphFormat.LeftIndent = baseLeftIndent;
                baseStyle.ParagraphFormat.RightIndent = baseRightIndent;

                var styleName = "TestStyle";
                doc.AddStyle(styleName, baseStyleName);
                var style = doc.Styles[styleName];
                style.Should().NotBeNull();

                // Set desired values in doc and write MDDDL.
                style!.ParagraphFormat.Alignment = desiredAlignment;
                style.ParagraphFormat.Borders = new Borders { Right = new Border { Width = desiredBorderRightWidth } };
                style.ParagraphFormat.FirstLineIndent = desiredFirstLineIndent;
                style.ParagraphFormat.Font.Bold = desiredFontBold;
                style.ParagraphFormat.Font.Italic = desiredFontItalic;
                style.ParagraphFormat.Font.Name = desiredFontName;
                style.ParagraphFormat.LineSpacing = desiredLineSpacing;
                style.ParagraphFormat.LineSpacingRule = desiredLineSpacingRule;
                style.ParagraphFormat.Shading = new Shading { Color = desiredShadingColor };
                style.ParagraphFormat.SpaceAfter = desiredSpaceAfter;

                var mdddl = DdlWriter.WriteToString(doc);
                mdddl.Should().NotBeNullOrEmpty();

                // Change values in doc.
                style.ParagraphFormat.Alignment = changedAlignment;
                style.ParagraphFormat.Borders.Right.Width = changedBorderRightWidth;
                style.ParagraphFormat.FirstLineIndent = changedFirstLineIndent;
                style.ParagraphFormat.Font.Bold = changedFontBold;
                style.ParagraphFormat.Font.Italic = changedFontItalic;
                style.ParagraphFormat.Font.Name = changedFontName;
                style.ParagraphFormat.LineSpacing = changedLineSpacing;
                style.ParagraphFormat.LineSpacingRule = changedLineSpacingRule;
                style.ParagraphFormat.Shading.Color = changedShadingColor;
                style.ParagraphFormat.SpaceAfter = changedSpaceAfter;

                // Read MDDDL and check if desired values are restored correctly.
                var docRead = DdlReader.DocumentFromString(mdddl);

                var baseStyleRead = docRead.Styles[baseStyleName];
                baseStyleRead.Should().NotBeNull();
                baseStyleRead!.ParagraphFormat.Alignment.Should().Be(baseAlignment);
                baseStyleRead.ParagraphFormat.Borders.Right.Width.Should().Be(baseBorderRightWidth);
                baseStyleRead.ParagraphFormat.FirstLineIndent.Should().Be(baseFirstLineIndent);
                baseStyleRead.ParagraphFormat.Font.Bold.Should().Be(baseFontBold);
                baseStyleRead.ParagraphFormat.Font.Italic.Should().Be(baseFontItalic);
                baseStyleRead.ParagraphFormat.Font.Name.Should().Be(baseFontName);
                baseStyleRead.ParagraphFormat.LineSpacing.Should().Be(baseLineSpacing);
                baseStyleRead.ParagraphFormat.LineSpacingRule.Should().Be(baseLineSpacingRule);
                baseStyleRead.ParagraphFormat.Shading.Color.Should().Be(baseShadingColor);
                baseStyleRead.ParagraphFormat.SpaceAfter.Should().Be(baseSpaceAfter);

                var styleRead = docRead.Styles[styleName];
                styleRead.Should().NotBeNull();
                styleRead!.ParagraphFormat.Alignment.Should().Be(desiredAlignment);
                styleRead.ParagraphFormat.Borders.Right.Width.Should().Be(desiredBorderRightWidth);
                styleRead.ParagraphFormat.FirstLineIndent.Should().Be(desiredFirstLineIndent);
                // Font.Bold is optimized away in MDDDL but will be available again after flattening.
                styleRead.ParagraphFormat.Font.Values.Bold.Should().BeNull();
                styleRead.ParagraphFormat.Font.Italic.Should().Be(desiredFontItalic);
                styleRead.ParagraphFormat.Font.Name.Should().Be(desiredFontName);
                styleRead.ParagraphFormat.LineSpacing.Should().Be(desiredLineSpacing);
                styleRead.ParagraphFormat.LineSpacingRule.Should().Be(desiredLineSpacingRule);
                styleRead.ParagraphFormat.Shading.Color.Should().Be(desiredShadingColor);
                styleRead.ParagraphFormat.SpaceAfter.Should().Be(desiredSpaceAfter);
                // Indents will be inherited after flattening.
                styleRead.ParagraphFormat.Values.LeftIndent.Should().BeNull();
                styleRead.ParagraphFormat.LeftIndent.Should().Be(defaultLeftIndent);
                styleRead.ParagraphFormat.Values.RightIndent.Should().BeNull();
                styleRead.ParagraphFormat.RightIndent.Should().Be(defaultRightIndent);

                // Assign style to paragraph and check that the paragraph uses default value (and null in Values)
                var section = docRead.AddSection();
                var p = section.AddParagraph("Test", styleName);
                p.Format.Values.Alignment.Should().BeNull();
                p.Format.Alignment.Should().Be(defaultAlignment);
                p.Format.Values.Borders.Should().BeNull();
                p.Format.Borders.Right.Width.Should().Be(defaultBorderRightWidth);
                p.Format.Values.FirstLineIndent.Should().BeNull();
                p.Format.FirstLineIndent.Should().Be(defaultFirstLineIndent);
                p.Format.Values.Font.Should().BeNull();
                p.Format.Font.Bold.Should().Be(defaultFontBold);
                p.Format.Font.Italic.Should().Be(defaultFontItalic);
                p.Format.Font.Name.Should().Be(defaultFontName);
                p.Format.Values.LineSpacing.Should().BeNull();
                p.Format.LineSpacing.Should().Be(defaultLineSpacing);
                p.Format.Values.LineSpacingRule.Should().BeNull();
                p.Format.LineSpacingRule.Should().Be(defaultLineSpacingRule);
                p.Format.Values.Shading.Should().BeNull();
                p.Format.Shading.Color.Should().Be(defaultShadingColor);
                p.Format.Values.SpaceAfter.Should().BeNull();
                p.Format.SpaceAfter.Should().Be(defaultSpaceAfter);

                // Initiate flattening and check that the paragraph uses the desired values.
                var docReadRenderer = new PdfDocumentRenderer
                {
                    Document = docRead
                };
                docReadRenderer.PrepareRenderPages();
                p.Format.Values.Alignment.Should().NotBeNull();
                p.Format.Alignment.Should().Be(desiredAlignment);
                p.Format.Values.Borders.Should().NotBeNull();
                p.Format.Values.Borders!.Values.Right.Should().NotBeNull();
                p.Format.Values.Borders.Values.Right!.Values.Width.Should().NotBeNull();
                p.Format.Borders.Right.Width.Should().Be(desiredBorderRightWidth);
                p.Format.Values.FirstLineIndent.Should().NotBeNull();
                p.Format.FirstLineIndent.Should().Be(desiredFirstLineIndent);
                p.Format.Values.Font.Should().NotBeNull();
                p.Format.Font.Bold.Should().Be(desiredFontBold);
                p.Format.Font.Italic.Should().Be(desiredFontItalic);
                p.Format.Font.Name.Should().Be(desiredFontName);
                p.Format.Values.LineSpacing.Should().NotBeNull();
                p.Format.LineSpacing.Should().Be(desiredLineSpacing);
                p.Format.Values.LineSpacingRule.Should().NotBeNull();
                p.Format.LineSpacingRule.Should().Be(desiredLineSpacingRule);
                p.Format.Values.Shading.Should().NotBeNull();
                p.Format.Shading.Color.Should().Be(desiredShadingColor);
                p.Format.Values.SpaceAfter.Should().NotBeNull();
                p.Format.SpaceAfter.Should().Be(desiredSpaceAfter);
                p.Format.LeftIndent.Should().Be(baseLeftIndent);
                p.Format.RightIndent.Should().Be(baseRightIndent);
            }
            finally
            {
                PdfSharpCore.ResetAll();
            }
        }

        [Theory] // Generate Section with no, one and two paragraphs, to use and avoid direct paragraph content in section.
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Test_WriteAndReadMdddl_Section_Header_Empty_And_Paragraphs(int paragraphCount)
        {
            var doc = new Document();

            TestHelper.RemoveStyles(doc);

            // Set desired value in doc and write MDDDL.
            var section = doc.AddSection();

            doc.Values.Sections.Should().HaveCount(1);
            section.Values.Headers.Should().BeNull();

            for (var i = 0; i < paragraphCount; i++)
                section.AddParagraph($"ParagraphText{i + 1}");

            var mdddl = DdlWriter.WriteToString(doc);

            mdddl.Should().NotContain("\\primaryheader");

            // For 0 there is no paragraph, for 1 the paragraph content is added directly to section due to MDDDL optimization.
            if (paragraphCount < 2)
                mdddl.Should().NotContain("\\paragraph");
            else
                mdddl.Should().Contain("\\paragraph");

            // Read Mdddl and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);

            // With no header and no paragraph Sections will be removed by MDDDL optimization.
            if (paragraphCount == 0)
            {
                docRead.Values.Sections.Should().BeNull();
                return;
            }

            docRead.Values.Sections.Should().HaveCount(1);

            var sectionRead = docRead.Values.Sections!.First as Section;
            sectionRead.Should().NotBeNull();
            sectionRead!.Values.Headers.Should().BeNull();

            for (var i = 0; i < paragraphCount; i++)
            {
                var paragraphRead = sectionRead.Elements[i] as Paragraph;
                paragraphRead.Should().NotBeNull();

                var textRead = paragraphRead!.Elements.First as Text;
                textRead.Should().NotBeNull();
                textRead!.Content.Should().Be($"ParagraphText{i + 1}");
            }
        }

        [Theory] // Generate Section with no, one and two paragraphs, to use and avoid direct paragraph content in section.
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Test_WriteAndReadMdddl_Section_Header_Empty_Cleared_And_Paragraphs(int paragraphCount)
        {
            var doc = new Document();

            TestHelper.RemoveStyles(doc);

            // Set desired value in doc and write MDDDL.
            var section = doc.AddSection();
            section.Headers.Primary.Elements.Clear();

            doc.Values.Sections.Should().HaveCount(1);
            section.Should().Be(doc.Sections.First);

            var headers = section.Values.Headers;
            headers.Should().NotBeNull();

            var primaryHeader = headers!.Values.Primary;
            primaryHeader.Should().NotBeNull();
            primaryHeader!.Elements.Should().HaveCount(0);

            for (var i = 0; i < paragraphCount; i++)
                section.AddParagraph($"ParagraphText{i + 1}");

            var mdddl = DdlWriter.WriteToString(doc);

            // For 0 there is no paragraph, for 1 the paragraph content is added directly to section due to MDDDL optimization.
            if (paragraphCount < 2)
                mdddl.Should().NotContain("\\paragraph");
            else
                mdddl.Should().Contain("\\paragraph");

            mdddl.Should().Contain("\\primaryheader");

            // Read Mdddl and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);
            docRead.Values.Sections.Should().HaveCount(1);

            var sectionRead = docRead.Values.Sections!.First as Section;
            sectionRead.Should().NotBeNull();

            var headersRead = sectionRead!.Values.Headers;
            headersRead.Should().NotBeNull();

            var primaryHeaderRead = headersRead!.Values.Primary;
            primaryHeaderRead.Should().NotBeNull();
            primaryHeaderRead!.Elements.Should().HaveCount(0);

            if (paragraphCount == 0)
            {
                sectionRead.Elements.Should().HaveCount(0);
                return;
            }

            for (var i = 0; i < paragraphCount; i++)
            {
                var paragraphRead = sectionRead.Elements[i] as Paragraph;
                paragraphRead.Should().NotBeNull();

                var textRead = paragraphRead!.Elements.First as Text;
                textRead.Should().NotBeNull();
                textRead!.Content.Should().Be($"ParagraphText{i + 1}");
            }
        }

        [Theory] // Generate Section with no, one and two paragraphs, to use and avoid direct paragraph content in section.
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Test_WriteAndReadMdddl_Section_Header_Not_Empty_And_Paragraphs(int paragraphCount)
        {
            var doc = new Document();

            TestHelper.RemoveStyles(doc);

            // Set desired value in doc and write MDDDL.
            var section = doc.AddSection();
            section.Headers.Primary.AddParagraph("HeaderText");

            doc.Values.Sections.Should().HaveCount(1);
            section.Should().Be(doc.Sections.First);

            var headers = section.Values.Headers;
            headers.Should().NotBeNull();

            var primaryHeader = headers!.Values.Primary;
            primaryHeader.Should().NotBeNull();
            primaryHeader!.Elements.Should().HaveCount(1);

            for (var i = 0; i < paragraphCount; i++)
                section.AddParagraph($"ParagraphText{i + 1}");

            var mdddl = DdlWriter.WriteToString(doc);

            mdddl.Should().Contain("\\primaryheader");

            // For 0 there is no paragraph, for 1 the paragraph content is added directly to section due to MDDDL optimization.
            if (paragraphCount < 2)
                mdddl.Should().NotContain("\\paragraph");
            else
                mdddl.Should().Contain("\\paragraph");

            // Read Mdddl and check if desired value is restored correctly.
            var docRead = DdlReader.DocumentFromString(mdddl);
            docRead.Values.Sections.Should().HaveCount(1);

            var sectionRead = docRead.Values.Sections!.First as Section;
            sectionRead.Should().NotBeNull();

            var headersRead = sectionRead!.Values.Headers;
            headersRead.Should().NotBeNull();

            var primaryHeaderRead = headersRead!.Values.Primary;
            primaryHeaderRead.Should().NotBeNull();
            primaryHeaderRead!.Elements.Should().HaveCount(1);

            if (paragraphCount == 0)
            {
                sectionRead.Elements.Should().HaveCount(0);
                return;
            }

            for (var i = 0; i < paragraphCount; i++)
            {
                var paragraphRead = sectionRead.Elements[i] as Paragraph;
                paragraphRead.Should().NotBeNull();

                var textRead = paragraphRead!.Elements.First as Text;
                textRead.Should().NotBeNull();
                textRead!.Content.Should().Be($"ParagraphText{i + 1}");
            }
        }

        [Fact(Skip = "Escaping bug in current implementation")]
        public void Test_Write_And_Read_MDDDL_CommentEscaping()
        {
            var doc = new Document();
            var section = doc.AddSection();

            section.AddParagraph(@"// Test 1");

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().NotBeNullOrEmpty();

            mdddl = """ 
                \document
                {
                    \section
                    {
                      //\paragraph []
                      //{
                         \// Test 1
                      //}
                    }
                }
                """;

            // Read MDDDL.
            var docRead = DdlReader.DocumentFromString(mdddl);
        }

        [Fact(Skip = "Escaping bug in current implementation")]
        public void Test_WriteAndReadMdddl_ReservedWords()
        {
            var tests = new[]
            {
                @"{Hello}",
                @"[World]",
                @"// Test 1",
                @"// Test 2 //",
                @"// Test 3 // Test 3",
                @"/* Test 4 */",
                @"\ Test 5",
                @"\\ Test 6",
                @"\\\ Test 7",
                @"\paragraph { Test 8. Hello, World! }",
            };

            var doc = new Document();
            var section = doc.AddSection();

            var table = section.AddTable();
            var col = table.AddColumn();

            foreach (var test in tests)
            {
                section.AddParagraph(test);
                var row = table.AddRow();
                row[0].AddParagraph(test);
            }

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().NotBeNullOrEmpty();

            // Read MDDDL.
            var docRead = DdlReader.DocumentFromString(mdddl);
            docRead.Should().NotBeNull();
        }

        [Fact(Skip = "Escaping bug in current implementation")]
        public void Test_Write_And_Read_MDDDL_LinefeedEscaping()
        {
            var doc = new Document
            {
                Info =
                {
                    Title = "A Title",
                    Subject = "A subject with a \r\n line break",
                    Author = "empira"
                },
            };

            //var section = doc.AddSection();
            //section.AddParagraph(@"Test 1");

            var mdddl = DdlWriter.WriteToString(doc);
            mdddl.Should().NotBeNullOrEmpty();

            // Read MDDDL.
            // Throws exception "DdlParserException" with message "Newline in string not allowed.".
            // Exception should be thrown in DdlWriter or linefeed should be escaped.
            var docRead = DdlReader.DocumentFromString(mdddl);
        }
    }
}
