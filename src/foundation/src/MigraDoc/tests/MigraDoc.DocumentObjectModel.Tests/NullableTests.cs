// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Tables;
using FluentAssertions;
using Xunit;

namespace MigraDoc.DocumentObjectModel.Tests
{
    [Collection("PDFsharp")]
    public class NullableTests
    {
        [Fact]
        public void Properties_that_can_be_null()
        {
            var cell = new Cell();
            var table = cell.Table;
            table.Should().BeNull();
        }
    }
}
