// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality.Testing;
using PdfSharp.Quality.Testing.TestModel;
using Xunit;

namespace PdfSharp.Tests.Pdf.ObjectModel
{
    [Collection("PDFsharp")]
    public class LayoutTests : ObjectModelTestsBase
    {
        private readonly string _tempRoot = GetTempRoot(typeof(LayoutTests));

        [Fact]
        public void Test_Verbose_layout_arrays_in_arrays()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(LayoutTests), nameof(Test_Compact_layout_bug));

            var catalog = rwh.Document.Catalog;
            var dict = new PdfDictionary(rwh.Document, true);
            catalog.Elements.Add("/TestStuff", dict);

            var array = new PdfArray();
            dict.Elements.Add("/DirectArray", array);

            array.Elements.Add(new PdfInteger(111));
            array.Elements.Add(new PdfArray(rwh.Document, new PdfInteger(7)));
            array.Elements.Add(new PdfArray());
            array.Elements.Add(PdfNull.Value);

            rwh.Save(nameof(Test_Verbose_layout_arrays_in_arrays), PdfWriterLayout.Verbose);
            rwh.Reload();
            rwh.Resave(PdfWriterLayout.Compact);
        }

        [Fact]
        public void Test_Compact_layout_bug()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(LayoutTests), nameof(Test_Compact_layout_bug));

            var catalog = rwh.Document.Catalog;
            var dict = new PdfDictionary(rwh.Document, true);
            catalog.Elements.Add("/TestStuff", dict);

            var array = new PdfArray();
            dict.Elements.Add("/DirectArray", array);

            array.Elements.Add(new PdfInteger(33));
            array.Elements.Add(new PdfInteger(55));
            array.Elements.Add(new PdfInteger(-55));
            array.Elements.Add(new PdfInteger(-773));
            array.Elements.Add(new PdfReal(Math.PI));
            array.Elements.Add(new PdfReal(-Math.PI));

            rwh.Save(nameof(Test_Compact_layout_bug), PdfWriterLayout.Compact);
            rwh.Reload();
            rwh.Resave(PdfWriterLayout.Compact);
        }

        [Fact]
        public void Test_Compact_layout()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(LayoutTests), nameof(Test_Compact_layout));

            CreateStuff(rwh.Document);

            rwh.Save(nameof(Test_Compact_layout), PdfWriterLayout.Compact);
            rwh.Reload();
            rwh.Resave(PdfWriterLayout.Compact);
        }

        [Fact]
        public void Test_Standard_layout()
        {
            // TODO: US265
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(LayoutTests), nameof(Test_Standard_layout));

            CreateStuff(rwh.Document);

            rwh.Save(nameof(Test_Standard_layout), PdfWriterLayout.Standard);
            rwh.Reload();
            rwh.Resave(PdfWriterLayout.Standard);
        }

        public void Test_Indented_layout()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(LayoutTests), nameof(Test_Indented_layout));
            // TODO: US265
        }

        [Fact]
        public void Test_Verbose_layout()
        {
            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(LayoutTests), nameof(Test_Verbose_layout));

            CreateStuff(rwh.Document);

            rwh.Save(nameof(Test_Verbose_layout), PdfWriterLayout.Verbose);
            rwh.Reload();
            rwh.Resave(PdfWriterLayout.Verbose);
        }

        [Fact]
        public void Test_empty_name()
        {
            const PdfWriterLayout layout = PdfWriterLayout.Compact;

            var rwh = new ReadWriteHelper();
            rwh.CreateDocument(typeof(LayoutTests), nameof(Test_Compact_layout));

            var catalog = rwh.Document.Catalog;
            var dict = new PdfDictionary(rwh.Document, true);
            catalog.Elements.Add("/TestStuff", dict);

            var array1 = new PdfArray();
            array1.Elements.Add(new PdfName());
            array1.Elements.Add(new PdfInteger(123));
            array1.Elements.Add(new PdfName());
            array1.Elements.Add(new PdfInteger(123));
            array1.Elements.Add(new PdfName());
            array1.Elements.Add(new PdfName());
            array1.Elements.Add(new PdfName());

            dict.Elements.Add("/DirectArray", array1);

            var dict1 = new PdfDictionary();
            dict1.Elements.Add("/1", new PdfName());
            dict1.Elements.Add("/2", new PdfInteger(123));
            dict1.Elements.Add("/3", new PdfName());
            dict1.Elements.Add("/4", new PdfInteger(123));
            dict1.Elements.Add("/5", new PdfName());
            dict1.Elements.Add("/6", new PdfName());

            dict.Elements.Add("/DirectDictionary", dict1);

            var array2 = new PdfArray();
            array2.Elements.Add(new PdfNameObject(rwh.Document, "/"));
            array2.Elements.Add(new PdfIntegerObject(rwh.Document, 123));
            array2.Elements.Add(new PdfNameObject(rwh.Document, "/"));
            array2.Elements.Add(new PdfIntegerObject(rwh.Document, 123));
            array2.Elements.Add(new PdfNameObject(rwh.Document, "/"));
            array2.Elements.Add(new PdfNameObject(rwh.Document, "/"));
            array2.Elements.Add(new PdfNameObject(rwh.Document, "/"));

            dict.Elements.Add("/IndirectArray", array2);

            var dict2 = new PdfDictionary();
            dict2.Elements.Add("/1", new PdfNameObject(rwh.Document, "/"));
            dict2.Elements.Add("/2", new PdfInteger(123));
            dict2.Elements.Add("/3", new PdfNameObject(rwh.Document, "/"));
            dict2.Elements.Add("/4", new PdfInteger(123));
            dict2.Elements.Add("/5", new PdfNameObject(rwh.Document, "/"));
            dict2.Elements.Add("/6", new PdfNameObject(rwh.Document, "/"));

            dict.Elements.Add("/DirectDictionary", dict2);
            //var array2 = new PdfArray(Document, true);
            //PopulateArray(array2);
            //dict.Elements.Add("/IndirectArray", array2);

            //var dict1 = new PdfDictionary();
            //PopulateDictionary(dict1);
            //dict.Elements.Add("/DirectDictionary", dict1);

            //var dict2 = new PdfDictionary();
            //PopulateDictionary(dict2);

            rwh.Save(nameof(Test_empty_name), layout);
            rwh.Reload();
            rwh.Resave(layout);
        }


        void CreateStuff(PdfDocument document)
        {
            var catalog = document.Catalog;
            var dict = new PdfDictionary(document, true);
            catalog.Elements.Add("/TestStuff", dict);

            var array1 = new PdfArray();
            new ObjectModelTestHelper(document).PopulateArray(array1);
            dict.Elements.Add("/DirectArray", array1);

            var array2 = new PdfArray(document, true);
            new ObjectModelTestHelper(document).PopulateArray(array2);
            dict.Elements.Add("/IndirectArray", array2);

            var dict1 = new PdfDictionary();
            new ObjectModelTestHelper(document).PopulateDictionary(dict1);
            dict.Elements.Add("/DirectDictionary", dict1);

            var dict2 = new PdfDictionary();
            new ObjectModelTestHelper(document).PopulateDictionary(dict2);
            dict.Elements.Add("/IndirectDictionary", dict2);
        }

