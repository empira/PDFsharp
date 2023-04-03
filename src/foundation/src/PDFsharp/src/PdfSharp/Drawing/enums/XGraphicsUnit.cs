// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies the unit of measure.
    /// </summary>
    public enum XGraphicsUnit  // NOT the same values as System.Drawing.GraphicsUnit
    {
        /// <summary>
        /// Specifies a printer's point (1/72 inch) as the unit of measure.
        /// </summary>
        Point = 0,  // Must be 0 to let a new XUnit be 0 point.

        /// <summary>
        /// Specifies the inch (2.54 cm) as the unit of measure.
        /// </summary>
        Inch = 1,

        /// <summary>
        /// Specifies the millimeter as the unit of measure.
        /// </summary>
        Millimeter = 2,

        /// <summary>
        /// Specifies the centimeter as the unit of measure.
        /// </summary>
        Centimeter = 3,

        /// <summary>
        /// Specifies a presentation point (1/96 inch) as the unit of measure.
        /// </summary>
        Presentation = 4,
    }
}
