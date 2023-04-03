using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Barcode : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Barcode(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Barcode", 1, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Barcode()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Barcode()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Barcode()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
