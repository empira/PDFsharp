// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Quality.Testing;
using PdfSharp.Quality.Testing.TestModel;
using Xunit;

namespace PdfSharp.Tests.Pdf.ObjectModel
{
    [Collection("PDFsharp")]
    public class DictionaryItemTests : ObjectModelTestsBase
    {
        [Fact]
        public void DealingWithIndirectObjects()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryItemTests),nameof(DealingWithIndirectObjects));
            var catalog = rwh.Document.Catalog;

            var dict1 = new TestDict1();
            rwh.Document.Internals.AddObject(dict1);
            catalog.Elements.Add("/TestDict1", dict1);

            // 
            {
                var item0 = catalog.Elements.GetRequiredValue("/TestDict1");
                item0.GetType().Name.Should().Be(nameof(TestDict1));

                var item1 = catalog.Elements.GetRequiredValue("/TestDict1", VCF.None, typeof(PdfDictionary));
                item1.GetType().Name.Should().Be(nameof(TestDict1));

                var item2 = catalog.Elements.GetRequiredValue("/TestDict1", VCF.None, typeof(PdfReference));
                item2.GetType().Name.Should().Be(nameof(PdfReference));
                ReferenceEquals(((PdfReference)item2).Value, item0).Should().BeTrue();
            }

            // 
            {
                var item10 = catalog.Elements.GetRequiredValue<PdfDictionary>("/TestDict1", VCF.None);
                item10.GetType().Name.Should().Be(nameof(TestDict1));

                var item11 = catalog.Elements.GetRequiredValue<TestDict1>("/TestDict1", VCF.None);
                item11.GetType().Name.Should().Be(nameof(TestDict1));

                var item12 = catalog.Elements.GetRequiredValue<PdfReference>("/TestDict1", VCF.None);
                item12.GetType().Name.Should().Be(nameof(PdfReference));
                ReferenceEquals(((PdfReference)item12).Value, item10).Should().BeTrue();
            }

            // Do not create
            {
                catalog.Elements.Remove("/TestDict1");

                var item0 = catalog.Elements.GetValue("/TestDict1");
                item0.Should().BeNull();

                var item1 = catalog.Elements.GetValue("/TestDict1", VCF.None, typeof(PdfDictionary));
                item1.Should().BeNull();

                var item2 = catalog.Elements.GetValue("/TestDict1", VCF.None, typeof(PdfReference));
                item2.Should().BeNull();
            }

            // Create
            {
                catalog.Elements.Remove("/TestDict1");

                var item0 = catalog.Elements.GetRequiredValue("/TestDict1", VCF.CreateIndirect, typeof(TestDict1));
                item0.GetType().Name.Should().Be(nameof(TestDict1));

                catalog.Elements.Remove("/TestDict1");

                var item1 = catalog.Elements.GetRequiredValue<PdfDictionary>("/TestDict1", VCF.CreateIndirect);
                item1.GetType().Name.Should().Be(nameof(PdfDictionary));

                var item2 = catalog.Elements.GetRequiredValue<TestDict1>("/TestDict1", VCF.CreateIndirect);
                item2.GetType().Name.Should().Be(nameof(TestDict1));
                item1.IsDead.Should().BeTrue();

                catalog.Elements.Remove("/TestDict1");

                var item3 = catalog.Elements.GetReference("/TestDict1");
                item3.Should().BeNull();

                catalog.Elements.Remove("/TestDict1");

                //var item4 = catalog.Elements.GetValue("/TestDict1", VCF.CreateIndirect, typeof(PdfReference));
                //item0.GetType().Name.Should().Be(nameof(TestDict1));


                //item3.GetType().Name.Should().Be(nameof(PdfReference));
                //item3.Value.GetType().Name.Should().Be(nameof(TestDict1))
                //ReferenceEquals(((PdfReference)item2).Value, item0).Should().BeTrue();
            }
        }

        [Fact]
        public void Test_PdfNull_and_PdfNullObject()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryItemTests), nameof(Test_PdfNull_and_PdfNullObject));
            var catalog = rwh.Document.Catalog;

            var dict1 = new TestDict1();
            rwh.Document.Internals.AddObject(dict1);
            catalog.Elements.Add("/TestDict1", dict1);

            var nullObject = new PdfNullObject(rwh.Document);

            dict1.Elements.Add("/NullItem", PdfNull.Value);
            dict1.Elements.Add("/NullObject", nullObject);

            var item1 = dict1.Elements.GetValue("/NullItem");
            var item2 = dict1.Elements.GetValue("/NullObject");


            rwh.Save(nameof(Test_PdfNull_and_PdfNullObject));
            var docReload = rwh.Reload();
            catalog = docReload.Catalog;
            dict1 = catalog.Elements.GetRequiredDictionary<TestDict1>("/TestDict1");
        }

        [Fact]
        public void ToDo()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryItemTests), nameof(ToDo));
            var catalog = rwh.Document.Catalog;

            var dict1 = new ApiTestDict1();
            rwh.Document.Internals.AddObject(dict1);
            catalog.Elements.Add("/TestDict1", dict1);

            dict1.Elements[ApiTestDict1.Keys.SomeBoolean] = new PdfBoolean(true);
            dict1.Elements.GetBoolean(ApiTestDict1.Keys.SomeBoolean).Should().BeTrue();
            // TryGetBoolean

            var bool1 = dict1.Elements.GetValue(ApiTestDict1.Keys.SomeBoolean);
            var bool2 = dict1.Elements.GetValue<PdfBoolean>(ApiTestDict1.Keys.SomeBoolean);


            dict1.Elements.Remove(ApiTestDict1.Keys.SomeBoolean);
            dict1.Elements[ApiTestDict1.Keys.SomeBoolean].Should().BeNull();

            bool1 = dict1.Elements.GetValue(ApiTestDict1.Keys.SomeBoolean);
            bool1.Should().BeNull();

            bool1 = dict1.Elements.GetValue(ApiTestDict1.Keys.SomeBoolean, VCF.Create);
            bool1.Should().NotBeNull();

            dict1.Elements.Remove(ApiTestDict1.Keys.SomeBoolean);
            // TODO fail bool1 = dict1.Elements.GetValue(ApiTestDict1.Keys.SomeBoolean, VCF.CreateIndirect);


            dict1.Elements[ApiTestDict1.Keys.SomeInteger] = new PdfInteger(7);
            dict1.Elements.GetInteger(ApiTestDict1.Keys.SomeInteger).Should().Be(7);

            // ...

            // ----- GetArray -----

            // ----- GetDictionary -----

            var someDirectDict = dict1.Elements.GetDictionary(ApiTestDict1.Keys.SomeDirectDict)!;
            someDirectDict.Should().BeNull();

            someDirectDict = dict1.Elements.GetDictionary(ApiTestDict1.Keys.SomeDirectDict, VCF.Create)!;
            someDirectDict.Should().NotBeNull();
            someDirectDict.IsIndirect.Should().BeFalse();

            var someIndirectDirectDict = dict1.Elements.GetDictionary(ApiTestDict1.Keys.SomeIndirectDict)!;
            someIndirectDirectDict.Should().BeNull();

            someIndirectDirectDict = dict1.Elements.GetDictionary(ApiTestDict1.Keys.SomeIndirectDict, VCF.CreateIndirect)!;
            someIndirectDirectDict.Should().NotBeNull();
            someIndirectDirectDict.IsIndirect.Should().BeTrue();

            rwh.Save(nameof(ToDo));
            rwh.Reload();
        }
    }
}
