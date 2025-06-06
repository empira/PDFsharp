﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// BigGustave is distributed with PDFsharp, but was published under a different license.
// See file LICENSE in the folder containing this file.

namespace PdfSharp.Internal.Png.BigGustave
{
    /// <summary>
    /// Settings to use when opening a PNG using <see cref="Png.Open(System.IO.Stream,PngOpenerSettings)"/>
    /// </summary>
    public class PngOpenerSettings
    {
        /// <summary>
        /// The code to execute whenever a chunk is read. Can be <see langword="null"/>.
        /// </summary>
        public IChunkVisitor? ChunkVisitor { get; set; } = null!;

        /// <summary>
        /// Whether to throw if the image contains data after the image end marker.
        /// <see langword="false"/> by default.
        /// </summary>
        public bool DisallowTrailingData { get; set; }
    }
}
