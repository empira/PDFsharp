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

#pragma warning disable CS8321 // Local function is declared but never used

// TODO: DELETE

namespace PdfSharp.Tests.PdfObjectModel
{
    //[Collection("PDFsharp")]
    public partial class ObjectModelTests
    {
        // Tests for PdfArray.

        [Fact]
        public void Test_Catalog_AF()
        {
            var doc1 = CreateDocument();
            var catalog = doc1.Internals.Catalog;

            var af0 = catalog.Elements[PdfCatalog.Keys.AF];

            var af2 = catalog.Elements.GetArray(PdfCatalog.Keys.AF);
            //var af3 = catalog.Elements.GetArray("/AF", VCF.Create);

            //ShouldBreak5 = true;

            var af4 = catalog.Elements.GetRequiredArray<PdfArrayOfDictionaries>(PdfCatalog.Keys.AF, VCF.Create);

            var dict1 = new TestDict1();
            doc1.Internals.AddObject(dict1);
            af4.AddDictionary(dict1);
            var count = af4.Elements.Count;

            //af4.RemoveDictionary(dict1);

            //var dict3 = new TestDict3(doc1);
            //dict3.Comment = "Dict3";
            //doc1.Internals.AddObject(dict3);
            //catalog.Elements.Add("/TestStuff3", dict3);

            // Save and reload.
            var path = Save(doc1, nameof(Test_Catalog_AF) + "1");
            var doc2 = Load(path);
            var cat2 = doc2.Internals.Catalog;

            //var dict32Ref = (PdfReference?)cat2.Elements["/TestStuff3"];

            //var abc = PdfObjectsHelper.TransformDictionary<TestDict3>((PdfDictionary)(dict32Ref!.Value));

            ////var x = dict32Ref.Value.AsDictionary().Elements[TestDict3.Keys.TestDict1];
            ////var xt = x.GetType();
            //var y = dict32Ref.Value.AsDictionary().Elements.GetValue(TestDict3.Keys.TestDict1);
            //var z = dict32Ref.Value.AsDictionary().Elements.GetValue(TestDict3.Keys.TestDict1Ref);
            //var z2 = dict32Ref.Value.AsDictionary().Elements.GetValue(TestDict3.Keys.TestDict1Ref);

            ////var xxx = dict32.Elements.GetItemNew(
            ////    TestDict3.Keys.TestDict1, ObjMagic.Default, typeof(TestDict1));

            var path2 = Save(doc2, nameof(Test_Catalog_AF) + "2");
        }

    }
}
