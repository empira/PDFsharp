// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable once CheckNamespace
namespace PdfSharp.BigGustave
{
    /// <summary>
    /// The Palette class.
    /// </summary>
    public class Palette
    {
        /// <summary>
        /// True if palette has alpha values.
        /// </summary>
        public bool HasAlphaValues { get; private set; }

        /// <summary>
        /// The palette data.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Creates a palette object. Input palette data length from PLTE chunk must be a multiple of 3.
        /// </summary>
        public Palette(byte[] data)
        {
            Data = new byte[data.Length * 4 / 3];
            var dataIndex = 0;
            for (var i = 0; i < data.Length; i += 3)
            {
                Data[dataIndex++] = data[i];
                Data[dataIndex++] = data[i + 1];
                Data[dataIndex++] = data[i + 2];
                Data[dataIndex++] = 255;
            }
        }

        /// <summary>
        /// Adds transparency values from tRNS chunk.
        /// </summary>
        public void SetAlphaValues(byte[] bytes)
        {
            HasAlphaValues = true;

            for (var i = 0; i < bytes.Length; i++)
            {
                Data[i * 4 + 3] = bytes[i];
            }
        }

        /// <summary>
        /// Gets the palette entry for a specific index.
        /// </summary>
        public Pixel GetPixel(int index)
        {
            var start = index * 4;
            return new Pixel(Data[start], Data[start + 1], Data[start + 2], Data[start + 3], false);
        }
    }
}