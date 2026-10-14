// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Internal;

#pragma warning disable CS1591

namespace PdfSharp.Quality.Testing.TestModel
{
    public class ObjectModelTestHelper
    {
        public ObjectModelTestHelper(PdfDocument document)
        {
            Document = document;
        }

        PdfDocument Document { get; set; }

        public void PopulateDictionary(PdfDictionary dict)
        {
            // Primitives
            dict.Elements.Add("/null", PdfNull.Value);
            dict.Elements.Add("/array", new PdfArray(Document, new PdfReal(5), new PdfReal(6), new PdfReal(7), new PdfReal(8))); // For GetRectangle test.
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
            dict.Elements.Add("/string-hex", new PdfString("HelloHex", true));
            dict.Elements.Add("/string-date1", new PdfString("D:19991231235959"));
            dict.Elements.Add("/string-date2", new PdfString("D:19991231235959+02'00'"));
            dict.Elements.Add("/string-date3", new PdfString("D:19991231235959-03'00'"));
            dict.Elements.Add("/name", new PdfName("/Mambo #5"));
            dict.Elements.Add("/rect", new PdfRectangle(new XRect(1, 2, 3, 4)));
            dict.Elements.Add("/-.999", new PdfDebugItem("-.99999"));
            dict.Elements.Add("/", new PdfNameObject(Document, "/"));
            dict.Elements.Add("/int-default", new PdfInteger());
            dict.Elements.Add("/date1", new PdfDate(new DateTimeOffset(1999, 12, 31, 23, 59, 59, TimeSpan.Zero)));
            dict.Elements.Add("/date2", new PdfDate(new DateTimeOffset(1999, 12, 31, 23, 59, 59, new TimeSpan(4, 0, 0))));
            dict.Elements.Add("/date3", new PdfDate(new DateTimeOffset(1999, 12, 31, 23, 59, 59, -new TimeSpan(3, 34, 0))));

            // TODO PdfSignaturePlaceHolder

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
            dict.Elements.Add("/string-obj-hex", new PdfStringObject(Document, "HelloHex") { HexLiteral = true });
            dict.Elements.Add("/name-obj", new PdfNameObject(Document, "/Mambo #5"));
            //dict.Elements.Add("/rect-obj", new PdfRectangleObject(new XRect(1, 2, 3, 4)));
            dict.Elements.Add("/-.999", new PdfDebugObject(Document, "-.99999"));
            dict.Elements.Add("/", new PdfNameObject(Document, "/"));

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

            // Some enum test data
            dict.Elements.Add("/pagelayout-test", new PdfInteger((int)PdfPageLayout.TwoColumnRight));
            dict.Elements.Add("/pagemode-test", new PdfInteger((int)PdfPageMode.FullScreen));
            dict.Elements.Add("/pagelayout-test-string", new PdfName("/" + PdfPageLayout.TwoColumnRight));
            dict.Elements.Add("/pagemode-test-string", new PdfName("/" + PdfPageMode.FullScreen));
        }

        public void PopulateArray(PdfArray array)
        {
            // Primitives
            array.Elements.Add(PdfNull.Value);
            // 1
            array.Elements.Add(new PdfBoolean(true));
            array.Elements.Add(new PdfInteger(42));
            array.Elements.Add(new PdfInteger(Int32.MinValue));
            array.Elements.Add(new PdfInteger(Int32.MaxValue));
            array.Elements.Add(new PdfLongInteger(Int64.MinValue));
            // 6
            array.Elements.Add(new PdfLongInteger(Int64.MaxValue));
            array.Elements.Add(new PdfReal(Math.PI));
            array.Elements.Add(new PdfReal(Single.MinValue));
            array.Elements.Add(new PdfReal(Single.MaxValue));
            array.Elements.Add(new PdfString("Hello"));
            // 11
            array.Elements.Add(new PdfName("/Mambo #5"));
            array.Elements.Add(new PdfDebugItem("-.99999"));
            array.Elements.Add(new PdfName("/"));
            array.Elements.Add(new PdfInteger(-1));

            // Primitive objects
            // 15
            array.Elements.Add(new PdfNullObject(Document, true));
            array.Elements.Add(new PdfBooleanObject(Document, true));
            array.Elements.Add(new PdfIntegerObject(Document, 42));
            array.Elements.Add(new PdfIntegerObject(Document, Int32.MinValue));
            array.Elements.Add(new PdfIntegerObject(Document, Int32.MaxValue));
            // 20
            array.Elements.Add(new PdfLongIntegerObject(Document, Int64.MinValue));
            array.Elements.Add(new PdfLongIntegerObject(Document, Int64.MaxValue));
            array.Elements.Add(new PdfRealObject(Document, Math.PI));
            array.Elements.Add(new PdfRealObject(Document, Single.MinValue));
            array.Elements.Add(new PdfRealObject(Document, Single.MaxValue));
            // 25
            array.Elements.Add(new PdfStringObject(Document, "Hello"));
            array.Elements.Add(new PdfNameObject(Document, "/Mambo #5"));
            array.Elements.Add(new PdfDebugObject(Document, "-.99999"));
            array.Elements.Add(new PdfNameObject(Document, "/"));

            // Direct and indirect array
            // 29
            array.Elements.Add(new TestArray1());
            array.Elements.Add(new TestArray1(Document, true));

            // Direct and indirect dictionary
            // 31
            array.Elements.Add(new TestDict1());
            array.Elements.Add(new TestDict1(Document, true));

            // Some direct array
            array.Elements.Add(CreateSomeArray());

            // Some direct dictionary
            array.Elements.Add(CreateSomeDictionary());
        }

        public PdfArray CreateSomeArray()
        {
            var array = new PdfArray();
            array.Elements.Add(new PdfInteger(123));
            array.Elements.Add(PdfNull.Value);
            array.Elements.Add(new PdfBoolean(true));
            array.Elements.Add(new PdfBoolean(false));
            array.Elements.Add(new PdfInteger(42));
            array.Elements.Add(new PdfArray());
            array.Elements.Add(new PdfInteger(43));
            array.Elements.Add(new PdfArray(Document, new PdfDictionary()));
            array.Elements.Add(new PdfInteger(44));
            array.Elements.Add(new PdfLiteral("123456 0 R"));

            return array;
        }

        public PdfDictionary CreateSomeDictionary()
        {
            var dict = new PdfDictionary();
            dict.Elements.Add("/null", PdfNull.Value);
            dict.Elements.Add("/true", new PdfBoolean(true));
            dict.Elements.Add("/false", new PdfBoolean(false));
            dict.Elements.Add("/42", new PdfInteger(42));
            dict.Elements.Add("/arrEmpty", new PdfArray());
            dict.Elements.Add("/43", new PdfInteger(43));
            dict.Elements.Add("/arr2", new PdfArray(Document, new PdfDictionary()));
            dict.Elements.Add("/44", new PdfInteger(44));
            dict.Elements.Add("/invalid-iref", new PdfLiteral("123456 0 R"));

            return dict;
        }
    }
}