#if true_
        void PopulateArray(PdfArray array)
        {
            // Primitives
            array.Elements.Add(PdfNull.Value);
            array.Elements.Add(new PdfBoolean(true));
            array.Elements.Add(new PdfInteger(42));
            array.Elements.Add(new PdfInteger(Int32.MinValue));
            array.Elements.Add(new PdfInteger(Int32.MaxValue));
            array.Elements.Add(new PdfLongInteger(Int64.MinValue));
            array.Elements.Add(new PdfLongInteger(Int64.MaxValue));
            array.Elements.Add(new PdfReal(Math.PI));
            array.Elements.Add(new PdfReal(Single.MinValue));
            array.Elements.Add(new PdfReal(Single.MaxValue));
            array.Elements.Add(new PdfString("Hello"));
            array.Elements.Add(new PdfName("/Mambo #5"));
            array.Elements.Add(new PdfDebugItem("-.99999"));
            array.Elements.Add(new PdfName("/"));

            // Primitive objects
            array.Elements.Add(new PdfNullObject(Document, true));
            array.Elements.Add(new PdfBooleanObject(Document, true));
            array.Elements.Add(new PdfIntegerObject(Document, 42));
            array.Elements.Add(new PdfIntegerObject(Document, Int32.MinValue));
            array.Elements.Add(new PdfIntegerObject(Document, Int32.MaxValue));
            array.Elements.Add(new PdfLongIntegerObject(Document, Int64.MinValue));
            array.Elements.Add(new PdfLongIntegerObject(Document, Int64.MaxValue));
            array.Elements.Add(new PdfRealObject(Document, Math.PI));
            array.Elements.Add(new PdfRealObject(Document, Single.MinValue));
            array.Elements.Add(new PdfRealObject(Document, Single.MaxValue));
            array.Elements.Add(new PdfStringObject(Document, "Hello"));
            array.Elements.Add(new PdfNameObject(Document, "/Mambo #5"));
            array.Elements.Add(new PdfDebugObject(Document, "-.99999"));
            array.Elements.Add(new PdfNameObject(Document, "/"));

            // Direct and indirect array
            array.Elements.Add(new TestArray1());
            array.Elements.Add(new TestArray1(Document, true));

            // Direct and indirect dictionary
            array.Elements.Add(new TestDict1());
            array.Elements.Add(new TestDict1(Document, true));

            // Some direct array
            array.Elements.Add(CreateSomeArray());

            // Some direct dictionary
            array.Elements.Add(CreateSomeDictionary());
        }

        void PopulateDictionary(PdfDictionary dict)
        {
            // Primitives
            dict.Elements.Add("/null", PdfNull.Value);
            dict.Elements.Add("/bool", new PdfBoolean(true));
            dict.Elements.Add("/int", new PdfInteger(42));
            dict.Elements.Add("/int-min", new PdfInteger(Int32.MinValue));
            dict.Elements.Add("/int-max", new PdfInteger(Int32.MaxValue));
            dict.Elements.Add("/long-min", new PdfLongInteger(Int64.MinValue));
            dict.Elements.Add("/long-max", new PdfLongInteger(Int64.MaxValue));
            dict.Elements.Add("/real", new PdfReal(Math.PI));
            dict.Elements.Add("/real-min", new PdfReal(Single.MinValue));
            dict.Elements.Add("/real-max", new PdfReal(Single.MaxValue));
            dict.Elements.Add("/string", new PdfString("Hello"));
            dict.Elements.Add("/name", new PdfName("/Mambo #5"));
            dict.Elements.Add("/-.999", new PdfDebugItem("-.99999"));
            dict.Elements.Add("/", new PdfName("/"));


            // Primitive objects
            dict.Elements.Add("/null-obj", new PdfNullObject(Document, true));
            dict.Elements.Add("/bool-obj", new PdfBooleanObject(Document, true));
            dict.Elements.Add("/int-obj", new PdfIntegerObject(Document, 42));
            dict.Elements.Add("/int-min-obj", new PdfIntegerObject(Document, Int32.MinValue));
            dict.Elements.Add("/int-max-obj", new PdfIntegerObject(Document, Int32.MaxValue));
            dict.Elements.Add("/long-min-obj", new PdfLongIntegerObject(Document, Int64.MinValue));
            dict.Elements.Add("/long-max-obj", new PdfLongIntegerObject(Document, Int64.MaxValue));
            dict.Elements.Add("/real-obj", new PdfRealObject(Document, Math.PI));
            dict.Elements.Add("/real-min-obj", new PdfRealObject(Document, Single.MinValue));
            dict.Elements.Add("/real-max-obj", new PdfRealObject(Document, Single.MaxValue));
            dict.Elements.Add("/string-obj", new PdfStringObject(Document, "Hello"));
            dict.Elements.Add("/name-obj", new PdfNameObject(Document, "/Mambo #5"));
            dict.Elements.Add("/-.999", new PdfDebugObject(Document, "-.99999"));
            dict.Elements.Add("/", new PdfNameObject("/"));

            // Direct and indirect array
            dict.Elements.Add("/array-direct", new TestArray1());
            dict.Elements.Add("/array-indirect", new TestArray1(Document, true));

            // Direct and indirect dictionary
            dict.Elements.Add("/dict1-direct", new TestDict1());
            dict.Elements.Add("/dict1-indirect", new TestDict1(Document, true));

            // Some direct array
            dict.Elements.Add("/some-array", CreateSomeArray());

            // Some direct dictionary
            dict.Elements.Add("/some-dictionary", CreateSomeDictionary());
        }

        PdfArray CreateSomeArray()
        {
            var array = new PdfArray();
            array.Elements.Add(new PdfInteger(123));
            array.Elements.Add(PdfNull.Value);
            array.Elements.Add(new PdfBoolean(true));
            array.Elements.Add(new PdfBoolean(false));
            array.Elements.Add(new PdfInteger(42));
            array.Elements.Add(new PdfLiteral("123456 0 R"));

            return array;
        }

        PdfDictionary CreateSomeDictionary()
        {
            var dict = new PdfDictionary();
            dict.Elements.Add("/null", PdfNull.Value);
            dict.Elements.Add("/true", new PdfBoolean(true));
            dict.Elements.Add("/false", new PdfBoolean(false));
            dict.Elements.Add("/42", new PdfInteger(42));
            dict.Elements.Add("/invalid-iref", new PdfLiteral("123456 0 R"));

            return dict;
        }
#endif
    }
}
