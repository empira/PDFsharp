// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using Xunit;
using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.PdfItemExtensions;
using PdfSharp.Pdf.PdfArrayExtensions;
using PdfSharp.Pdf.PdfDictionaryExtensions;
using PdfSharp.Tests.PdfObjectModel;
using PdfSharp.Quality;

//using static PdfSharp.Diagnostics.DebugBreakHelper;
using System.Linq;

#pragma warning disable CS8321 // Local function is declared but never used

// TODO: DELETE

namespace PdfSharp.Tests.PdfObjectModel
{
    //[Collection("PDFsharp")]
    public partial class ObjectModelTests
    {
        // Tests for PdfDictionary.

        //[Fact]
        public void ToDoTest()
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

            var dict32Ref = cat2.Elements.GetRequiredReference("/TestStuff3");

            //var abc = PdfObjectsHelper.TransformDictionary<TestDict3>((PdfDictionary)(dict32Ref!.Value));

            //var x = dict32Ref.Value.AsDictionary().Elements[TestDict3.Keys.TestDict1];
            //var xt = x.GetType();
            var y = dict32Ref.Value.AsDictionary().Elements.GetValue(TestDict3.Keys.TestDict1);
            var z = dict32Ref.Value.AsDictionary().Elements.GetValue(TestDict3.Keys.TestDict1Ref);
            var z2 = dict32Ref.Value.AsDictionary().Elements.GetValue(TestDict3.Keys.TestDict1Ref);

            //var xxx = dict32.Elements.GetItemNew(
            //    TestDict3.Keys.TestDict1, ObjMagic.Default, typeof(TestDict1));

            var path2 = Save(doc2, nameof(TestTest) + "2");
        }
    }
}
