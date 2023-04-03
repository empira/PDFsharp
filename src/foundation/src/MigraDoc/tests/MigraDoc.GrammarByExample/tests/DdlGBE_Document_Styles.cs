using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Document_Styles : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Document_Styles(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Document-Styles", 2, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Document_Styles()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Document_Styles()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Document_Styles()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
