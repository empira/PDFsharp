// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Tables;
using Xunit;
using FluentAssertions;

namespace MigraDoc.DocumentObjectModel.Tests
{
    /// <summary>
    /// A DocumentObject for testing purposes.
    /// </summary>
    /// <seealso cref="MigraDoc.DocumentObjectModel.DocumentObject" />
    public class Test1Object : DocumentObject
    {
        public Test1Object()
        {
            BaseValues = new Test1ObjectValues(this);
        }

        internal override void Serialize(Serializer serializer)
            => throw new NotImplementedException();

        internal override Meta Meta => TheMeta;

        /* not static here */
        static readonly Meta TheMeta = new(typeof(Test1Object));

        public Test1ObjectValues Values => (Test1ObjectValues)BaseValues;

        public class Test1ObjectValues : Values
        {
            public Test1ObjectValues(DocumentObject owner) : base(owner)
            { }

            //public Unit UnitProp { get; set; }

            public int? IntProp { get; set; }

            public int? NIntProp { get; set; }

            public Color? Color { get; set; }

            public BorderStyle? BorderStyle { get; set; }

            public Border? Border { get; set; }

            public Cells? Cells { get; set; }
        }
    }

    /// <summary>
    /// Low level test of document object model.
    /// </summary>
    [Collection("PDFsharp")]
    public class BasicTests
    {
        [Fact]
        public void Some_initial_tests()
        {
            var r1 = typeof(int);
            var r2 = typeof(int?).IsValueType;
            var r3 = typeof(BorderStyle).IsValueType;
            var r4 = typeof(BorderStyle?).IsValueType;
            var r5 = typeof(Paragraph).IsValueType;
            var r6 = typeof(int?);
            var r7 = typeof(int?);
            var r8 = typeof(int?);

            var value = new Test1Object();
            var meta = value.Meta;

            //_ = typeof(int);

            var p = new Border();
            var m = p.Meta;

            var s = m.GetValue(p, "Style", GV.ReadWrite);

            m.SetValue(p, "Color", Color.Empty);
        }

        [Fact]
        public void Test_value_type_Int32()
        {
            const string intPropName = "IntProp";
            var obj = new Test1Object();

            obj.GetValue(intPropName, GV.ReadOnly).Should().BeNull();
            obj.IsNull(intPropName).Should().BeTrue();

            obj.SetValue(intPropName, 42);
            obj.GetValue(intPropName).Should().Be(42);
            obj.IsNull(intPropName).Should().BeFalse();

            obj.SetNull(intPropName);
            obj.GetValue(intPropName, GV.ReadOnly).Should().BeNull();
            obj.IsNull(intPropName).Should().BeTrue();

            obj.SetValue(intPropName, 0);
            obj.GetValue(intPropName).Should().Be(0);
            obj.IsNull(intPropName).Should().BeFalse();

            obj.SetValue(intPropName, null);
            obj.GetValue(intPropName, GV.ReadOnly).Should().BeNull();
            obj.IsNull(intPropName).Should().BeTrue();
        }

        [Fact]
        public void Test_value_type_Color()
        {
            const string colorName = "Color";
            var cbj = new Test1Object();

            cbj.GetValue(colorName, GV.ReadOnly).Should().BeNull();
            cbj.IsNull(colorName).Should().BeTrue();
            cbj.GetValue(colorName).Should().Be(Color.Empty);
            cbj.IsNull(colorName).Should().BeTrue();

            cbj.SetValue(colorName, Colors.Red);
            cbj.GetValue(colorName).Should().Be(Colors.Red);
            cbj.IsNull(colorName).Should().BeFalse();

            cbj.SetNull(colorName);
            cbj.GetValue(colorName, GV.GetNull).Should().BeNull();
            cbj.IsNull(colorName).Should().BeTrue();

            cbj.SetValue(colorName, Color.Empty);
            cbj.GetValue(colorName).Should().Be(Color.Empty);
            ((INullableValue?)cbj.GetValue(colorName))!.IsNull.Should().BeTrue();
            cbj.IsNull(colorName).Should().BeTrue();

            cbj.SetValue(colorName, null);
            cbj.GetValue(colorName, GV.ReadOnly).Should().BeNull();
            cbj.IsNull(colorName).Should().BeTrue();
        }

