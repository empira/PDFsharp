// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf  // #FOLDER /Pdf/enums
{
    /// <summary>
    /// Internal flags for fast type and state testing.
    /// </summary>
    [Flags]
    enum ItemFlags
    {
        // Note that these flags are used only internally in PDFsharp and therefore can
        // safely be changed if needed.

        /// <summary>
        /// No bit is set. The default value.
        /// </summary>
        Null = 0,

        // Type flags

        /// <summary>
        /// Is PdfItem but not PdfObject.
        /// </summary>
        IsPrimitiveItem = 0b_00000000_0000_0001,

        /// <summary>
        /// Is PdfObject, but not PdfArray or PdfDictionary.
        /// </summary>
        IsCompoundObject = 0b_00000000_0000_0010,

        /// <summary>
        /// Is PdfArray.
        /// </summary>
        IsArray = 0b_00000000_0000_0100,

        /// <summary>
        /// Is PdfDictionary.
        /// </summary>
        IsDictionary = 0b_0000_0000_0000_1000,

        /// <summary>
        /// Is PdfArray or PdfDictionary.
        /// </summary>
        IsArrayOrDictionary = 0b_0000_1100,

        /// <summary>
        /// 
        /// </summary>
        //TypeMask____ = 0x0000_00FF,
        //TypeMask = 0b00000000_00000000_00000000_11111111,

        // Object type transformation

        IsTransformed = 0b_1000_0000, // 0x0080
        TransformationWasTried = 0b_0100_0000, // 0x0040

        /// <summary>
        /// Bits used for transformation flags.
        /// </summary>
        TransformationMask = 0b_1100_0000, // 0xC0

        /// <summary>
        /// After an indirect container (PdfArray or PdfDictionary) is transformed into a derived class,
        /// the object is considered to be dead. If user code contains C# references to such an object
        /// PDFsharp can detect such objects.
        /// </summary>
        IsDead = 0b_0000_0001_0000_0000,

        /// <summary>
        /// Marks a PdfReference during reading a PDF document.
        /// </summary>
        IsTempRef = 0b_0000_0010_0000_0000,

        /// <summary>
        /// If you set a stream to a direct PdfDictionary, it must become an indirect object.
        /// </summary>
        MustBeIndirect = 0b_0000_0100_0000_0000,

        // IsProxyReference = 0b_0000_1000_0000_0000, // not used anymore
    }
}
