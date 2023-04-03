using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Index : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Index(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "!Index", 2, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Index()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Index()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Index()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
