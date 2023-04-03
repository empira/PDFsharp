using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Paragraph_Fields : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Paragraph_Fields(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Paragraph-Fields", 2, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Paragraph_Fields()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Paragraph_Fields()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Paragraph_Fields()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
