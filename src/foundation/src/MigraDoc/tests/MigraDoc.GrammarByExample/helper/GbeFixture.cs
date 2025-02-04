// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Diagnostics;
#if CORE
using PdfSharp.Quality;
#endif
using PdfSharp.Fonts;

namespace MigraDoc.GrammarByExample
{
    public class GbeFixture : IDisposable
    {
        public GbeFixture()
        {
            PdfSharpCore.ResetAll();
#if CORE
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; } = TestContext.GetOrCreate();

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }
    }
}
