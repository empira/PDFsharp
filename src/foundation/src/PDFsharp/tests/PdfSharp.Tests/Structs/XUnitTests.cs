// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using Xunit;

namespace PdfSharp.Tests.Structs
{
    [Collection("PDFsharp")]
    public class GlobalsTests
    {
        [Fact]
        public void XUnit_Test()
        {
            const double acceptedRoundingError = 0.00001;

            const double d2 = 2;

            const double d2Cm = d2;
            const double d2CmMm = d2Cm * 10;
            const double d2CmIn = d2Cm / 2.54;
            const double d2CmPt = d2CmIn * 72;
            const double d2CmPu = d2CmIn * 96;


            // FromXxx()
            XUnit xUnit2Mm = XUnit.FromMillimeter(d2);
            XUnit xUnit2In = XUnit.FromInch(d2);
            XUnit xUnit2Pt = XUnit.FromPoint(d2);
            XUnit xUnit2Pu = XUnit.FromPresentation(d2);

            XUnit xUnit2Cm = XUnit.FromCentimeter(d2Cm);
            XUnit xUnit2CmMm = XUnit.FromMillimeter(d2CmMm);
            XUnit xUnit2CmIn = XUnit.FromInch(d2CmIn);
            XUnit xUnit2CmPt = XUnit.FromPoint(d2CmPt);
            XUnit xUnit2CmPu = XUnit.FromPresentation(d2CmPu);

            xUnit2Cm.Value.Should().Be(d2);
            xUnit2CmMm.Centimeter.Should().Be(d2);
            xUnit2CmIn.Centimeter.Should().BeApproximately(d2, acceptedRoundingError);
            xUnit2CmPt.Centimeter.Should().BeApproximately(d2, acceptedRoundingError);
            xUnit2CmPu.Centimeter.Should().BeApproximately(d2, acceptedRoundingError);


            // ToString(), XUnit(string), == and !=
            string str2Cm = xUnit2Cm.ToString();
            string str2CmMm = xUnit2CmMm.ToString();
            string str2CmIn = xUnit2CmIn.ToString();
            string str2CmPt = xUnit2CmPt.ToString();
            string str2CmPu = xUnit2CmPu.ToString();

            XUnit xUnit2Cm2 = str2Cm;
            XUnit xUnit2CmMm2 = str2CmMm;
            XUnit xUnit2CmIn2 = str2CmIn;
            XUnit xUnit2CmPt2 = str2CmPt;
            XUnit xUnit2CmPu2 = str2CmPu;

            // Only check cm variables, which are totally equal because str2Cm with "2cm" won't cause rounding errors.
            (xUnit2Cm2 == xUnit2Cm).Should().BeTrue();

            // Use IsSameValue() for other variables.
            xUnit2CmMm2.IsSameValue(xUnit2CmMm).Should().BeTrue();
            xUnit2CmIn2.IsSameValue(xUnit2CmIn).Should().BeTrue();
            xUnit2CmPt2.IsSameValue(xUnit2CmPt).Should().BeTrue();
            xUnit2CmPu2.IsSameValue(xUnit2CmPu).Should().BeTrue();

            // Only check cm variables, which are totally equal because str2Cm with "2cm" won't cause rounding errors.
            (xUnit2Cm2 != xUnit2Cm).Should().BeFalse();


            // Same value - other unit.
            (xUnit2Mm == xUnit2CmMm).Should().BeFalse();
            (xUnit2In == xUnit2CmIn).Should().BeFalse();
            (xUnit2Pt == xUnit2CmPt).Should().BeFalse();
            (xUnit2Pu == xUnit2CmPu).Should().BeFalse();

            (xUnit2Mm != xUnit2CmMm).Should().BeTrue();
            (xUnit2In != xUnit2CmIn).Should().BeTrue();
            (xUnit2Pt != xUnit2CmPt).Should().BeTrue();
            (xUnit2Pu != xUnit2CmPu).Should().BeTrue();


            // >, >=
            (xUnit2Mm > xUnit2CmMm).Should().BeFalse();
            (xUnit2In > xUnit2CmIn).Should().BeTrue();
            (xUnit2Pt > xUnit2CmPt).Should().BeFalse();
            (xUnit2Pu > xUnit2CmPu).Should().BeFalse();

            (xUnit2Mm >= xUnit2CmMm).Should().BeFalse();
            (xUnit2In >= xUnit2CmIn).Should().BeTrue();
            (xUnit2Pt >= xUnit2CmPt).Should().BeFalse();
            (xUnit2Pu >= xUnit2CmPu).Should().BeFalse();


            // <, <=
            (xUnit2Mm < xUnit2CmMm).Should().BeTrue();
            (xUnit2In < xUnit2CmIn).Should().BeFalse();
            (xUnit2Pt < xUnit2CmPt).Should().BeTrue();
            (xUnit2Pu < xUnit2CmPu).Should().BeTrue();

            (xUnit2Mm <= xUnit2CmMm).Should().BeTrue();
            (xUnit2In <= xUnit2CmIn).Should().BeFalse();
            (xUnit2Pt <= xUnit2CmPt).Should().BeTrue();
            (xUnit2Pu <= xUnit2CmPu).Should().BeTrue();


            // >=, <=
            (xUnit2Cm2 >= xUnit2Cm).Should().BeTrue();

            (xUnit2Cm2 <= xUnit2Cm).Should().BeTrue();


            XUnit x1;
            XUnit x2;
            XUnit x3;


            // -x
            x1 = XUnit.FromCentimeter(-d2);
            x2 = -xUnit2Cm;
            (x2 == x1).Should().BeTrue();


            // +, -
            x1 = XUnit.FromCentimeter(d2 + d2);
            x2 = xUnit2Cm + xUnit2Cm;
            x3 = xUnit2Cm + str2Cm;
            x1.Value.Should().Be(4);
            (x2 == x1).Should().BeTrue();
            (x3 == x1).Should().BeTrue();

            x1 = XUnit.FromCentimeter(d2 - d2);
            x2 = xUnit2Cm - xUnit2Cm;
            x3 = xUnit2Cm - str2Cm;
            x1.Value.Should().Be(0);
            (x2 == x1).Should().BeTrue();
            (x3 == x1).Should().BeTrue();


            // *, /
            x1 = XUnit.FromCentimeter(d2 * d2);
            x2 = xUnit2Cm * d2;
            x1.Value.Should().Be(4);
            (x2 == x1).Should().BeTrue();

            x1 = XUnit.FromCentimeter(d2 / d2);
            x2 = xUnit2Cm / d2;
            x1.Value.Should().Be(1);
            (x2 == x1).Should().BeTrue();


            // IsSameValue()
            xUnit2CmMm.IsSameValue(xUnit2Cm).Should().BeTrue();
            xUnit2CmIn.IsSameValue(xUnit2Cm).Should().BeTrue();
            xUnit2CmPt.IsSameValue(xUnit2Cm).Should().BeTrue();
            xUnit2CmPu.IsSameValue(xUnit2Cm).Should().BeTrue();

            xUnit2Mm.IsSameValue(xUnit2Cm).Should().BeFalse();
            xUnit2In.IsSameValue(xUnit2Cm).Should().BeFalse();
            xUnit2Pt.IsSameValue(xUnit2Cm).Should().BeFalse();
            xUnit2Pu.IsSameValue(xUnit2Cm).Should().BeFalse();


            // ConvertType()
            XUnit xUnit2CmMmConv = xUnit2Cm;
            xUnit2CmMmConv.ConvertType(XGraphicsUnit.Millimeter);
            XUnit xUnit2CmInConv = xUnit2Cm;
            xUnit2CmInConv.ConvertType(XGraphicsUnit.Inch);
            XUnit xUnit2CmPtConv = xUnit2Cm;
            xUnit2CmPtConv.ConvertType(XGraphicsUnit.Point);
            XUnit xUnit2CmPuConv = xUnit2Cm;
            xUnit2CmPuConv.ConvertType(XGraphicsUnit.Presentation);

            (xUnit2CmMmConv == xUnit2CmMm).Should().BeTrue();
            (xUnit2CmInConv == xUnit2CmIn).Should().BeTrue();
            (xUnit2CmPtConv == xUnit2CmPt).Should().BeTrue();
            (xUnit2CmPuConv == xUnit2CmPu).Should().BeTrue();
        }

