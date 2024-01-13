// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
#if NET6_0_OR_GREATER
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
#endif
using MigraDoc.DocumentObjectModel;
using MigraDoc.GrammarByExample;
using PdfSharp.Fonts;
#if CORE
using PdfSharp.Snippets.Font;
#endif
using Xunit;

namespace GdiGrammarByExample
{
    public abstract class DdlGbeTestBase : VisualComparisonTestBase
    {
        // Documents containing landscape page:
        // Section-HeaderAndFooter.pdf: 5 through 19: 1111111111111110000 = 0x7FFF0
        // Section-PageLayout.pdf: 3: 100 = 0x4

        /// <summary>
        /// Initializes the test.
        /// </summary>
        /// <param name="fixture"></param>
        /// <param name="testName">Name of the test.</param>
        /// <param name="pages">The number of pages of the resulting PDF file.</param>
        /// <param name="bitmapLandscape">A bitmap containing a 1 for every page that is landscape (0-based).</param>
        public void InitializeTest(GbeFixture fixture, string testName, int pages, uint bitmapLandscape)
        {
            _fixture = fixture;
            _testName = testName;
            _pages = pages;
            _bitmapLandscape = bitmapLandscape;
        }

        public virtual void CleanupTest()
        { }

        /// <summary>
        /// Implemented in test files to invoke InitializeTest with the correct parameters.
        /// </summary>
        public abstract void TestInitialize();

        /// <summary>
        /// Runs the test, including initialization and cleanup.
        /// </summary>
        public void RunTest()
        {
            try
            {
                TestInitialize();
                DoTest();
            }
            finally
            {
                CleanupTest();
            }
        }

        void DoTest()
        {
//#if true
//            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
//#endif

#if CORE
            //GlobalFontSettings.FontResolver ??= new SegoeUiFontResolver();
            GlobalFontSettings.FontResolver ??= SnippetsFontResolver.Get();
#endif

            Environment.CurrentDirectory = WslPathHack(_fixture.TestContext.TempDirectory);

            var workingDir = Environment.CurrentDirectory;
            string pdfFile = _fixture.TestContext.AddResultFileEx(_testName + ".pdf");
            CreatePdfFromMdddlFile(pdfFile, WslPathHack(PathSource), _testName ?? throw new InvalidOperationException(), CompatibilityPatchCallback);

            // ReSharper disable once RedundantCheckBeforeAssignment
            if (Environment.CurrentDirectory != workingDir)
                Environment.CurrentDirectory = workingDir;

            string referenceFile = FindReferenceFile(WslPathHack(ReferenceSource), _testName) ?? throw new InvalidOperationException();

            // ReSharper disable once RedundantCheckBeforeAssignment
            if (Environment.CurrentDirectory != workingDir)
                Environment.CurrentDirectory = workingDir;

            // Set this to true to create a single result file that shows pages from test run and reference side by side on one page.
            const bool createSideBySideFile = true;

            // Set this to true to create two result files (one from test run, one from references) for comparison with Ctrl+F6 (MDI) or Alt+Tab (non-MDI) or "Gegenlichtkontrolle" after printing.
            const bool createSeparateFiles = true;

            AppendToResultPdf(_fixture.TestContext, pdfFile, referenceFile, _pages, _bitmapLandscape, _testName, createSideBySideFile, createSeparateFiles);
        }

        void CompatibilityPatchCallback(Document document)
        {
#if GDI || WPF
            var style = document.Styles[Style.DefaultParagraphName];
            // Verdana was the default style until about 2012 or so.
            // Since all reference documents created with PDFsharp 1.40 or earlier use Verdana, we change the default to Verdana here for all DLL snippets.
            Debug.Assert(style != null, nameof(style) + " != null");
            style!.Font.Name = "Verdana";
#endif
#if CORE
            // Note: CORE uses SnippetsFontResolver and all required fonts should be available.
            var style = document.Styles[Style.DefaultParagraphName];
            Debug.Assert(style != null, nameof(style) + " != null");
            // Since all reference documents created with PDFsharp 1.40 or earlier use Verdana, we change the default to Verdana here for all DLL snippets.
            style!.Font.Name = "Verdana";
#endif
        }

        internal static string WslPathHack(string path)
        {
#if !NET6_0_OR_GREATER
            // .NET 4.7.2 or .NETStandard 2.0, for Windows only.
            return path;
#else
            if (OperatingSystem.IsWindows())
            {
                return path;
            }
            if (OperatingSystem.IsLinux())
            {
                // Hack: Assume WSL and use drive C:\ instead of D:\.
                return path.Replace(@"D:\", "/mnt/c/").Replace('\\', '/');
            }

            throw new NotImplementedException($"Platform {Environment.OSVersion} not yet supported.");
#endif
        }

        //const string PathSource = @"D:\MigraDocAssets\GBE\GBE-DDL";
        const string PathSource = @"..\..\..\..\..\..\..\..\..\assets\archives\grammar-by-example\GBE\GBE-DDL";

        // ***** Select the reference for comparison *****
        // Legacy (the old C++ build):
        //const string ReferenceSource = @"..\..\..\..\..\..\..\..\..\assets\archives\grammar-by-example\GBE\ReferencePDFs\CPP 1.10";

        // GDI+:
        //const string ReferenceSource = @"..\..\..\..\..\..\..\..\..\assets\archives\grammar-by-example\GBE\ReferencePDFs\GDI 1.30";
        //const string ReferenceSource = @"..\..\..\..\..\..\..\..\..\assets\archives\grammar-by-example\GBE\ReferencePDFs\GDI 1.31";
        //const string ReferenceSource = @"..\..\..\..\..\..\..\..\..\assets\archives\grammar-by-example\GBE\ReferencePDFs\GDI 1.40";

        // WPF:
        //const string ReferenceSource = @"..\..\..\..\..\..\..\..\..\assets\archives\grammar-by-example\GBE\ReferencePDFs\WPF 1.30";
        const string ReferenceSource = @"..\..\..\..\..\..\..\..\..\assets\archives\grammar-by-example\GBE\ReferencePDFs\WPF 1.31";
        //const string ReferenceSource = @"..\..\..\..\..\..\..\..\..\assets\archives\grammar-by-example\GBE\ReferencePDFs\WPF 1.40";

        GbeFixture _fixture = null!;
        string? _testName;
        int _pages;
        uint _bitmapLandscape;
    }
}
