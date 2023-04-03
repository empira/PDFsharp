using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Attributes_Color : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Attributes_Color(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Attributes-Color", 2, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Attributes_Color()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Attributes_Color()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Attributes_Color()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
