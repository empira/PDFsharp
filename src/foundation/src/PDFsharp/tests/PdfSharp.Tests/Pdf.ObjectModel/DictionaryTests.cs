// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Quality.Testing;
using PdfSharp.Quality.Testing.TestModel;
using Xunit;

namespace PdfSharp.Tests.Pdf.ObjectModel
{
    [Collection("PDFsharp")]
    public class DictionaryTests : ObjectModelTestsBase
    {
        [Fact]
        public void ToDo()
        {
        }

        [Fact]
        public void Test_cloning()
        {
        }

        [Fact]
        public void Test_basic_type_transformation()
        {
            // Indirect object.

            // Direct object.
        }

        [Fact]
        public void Dictionaries_with_streams_must_be_indirect()
        {
        }

        //[Fact]
        //public void TestDictionaryApi()
        //{
        //    var rwh = new ReadWriteHelper();
        //    rwh.CreateDocument(typeof(DictionaryTests), nameof(TestDictionaryApi));
        //    var dict = new PdfDictionary();
        //    var dict1 = new TestDict1();
        //    dict.CloneElementsOf(dict1);
        //    var c = dict.Elements.Count;

        //    rwh.Document.Catalog.Elements.Add("/test", dict);
        //    rwh.Save();
        //    rwh.Reload();
        //    rwh.ReloadedDocument.Catalog.Elements.GetRequiredValue<PdfDictionary>("/test").Count().Should().Be(1);
        //}

        [Fact]
        public void Reuse_of_direct_object_correctly()
        {
            // === Test direct array reuse after remove ===
            {
                var array = new TestArray1();
                array.IsIndirect.Should().BeFalse();
                array.ParentInfo.Should().BeNull();

                var owningDict = new PdfDictionary()
                {
                    Elements = { ["Key"] = array }
                };
                array.ParentInfo.Should().NotBeNull();
                array.ParentInfo!.OwningDictionary.Equals(owningDict).Should().BeTrue();
                owningDict.Elements.Count.Should().Be(1);

                owningDict.Elements.Remove("Key");
                array.ParentInfo.Should().BeNull();
                array.IsDead.Should().BeFalse();
                owningDict.Elements.Count.Should().Be(0);

                // array can be reused after removing.
                owningDict.Elements["Key"] = array;
                array.ParentInfo.Should().NotBeNull();
                array.ParentInfo!.OwningDictionary.Equals(owningDict).Should().BeTrue();
                owningDict.Elements.Count.Should().Be(1);
            }

            // === Test direct dictionary reuse after remove ===
            {
                var dict = new TestDict1();
                dict.IsIndirect.Should().BeFalse();
                dict.ParentInfo.Should().BeNull();

                var owningDict = new PdfDictionary()
                {
                    Elements = { ["Key"] = dict }
                };
                dict.ParentInfo.Should().NotBeNull();
                dict.ParentInfo!.OwningDictionary.Equals(owningDict).Should().BeTrue();
                owningDict.Elements.Count.Should().Be(1);

                owningDict.Elements.Remove("Key");
                dict.ParentInfo.Should().BeNull();
                dict.IsDead.Should().BeFalse();
                owningDict.Elements.Count.Should().Be(0);

                // dict can be reused after removing.
                owningDict.Elements["Key"] = dict;
                dict.ParentInfo.Should().NotBeNull();
                dict.ParentInfo!.OwningDictionary.Equals(owningDict).Should().BeTrue();
                owningDict.Elements.Count.Should().Be(1);
            }

            // === Test self replacement ===
            {
                var dict = new TestDict1();
                var owningDict = new PdfDictionary
                {
                    Elements =
                    {
                        ["Key"] = dict,
                        // Replace by itself is senseless but must work.
                        ["Key"] = dict
                    }
                };
                dict.ParentInfo.Should().NotBeNull();
                dict.ParentInfo!.OwningDictionary.Equals(owningDict).Should().BeTrue();
                owningDict.Elements.Count.Should().Be(1);
            }
        }

