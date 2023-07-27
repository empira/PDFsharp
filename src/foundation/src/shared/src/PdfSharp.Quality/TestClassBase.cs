// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using PdfSharp;
//using PdfSharp.Drawing;
//using PdfSharp.Pdf;
//using PdfSharp.Pdf.IO;
//using PdfSharp.Fonts;

//namespace PdfSharp.Quality
//{
//    /// <summary>
//    /// Base class for unit test classes.
//    /// </summary>
//    public abstract class TestClassBase : CommonBase
//    {
//        public const string TestPassword = "gohome"; // English translation of "gehheim".

//        protected TestClassBase()
//        {
//            // Create Testinstance as early as possible (to set StartTime).
//            TestInstance.Current();
//        }

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext
//        {
//            get { return _testContextInstance; }
//            set
//            {
//                if (value == null)
//                    GetType();
//                _testContextInstance = value;
//            }
//        }
//        private TestContext _testContextInstance;

//        protected class TestInstance
//        {
//            private static TestInstance _current;
//            private readonly List<string> _openedDirectories = new List<string>();

//            public static TestInstance Current()
//            {
//                return _current ?? (_current = new TestInstance());
//            }

//            private TestInstance()
//            {
//                StartTime = DateTime.Now;
//            }

//            public void OpenDirectoryOnce(string directory)
//            {
//              if (_openedDirectories.Contains(directory))
//                  return;

//              Process.Start(directory);
//              _openedDirectories.Add(directory);
//            }

//            public IFontResolver FontResolver;

//            public DateTime StartTime;

//            public String TestDocumentsDirectorySL
//            {
//                get { return StartTime.ToString("yy-MM-dd HH_mm_ss"); }
//            }
//        }

//        /// <summary>
//        /// Creates a PDF test document.
//        /// </summary>
//        protected PdfDocument CreateNewPdfDocument()
//        {
//            var document = new PdfDocument();
//            document.Info.Title = "Created with PDFsharp";
//#if !NET/FX_CORE
//            document.Info.Subject = String.Format("OS: {0}", Environment.OSVersion);
//#endif
//            document.PageLayout = PdfPageLayout.SinglePage;
//            return document;
//        }

//        public PdfPage CreatePdfPage(PdfDocument doc)
//        {
//            var page = doc.AddPage();
//            page.Width = WidthInPoint;
//            page.Height = HeightInPoint;
//            //page.Width = XUnit.FromPresentation(WidthInPU);
//            //page.Height = XUnit.FromPresentation(HeightInPU);
//            return page;
//        }

//        protected void TestSnippet(SnippetBase snippet, bool startViewer = false)
//        {
//            snippet.RenderSnippetAsPdf();
//            //snippet.SaveAndShowPdfDocument(TestDocumentsDirectory);

//            snippet.RenderSnippetAsPng();
//            //snippet.SaveAndShowPngImage(TestDocumentsDirectory);

//            //snippet.GenerateSerialComparisonDocument();
//            snippet.GenerateParallelComparisonDocument();
//            AddCurrentFilepath(snippet.SaveAndShowComparisonDocument(TestDocumentsDirectory, startViewer));

//            if (!startViewer)
//                TestInstance.Current().OpenDirectoryOnce(TestDocumentsDirectory);
//        }

//        //[Obsolete]
//        protected string GetTestDocumentPath(string filename)
//        {
//            return Path.Combine(TestDocumentsDirectory, filename);
//        }

//        public string TestDocumentsDirectory
//        {
//            get
//            {
//                return TestContext.ResultsDirectory;
//            }
//        }

//        //[Obsolete]
//        protected void SaveTestDocument(PdfDocument document, string filepath)
//        {
//            document.Save(filepath);
//            TestContext.AddResultFile(filepath);
//        }

//        //[Obsolete]
//        protected void SaveAndShowDocument(PdfDocument document, string filename, bool startViewer = false)
//        {
//            // Save the PDF document...
//            //var filename = "Test-temp.pdf"; //veDocumentAsync(document, filenameTag);

//            var filepath = GetTestDocumentPath(filename);

//            SaveTestDocument(document, filepath);
//            AddCurrentFilepath(filepath);

//            // ... and start a viewer.
//            if (startViewer)
//                Process.Start(filepath);
//            else
//                Process.Start(TestDocumentsDirectory);
//            // return filename;
//        }

//        //[Obsolete]
//        protected void SaveAndShowDocument(PdfDocument document, bool startViewer = false)
//        {
//            // Save the PDF document...
//            //var filename = "Test-temp.pdf"; //veDocumentAsync(document, filenameTag);

//            var filename = String.Format("{0}-{1}-temp.pdf", TestContext.TestName, PdfSharpTechnology);
//            SaveAndShowDocument(document, filename, startViewer);
//        }

//        protected void ConcatPdfFiles()
//        {
//            var currentFilePath = GetAndRemoveFirstCurrentFilePath();

//            if (String.IsNullOrEmpty(currentFilePath))
//                return;

//            var concatFilename = String.Format("!Results-{0}-temp.pdf", PdfSharpTechnology);
//            var concatFilepath = GetTestDocumentPath(concatFilename);

//            var document = File.Exists(concatFilepath) ? PdfReader.Open(concatFilepath, PdfDocumentOpenMode.Modify) : new PdfDocument();

//            while (!String.IsNullOrEmpty(currentFilePath))
//            {
//                var current = PdfReader.Open(currentFilePath, TestPassword, PdfDocumentOpenMode.Import);
//                foreach (var page in current.Pages)
//                {
//                    document.Pages.Add(page);
//                }

//                currentFilePath = GetAndRemoveFirstCurrentFilePath();
//            }

//            document.Save(concatFilepath);
//            TestContext.AddResultFile(concatFilepath);
//        }

////        void ConcatPdfFiles__2(List<string> filenames)
////        {
////            var document = new PdfDocument();

////            foreach (var filename in filenames)
////            {
////                var current = PdfReader.Open(filename);
////                foreach (var page in current.Pages)
////                {
////                    document.Pages.Add(page);
////                }
////            }

////            var fp = GetTestDocumentPath("!Result-temp.pdf");
////            SaveTestDocument(document, fp);
////        }

//        protected void AddCurrentFilepath(string currentFilepath)
//        {
//            if (String.IsNullOrEmpty(currentFilepath) || _currentFilepaths.Contains(currentFilepath))
//                return;

//            _currentFilepaths.Add(currentFilepath);
//            TestContext.AddResultFile(currentFilepath);
//        }

//        protected string GetAndRemoveFirstCurrentFilePath()
//        {
//            if (_currentFilepaths.Count == 0)
//                return null;

//            var result = _currentFilepaths[0];
//            _currentFilepaths.RemoveAt(0);
//            return result;
//        }

//        private readonly List<string> _currentFilepaths = new List<string>();
//    }
//}