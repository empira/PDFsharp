// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.Filters
{
    /// <summary>
    /// Reserved for future extension.
    /// </summary>
    public class FilterParms
    {
        /// <summary>
        /// Gets the decoding-parameters for a filter. May be null.
        /// </summary>
        public PdfDictionary? DecodeParms { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterParms"/> class.
        /// </summary>
        /// <param name="decodeParms">The decode parms dictionary.</param>
        public FilterParms(PdfDictionary? decodeParms)
        {
            DecodeParms = decodeParms;
        }
    }

    /// <summary>
    /// Base class for all stream filters
    /// </summary>
    public abstract class Filter
    {
        /// <summary>
        /// When implemented in a derived class encodes the specified data.
        /// </summary>
        public abstract byte[] Encode(byte[] data);

        /// <summary>
        /// Encodes a raw string.
        /// </summary>
        public virtual byte[] Encode(string rawString)
        {
            byte[] bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
            bytes = Encode(bytes);
            return bytes;
        }

        /// <summary>
        /// When implemented in a derived class decodes the specified data.
        /// </summary>
        public abstract byte[] Decode(byte[] data, FilterParms? parms);

        /// <summary>
        /// Decodes the specified data.
        /// </summary>
        public byte[] Decode(byte[] data, PdfDictionary? decodeParms = null)
        {
            return Decode(data, new FilterParms(decodeParms));
        }

        /// <summary>
        /// Decodes to a raw string.
        /// </summary>
        public virtual string DecodeToString(byte[] data, FilterParms? parms)
        {
            byte[] bytes = Decode(data, parms);
            string text = PdfEncoders.RawEncoding.GetString(bytes, 0, bytes.Length);
            return text;
        }

        /// <summary>
        /// Decodes to a raw string.
        /// </summary>
        public string DecodeToString(byte[] data)
        {
            return DecodeToString(data, null);
        }

        /// <summary>
        /// Removes all white spaces from the data. The function assumes that the bytes are characters.
        /// </summary>
        protected byte[] RemoveWhiteSpace(byte[] data)
        {
            int count = data.Length;
            int j = 0;
            for (int i = 0; i < count; i++, j++)
            {
                switch (data[i])
                {
                    case (byte)Chars.NUL:  // 0 Null
                    case (byte)Chars.HT:   // 9 Tab
                    case (byte)Chars.LF:   // 10 Line feed
                    case (byte)Chars.FF:   // 12 Form feed
                    case (byte)Chars.CR:   // 13 Carriage return
                    case (byte)Chars.SP:   // 32 Space
                        j--;
                        break;

                    default:
                        if (i != j)
                            data[j] = data[i];
                        break;
                }
            }
            if (j < count)
            {
                byte[] temp = data;
                data = new byte[j];
                for (int idx = 0; idx < j; idx++)
                    data[idx] = temp[idx];
            }
            return data;
        }
    }
}
