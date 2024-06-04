// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering.Extensions
{
    /// <summary>
    /// Provides conversion extension methods between Unit and XUnit/XUnitPt.
    /// </summary>
    public static class UnitConversions
    {
        /// <summary>
        /// Converts a Unit to an XUnit and tries to convert its UnitType to XGraphisUnit.
        /// </summary>
        public static XUnit ToXUnit(this Unit unit)
        {
            var xGraphicsUnit = unit.Type.TryGetAsXGraphicsUnit();
            if (xGraphicsUnit != null)
                return new XUnit(unit.Value, xGraphicsUnit.Value);
            
            return XUnit.FromPoint(unit.Point);
        }

        static XGraphicsUnit? TryGetAsXGraphicsUnit(this UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Point => XGraphicsUnit.Point,
                UnitType.Centimeter => XGraphicsUnit.Centimeter,
                UnitType.Inch => XGraphicsUnit.Inch,
                UnitType.Millimeter => XGraphicsUnit.Millimeter,
                UnitType.Pica => null, // Pica is not supported in XGraphicsUnit
                _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
            };
        }

        /// <summary>
        /// Converts an XUnit to a Unit and tries to convert its XGraphisUnit to UnitType.
        /// </summary>
        public static Unit ToUnit(this XUnit xUnit)
        {
            var unitType = xUnit.Type.TryGetAsUnitType();
            if (unitType != null)
                return new Unit(xUnit.Value, unitType.Value);
            
            return Unit.FromPoint(xUnit.Point);
        }

        static UnitType? TryGetAsUnitType(this XGraphicsUnit xGraphicsUnit)
        {
            return xGraphicsUnit switch
            {
                XGraphicsUnit.Point => UnitType.Point,
                XGraphicsUnit.Inch => UnitType.Inch,
                XGraphicsUnit.Millimeter => UnitType.Millimeter,
                XGraphicsUnit.Centimeter => UnitType.Centimeter,
                XGraphicsUnit.Presentation => null, // Presentation is not supported in XGraphicsUnit
                _ => throw new ArgumentOutOfRangeException(nameof(xGraphicsUnit), xGraphicsUnit, null)
            };
        }

        /// <summary>
        /// Converts a Unit to an XUnitPt.
        /// </summary>
        public static XUnitPt ToXUnitPt(this Unit unit)
        {
            return new XUnitPt(unit.Point);
        }

        /// <summary>
        /// Converts an XUnitPt to a Unit.
        /// </summary>
        public static Unit ToUnit(this XUnitPt xUnitPt)
        {
            return Unit.FromPoint(xUnitPt.Point);
        }
    }
}
