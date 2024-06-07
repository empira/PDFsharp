// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("PDFsharp")]
    public class DdlGBE_Paragraph_PagebreakControl : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Paragraph_PagebreakControl(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Paragraph-PagebreakControl", 2, 0);
        }

        //[SkippableFact]
        [Fact(Skip = "Disabled until /Annots bug is fixed")]
#if CORE
        public void DDL_Grammar_By_Example_Paragraph_PagebreakControl()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Paragraph_PagebreakControl()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Paragraph_PagebreakControl()
#endif
        {
            Skip.If(SkippableTests.SkipSlowTests());
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
