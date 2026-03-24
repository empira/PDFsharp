// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Xunit;
using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.PdfItemExtensions;
using PdfSharp.Pdf.PdfDictionaryExtensions;

//using static PdfSharp.Diagnostics.DebugBreakHelper;

#pragma warning disable CS8321 // Local function is declared but never used

// TODO: DELETE

namespace PdfSharp.Tests.PdfObjectModel
{
    [Collection("PDFsharp")]
    public partial class ObjectModelTests
    {
        [Fact]
        public void TestTest()
        {
            var doc1 = CreateDocument();
            var catalog = doc1.Internals.Catalog;

            var dict3 = new TestDict3(doc1);
            dict3.Comment = "Dict3";
            doc1.Internals.AddObject(dict3);
            catalog.Elements.Add("/TestStuff3", dict3);

            // Save and reload.
            var path = Save(doc1, nameof(TestTest) + "1");
            var doc2 = Load(path);
            var cat2 = doc2.Internals.Catalog;

            var dict32Ref = (PdfReference?)cat2.Elements["/TestStuff3"];

            //var abc = PdfObjectsHelper.TransformDictionary<TestDict3>((PdfDictionary)(dict32Ref!.Value));

            //var x = dict32Ref.Value.AsDictionary().Elements[TestDict3.Keys.TestDict1];
            //var xt = x.GetType();
            var y = dict32Ref!.Value.AsDictionary().Elements.GetValue(TestDict3.Keys.TestDict1);
            var z = dict32Ref.Value.AsDictionary().Elements.GetValue(TestDict3.Keys.TestDict1Ref);
            var z2 = dict32Ref.Value.AsDictionary().Elements.GetValue(TestDict3.Keys.TestDict1Ref);

            //var xxx = dict32.Elements.GetItemNew(
            //    TestDict3.Keys.TestDict1, ObjMagic.Default, typeof(TestDict1));

            var path2 = Save(doc2, nameof(TestTest) + "2");
        }

        [Fact]
        public void Null_Objects_Test()
        {
            var doc1 = CreateDocument();
            var cat1 = doc1.Internals.Catalog;

            cat1.Elements["/NullItem"] = PdfNull.Value;

            //var nullObject1 = new PdfNullObject();
            //cat1.Elements["/NullDirectObject"] = nullObject1;

            var nullObject2 = new PdfNullObject();
            doc1.Internals.AddObject(nullObject2);
            cat1.Elements["/NullIndirectObject"] = nullObject2.RequiredReference;

            cat1.Elements["/undefRef"] = new PdfDebugItem(" 42000 0 R");

            CheckParentInfoConsistency(cat1);

            // Save and reload.
            var path = Save(doc1, nameof(Null_Objects_Test) + "1");
            //ShouldBreak2 = true;
            var doc2 = Load(path);
            //ShouldBreak2 = false;
            var cat2 = doc2.Internals.Catalog;

            cat2.Elements["/NullItem"].Should().Be(PdfNull.Value);

            // PDFsharp reads 'null' as PdfNull, not as PdfNullObject.
            //cat2.Elements["/NullDirectObject"]!.GetType().Should().Be(typeof(PdfNull));

            // PdfNullObject is only read when it is an indirect object.
            cat2.Elements["/NullIndirectObject"]!.GetType().Should().Be(typeof(PdfReference));
            cat2.Elements["/NullIndirectObject"].AsReference().Value.GetType().Should().Be(typeof(PdfNullObject));

            // An undefined reference is read as null object.
            cat2.Elements["/undefRef"]!.Should().Be(PdfNull.Value);

            var pages1a = cat2.Elements["/Pages"].AsReference().Value.AsDictionary();
            //var pages1b = cat2.GetItem("/Pages").AsReference().Value.AsDictionary();
            //var pages1c = cat2.GetItem("/Pages").AsDictionary();
            //ReferenceEquals(pages1a, pages1b).Should().BeTrue();
            //ReferenceEquals(pages1a, pages1c).Should().BeTrue();
            var pages2 = cat2.Elements["/Pages"].AsReference().AsDictionary();
            var pages3 = cat2.Elements["/Pages"].AsDictionary();
            var k = pages3.Elements["/Kids"];
            var kids = pages3.Elements["/Kids"].AsArray();
            kids.IsIndirect.Should().BeFalse();
            kids.ParentInfo.Should().NotBeNull();
            kids.IsDead.Should().BeFalse();

            //ShouldBreak1 = false;

            var path2 = Save(doc2, nameof(Null_Objects_Test) + "2");
        }

        [Fact]
        public void Throw_on_used_object_Test()
        {
            var doc1 = CreateDocument();
            var cat1 = doc1.Internals.Catalog;

            var dict1 = new TestDict1();

            cat1.Elements["/DirectObject"] = dict1;

            // Must not add direct object more than once.
            var addTwice = () => cat1.Elements["/DirectObjectTwice"] = dict1;
            addTwice.Should().Throw<InvalidOperationException>();

            // Must not convert used direct object to indirect object.
            var makeIndirect = () => doc1.Internals.AddObject(dict1);
            makeIndirect.Should().Throw<InvalidOperationException>();

            // Save and reload.
            var path = Save(doc1, nameof(Throw_on_used_object_Test) + "1");
            var doc2 = Load(path);
            var cat2 = doc2.Internals.Catalog;

            var dict21 = cat2.Elements["/DirectObject"];
            var dict22 = cat2.Elements.GetValue("/DirectObject");


            var path2 = Save(doc2, nameof(Throw_on_used_object_Test) + "2");



            // Make used direct object indirect.

            // Set indirect object as direct one.

            // Ensure alive test

            // 2 reference to same object.
        }

