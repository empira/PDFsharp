using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Chart_Types : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Chart_Types(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Chart-Types", 2, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Chart_Types()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Chart_Types()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Chart_Types()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
