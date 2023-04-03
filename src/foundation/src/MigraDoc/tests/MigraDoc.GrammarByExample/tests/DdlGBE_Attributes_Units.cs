using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Attributes_Units : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Attributes_Units(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Attributes-Units", 1, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Attributes_Units()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Attributes_Units()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Attributes_Units()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