        [Fact]
        public void Throw_on_dead_object_Test()
        {
            var doc1 = CreateDocument();
            var cat1 = doc1.Internals.Catalog;

            var dict3 = new TestDict3(doc1);
            doc1.Internals.AddObject(dict3);
            cat1.Elements["/IndirectObject3"] = dict3;

            // Save and reload.
            var path = Save(doc1, nameof(Throw_on_dead_object_Test) + "1");
            var doc2 = Load(path);
            var cat2 = doc2.Internals.Catalog;

            var dict32 = cat2.Elements["/IndirectObject3"];

            var b1 = ((PdfReference)dict32!).AsDictionary();
            ////var b2 = PdfObjectsHelper.TransformDictionary<TestDict3>(b1);
            ////var abc = PdfObjectsHelper.TransformDictionary<TestDict3>(((PdfReference)dict32!).AsDictionary());

            //var a1 = abc.Elements[TestDict3.Keys.TestDict1Ref];
            //var a2 = a1.AsReference();
            //var a3 = a2.Value;

            //var dict1Base = abc.Elements[TestDict3.Keys.TestDict1Ref].AsReference().Value;
            //dict1Base.IsDead.Should().BeFalse();
            //var dict1Der = abc.Elements.GetValue(TestDict3.Keys.TestDict1Ref);
            //dict1Base.IsDead.Should().BeTrue();

            //var d1 = dict1Base.IsDead;
            ////var d2 = dict1Base.IsDead2;
            ////var d3 = dict1Base.IsDead3;
            ////  var elements = dict1Base.As<PdfDictionary>().Elements;

            //var test1 = () => _ = dict1Base.AsDictionary().Elements;
            //test1.Should().Throw<InvalidOperationException>();

            //var addDeadObject = () => cat2.Elements.Add("/TestDeadObject", dict1Base);
            //addDeadObject.Should().Throw<InvalidOperationException>();

            ////var reference = new PdfReference(cat2, PdfObjectID.Empty, 42);

            //var path2 = Save(doc2, nameof(Throw_on_used_object_Test) + "2");
        }

        [Fact]
        public void Throw_on_direct_non_containers_Test()
        {
            var doc = CreateDocument();
            var cat = doc.Internals.Catalog;

            var addDirectObject = () => cat.Elements.Add("/test", new PdfIntegerObject(42));
            addDirectObject.Should().Throw<InvalidOperationException>();

            var path2 = Save(doc, nameof(Throw_on_direct_non_containers_Test) + "2");
        }

        [Fact]
        public void Basic_PdfDictionary_Test()
        {
            var doc = CreateDocument();
            var catalog = doc.Internals.Catalog;

            var dict1 = new TestDict1();
            catalog.Elements.Add("/TestStuff1", dict1);
            //catalog.Elements.Add("/TestStuff1a", dict1); now fails
            var addDirectObjectTwice = () => catalog.Elements.Add("/TestStuff1a", dict1);
            addDirectObjectTwice.Should().Throw<InvalidOperationException>();

            dict1.Elements.Add("/dummy", new PdfString("Hello"));

            var someInt = new PdfInteger(42);
            catalog.Elements.Add("/SomeInt1", someInt);
            catalog.Elements.Add("/SomeInt1a", someInt);


            var dict2 = new TestDict2();
            doc.Internals.AddObject(dict2);
            catalog.Elements.Add("/TestStuff2", dict2);

            var dict3 = new TestDict3(doc);
            doc.Internals.AddObject(dict3);
            catalog.Elements.Add("/TestStuff3", dict3);



            var path = Save(doc, nameof(Basic_PdfDictionary_Test));
            var doc2 = Load(path);


            var cat2 = doc2.Internals.Catalog;

            var dict32Ref = (PdfReference?)cat2.Elements["/TestStuff3"];
            //var dict32 = (PdfDictionary?)cat2.Elements["/TestStuff3"];
            //var dict32 = PdfReference.Dereference<PdfDictionary>(dict32Ref!);
            var dict32 = cat2.Elements["/TestStuff3"].AsDictionary();

            var dict1Dir = dict32.Elements[TestDict3.Keys.TestDict1];
            var dict1Ind = dict32.Elements[TestDict3.Keys.TestDict1Ref];
        }

        [Fact]
        public void Basic_PdfArray_Test()
        {
            var doc = CreateDocument();
            var catalog = doc.Internals.Catalog;

            var dict1 = new TestDict1();
            var dict2 = new TestDict1();
            var array1 = new PdfArray();
            catalog.Elements.Add("/Array1", array1);
            array1.Elements.Add(dict1);
            array1.Elements.Add(dict2);

            dict1 = new TestDict1();
            dict2 = new TestDict1();
            var array2 = new PdfArray();
            doc.Internals.AddObject(array2);
            catalog.Elements.Add("/Array2", array2);
            array2.Elements.Add(dict1);
            array2.Elements.Add(dict2);

            var path = Save(doc, nameof(Basic_PdfArray_Test));
            var doc2 = Load(path);
        }

        [Fact]
        public void Reuse_of_direct_object_Test()
        {
            //var item = new PdfStringObject("abc", PdfStringEncoding.Unicode);
            var item = new PdfDictionary();

            var useDirectObjectTwice = () =>
            {
                var dict = new TestDict1
                {
                    Elements =
                    {
                        ["Key1"] = item,
                        ["Key2"] = item
                    }
                };
            };
            useDirectObjectTwice.Should().Throw<InvalidOperationException>();
        }
    }
}
