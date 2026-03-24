// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Quality.Testing.TestModel;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.Pdf.ObjectModel
{
    [Collection("PDFsharp")]
    public class ArrayTests : ObjectModelTestsBase
    {
        //[Fact]
        //public void BugWithNumbers()
        //{
        //    CreateDocument(nameof(BugWithNumbers));
        //    var catalog = Document.Catalog;

        //    var dict1 = new TestDict1();
        //    Document.Internals.AddObject(dict1);
        //    catalog.Elements.Add("/TestDict1", dict1);

        //    // 
        //    {
        //        var item0 = catalog.Elements.GetRequiredValue("/TestDict1");
        //        item0.GetType().Name.Should().Be(nameof(TestDict1));

        //        var item1 = catalog.Elements.GetRequiredValue("/TestDict1", VCF.None, typeof(PdfDictionary));
        //        item1.GetType().Name.Should().Be(nameof(TestDict1));

        //        var item2 = catalog.Elements.GetRequiredValue("/TestDict1", VCF.None, typeof(PdfReference));
        //        item2.GetType().Name.Should().Be(nameof(PdfReference));
        //        ReferenceEquals(((PdfReference)item2).Value, item0).Should().BeTrue();
        //    }

        //}

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
        public void Test_index_adjustment_in_StructParent()
        {
            // Insert object.

            // Remove object.
        }

        //[Fact]
        //public void TestDictionaryApi()
        //{
        //    CreateDocument("InitialTest");
        //    var dict = new PdfDictionary();
        //    var dict1 = new TestDict1();
        //    dict.CloneElementsOf(dict1);
        //    var c = dict.Elements.Count;

        //    Document.Catalog.Elements.Add("/test", dict);
        //    Save();
        //    Reload();
        //    ReloadedDocument.Catalog.Elements.GetRequiredValue<PdfDictionary>("/test").Count().Should().Be(1);
        //}

        [Fact]
        public void Reuse_of_array_object_correctly()
        {
            // === Test direct array reuse after remove ===
            {
                var array = new TestArray1();
                array.IsIndirect.Should().BeFalse();
                array.ParentInfo.Should().BeNull();

                var owningDict = new PdfArray()
                {
                    Elements = { [0] = array }
                };
                array.ParentInfo.Should().NotBeNull();
                array.ParentInfo!.OwningArray.Equals(owningDict).Should().BeTrue();
                // owning array not set
                owningDict.Elements.Count.Should().Be(1);

                owningDict.Elements.RemoveAt(0);
                array.ParentInfo.Should().BeNull();
                array.IsDead.Should().BeFalse();
                owningDict.Elements.Count.Should().Be(0);

                // array can be reused after removing.
                owningDict.Elements[0] = array;
                array.ParentInfo.Should().NotBeNull();
                array.ParentInfo!.OwningArray.Equals(owningDict).Should().BeTrue();
                owningDict.Elements.Count.Should().Be(1);
            }

            // === Test direct dictionary reuse after remove ===
            {
                var dict = new TestDict1();
                dict.IsIndirect.Should().BeFalse();
                dict.ParentInfo.Should().BeNull();

                var owningArray = new PdfArray()
                {
                    Elements = { [0] = dict }
                };
                dict.ParentInfo.Should().NotBeNull();
                dict.ParentInfo!.OwningArray.Equals(owningArray).Should().BeTrue();
                owningArray.Elements.Count.Should().Be(1);

                owningArray.Elements.RemoveAt(0);
                dict.ParentInfo.Should().BeNull();
                dict.IsDead.Should().BeFalse();
                owningArray.Elements.Count.Should().Be(0);

                // dict can be reused after removing.
                owningArray.Elements[0] = dict;
                dict.ParentInfo.Should().NotBeNull();
                dict.ParentInfo!.OwningArray.Equals(owningArray).Should().BeTrue();
                owningArray.Elements.Count.Should().Be(1);
            }

            // === Test self replacement ===
            {
                var dict = new TestDict1();
                var owningArray = new PdfArray
                {
                    Elements =
                    {
                        [0] = dict,
                        // Replace by itself is senseless but must work.
                        [0] = dict
                    }
                };
                dict.ParentInfo.Should().NotBeNull();
                dict.ParentInfo!.OwningArray.Equals(owningArray).Should().BeTrue();
                owningArray.Elements.Count.Should().Be(1);
            }
        }

        [Fact]
        public void Illegal_reuse_of_direct_object()
        {
            // === TestArray1 must not be used twice in array ===
            {
                var array = new TestArray1();
                array.IsIndirect.Should().BeFalse();
                var action = () =>
                {
                    var _ = new TestArray1
                    {
                        Elements =
                        {
                            [0] = array,
                            [1] = array
                        }
                    };
                };
                action.Should().Throw<InvalidOperationException>();
            }

            // === TestDict1 must not be used twice in array ===
            {
                var dict = new TestDict1();
                dict.IsIndirect.Should().BeFalse();
                var action = () =>
                {
                    var _ = new TestArray1
                    {

                        Elements =
                        {
                            [0] = dict,
                            [1] = dict
                        }
                    };
                };
                action.Should().Throw<InvalidOperationException>();
            }

            // === Test array must not be used twice ===
            {
                var dict = new TestDict1();
                var action = () =>

                {
                    var _1 = new TestArray1
                    {

                        Elements =
                        {
                            [0] = dict,
                        }
                    };
                    var _2 = new TestArray1
                    {

                        Elements =
                        {
                            [0] = dict  // Must throw because of reuse of direct container.
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
                // Primitive objects (non-containers) like PdfStringObject or
                // PdfIntegerObject must not be used as direct objects.
                var str = new PdfStringObject();
                str.IsIndirect.Should().BeFalse();
                var action = () =>
                {
                    var _ = new TestArray1
                    {
                        Elements =
                        {
                            [0] = str  // Use PdfString instead.
                        }
                    };
                };
                action.Should().Throw<InvalidOperationException>();
            }
        }

        //[Fact]
        //public void Test_ArrayElements_API()
        //{
        //}
    }
}
