// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using PdfSharp.TestHelper;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using Xunit;
using System.Text.RegularExpressions;

namespace MigraDoc.Tests
{
    [Collection("PDFsharp")]
    public class CultureAndRegionTests
    {
        [Fact]
        public void Culture_Region_and_metric()
        {
            //var culture1 = CultureInfo.InvariantCulture;
            //var region1 = new RegionInfo(culture1.Name);
            //var isMetric1 = region1.IsMetric;
        }
    }
}
