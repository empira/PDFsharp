using FluentAssertions;
using MigraDoc.DocumentObjectModel.Tables;
using Xunit;

namespace MigraDoc.DocumentObjectModel.Tests
{

    /// <summary>
    /// Low level test of document object model.
    /// </summary>
    public class BasicTests2
    {
        [Fact]
        public void ResourceTests()
        {
            DomSR.TestResourceMessages();
            //var r1 = typeof(int);
            //var r2 = typeof(int?).IsValueType;
            //var r3 = typeof(BorderStyle).IsValueType;
            //var r4 = typeof(BorderStyle?).IsValueType;
            //var r5 = typeof(Paragraph).IsValueType;
            //var r6 = typeof(int?);
            //var r7 = typeof(int?);
            //var r8 = typeof(int?);

            //var value = new Test1Object();
            //var meta = value.Meta;

            ////GetType();

            //var p = new Border();
            //var m = p.Meta;

            //var s = m.GetValue(p, "Style", GV.ReadWrite);

            //m.SetValue(p, "Color", Color.Empty);
        }
    }
}
