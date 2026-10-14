// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Forms;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.Pdf.ObjectModel
{
    [Collection("PDFsharp")]
    public class HybridContainerTests
    {
        const string TempRoot = "object-model/";

        [Fact]
        public void Test_the_concept()
        {
            int creationCounter = 0;

            var doc = new PdfDocument();
            doc.Pages.Add();

            var catalog = doc.Catalog;
            var testStuff = new PdfDictionary(doc, true);
            catalog.Elements.Add("/TestStuff", testStuff);

            // Array of fields.
            var fields = new PdfArray(doc, false);
            fields.Elements.Add(new PdfInteger(++creationCounter));

            // Array of widgets.
            var widgets = new PdfArray(doc, false);
            widgets.Elements.Add(new PdfInteger(++creationCounter));

            // Create regular field.
            var field = new SampleField(doc);
            field.IsIndirect.Should().BeTrue();
            field.Elements.Add("/CreationCounter_3", new PdfInteger(++creationCounter));
            fields.Elements.Add(field);
            field.Elements.Add("/Comment", new PdfString("This is a regular field."));

            // Create regular widget.
            var widget = new SampleWidget(doc);
            widget.Elements.Add("/CreationCounter_4", new PdfInteger(++creationCounter));
            widget.IsIndirect.Should().BeTrue();
            widgets.Elements.Add(widget);
            widget.Elements.Add("/Comment", new PdfString("This is a regular widget."));

            testStuff.Elements.Add("/1_fields", fields);
            testStuff.Elements.Add("/2_widgets", widgets);
            testStuff.Elements.Add("/xxx", new PdfInteger(9048));
            testStuff.Elements.Add("/RegularField", field);
            testStuff.Elements.Add("/RegularWidget", widget);

            // Create regular field also used as widget.
            var hybridField = new SampleField(doc);
            hybridField.Elements.Add("/CreationCounter_5", new PdfInteger(++creationCounter));
            hybridField.IsIndirect.Should().BeTrue();
            fields.Elements.Add(hybridField);
            hybridField.Elements.Add("/Comment2", new PdfString("This is a regular field but also a widget."));

            // Create widget from field.
            var hybridWidget = SampleWidget.CreateFromField(hybridField);  // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            hybridWidget.Elements.Add("/CreationCounter_6", new PdfInteger(++creationCounter));
            widgets.Elements.Add(hybridWidget);
            hybridWidget.Elements.Add("/Comment_2", new PdfString("This is added to the hybrid widget."));

            testStuff.Elements.Add("/HybridField", hybridField);
            testStuff.Elements.Add("/HybridWidget", hybridWidget);

            var dest = PdfFileUtility.GetTempPdfFullFileName(TempRoot + nameof(Test_the_concept));
            doc.Options.Layout = PdfWriterLayout.Verbose;
            doc.Save(dest);

            var doc2 = PdfReader.Open(dest, PdfDocumentOpenMode.Modify);

            var dest2 = PdfFileUtility.GetTempPdfFullFileName(TempRoot + nameof(Test_the_concept) + "#2");
            var test = doc.Catalog.Elements.GetRequiredDictionary<PdfDictionary>("/TestStuff");
            var field2 = test.Elements.GetRequiredDictionary<SampleField>("/HybridField");
            var widget2 = SampleWidget.CreateFromField(field2);

            // item1 and item2 should be the same object.
            var item1 = field2.Elements.GetValue("/Iam-1");
            var item2 = widget2.Elements.GetValue("/Iam-1");
            item1.Should().BeEquivalentTo(item2);
            ReferenceEquals(item1, item2).Should().BeTrue();

            doc2.Options.Layout = PdfWriterLayout.Verbose;
            doc2.Save(dest2);
        }
    }

    class SampleField : PdfFormField
    {
        public SampleField(PdfDocument doc) : base(doc)
        {
            Initialize();
        }

        void Initialize()
        {
            Elements.Add("/Iam-1", new PdfString(nameof(SampleField)));
        }

    }

    class SampleWidget : PdfAnnotation
    {
        public SampleWidget(PdfDocument doc) : base(doc)
        {
            Initialize();
        }

        SampleWidget(SampleField field)
        {
            Debug.Assert(field.IsIndirect);

            var oldElementsFromBaseClasses = Elements;
            this.Elements = null!;
            this.Elements = field.Elements;
            foreach (var item in oldElementsFromBaseClasses)
            {
                Elements.Add(item.Key, item.Value);
            }

            // Create new reference. Same ObjectID, but different value.
            var iref = new PdfReference(field, this);
            Initialize();
        }

        void Initialize()
        {
            Elements.Add("/Iam-2", new PdfString(nameof(SampleWidget)));
        }

        public static SampleWidget CreateFromField(SampleField field)
        {
            var widget = new SampleWidget(field);
            return widget;
        }
    }
}
