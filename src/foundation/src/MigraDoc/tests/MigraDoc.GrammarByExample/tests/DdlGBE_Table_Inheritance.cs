using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Table_Inheritance : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Table_Inheritance(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Table-Inheritance", 1, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Table_Inheritance()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Table_Inheritance()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Table_Inheritance()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
