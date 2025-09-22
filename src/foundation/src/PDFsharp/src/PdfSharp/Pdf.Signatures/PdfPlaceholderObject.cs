// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// This object reserves space in the stream of the PdfWriter by adding spaces for objects that can not be written at this time.
    /// It is used e.g. for digital signatures to reserve space for the ByteRange Array.
    /// </summary>
    /// <param name="length">The length of the reserved space.</param>
    sealed class PdfPlaceholderObject(int length) : PdfObject()
    {
        public int Length { get; init; } = length;

        /// <summary>
        /// This only writes spaces as placeholder for the actual object.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            StartPosition = writer.Position;

            writer.WriteRaw(new String(' ', Length));
        }

        /// <summary>
        /// Writes the object, the placeholder is used for.
        /// </summary>
        /// <param name="obj">The object to write.</param>
        /// <param name="writer">The PDFWriter.</param>
        /// <exception cref="Exception">Throws an exception, when the space reserved by the placeholder isn’t enough.</exception>
        internal void WriteActualObject(PdfObject obj, PdfWriter writer)
        {
            Object = obj;

            // Cache current writer position.
            var initialPosition = writer.Position;

            // Write Object at StartPosition.
            writer.Stream.Position = StartPosition;
            Object.WriteObject(writer);

            // Ensure that object doesn’t exceed the reserved space.
            var endPosition = writer.Position;
            var actualLength = endPosition - StartPosition;
            if (actualLength > Length)
            {
                throw new Exception($"The actual length {actualLength} of this object is larger than the length {Length} of its placeholder.");
            }

            // Restore writer position.
            writer.Stream.Position = initialPosition;
        }

        /// <summary>
        /// Position of this object in PdfWriter’s stream.
        /// </summary>
        public SizeType StartPosition { get; internal set; }

        /// <summary>
        /// The object replacing the reserved space.
        /// </summary>
        public PdfObject? Object { get; private set; }
    }
}