        [Fact]
        public void Illegal_reuse_of_direct_object()
        {
            // === Test array must not be used twice ===
            {
                var array = new TestArray1();
                array.IsIndirect.Should().BeFalse();
                var action = () =>
                {
                    var _ = new TestDict1
                    {
                        Elements =
                        {
                            ["Key1"] = array,
                            ["Key2"] = array
                        }
                    };
                };
                action.Should().Throw<InvalidOperationException>();
            }

            // === Test dict must not be used twice ===
            {
                var dict = new TestDict1();
                dict.IsIndirect.Should().BeFalse();
                var action = () =>
                {
                    var _ = new TestDict1
                    {

                        Elements =
                        {
                            ["Key1"] = dict,
                            ["Key2"] = dict
                        }
                    };
                };
                action.Should().Throw<InvalidOperationException>();
            }

            // === Test dict must not be used twice ===
            {
                var dict = new TestDict1();

                var action = () =>

                {
                    var _1 = new TestDict1
                    {

                        Elements =
                        {
                            ["Key"] = dict,
                        }
                    };
                    var _2 = new TestDict1
                    {

                        Elements =
                        {
                            ["Key"] = dict
                        }
                    };
                };
                action.Should().Throw<InvalidOperationException>();
            }
        }

        [Fact]
        public void Illegal_direct_use_of_primitive_object()
        {
            // === Test primitive objects must not be direct ===
            {
                // Primitive objects (non-container) like PdfStringObject or
                // PdfIntegerObject must not be used as direct objects.
                var str = new PdfStringObject();
                str.IsIndirect.Should().BeFalse();
                var action = () =>
                {
                    var _ = new TestDict1
                    {
                        Elements =
                        {
                            ["Key1"] = str  // Use PdfString instead.
                        }
                    };
                };
                action.Should().Throw<InvalidOperationException>();
            }
        }

        [Fact]
        public void Test_multiple_transformation()
        {
            const string someKey = "/SomeKey";

            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryTests), nameof(Test_multiple_transformation));
            var catalog = rwh.Document.Catalog;

            var dict1 = new PdfDictionary();
            rwh.Document.Internals.AddObject(dict1);

            catalog.Elements.Add(someKey, dict1);

            var value0 = catalog.Elements.GetRequiredValue(someKey);
            value0.GetType().Name.Should().Be(nameof(PdfDictionary));
            ReferenceEquals(dict1, value0).Should().BeTrue();

            var value1 = catalog.Elements.GetRequiredValue<PdfDictionary>(someKey);
            value1.GetType().Name.Should().Be(nameof(PdfDictionary));
            ReferenceEquals(value1, value0).Should().BeTrue();

            var value2 = catalog.Elements.GetRequiredValue<TestBaseDict>(someKey);
            value2.GetType().Name.Should().Be(nameof(TestBaseDict));
            value1.IsDead.Should().BeTrue();

            var value3 = catalog.Elements.GetRequiredValue<TestDerivedDict>(someKey);
            value3.GetType().Name.Should().Be(nameof(TestDerivedDict));
            value2.IsDead.Should().BeTrue();

            value2 = catalog.Elements.GetRequiredValue<TestBaseDict>(someKey);
            value2.GetType().Name.Should().Be(nameof(TestDerivedDict));
        }

        [Fact]
        public void Test_DictionaryElements_API()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryTests), nameof(Test_DictionaryElements_API));
            var catalog = rwh.Document.Catalog;

            var dict1 = new ApiTestDict1();
            rwh.Document.Internals.AddObject(dict1);
            catalog.Elements.Add("/TestDict1", dict1);

            dict1.Elements[ApiTestDict1.Keys.SomeBoolean] = new PdfBoolean(true);
            dict1.Elements.GetBoolean(ApiTestDict1.Keys.SomeBoolean).Should().BeTrue();
            // TryGetBoolean

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

            rwh.Save(nameof(Test_DictionaryElements_API));
            rwh.Reload();
        }
    }
}
