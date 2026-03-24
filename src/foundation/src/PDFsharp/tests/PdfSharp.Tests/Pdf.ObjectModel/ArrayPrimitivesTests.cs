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
    public class ArrayPrimitivesTests : ObjectModelTestsBase
    {
        [Fact]
        public void GetBoolean_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryTests), nameof(GetBoolean_Tests));
            
            var dict = new PdfDictionary();
            var array1 = new PdfArray();
            new ObjectModelTestHelper(rwh.Document).PopulateArray(array1);
            dict.Elements.Add("/DirectDictionary", array1);

            // Test cases that succeed for existing values.

            var b = array1.Elements.GetBoolean(1);
            b.Should().BeTrue();

            b = array1.Elements.GetBoolean(16);
            b.Should().BeTrue();

            // Test cases that throw.

            Action getValueFromIncompatibleItem = () => array1.Elements.GetBoolean(0);
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();

            getValueFromIncompatibleItem = () => array1.Elements.GetBoolean(2);
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetInteger_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryTests), nameof(GetInteger_Tests));


            var dict = new PdfDictionary();
            var array1 = new PdfArray();
            new ObjectModelTestHelper(rwh.Document).PopulateArray(array1);
            dict.Elements.Add("/DirectDictionary", array1);

            // Test cases that succeed for existing values.

            var i = array1.Elements.GetInteger(2);
            i.Should().Be(42);

            i = array1.Elements.GetInteger(17);
            i.Should().Be(42);

            // Test cases that throw.

            Action getIntegerFromString = () => i = array1.Elements.GetInteger(10);
            getIntegerFromString.Should().Throw<InvalidOperationException>();

            Action getIntegerFromLong = () => i = array1.Elements.GetInteger(6);
            getIntegerFromLong.Should().Throw<InvalidOperationException>();

            Action getIntegerFromNull = () => i = array1.Elements.GetInteger(0);
            getIntegerFromNull.Should().Throw<InvalidOperationException>();

            Action getValueFromIncompatibleItem = () => i = array1.Elements.GetInteger(11);
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetLongInteger_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryTests), nameof(GetLongInteger_Tests));

            var dict = new PdfDictionary();
            var array1 = new PdfArray();
            new ObjectModelTestHelper(rwh.Document).PopulateArray(array1);
            dict.Elements.Add("/DirectDictionary", array1);

            // Test cases that succeed for existing values.

            var l = array1.Elements.GetLongInteger(2);
            l.Should().Be(42);

            l = array1.Elements.GetLongInteger(17);
            l.Should().Be(42);

            l = array1.Elements.GetLongInteger(6);
            l.Should().Be(Int64.MaxValue);

            // Test cases that throw.

            Action getIntegerFromString = () => l = array1.Elements.GetLongInteger(10);
            getIntegerFromString.Should().Throw<InvalidOperationException>();

            Action getIntegerFromNull = () => l = array1.Elements.GetLongInteger(0);
            getIntegerFromNull.Should().Throw<InvalidOperationException>();

            Action getValueFromIncompatibleItem = () => l = array1.Elements.GetInteger(11);
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetReal_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryTests), nameof(GetReal_Tests));

            var dict = new PdfDictionary();
            var array1 = new PdfArray();
            new ObjectModelTestHelper(rwh.Document).PopulateArray(array1);
            dict.Elements.Add("/DirectDictionary", array1);

            // Test cases that succeed for existing values.

            var r = array1.Elements.GetReal(7);
            r.Should().Be(Math.PI);

            r = array1.Elements.GetReal(8);
            r.Should().Be(Single.MinValue);

            r = array1.Elements.GetReal(9);
            r.Should().Be(Single.MaxValue);

            r = array1.Elements.GetReal(14);
            r.Should().Be(-1);

            r = array1.Elements.GetReal(22);
            r.Should().Be(Math.PI);

            r = array1.Elements.GetReal(23);
            r.Should().Be(Single.MinValue);

            r = array1.Elements.GetReal(24);
            r.Should().Be(Single.MaxValue);

            // Test cases that throw.

            Action getValueFromIncompatibleItem = () => array1.Elements.GetReal(0);
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();

            getValueFromIncompatibleItem = () => array1.Elements.GetReal(11);
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();

            getValueFromIncompatibleItem = () => array1.Elements.GetReal(15);
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetNullableReal_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryTests), nameof(GetNullableReal_Tests));

            var dict = new PdfDictionary();
            var array1 = new PdfArray();
            new ObjectModelTestHelper(rwh.Document).PopulateArray(array1);
            dict.Elements.Add("/DirectDictionary", array1);

            // Test cases that succeed for existing values.

            var r = array1.Elements.GetNullableReal(7);
            r.Should().Be(Math.PI);

            r = array1.Elements.GetNullableReal(8);
            r.Should().Be(Single.MinValue);

            r = array1.Elements.GetNullableReal(9);
            r.Should().Be(Single.MaxValue);

            r = array1.Elements.GetNullableReal(14);
            r.Should().Be(-1);

            r = array1.Elements.GetNullableReal(22);
            r.Should().Be(Math.PI);

            r = array1.Elements.GetNullableReal(23);
            r.Should().Be(Single.MinValue);

            r = array1.Elements.GetNullableReal(24);
            r.Should().Be(Single.MaxValue);

            r = array1.Elements.GetNullableReal(0);
            r.Should().BeNull();

            r = array1.Elements.GetNullableReal(15);
            r.Should().BeNull();

            // Test cases that throw.

            Action getValueFromIncompatibleItem = () => array1.Elements.GetNullableReal(11);
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetString_GetName_Tests()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(DictionaryTests), nameof(GetString_GetName_Tests));

            var dict = new PdfDictionary();
            var array1 = new PdfArray();
            new ObjectModelTestHelper(rwh.Document).PopulateArray(array1);
            dict.Elements.Add("/DirectDictionary", array1);

            // Test cases that succeed for existing values.

            var s = array1.Elements.GetString(10);
            s.Should().Be("Hello");

            var n = array1.Elements.GetName(11);
            n.Should().Be("/Mambo #5");

            s = array1.Elements.GetString(25);
            s.Should().Be("Hello");

            n = array1.Elements.GetName(26);
            n.Should().Be("/Mambo #5");


            // Test cases that throw.

            Action getValueFromIncompatibleItem = () => array1.Elements.GetString(2);
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();

            getValueFromIncompatibleItem = () => array1.Elements.GetName(2);
            getValueFromIncompatibleItem.Should().Throw<InvalidOperationException>();
        }
    }
}
