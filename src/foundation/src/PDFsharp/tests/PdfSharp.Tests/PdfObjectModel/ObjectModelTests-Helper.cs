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

// TODO: DELETE

#pragma warning disable CS8321 // Local function is declared but never used

namespace PdfSharp.Tests.PdfObjectModel
{
    public partial class ObjectModelTests
    {
        string Save(PdfDocument doc, string name)
        {
            string filename = PdfFileUtility.GetTempPdfFullFileName("_newStuff/" + name);
            doc.Save(filename);
            return filename;
        }

        PdfDocument Load(string path)
        {
            var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
            return document;
        }

        PdfDocument CreateDocument()
        {
            var doc = new PdfDocument();
            var page = new PdfPage();
            doc.AddPage(page);
            return doc;
        }

        // TODO
        static void CheckParentInfoConsistency(PdfObject root)
        {
            Debug.Assert(root.IsIndirect);
            var owner = root.Owner;
            var closure = owner.IrefTable.TransitiveClosure(root);


            foreach (var reference in closure)
            {
                var obj = reference.Value;
            }
            return;

            static void CheckArray(PdfArray array)
            {
                foreach (var item in array.Elements)
                {
                    //var item = elements[name];
                    if (item is PdfObject obj)
                    {
                        if (obj.Reference != null)
                        {
                            if (obj.ParentInfo != null)
                                throw new InvalidOperationException("ParentInfo must be null.");
                        }
                        else
                        {
                            if (obj.ParentInfo == null)
                                throw new InvalidOperationException("ParentInfo must not be null.");

                            if (obj.ParentInfo.OwningArray != array)
                                throw new InvalidOperationException("ParentInfo must be owning array.");
                        }
                    }
                }
            }

            static void CheckDictionary(PdfDictionary dict)
            {
                foreach (var item in dict.Elements.Values)
                {
                    if (item is PdfObject obj)
                    {
                        if (obj.Reference != null)
                        {
                            if (obj.ParentInfo != null)
                                throw new InvalidOperationException("ParentInfo must be null.");
                        }
                        else
                        {
                            if (obj.ParentInfo == null)
                                throw new InvalidOperationException("ParentInfo must not be null.");

                            if (obj.ParentInfo.OwningDictionary != dict)
                                throw new InvalidOperationException("ParentInfo must  be owning dictionary.");
                        }
                    }
                }
            }
        }
    }
}
