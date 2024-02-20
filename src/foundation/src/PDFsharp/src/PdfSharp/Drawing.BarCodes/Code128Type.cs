using System;
using System.Collections.Generic;
using System.Text;

using PdfSharp.Drawing;
using PdfSharp.Drawing.BarCodes;

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>Code types for Code 128 bar code</summary>
    public enum Code128Type
    {
        /// <summary>Code A</summary>
        CodeA = 103,
        /// <summary>Code B</summary>
        CodeB = 104,
        /// <summary>Code C</summary>
        CodeC = 105,
    }
}