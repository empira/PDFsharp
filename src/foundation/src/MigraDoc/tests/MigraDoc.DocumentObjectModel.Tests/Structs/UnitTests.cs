// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using Xunit;

namespace MigraDoc.DocumentObjectModel.Tests.Structs
{
    public class UnitTests
    {
        [Fact]
        public void Unit_Test()
        {
            const double acceptedRoundingError = 0.00001;

            const double d2 = 2;

            const double d2Cm = d2;
            const double d2CmMm = d2Cm * 10;
            const double d2CmIn = d2Cm / 2.54;
            const double d2CmPt = d2CmIn * 72;
            const double d2CmPc = d2CmPt / 12;


            // FromXxx()
            Unit unit2Mm = Unit.FromMillimeter(d2);
            Unit unit2In = Unit.FromInch(d2);
            Unit unit2Pt = Unit.FromPoint(d2);
            Unit unit2Pc = Unit.FromPica(d2);

            Unit unit2Cm = Unit.FromCentimeter(d2Cm);
            Unit unit2CmMm = Unit.FromMillimeter(d2CmMm);
            Unit unit2CmIn = Unit.FromInch(d2CmIn);
            Unit unit2CmPt = Unit.FromPoint(d2CmPt);
            Unit unit2CmPc = Unit.FromPica(d2CmPc);

            unit2Cm.Value.Should().Be(d2);
            unit2CmMm.Centimeter.Should().Be(d2);
            unit2CmIn.Centimeter.Should().BeApproximately(d2, acceptedRoundingError);
            unit2CmPt.Centimeter.Should().BeApproximately(d2, acceptedRoundingError);
            unit2CmPc.Centimeter.Should().BeApproximately(d2, acceptedRoundingError);


            // ToString(), Unit(string?), == and !=
            string? strNull = null;

            string str2Cm = unit2Cm.ToString();
            string str2CmMm = unit2CmMm.ToString();
            string str2CmIn = unit2CmIn.ToString();
            string str2CmPt = unit2CmPt.ToString();
            string str2CmPc = unit2CmPc.ToString();

            Unit unitNull = strNull;

            Unit unit2Cm2 = str2Cm;
            Unit unit2CmMm2 = str2CmMm;
            Unit unit2CmIn2 = str2CmIn;
            Unit unit2CmPt2 = str2CmPt;
            Unit unit2CmPc2 = str2CmPc;

            (unitNull == null).Should().BeTrue();
            (unitNull != null).Should().BeFalse();

            // Only check cm variables, which are totally equal because str2Cm with "2cm" won't cause rounding errors.
            (unit2Cm2 == unit2Cm).Should().BeTrue();

            // Use IsSameValue() for other variables.
            unit2CmMm2.IsSameValue(unit2CmMm).Should().BeTrue();
            unit2CmIn2.IsSameValue(unit2CmIn).Should().BeTrue();
            unit2CmPt2.IsSameValue(unit2CmPt).Should().BeTrue();
            unit2CmPc2.IsSameValue(unit2CmPc).Should().BeTrue();

            // Only check cm variables, which are totally equal because str2Cm with "2cm" won't cause rounding errors.
            (unit2Cm2 != unit2Cm).Should().BeFalse();


            // Same value - other unit.
            (unit2Mm == unit2CmMm).Should().BeFalse();
            (unit2In == unit2CmIn).Should().BeFalse();
            (unit2Pt == unit2CmPt).Should().BeFalse();
            (unit2Pc == unit2CmPc).Should().BeFalse();

            (unit2Mm != unit2CmMm).Should().BeTrue();
            (unit2In != unit2CmIn).Should().BeTrue();
            (unit2Pt != unit2CmPt).Should().BeTrue();
            (unit2Pc != unit2CmPc).Should().BeTrue();


            // >, >=
            (unitNull > unit2CmMm).Should().BeFalse();
            (unit2CmMm > unitNull).Should().BeTrue();

            (unit2Mm > unit2CmMm).Should().BeFalse();
            (unit2In > unit2CmIn).Should().BeTrue();
            (unit2Pt > unit2CmPt).Should().BeFalse();
            (unit2Pc > unit2CmPc).Should().BeFalse();

            (unitNull >= unit2CmMm).Should().BeFalse();
            (unit2CmMm >= unitNull).Should().BeTrue();

            (unit2Mm >= unit2CmMm).Should().BeFalse();
            (unit2In >= unit2CmIn).Should().BeTrue();
            (unit2Pt >= unit2CmPt).Should().BeFalse();
            (unit2Pc >= unit2CmPc).Should().BeFalse();


            // <, <=
            (unitNull < unit2CmMm).Should().BeTrue();
            (unit2CmMm < unitNull).Should().BeFalse();

            (unit2Mm < unit2CmMm).Should().BeTrue();
            (unit2In < unit2CmIn).Should().BeFalse();
            (unit2Pt < unit2CmPt).Should().BeTrue();
            (unit2Pc < unit2CmPc).Should().BeTrue();

            (unitNull <= unit2CmMm).Should().BeTrue();
            (unit2CmMm <= unitNull).Should().BeFalse();

            (unit2Mm <= unit2CmMm).Should().BeTrue();
            (unit2In <= unit2CmIn).Should().BeFalse();
            (unit2Pt <= unit2CmPt).Should().BeTrue();
            (unit2Pc <= unit2CmPc).Should().BeTrue();


            // >=, <=
            (unit2Cm2 >= unit2Cm).Should().BeTrue();

            (unit2Cm2 <= unit2Cm).Should().BeTrue();


            Unit x1;
            Unit x2;
            Unit x3;


            // -x
            x1 = Unit.FromCentimeter(-d2);
            x2 = -unit2Cm;
            (x2 == x1).Should().BeTrue();


            // +, -
            x1 = Unit.FromCentimeter(d2 + d2);
            x2 = unit2Cm + unit2Cm;
            x3 = unit2Cm + str2Cm;
            x1.Value.Should().Be(4);
            (x2 == x1).Should().BeTrue();
            (x3 == x1).Should().BeTrue();

            x1 = Unit.FromCentimeter(d2 - d2);
            x2 = unit2Cm - unit2Cm;
            x3 = unit2Cm - str2Cm;
            x1.Value.Should().Be(0);
            (x2 == x1).Should().BeTrue();
            (x3 == x1).Should().BeTrue();


            // *, /
            x1 = Unit.FromCentimeter(d2 * d2);
            x2 = unit2Cm * d2;
            x1.Value.Should().Be(4);
            (x2 == x1).Should().BeTrue();

            x1 = Unit.FromCentimeter(d2 / d2);
            x2 = unit2Cm / d2;
            x1.Value.Should().Be(1);
            (x2 == x1).Should().BeTrue();


            // IsSameValue()
            unit2CmMm.IsSameValue(unit2Cm).Should().BeTrue();
            unit2CmIn.IsSameValue(unit2Cm).Should().BeTrue();
            unit2CmPt.IsSameValue(unit2Cm).Should().BeTrue();
            unit2CmPc.IsSameValue(unit2Cm).Should().BeTrue();

            unit2Mm.IsSameValue(unit2Cm).Should().BeFalse();
            unit2In.IsSameValue(unit2Cm).Should().BeFalse();
            unit2Pt.IsSameValue(unit2Cm).Should().BeFalse();
            unit2Pc.IsSameValue(unit2Cm).Should().BeFalse();


            // ConvertType()
            Unit unit2CmMmConv = unit2Cm;
            unit2CmMmConv.ConvertType(UnitType.Millimeter);
            Unit unit2CmInConv = unit2Cm;
            unit2CmInConv.ConvertType(UnitType.Inch);
            Unit unit2CmPtConv = unit2Cm;
            unit2CmPtConv.ConvertType(UnitType.Point);
            Unit unit2CmPcConv = unit2Cm;
            unit2CmPcConv.ConvertType(UnitType.Pica);

            (unit2CmMmConv == unit2CmMm).Should().BeTrue();
            (unit2CmInConv == unit2CmIn).Should().BeTrue();
            (unit2CmPtConv == unit2CmPt).Should().BeTrue();
            (unit2CmPcConv == unit2CmPc).Should().BeTrue();


            // Unit(int)
            x1 = 2;
            x1.Value.Should().Be(2);
            x1.Type.Should().Be(UnitType.Point);
            (x1 == unit2Pt).Should().BeTrue();
            x2 = Unit.Zero + 2;
            (x2 == unit2Pt).Should().BeTrue();


            // Unit(float)
            x1 = 2f;
            x1.Value.Should().Be(2f);
            x1.Type.Should().Be(UnitType.Point);
            (x1 == unit2Pt).Should().BeTrue();
            x2 = Unit.Zero + 2d;
            (x2 == unit2Pt).Should().BeTrue();


            // Unit(double)
            x1 = 2d;
            x1.Value.Should().Be(2d);
            x1.Type.Should().Be(UnitType.Point);
            (x1 == unit2Pt).Should().BeTrue();
            x2 = Unit.Zero + 2d;
            (x2 == unit2Pt).Should().BeTrue();
        }
    }
}
