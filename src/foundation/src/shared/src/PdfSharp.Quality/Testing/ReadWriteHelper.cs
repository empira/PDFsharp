// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

#pragma warning disable CS8321 // Local function is declared but never used
#pragma warning disable CS1591

namespace PdfSharp.Quality.Testing
{
    public class ReadWriteHelper
    {
        public ReadWriteHelper()
        {
            _document = null!;
            _reloadedDocument = null!;
            _testClass = null!;
            _filename = null!;
            _fullFilename = null!;
            _fullFilename2 = null!;
            _page = null!;
        }

        public PdfDocument CreateDocument(Type testClass, string name)
        {
            _testClass = testClass;
            _document = PdfDocUtility.CreateNewPdfDocument(name);
            _document.Info.Title = name;
            // Add empty page to make it savable.
            _page = _document.AddPage();

            return _document;
        }

        public PdfDocument ImportDocument(string fileName, string passWord, PdfDocumentOpenMode mode = PdfDocumentOpenMode.Modify)
        {
            var name = Path.GetFileName(fileName);
            _document = PdfReader.Open(fileName, passWord, mode);

            return _document;
        }

        public string Save(string? filename = null, PdfWriterLayout layout = PdfWriterLayout.Compact)
        {
            _filename = filename ?? Document.Info.Title;
            //_fullFilename = PdfFileUtility.GetTempPdfFullFileName("Pdf-ObjectModel/" + filename);
            _fullFilename = PdfFileUtility.GetTempPdfFullFileName(PdfFileUtility.GetUnitTestPath(_testClass) + _filename);
            Document.Options.Layout = layout;
            Document.Save(_fullFilename);
            return _fullFilename;
        }

        public PdfDocument Reload()
        {
            _reloadedDocument = PdfReader.Open(_fullFilename, PdfDocumentOpenMode.Modify);
            return _reloadedDocument;
        }

        public string Resave(PdfWriterLayout layout = PdfWriterLayout.Compact)
        {
            var filename = _filename + "_#2";
            //_fullFilename2 = PdfFileUtility.GetTempPdfFullFileName("Pdf-ObjectModel/" + filename);
            _fullFilename2 = PdfFileUtility.GetTempPdfFullFileName(PdfFileUtility.GetUnitTestPath(_testClass) + _filename);

            _reloadedDocument.Options.Layout = layout;
            _reloadedDocument.Save(_fullFilename2);
            return _fullFilename2;
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

        public void SetDocument(PdfDocument doc)
        {
            _document = doc;
        }

        public void SetTestClass(Type testClass)
        {
            _testClass = testClass;
        }

        public PdfDocument Document => _document;

        public PdfDocument ReloadedDocument => _reloadedDocument;

        public PdfCatalog Catalog => _document.Internals.Catalog;

        public PdfPage Page => _page;

        PdfDocument _document;
        PdfDocument _reloadedDocument;
        Type _testClass;
        string _filename;
        string _fullFilename;
        string _fullFilename2;
        PdfPage _page;

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

            // TODO
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

            // TODO
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
