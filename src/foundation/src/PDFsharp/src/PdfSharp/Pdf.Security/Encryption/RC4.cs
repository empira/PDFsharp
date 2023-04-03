// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Security.Encryption
{
    /// <summary>
    /// Implements the RC4 encryption algorithm.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    class RC4
    {
        /// <summary>
        /// Sets the encryption key.
        /// </summary>
        public void SetKey(byte[] key)
        {
            SetKey(key, key.Length);
        }

        /// <summary>
        /// Sets the encryption key.
        /// </summary>
        public void SetKey(byte[] key, int length)
        {
            SetKey(key, 0, length);
        }

        /// <summary>
        /// Sets the encryption key.
        /// </summary>
        public void SetKey(byte[] key, int offset, int length)
        {
            var idx1 = 0;
            var idx2 = 0;
            for (var idx = 0; idx < 256; idx++)
                _state[idx] = (byte)idx;
            byte tmp;
            for (var idx = 0; idx < 256; idx++)
            {
                idx2 = (key[idx1 + offset] + _state[idx] + idx2) & 255;
                tmp = _state[idx];
                _state[idx] = _state[idx2];
                _state[idx2] = tmp;
                idx1 = (idx1 + 1) % length;
            }
        }

        /// <summary>
        /// Encrypts the data.
        /// </summary>
        public void Encrypt(byte[] data)
        {
            Encrypt(data, data.Length);
        }

        /// <summary>
        /// Encrypts the data.
        /// </summary>
        public void Encrypt(byte[] data, int length)
        {
            Encrypt(data, 0, length);
        }

        /// <summary>
        /// Encrypts the data.
        /// </summary>
        public void Encrypt(byte[] data, int offset, int length)
        {
            Encrypt(data, offset, length, data);
        }

        /// <summary>
        /// Encrypts the data.
        /// </summary>
        public void Encrypt(byte[] inputData, byte[] outputData)
        {
            Encrypt(inputData, 0, inputData.Length, outputData);
        }

        /// <summary>
        /// Encrypts the data.
        /// </summary>
        public void Encrypt(byte[] inputData, int offset, int length, byte[] outputData)
        {
            length += offset;
            int x = 0, y = 0;
            byte b;
            for (var idx = offset; idx < length; idx++)
            {
                x = (x + 1) & 255;
                y = (_state[x] + y) & 255;
                b = _state[x];
                _state[x] = _state[y];
                _state[y] = b;
                outputData[idx] = (byte)(inputData[idx] ^ _state[(_state[x] + _state[y]) & 255]);
            }
        }

        /// <summary>
        /// Bytes used for RC4 encryption.
        /// </summary>
        readonly byte[] _state = new byte[256];
    }
}
