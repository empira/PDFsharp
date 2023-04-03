// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Specifies the type of the barcode.
    /// </summary>
    public enum BarcodeType
    {
        /// <summary>
        /// Barcode "Interleaved 2 of 5"
        /// </summary>
        Barcode25i,

        /// <summary>
        /// Barcode "3 of 9"
        /// </summary>
        Barcode39,

        /// <summary>
        /// Barcode "Code 128"
        /// </summary>
        Barcode128
    }
}
