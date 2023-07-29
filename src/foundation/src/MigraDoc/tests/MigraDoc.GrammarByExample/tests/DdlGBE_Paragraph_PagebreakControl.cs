using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
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

        [Fact(Skip = "Disabled until /Annots bug is fixed")]
#if CORE
        public void DDL_Grammar_By_Example_Paragraph_PagebreakControl()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Paragraph_PagebreakControl()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Paragraph_PagebreakControl()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
