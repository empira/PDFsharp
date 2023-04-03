using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Section_MirrorMargins : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Section_MirrorMargins(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Section-MirrorMargins", 2, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Section_MirrorMargins()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Section_MirrorMargins()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Section_MirrorMargins()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
