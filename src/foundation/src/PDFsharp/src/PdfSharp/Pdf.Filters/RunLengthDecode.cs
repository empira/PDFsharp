namespace PdfSharp.Pdf.Filters
{
    /// <summary>
    /// https://opensource.adobe.com/dc-acrobat-sdk-docs/pdfstandards/pdfreference1.7old.pdf
    /// The RunLengthDecode filter decodes data that has been encoded in a simple
    /// byte-oriented format based on run length.The encoded data is a sequence of
    /// runs, where each run consists of a length byte followed by 1 to 128 bytes of data.If
    /// the length byte is in the range 0 to 127, the following length + 1 (1 to 128) bytes
    /// are copied literally during decompression.If length is in the range 129 to 255, the
    /// following single byte is to be copied 257 − length (2 to 128) times during
    /// decompression.A length value of 128 denotes EOD.
    /// The compression achieved by run-length encoding depends on the input data.In
    /// the best case (all zeros), a compression of approximately 64:1 is achieved for long
    /// files. The worst case (the hexadecimal sequence 00 alternating with FF) results in
    /// an expansion of 127:128. 
    /// </summary>
    internal class RunLengthDecode : Filter
    {
        public override Byte[] Encode(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override Byte[] Decode(byte[] compressedData, FilterParms? parms)
        {
            var decompressedData = new List<byte>();
            var i = 0;

            // Iterate through the compressed stream and decode the RLE data
            while (i < compressedData.Length)
            {
                var lengthByte = compressedData[i];
                i++;

                // If lengthByte is 128, it means EOD (End of Data)
                if (lengthByte == 128)
                {
                    break;
                }

                if (lengthByte <= 127)
                {
                    // Copy (lengthByte + 1) bytes literally
                    var length = lengthByte + 1;
                    for (var j = 0; j < length; j++)
                    {
                        decompressedData.Add(compressedData[i]);
                        i++;
                    }
                }
                else
                {
                    // Repeat the next byte (257 - lengthByte) times
                    var repeatCount = 257 - lengthByte;
                    var value = compressedData[i];
                    i++;

                    for (var j = 0; j < repeatCount; j++)
                    {
                        decompressedData.Add(value);
                    }
                }
            }

            // Return the decompressed byte array
            return decompressedData.ToArray();
        }
    }
}