        [Fact]
        public void Test_value_type_BorderStyle()
        {
            const string borderStyleName = "BorderStyle";
            var test1 = new Test1Object();

            test1.GetValue(borderStyleName, GV.ReadOnly).Should().BeNull();
            test1.IsNull(borderStyleName).Should().BeTrue();
            test1.GetValue(borderStyleName).Should().Be(BorderStyle.None);
            test1.IsNull(borderStyleName).Should().BeFalse();

            test1.SetValue(borderStyleName, BorderStyle.DashDot);
            test1.GetValue(borderStyleName).Should().Be(BorderStyle.DashDot);
            test1.IsNull(borderStyleName).Should().BeFalse();

            test1.SetNull(borderStyleName);
            test1.GetValue(borderStyleName, GV.ReadOnly).Should().BeNull();
            test1.IsNull(borderStyleName).Should().BeTrue();

            test1.SetValue(borderStyleName, BorderStyle.None);
            test1.GetValue(borderStyleName).Should().Be(BorderStyle.None);
            test1.IsNull(borderStyleName).Should().BeFalse();

            test1.SetValue(borderStyleName, null);
            test1.GetValue(borderStyleName, GV.ReadOnly).Should().BeNull();
            test1.IsNull(borderStyleName).Should().BeTrue();
        }

        [Fact]
        public void Test_reference_type_Border()
        {
            const string borderName = "Border";
            var test1 = new Test1Object();
            var border = new Border();
            border.IsNull().Should().BeTrue();

            test1.GetValue(borderName, GV.ReadOnly).Should().BeNull();
            test1.IsNull(borderName).Should().BeTrue();
            test1.GetValue(borderName).Should().NotBeNull();
            test1.IsNull(borderName).Should().BeTrue();

            test1.SetValue(borderName, border);
            test1.GetValue(borderName).Should().Be(border);
            test1.IsNull(borderName).Should().BeTrue();

            border.Color = Colors.Green;
            border.IsNull().Should().BeFalse();
            test1.IsNull(borderName).Should().BeFalse();

            test1.SetNull(borderName);
            ((Border?)test1.GetValue(borderName))?.IsNull().Should().BeTrue();
            var x1 = (Border?)test1.GetValue(borderName);
            var x2 = x1?.IsNull();
            x2.Should().BeTrue();

            test1.IsNull(borderName).Should().BeTrue();

            test1.SetValue(borderName, null);
            test1.GetValue(borderName, GV.ReadOnly).Should().BeNull();
            test1.IsNull(borderName).Should().BeTrue();
            test1.GetValue(borderName).Should().NotBeNull();
        }

        [Fact]
        public void Test_reference_type_Cells()
        {
            const string propertyName = "Cells";
            var test1 = new Test1Object();
            var cell = new Cells();
            cell.IsNull().Should().BeTrue();

            test1.GetValue(propertyName, GV.ReadOnly).Should().BeNull();
            test1.IsNull(propertyName).Should().BeTrue();
            test1.GetValue(propertyName).Should().NotBeNull();
            test1.IsNull(propertyName).Should().BeTrue();

            test1.SetValue(propertyName, cell);
            test1.GetValue(propertyName).Should().Be(cell);
            test1.IsNull(propertyName).Should().BeTrue();

            cell.Add(new Cell());
            cell.IsNull().Should().BeTrue();
            test1.IsNull(propertyName).Should().BeTrue();
            cell[0].MergeDown = 1;
            cell.IsNull().Should().BeFalse();
            test1.IsNull(propertyName).Should().BeFalse();

            test1.SetNull(propertyName);
            ((Cells?)test1.GetValue(propertyName))?.IsNull().Should().BeTrue();
            var x1 = (Cells?)test1.GetValue(propertyName);
            var x2 = x1?.IsNull();
            x2.Should().BeTrue();

            test1.IsNull(propertyName).Should().BeTrue();

            test1.SetValue(propertyName, null);
            test1.GetValue(propertyName, GV.ReadOnly).Should().BeNull();
            test1.IsNull(propertyName).Should().BeTrue();
            test1.GetValue(propertyName).Should().NotBeNull();
        }
    }
}
