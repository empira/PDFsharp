using FluentAssertions;
using MigraDoc.DocumentObjectModel.Shapes;
using Xunit;

namespace MigraDoc.DocumentObjectModel.Tests
{
    public class AdvancedTests
    {
        [Fact]
        void Inherited_values()
        {
            var dom = new TextFrame();
            var meta = dom.Meta;

            var vd1 = meta.ValueDescriptors["MarginLeft"];
            var vd2 = meta.ValueDescriptors["Left"];

            var val1 = vd1.GetValue(dom, GV.ReadWrite);
            var val2 = vd2.GetValue(dom, GV.ReadWrite);

            var ival1 = val1 as INullableValue;
            var ival2 = val2 as INullableValue;

            ival1.Should().NotBeNull();
            ival2.Should().NotBeNull();
        }
    }
}
