using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Paragraph_Hyphenation : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Paragraph_Hyphenation(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Paragraph-Hyphenation", 1, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Paragraph_Hyphenation()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Paragraph_Hyphenation()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Paragraph_Hyphenation()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
