// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering.Extensions;
using PdfSharp.Drawing;
using Xunit;

namespace MigraDoc.Tests.Extensions
{
    public class UnitConversionsTests
    {
        [Fact]
        public void Unit_Test()
        {
            const double d2 = 2;

            const double d2Cm = d2;
            const double d2CmMm = d2Cm * 10;
            const double d2CmIn = d2Cm / 2.54;
            const double d2CmPt = d2CmIn * 72;
            const double d2CmPu = d2CmIn * 96;
            const double d2CmPc = d2CmPt / 12;

            Unit unit2Cm = Unit.FromCentimeter(d2Cm);
            Unit unit2CmMm = Unit.FromMillimeter(d2CmMm);
            Unit unit2CmIn = Unit.FromInch(d2CmIn);
            Unit unit2CmPt = Unit.FromPoint(d2CmPt);
            Unit unit2CmPc = Unit.FromPica(d2CmPc);

            XUnit xUnit2Cm = XUnit.FromCentimeter(d2Cm);
            XUnit xUnit2CmMm = XUnit.FromMillimeter(d2CmMm);
            XUnit xUnit2CmIn = XUnit.FromInch(d2CmIn);
            XUnit xUnit2CmPt = XUnit.FromPoint(d2CmPt);
            XUnit xUnit2CmPu = XUnit.FromPresentation(d2CmPu);

            XUnitPt xUnitPt2Cm = XUnitPt.FromCentimeter(d2Cm);

            Unit unit2Pt = Unit.FromPoint(d2);

            XUnit xUnit2Pt = XUnit.FromPoint(d2);

            XUnitPt xUnitPt2Pt = XUnitPt.FromPoint(d2);

            // ToXUnit(Unit)
            XUnit unit2PtAsXUnit = unit2Pt.ToXUnit();

            (unit2PtAsXUnit == xUnit2Pt).Should().BeTrue();

            XUnit unit2CmAsXUnit = unit2Cm.ToXUnit();
            XUnit unit2CmMmAsXUnit = unit2CmMm.ToXUnit();
            XUnit unit2CmInAsXUnit = unit2CmIn.ToXUnit();
            XUnit unit2CmPtAsXUnit = unit2CmPt.ToXUnit();
            XUnit unit2CmPcAsXUnit = unit2CmPc.ToXUnit();

            (unit2CmAsXUnit == xUnit2Cm).Should().BeTrue();

            // Unit uses float to save value, therefore converted values are not exactly the same.
            unit2CmMmAsXUnit.IsSameValue(xUnit2CmMm).Should().BeTrue();
            unit2CmInAsXUnit.IsSameValue(xUnit2CmIn).Should().BeTrue();
            unit2CmPtAsXUnit.IsSameValue(xUnit2CmPt).Should().BeTrue();
            unit2CmPcAsXUnit.IsSameValue(xUnit2CmPt).Should().BeTrue(); // There is no Pica unit in XUnit. Therefore, unit2CmPc converted to XUnit should be point.

            // ToUnit(XUnit)
            Unit xUnit2PtAsUnit = xUnit2Pt.ToUnit();

            (xUnit2PtAsUnit == unit2Pt).Should().BeTrue();

            Unit xUnit2CmAsUnit = xUnit2Cm.ToUnit();
            Unit xUnit2CmMmAsUnit = xUnit2CmMm.ToUnit();
            Unit xUnit2CmInAsUnit = xUnit2CmIn.ToUnit();
            Unit xUnit2CmPtAsUnit = xUnit2CmPt.ToUnit();
            Unit xUnit2CmPuAsUnit = xUnit2CmPu.ToUnit();

            (xUnit2CmAsUnit == unit2Cm).Should().BeTrue();
            (xUnit2CmMmAsUnit == unit2CmMm).Should().BeTrue();
            (xUnit2CmInAsUnit == unit2CmIn).Should().BeTrue();
            (xUnit2CmPtAsUnit == unit2CmPt).Should().BeTrue();
            (xUnit2CmPuAsUnit == unit2CmPt).Should().BeTrue(); // There is no Presentation unit in Unit. Therefore, xUnit2CmPu converted to Unit should be point.

            // ToXUnitPt(Unit)
            XUnitPt unit2PtAsXUnitPt = unit2Pt.ToXUnitPt();

            (unit2PtAsXUnitPt == xUnitPt2Pt).Should().BeTrue();

            XUnitPt unit2CmAsXUnitPt = unit2Cm.ToXUnitPt();
            XUnitPt unit2CmMmAsXUnitPt = unit2CmMm.ToXUnitPt();
            XUnitPt unit2CmInAsXUnitPt = unit2CmIn.ToXUnitPt();
            XUnitPt unit2CmPtAsXUnitPt = unit2CmPt.ToXUnitPt();
            XUnitPt unit2CmPcAsXUnitPt = unit2CmPc.ToXUnitPt();

            unit2CmAsXUnitPt.IsSameValue(xUnitPt2Cm).Should().BeTrue();
            unit2CmMmAsXUnitPt.IsSameValue(xUnitPt2Cm).Should().BeTrue();
            unit2CmInAsXUnitPt.IsSameValue(xUnitPt2Cm).Should().BeTrue();
            unit2CmPtAsXUnitPt.IsSameValue(xUnitPt2Cm).Should().BeTrue();
            unit2CmPcAsXUnitPt.IsSameValue(xUnitPt2Cm).Should().BeTrue();

            // ToUnit(XUnitPt)
            Unit xUnitPt2PtAsUnit = xUnitPt2Pt.ToUnit();

            (xUnitPt2PtAsUnit == unit2Pt).Should().BeTrue();

            Unit xUnitPt2CmAsUnit = xUnitPt2Cm.ToUnit();

            xUnitPt2CmAsUnit.IsSameValue(unit2Cm).Should().BeTrue();
        }
    }
}
