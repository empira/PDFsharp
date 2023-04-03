using MigraDoc.GrammarByExample;
using Xunit;

namespace GdiGrammarByExample
{
    /// <summary>
    /// Grammar by example unit test class.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Collection("GBE")]
    public class DdlGBE_Document_Style : DdlGbeTestBase, IClassFixture<GbeFixture>
    {
        public DdlGBE_Document_Style(GbeFixture fixture)
        {
            _fixture = fixture;
        }

        public override void TestInitialize()
        {
            InitializeTest(_fixture, "Document-Style", 2, 0);
        }

        [Fact]
#if CORE
        public void DDL_Grammar_By_Example_Document_Style()
#elif GDI
        public void GDI_DDL_Grammar_By_Example_Document_Style()
#elif WPF
        public void WPF_DDL_Grammar_By_Example_Document_Style()
#endif
        {
            RunTest();
        }
        // ReSharper restore InconsistentNaming

        readonly GbeFixture _fixture;
    }
}
