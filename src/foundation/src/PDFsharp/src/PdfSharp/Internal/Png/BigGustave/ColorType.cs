﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// BigGustave is distributed with PDFsharp, but was published under a different license.
// See file LICENSE in the folder containing this file.

namespace PdfSharp.Internal.Png.BigGustave
{
    using System;

    /// <summary>
    /// Describes the interpretation of the image data.
    /// </summary>
    [Flags]
    public enum ColorType : byte
    {
        /// <summary>
        /// Grayscale.
        /// </summary>
        None = 0,
        /// <summary>
        /// Colors are stored in a palette rather than directly in the data.
        /// </summary>
        PaletteUsed = 1,
        /// <summary>
        /// The image uses color.
        /// </summary>
        ColorUsed = 2,
        /// <summary>
        /// The image has an alpha channel.
        /// </summary>
        AlphaChannelUsed = 4
    }
}