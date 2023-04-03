using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Document_Info : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Document_Info(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Document-Info", 1, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Document_Info()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Document_Info()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Document_Info()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