        [Fact]
        public void XUnitPt_Test()
        {
            const double acceptedRoundingError = 0.00001;

            const double d2 = 2;

            const double d2Cm = d2;
            const double d2CmMm = d2Cm * 10;
            const double d2CmIn = d2Cm / 2.54;
            const double d2CmPt = d2CmIn * 72;
            const double d2CmPu = d2CmIn * 96;


            // FromXxx()
            XUnitPt xUnitPt2Mm = XUnitPt.FromMillimeter(d2);
            XUnitPt xUnitPt2In = XUnitPt.FromInch(d2);
            XUnitPt xUnitPt2Pt = XUnitPt.FromPoint(d2);
            XUnitPt xUnitPt2Pu = XUnitPt.FromPresentation(d2);

            XUnitPt xUnitPt2Cm = XUnitPt.FromCentimeter(d2Cm);
            XUnitPt xUnitPt2CmMm = XUnitPt.FromMillimeter(d2CmMm);
            XUnitPt xUnitPt2CmIn = XUnitPt.FromInch(d2CmIn);
            XUnitPt xUnitPt2CmPt = XUnitPt.FromPoint(d2CmPt);
            XUnitPt xUnitPt2CmPu = XUnitPt.FromPresentation(d2CmPu);

            xUnitPt2Cm.Value.Should().Be(d2CmPt);
            xUnitPt2CmMm.Value.Should().BeApproximately(d2CmPt, acceptedRoundingError);
            xUnitPt2CmIn.Value.Should().BeApproximately(d2CmPt, acceptedRoundingError);
            xUnitPt2CmPt.Value.Should().BeApproximately(d2CmPt, acceptedRoundingError);
            xUnitPt2CmPu.Value.Should().BeApproximately(d2CmPt, acceptedRoundingError);


            // ToString(), XUnitPt(string), == and !=
            string str2Cm = xUnitPt2Cm.ToString();
            string str2Pt = xUnitPt2Pt.ToString();

            XUnitPt xUnitPt2Pt2 = str2Pt;

            (xUnitPt2Pt2 == xUnitPt2Pt).Should().BeTrue();

            (xUnitPt2Pt2 != xUnitPt2Pt).Should().BeFalse();


            // Same value - other unit.
            (xUnitPt2Mm == xUnitPt2CmMm).Should().BeFalse();
            (xUnitPt2In == xUnitPt2CmIn).Should().BeFalse();
            (xUnitPt2Pt == xUnitPt2CmPt).Should().BeFalse();
            (xUnitPt2Pu == xUnitPt2CmPu).Should().BeFalse();

            (xUnitPt2Mm != xUnitPt2CmMm).Should().BeTrue();
            (xUnitPt2In != xUnitPt2CmIn).Should().BeTrue();
            (xUnitPt2Pt != xUnitPt2CmPt).Should().BeTrue();
            (xUnitPt2Pu != xUnitPt2CmPu).Should().BeTrue();


            // >, >=
            (xUnitPt2Mm > xUnitPt2CmMm).Should().BeFalse();
            (xUnitPt2In > xUnitPt2CmIn).Should().BeTrue();
            (xUnitPt2Pt > xUnitPt2CmPt).Should().BeFalse();
            (xUnitPt2Pu > xUnitPt2CmPu).Should().BeFalse();

            (xUnitPt2Mm >= xUnitPt2CmMm).Should().BeFalse();
            (xUnitPt2In >= xUnitPt2CmIn).Should().BeTrue();
            (xUnitPt2Pt >= xUnitPt2CmPt).Should().BeFalse();
            (xUnitPt2Pu >= xUnitPt2CmPu).Should().BeFalse();


            // <, <=
            (xUnitPt2Mm < xUnitPt2CmMm).Should().BeTrue();
            (xUnitPt2In < xUnitPt2CmIn).Should().BeFalse();
            (xUnitPt2Pt < xUnitPt2CmPt).Should().BeTrue();
            (xUnitPt2Pu < xUnitPt2CmPu).Should().BeTrue();

            (xUnitPt2Mm <= xUnitPt2CmMm).Should().BeTrue();
            (xUnitPt2In <= xUnitPt2CmIn).Should().BeFalse();
            (xUnitPt2Pt <= xUnitPt2CmPt).Should().BeTrue();
            (xUnitPt2Pu <= xUnitPt2CmPu).Should().BeTrue();


            // >=, <=
            (xUnitPt2Pt2 >= xUnitPt2Pt).Should().BeTrue();

            (xUnitPt2Pt2 <= xUnitPt2Pt).Should().BeTrue();


            XUnitPt x1;
            XUnitPt x2;
            XUnitPt x3;


            // -x
            x1 = XUnitPt.FromCentimeter(-d2);
            x2 = -xUnitPt2Cm;
            (x2 == x1).Should().BeTrue();


            // +, -
            x1 = XUnitPt.FromCentimeter(d2 + d2);
            x2 = xUnitPt2Cm + xUnitPt2Cm;
            x3 = xUnitPt2Cm + str2Cm;
            x1.Centimeter.Should().BeApproximately(4, acceptedRoundingError);
            x2.Centimeter.Should().BeApproximately(x1.Centimeter, acceptedRoundingError);
            x3.Centimeter.Should().BeApproximately(x1.Centimeter, acceptedRoundingError);

            x1 = XUnitPt.FromCentimeter(d2 - d2);
            x2 = xUnitPt2Cm - xUnitPt2Cm;
            x3 = xUnitPt2Cm - str2Cm;
            x1.Value.Should().Be(0);
            x2.Centimeter.Should().BeApproximately(x1.Centimeter, acceptedRoundingError);
            x3.Centimeter.Should().BeApproximately(x1.Centimeter, acceptedRoundingError);


            // *, /
            x1 = XUnitPt.FromCentimeter(d2 * d2);
            x2 = xUnitPt2Cm * d2;
            x1.Centimeter.Should().BeApproximately(4, acceptedRoundingError);
            x2.Centimeter.Should().BeApproximately(x1.Centimeter, acceptedRoundingError);

            x1 = XUnitPt.FromCentimeter(d2 / d2);
            x2 = xUnitPt2Cm / d2;
            x1.Centimeter.Should().BeApproximately(1, acceptedRoundingError);
            x2.Centimeter.Should().BeApproximately(x1.Centimeter, acceptedRoundingError);


            // IsSameValue()
            xUnitPt2CmMm.IsSameValue(xUnitPt2Cm).Should().BeTrue();
            xUnitPt2CmIn.IsSameValue(xUnitPt2Cm).Should().BeTrue();
            xUnitPt2CmPt.IsSameValue(xUnitPt2Cm).Should().BeTrue();
            xUnitPt2CmPu.IsSameValue(xUnitPt2Cm).Should().BeTrue();

            xUnitPt2Mm.IsSameValue(xUnitPt2Cm).Should().BeFalse();
            xUnitPt2In.IsSameValue(xUnitPt2Cm).Should().BeFalse();
            xUnitPt2Pt.IsSameValue(xUnitPt2Cm).Should().BeFalse();
            xUnitPt2Pu.IsSameValue(xUnitPt2Cm).Should().BeFalse();


            // XUnitPt(int)
            x1 = 2;
            x1.Value.Should().Be(2);
            (x1 == xUnitPt2Pt).Should().BeTrue();
            x2 = XUnitPt.Zero + 2;
            (x2 == xUnitPt2Pt).Should().BeTrue();


            // XUnitPt(double)
            x1 = 2d;
            (x1 == xUnitPt2Pt).Should().BeTrue();
            x2 = XUnitPt.Zero + 2d;
            (x2 == xUnitPt2Pt).Should().BeTrue();


            // double(XUnitPt)
            double xd1 = xUnitPt2Pt;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            (xd1 == xUnitPt2Pt.Point).Should().BeTrue();
        }

        [Fact]
        public void XUnit_vs_XUnitPt_Test()
        {
            const double acceptedRoundingError = 0.00001;

            const double d2 = 2;

            const double d2Cm = d2;
            
            XUnit xUnit2Cm = XUnit.FromCentimeter(d2Cm);
            XUnitPt xUnitPt2Cm = XUnitPt.FromCentimeter(d2Cm);


            // XUnitPt(XUnit)
            XUnitPt xUnit2CmAsXUnitPt = xUnit2Cm;
            xUnit2CmAsXUnitPt.Should().Be(xUnitPt2Cm);
            xUnit2CmAsXUnitPt.Point.Should().Be(xUnitPt2Cm.Point);
            xUnit2CmAsXUnitPt.Should().Be(xUnitPt2Cm.Point);


            // XUnit(XUnitPt)
            XUnit xUnitPt2CmAsXUnit = xUnitPt2Cm;
            xUnitPt2CmAsXUnit.Should().NotBe(xUnit2Cm);
            xUnitPt2CmAsXUnit.Point.Should().Be(xUnit2Cm.Point);
            xUnitPt2CmAsXUnit.Centimeter.Should().BeApproximately(xUnit2Cm.Centimeter, acceptedRoundingError);
        }

    }
}
