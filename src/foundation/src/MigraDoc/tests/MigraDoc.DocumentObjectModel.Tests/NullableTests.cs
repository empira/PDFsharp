using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MigraDoc.DocumentObjectModel.Tables;
using Xunit;

namespace MigraDoc.DocumentObjectModel.Tests
{
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
