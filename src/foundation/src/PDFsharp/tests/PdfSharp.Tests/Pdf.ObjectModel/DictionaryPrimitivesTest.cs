// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.PdfItemExtensions;
using PdfSharp.Quality.Testing;
using PdfSharp.Quality.Testing.TestModel;
using Xunit;

namespace PdfSharp.Tests.Pdf.ObjectModel
{
    [Collection("PDFsharp")]
    public class DictionaryPrimitivesTest : ObjectModelTestsBase
    {
        [Fact]
        public void Primitives_GetBoolean_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryPrimitivesTest), nameof(Primitives_GetBoolean_Tests));

            var dict = new PdfDictionary();
            var dict1 = new PdfDictionary();
            new ObjectModelTestHelper(rwh.Document).PopulateDictionary(dict1);
            dict.Elements.Add("/DirectDictionary", dict1);

            // Test cases that succeed for existing values.

            var b = dict1.Elements.GetBoolean("/bool");
            b.Should().BeTrue();

            b = dict1.Elements.GetBoolean("/bool-obj");
            b.Should().BeTrue();

            // Test cases that succeed for non-existing values.

            b = dict1.Elements.GetBoolean("/key-does-not-exist");
            b.Should().BeFalse();

            // Test cases that throw.

            Action getValueFromIncompatibleItem = () => dict1.Elements.GetBoolean("/real");
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Primitives_GetInteger_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryPrimitivesTest), nameof(Primitives_GetInteger_Tests));

            var dict = new PdfDictionary();
            var dict1 = new PdfDictionary();
            new ObjectModelTestHelper(rwh.Document).PopulateDictionary(dict1);
            dict.Elements.Add("/DirectDictionary", dict1);

            // Test cases that succeed for existing values.

            var i = dict1.Elements.GetInteger("/int");
            i.Should().Be(42);

            i = dict1.Elements.GetInteger("/int-obj");
            i.Should().Be(42);

            // Test cases that succeed for non-existing values.

            i = dict1.Elements.GetInteger("/key-does-not-exist");
            i.Should().Be(0);
            
            // Test cases that throw.

            Action getIntegerFromString = () => i = dict1.Elements.GetInteger("/string");
            getIntegerFromString.Should().Throw<InvalidOperationException>();

            Action getIntegerFromLong = () => i = dict1.Elements.GetInteger("/long-max");
            getIntegerFromLong.Should().Throw<InvalidOperationException>();

            Action getIntegerFromNull = () => i = dict1.Elements.GetInteger("/null");
            getIntegerFromNull.Should().Throw<InvalidOperationException>();

            Action getValueFromIncompatibleItem = () => i = dict1.Elements.GetInteger("/-.999");
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Primitives_GetLongInteger_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryPrimitivesTest), nameof(Primitives_GetLongInteger_Tests));

            var dict = new PdfDictionary();
            var dict1 = new PdfDictionary();
            new ObjectModelTestHelper(rwh.Document).PopulateDictionary(dict1);
            dict.Elements.Add("/DirectDictionary", dict1);

            // Test cases that succeed for existing values.

            var l = dict1.Elements.GetLongInteger("/int");
            l.Should().Be(42);

            l = dict1.Elements.GetLongInteger("/int-obj");
            l.Should().Be(42);

            l = dict1.Elements.GetLongInteger("/long-max");
            l.Should().Be(Int64.MaxValue);

            // Test cases that succeed for non-existing values.

            l = dict1.Elements.GetLongInteger("/key-does-not-exist");
            l.Should().Be(0);

            // Test cases that throw.

            Action getIntegerFromString = () => l = dict1.Elements.GetLongInteger("/string");
            getIntegerFromString.Should().Throw<InvalidOperationException>();

            Action getIntegerFromNull = () => l = dict1.Elements.GetLongInteger("/null");
            getIntegerFromNull.Should().Throw<InvalidOperationException>();

            Action getValueFromIncompatibleItem = () => l = dict1.Elements.GetLongInteger("/-.999");
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Primitives_GetReal_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryPrimitivesTest), nameof(Primitives_GetReal_Tests));

            var dict = new PdfDictionary();
            var dict1 = new PdfDictionary();
            new ObjectModelTestHelper(rwh.Document).PopulateDictionary(dict1);
            dict.Elements.Add("/DirectDictionary", dict1);

            // Test cases that succeed for existing values.

            var r = dict1.Elements.GetReal("/real");
            r.Should().Be(Math.PI);

            r = dict1.Elements.GetReal("/real-obj");
            r.Should().Be(Math.PI);

            // Test cases that succeed for non-existing values.

            r = dict1.Elements.GetReal("/key-does-not-exist");
            r.Should().Be(0d);

            // Test cases that throw.

            Action getValueFromIncompatibleItem = () => dict1.Elements.GetReal("/name");
            // TODO Should be IOE.
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Primitives_GetDateTime_Tests()
        {
            var now = DateTimeOffset.Now;
            var d1 = now.ToString();
            var d2 = now.ToString("zzz");

            var xx = now.Offset;

            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryPrimitivesTest), nameof(Primitives_GetDateTime_Tests));


            var dict = new PdfDictionary();
            var dict1 = new PdfDictionary();
            new ObjectModelTestHelper(rwh.Document).PopulateDictionary(dict1);
            dict.Elements.Add("/DirectDictionary", dict1);

            // === Test cases that succeed for existing values ===
            {
                var date = dict1.Elements.GetDateTime("/string-date1");
                date.Should().Be(new DateTimeOffset(1999, 12, 31, 23, 59, 59, TimeSpan.Zero));

                date = dict1.Elements.GetDateTime("/string-date2");
                date.Should().Be(new DateTimeOffset(1999, 12, 31, 23, 59, 59, new TimeSpan(2, 0, 0)));

                date = dict1.Elements.GetDateTime("/string-date3");
                date.Should().Be(new DateTimeOffset(1999, 12, 31, 23, 59, 59, -new TimeSpan(3, 0, 0)));

                date = dict1.Elements.GetDateTime("/date1");
                date.Should().Be(new DateTimeOffset(1999, 12, 31, 23, 59, 59, TimeSpan.Zero));

                date = dict1.Elements.GetDateTime("/date2");
                date.Should().Be(new DateTimeOffset(1999, 12, 31, 23, 59, 59, new TimeSpan(4, 0, 0)));

                date = dict1.Elements.GetDateTime("/date3");
                date.Should().Be(new DateTimeOffset(1999, 12, 31, 23, 59, 59, -new TimeSpan(3, 34, 0)));
            }

            // === Test cases that succeed for non-existing values ===
            {
                // Called with given defaultValue.
                var date = dict1.Elements.GetDateTime("/key-does-not-exist", new DateTimeOffset());
                date.Should().Be(new DateTimeOffset());

                // Called without specifying a defaultValue.
                date = dict1.Elements.GetDateTime("/key-does-not-exist");
                date.Should().BeNull();
            }

            // === Test cases that throw ===
            {
                // Get value from incompatible item.

                Action action = () => dict1.Elements.GetDateTime("/name");
                action.Should().Throw<InvalidOperationException>();

                action = () => dict1.Elements.GetDateTime("/string");
                action.Should().Throw<InvalidOperationException>();

                action = () => dict1.Elements.GetDateTime("/int");
                action.Should().Throw<InvalidOperationException>();

                action = () => dict1.Elements.GetDateTime("/bool");
                action.Should().Throw<InvalidOperationException>();
            }
        }

        [Fact]
        public void Primitives_GetRectangle_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryPrimitivesTest), nameof(Primitives_GetRectangle_Tests));

            var dict = new PdfDictionary();
            var dict1 = new PdfDictionary();
            new ObjectModelTestHelper(rwh.Document).PopulateDictionary(dict1);
            dict.Elements.Add("/DirectDictionary", dict1);

            // Test cases that succeed for existing values.

            var rect = dict1.Elements.GetRequiredRectangle("/rect");
            rect.X1.Should().Be(1);
            rect.X2.Should().Be(4);
            rect.Y1.Should().Be(2);
            rect.Y2.Should().Be(6);

            rect = dict1.Elements.GetRequiredRectangle("/array");
            rect.X1.Should().Be(5);
            rect.X2.Should().Be(7);
            rect.Y1.Should().Be(6);
            rect.Y2.Should().Be(8);


            // Test cases that succeed for non-existing values.

            rect = dict1.Elements.GetRequiredRectangle("/key-does-not-exist", false, new());
            rect.X1.Should().Be(0);
            rect.X2.Should().Be(0);
            rect.Y1.Should().Be(0);
            rect.Y2.Should().Be(0);


            // Test cases that throw.

            Action getValueFromIncompatibleItem = () => dict1.Elements.GetRequiredRectangle("/name");
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetSetRectangle_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryPrimitivesTest), nameof(GetSetRectangle_Tests));

            var dict = new PdfDictionary();
            var dict1 = new PdfDictionary();
            //PopulateDictionary(dict1);

            // Get direct rectangle
            {
                var rect = new PdfRectangle(1, 2, 3, 4);

                dict.Elements.Add("/rect1", rect);
                var r1 = dict.Elements.GetRequiredRectangle("/rect1");
                ReferenceEquals(rect, r1).Should().BeFalse();
                var r2 = dict.Elements.GetArray("/rect1");
                r2.Should().NotBeNull();
                Debug.Assert(r2 != null);
                r2!.Elements.Count.Should().Be(4);
            }

            // Get indirect rectangle
            {
                var rect = new PdfArray(rwh.Document, new PdfReal(1), new PdfReal(2), new PdfReal(3), new PdfReal(4));
                var ind = rect.IsIndirect;

                rwh.Document.IrefTable.Add(rect);

                dict.Elements.Add("/rect2a", rect);
                dict.Elements.Add("/rect2b", rect);
                var r1 = dict.Elements.GetRequiredRectangle("/rect2a");
                //ReferenceEquals(rect, r1).Should().BeFalse();
                var r2 = dict.Elements.GetArray("/rect2b");
                r2.Should().NotBeNull();
                r2!.Elements.Count.Should().Be(4);
            }

            // Create direct rectangle
            {
                var rect = new PdfRectangle(1, 2, 3, 4);

                var r1 = dict.Elements.GetRectangle("/rect-na", false, null);
                r1.Should().BeNull();
                dict.Elements.GetValue("/rect-na").Should().BeNull();

                var r2 = dict.Elements.GetRectangle("/rect-na", false, new(1, 2, 3, 4));
                r2.Should().NotBeNull();
                dict.Elements.GetValue("/rect-na").Should().BeNull();

                //var r3 = dict.Elements.GetRectangle2("/rect-new1", true, null);
                //dict.Elements.GetValue("/rect-new1").Should().NotBeNull();

                var r4 = dict.Elements.GetRectangle("/rect-new1", true, new(1, 2, 3, 4));
                r4.Should().NotBeNull();
                dict.Elements.GetValue("/rect-new1").Should().NotBeNull();


                //dict.Elements.Add("/rect1", rect);
                //var r1 = dict.Elements.GetRectangle("/rect1");
                //ReferenceEquals(rect, r1).Should().BeFalse();
                //var r2 = dict.Elements.GetArray("/rect1");
                //r2.Should().NotBeNull();
                //r2.Elements.Count.Should().Be(4);
            }


            // TODO Tests, where the type PdfRectangle comes from Key’s metadata

            //// Test cases that succeed for existing values.

            //var rect = dict1.Elements.GetRectangle("/rect");
            //rect.X1.Should().Be(1);
            //rect.X2.Should().Be(4);
            //rect.Y1.Should().Be(2);
            //rect.Y2.Should().Be(6);

            //rect = dict1.Elements.GetRectangle("/array");
            //rect.X1.Should().Be(5);
            //rect.X2.Should().Be(7);
            //rect.Y1.Should().Be(6);
            //rect.Y2.Should().Be(8);

            //// Test cases that succeed for non-existing values.

            //rect = dict1.Elements.GetRectangle("/key-does-not-exist");
            //rect.X1.Should().Be(0);
            //rect.X2.Should().Be(0);
            //rect.Y1.Should().Be(0);
            //rect.Y2.Should().Be(0);

            //// Test cases that throw.

            //Action getValueFromIncompatibleItem = () => dict1.Elements.GetRectangle("/name");
            //getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Primitives_GetString_GetName_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryPrimitivesTest), nameof(Primitives_GetString_GetName_Tests));

            var dict = new PdfDictionary();
            var dict1 = new PdfDictionary();
            new ObjectModelTestHelper(rwh.Document).PopulateDictionary(dict1);
            dict.Elements.Add("/DirectDictionary", dict1);

            // Test cases that succeed for existing values.

            var s = dict1.Elements.GetString("/string");
            s.Should().Be("Hello");

            s = dict1.Elements.GetString("/string-hex");
            s.Should().Be("HelloHex");
            var item1 = dict1.Elements.GetRequiredValue<PdfString>("/string-hex");
            item1.HexLiteral.Should().BeTrue();

            var n = dict1.Elements.GetName("/name");
            n.Should().Be("/Mambo #5");

            s = dict1.Elements.GetString("/string-obj");
            s.Should().Be("Hello");

            s = dict1.Elements.GetString("/string-obj-hex");
            s.Should().Be("HelloHex");
            var item2 = dict1.Elements.GetRequiredValue<PdfStringObject>("/string-obj-hex");
            item2.HexLiteral.Should().BeTrue();

            n = dict1.Elements.GetName("/name-obj");
            n.Should().Be("/Mambo #5");

            // Test cases that succeed for non-existing values.

            s = dict1.Elements.GetString("/key-does-not-exist");
            s.Should().Be("");

            n = dict1.Elements.GetName("/key-does-not-exist");
            n.Should().Be("/");

            // Test cases that throw.

            Action getValueFromIncompatibleItem = () => dict1.Elements.GetString("/int");
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();

            getValueFromIncompatibleItem = () => dict1.Elements.GetName("/int");
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Primitives_GetEnum_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryPrimitivesTest), nameof(Primitives_GetEnum_Tests));

            var dict = new PdfDictionary();
            var dict1 = new PdfDictionary();
            new ObjectModelTestHelper(rwh.Document).PopulateDictionary(dict1);
            dict.Elements.Add("/DirectDictionary", dict1);

            // Test cases that succeed for existing values.

            // Enum from integer.
            var pageLayout = dict1.Elements.GetEnum<PdfPageLayout>("/pagelayout-test");
            pageLayout.Should().Be(PdfPageLayout.TwoColumnRight);

            // Enum from name.
            pageLayout = dict1.Elements.GetEnum<PdfPageLayout>("/pagelayout-test-string");
            pageLayout.Should().Be(PdfPageLayout.TwoColumnRight);

            // Enum from integer.
            var pageMode = dict1.Elements.GetEnum<PdfPageMode>("/pagemode-test");
            pageMode.Should().Be(PdfPageMode.FullScreen);

            // Enum from name.
            pageMode = dict1.Elements.GetEnum<PdfPageMode>("/pagemode-test-string");
            pageMode.Should().Be(PdfPageMode.FullScreen);

            pageMode = dict1.Elements.GetEnum<PdfPageMode>("/real");
            pageMode.Should().Be(null);

            pageMode = dict1.Elements.GetEnum<PdfPageMode>("/string-date1");
            pageMode.Should().Be(null);

            pageMode = dict1.Elements.GetEnum<PdfPageMode>("/non-existent-item");
            pageMode.Should().Be(null);

            // Test cases that throw.
            //Action getValueFromIncompatibleItem = () => dict1.Elements.GetEnum<PdfPageMode>("/real");
            //getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();

            //Action getValueFromIncompatibleItem = () => dict1.Elements.GetEnum<PdfPageMode>("/string-date1");
            //getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();

            //Action getValueFromIncompatibleItem = () => dict1.Elements.GetEnum<PdfPageMode>("/non-existent-item");
            //getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Primitives_References_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryPrimitivesTest), nameof(Primitives_References_Tests));

            var dict = new PdfDictionary();
            var dict1 = new PdfDictionary();
            new ObjectModelTestHelper(rwh.Document).PopulateDictionary(dict1);
            dict.Elements.Add("/DirectDictionary", dict1);

            // Test cases that succeed for existing values.

            var l = dict1.Elements.GetLongInteger("/int-obj");
            l.Should().Be(42);

            var o = dict1.Elements["/int-obj"]!;
            o!.AsReference().Should().NotBeNull();
            PdfReference.Dereference(ref o);
            var i = (PdfIntegerObject)o;
            i.Value.Should().Be(42);

            o = dict1.Elements.GetValue("/int-obj")!;
            PdfReference.Dereference(ref o);
            i = (PdfIntegerObject)o;
            i.Value.Should().Be(42);

            // Test cases that throw.
            //Action getValueFromReference = () => o = dict1.Elements.GetValue("/int-obj");
            //getValueFromReference.Should().Throw<InvalidCastException>();

            //// Test cases that succeed for non-existing values.

            //l = dict1.Elements.GetLongInteger("/key-does-not-exist");
            //l.Should().Be(0);

            //// Test cases that throw.

            //Action getIntegerFromString = () => l = dict1.Elements.GetLongInteger("/string");
            //getIntegerFromString.Should().Throw<InvalidOperationException>();

            //Action getIntegerFromNull = () => l = dict1.Elements.GetLongInteger("/null");
            //getIntegerFromNull.Should().Throw<InvalidOperationException>();

            //Action getValueFromIncompatibleItem = () => l = dict1.Elements.GetLongInteger("/-.999");
            //getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }
    }
}
