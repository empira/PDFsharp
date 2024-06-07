﻿// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.GrammarByExample;
using PdfSharp.Diagnostics;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;

using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("PDFsharp")]
    public class DdlGBE_Paragraph_Layout : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Paragraph_Layout(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Paragraph-Layout", 2, 0);
        }

        [SkippableFact]
#if CORE
        public void DDL_Grammar_By_Example_Paragraph_Layout()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Paragraph_Layout()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Paragraph_Layout()
#endif
        {
            Skip.If(SkippableTests.SkipSlowTests());
            if (!PdfSharp.Capabilities.Build.IsCoreBuild)
            {
                RunTest();
            }
            else
            {
                // This test requires Wingdings font, so we set FailsafeFontResolver as fallback.
                PdfSharpCore.ResetAll();
                try
                {
                    GlobalFontSettings.FallbackFontResolver = new FailsafeFontResolver();
                    RunTest();
                }
                finally
                {
                    PdfSharpCore.ResetAll();
                }
            }
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
