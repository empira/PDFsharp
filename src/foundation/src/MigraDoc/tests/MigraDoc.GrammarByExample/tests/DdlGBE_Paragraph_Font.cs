using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Paragraph_Font : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Paragraph_Font(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Paragraph-Font", 2, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Paragraph_Font()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Paragraph_Font()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Paragraph_Font()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
