using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Paragraph_Spaces : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Paragraph_Spaces(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Paragraph-Spaces", 2, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Paragraph_Spaces()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Paragraph_Spaces()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Paragraph_Spaces()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
