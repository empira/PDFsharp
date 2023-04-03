// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows;
#endif

#pragma warning disable 649

namespace PdfSharp.Internal
{
    struct SColor
    {
        public byte a;
        public byte r;
        public byte g;
        public byte b;
    }

    struct SCColor
    {
        public float a;
        public float r;
        public float g;
        public float b;
    }

    static class ColorHelper
    {
        public static float sRgbToScRgb(byte bval)
        {
            float num = ((float)bval) / 255f;
            if (num <= 0.0)
                return 0f;
            if (num <= 0.04045)
                return (num / 12.92f);
            if (num < 1f)
                return (float)Math.Pow((num + 0.055) / 1.055, 2.4);
            return 1f;
        }

        public static byte ScRgbTosRgb(float val)
        {
            if (val <= 0.0)
                return 0;
            if (val <= 0.0031308)
                return (byte)(((255f * val) * 12.92f) + 0.5f);
            if (val < 1.0)
                return (byte)((255f * ((1.055f * ((float)Math.Pow((double)val, 0.41666666666666669))) - 0.055f)) + 0.5f);
            return 0xff;
        }
    }
}
