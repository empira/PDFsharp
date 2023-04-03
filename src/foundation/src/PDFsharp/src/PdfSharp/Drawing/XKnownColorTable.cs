// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
    static class XKnownColorTable
    {
        public static uint KnownColorToArgb(XKnownColor color)
        {
            if (color <= XKnownColor.YellowGreen)
                return ColorTable[(int)color];
            return 0;
        }

        public static bool IsKnownColor(uint argb)
        {
            for (int idx = 0; idx < ColorTable.Length; idx++)
            {
                if (ColorTable[idx] == argb)
                    return true;
            }
            return false;
        }

        public static XKnownColor GetKnownColor(uint argb)
        {
            for (int idx = 0; idx < ColorTable.Length; idx++)
            {
                if (ColorTable[idx] == argb)
                    return (XKnownColor)idx;
            }
            return (XKnownColor)(-1);
        }

        static XKnownColorTable()
        {
            // Same values as in GDI+ and System.Windows.Media.XColors.
            // Note that Magenta is the same as Fuchsia and Cyan is the same as Aqua.
            ColorTable[0] = 0xFFF0F8FF;  // AliceBlue
            ColorTable[1] = 0xFFFAEBD7;  // AntiqueWhite
            ColorTable[2] = 0xFF00FFFF;  // Aqua
            ColorTable[3] = 0xFF7FFFD4;  // Aquamarine
            ColorTable[4] = 0xFFF0FFFF;  // Azure
            ColorTable[5] = 0xFFF5F5DC;  // Beige
            ColorTable[6] = 0xFFFFE4C4;  // Bisque
            ColorTable[7] = 0xFF000000;  // Black
            ColorTable[8] = 0xFFFFEBCD;  // BlanchedAlmond
            ColorTable[9] = 0xFF0000FF;  // Blue
            ColorTable[10] = 0xFF8A2BE2;  // BlueViolet
            ColorTable[11] = 0xFFA52A2A;  // Brown
            ColorTable[12] = 0xFFDEB887;  // BurlyWood
            ColorTable[13] = 0xFF5F9EA0;  // CadetBlue
            ColorTable[14] = 0xFF7FFF00;  // Chartreuse
            ColorTable[15] = 0xFFD2691E;  // Chocolate
            ColorTable[16] = 0xFFFF7F50;  // Coral
            ColorTable[17] = 0xFF6495ED;  // CornflowerBlue
            ColorTable[18] = 0xFFFFF8DC;  // Cornsilk
            ColorTable[19] = 0xFFDC143C;  // Crimson
            ColorTable[20] = 0xFF00FFFF;  // Cyan
            ColorTable[21] = 0xFF00008B;  // DarkBlue
            ColorTable[22] = 0xFF008B8B;  // DarkCyan
            ColorTable[23] = 0xFFB8860B;  // DarkGoldenrod
            ColorTable[24] = 0xFFA9A9A9;  // DarkGray
            ColorTable[25] = 0xFF006400;  // DarkGreen
            ColorTable[26] = 0xFFBDB76B;  // DarkKhaki
            ColorTable[27] = 0xFF8B008B;  // DarkMagenta
            ColorTable[28] = 0xFF556B2F;  // DarkOliveGreen
            ColorTable[29] = 0xFFFF8C00;  // DarkOrange
            ColorTable[30] = 0xFF9932CC;  // DarkOrchid
            ColorTable[31] = 0xFF8B0000;  // DarkRed
            ColorTable[32] = 0xFFE9967A;  // DarkSalmon
            ColorTable[33] = 0xFF8FBC8B;  // DarkSeaGreen
            ColorTable[34] = 0xFF483D8B;  // DarkSlateBlue
            ColorTable[35] = 0xFF2F4F4F;  // DarkSlateGray
            ColorTable[36] = 0xFF00CED1;  // DarkTurquoise
            ColorTable[37] = 0xFF9400D3;  // DarkViolet
            ColorTable[38] = 0xFFFF1493;  // DeepPink
            ColorTable[39] = 0xFF00BFFF;  // DeepSkyBlue
            ColorTable[40] = 0xFF696969;  // DimGray
            ColorTable[41] = 0xFF1E90FF;  // DodgerBlue
            ColorTable[42] = 0xFFB22222;  // Firebrick
            ColorTable[43] = 0xFFFFFAF0;  // FloralWhite
            ColorTable[44] = 0xFF228B22;  // ForestGreen
            ColorTable[45] = 0xFFFF00FF;  // Fuchsia
            ColorTable[46] = 0xFFDCDCDC;  // Gainsboro
            ColorTable[47] = 0xFFF8F8FF;  // GhostWhite
            ColorTable[48] = 0xFFFFD700;  // Gold
            ColorTable[49] = 0xFFDAA520;  // Goldenrod
            ColorTable[50] = 0xFF808080;  // Gray
            ColorTable[51] = 0xFF008000;  // Green
            ColorTable[52] = 0xFFADFF2F;  // GreenYellow
            ColorTable[53] = 0xFFF0FFF0;  // Honeydew
            ColorTable[54] = 0xFFFF69B4;  // HotPink
            ColorTable[55] = 0xFFCD5C5C;  // IndianRed
            ColorTable[56] = 0xFF4B0082;  // Indigo
            ColorTable[57] = 0xFFFFFFF0;  // Ivory
            ColorTable[58] = 0xFFF0E68C;  // Khaki
            ColorTable[59] = 0xFFE6E6FA;  // Lavender
            ColorTable[60] = 0xFFFFF0F5;  // LavenderBlush
            ColorTable[61] = 0xFF7CFC00;  // LawnGreen
            ColorTable[62] = 0xFFFFFACD;  // LemonChiffon
            ColorTable[63] = 0xFFADD8E6;  // LightBlue
            ColorTable[64] = 0xFFF08080;  // LightCoral
            ColorTable[65] = 0xFFE0FFFF;  // LightCyan
            ColorTable[66] = 0xFFFAFAD2;  // LightGoldenrodYellow
            ColorTable[67] = 0xFFD3D3D3;  // LightGray
            ColorTable[68] = 0xFF90EE90;  // LightGreen
            ColorTable[69] = 0xFFFFB6C1;  // LightPink
            ColorTable[70] = 0xFFFFA07A;  // LightSalmon
            ColorTable[71] = 0xFF20B2AA;  // LightSeaGreen
            ColorTable[72] = 0xFF87CEFA;  // LightSkyBlue
            ColorTable[73] = 0xFF778899;  // LightSlateGray
            ColorTable[74] = 0xFFB0C4DE;  // LightSteelBlue
            ColorTable[75] = 0xFFFFFFE0;  // LightYellow
            ColorTable[76] = 0xFF00FF00;  // Lime
            ColorTable[77] = 0xFF32CD32;  // LimeGreen
            ColorTable[78] = 0xFFFAF0E6;  // Linen
            ColorTable[79] = 0xFFFF00FF;  // Magenta
            ColorTable[80] = 0xFF800000;  // Maroon
            ColorTable[81] = 0xFF66CDAA;  // MediumAquamarine
            ColorTable[82] = 0xFF0000CD;  // MediumBlue
            ColorTable[83] = 0xFFBA55D3;  // MediumOrchid
            ColorTable[84] = 0xFF9370DB;  // MediumPurple
            ColorTable[85] = 0xFF3CB371;  // MediumSeaGreen
            ColorTable[86] = 0xFF7B68EE;  // MediumSlateBlue
            ColorTable[87] = 0xFF00FA9A;  // MediumSpringGreen
            ColorTable[88] = 0xFF48D1CC;  // MediumTurquoise
            ColorTable[89] = 0xFFC71585;  // MediumVioletRed
            ColorTable[90] = 0xFF191970;  // MidnightBlue
            ColorTable[91] = 0xFFF5FFFA;  // MintCream
            ColorTable[92] = 0xFFFFE4E1;  // MistyRose
            ColorTable[93] = 0xFFFFE4B5;  // Moccasin
            ColorTable[94] = 0xFFFFDEAD;  // NavajoWhite
            ColorTable[95] = 0xFF000080;  // Navy
            ColorTable[96] = 0xFFFDF5E6;  // OldLace
            ColorTable[97] = 0xFF808000;  // Olive
            ColorTable[98] = 0xFF6B8E23;  // OliveDrab
            ColorTable[99] = 0xFFFFA500;  // Orange
            ColorTable[100] = 0xFFFF4500;  // OrangeRed
            ColorTable[101] = 0xFFDA70D6;  // Orchid
            ColorTable[102] = 0xFFEEE8AA;  // PaleGoldenrod
            ColorTable[103] = 0xFF98FB98;  // PaleGreen
            ColorTable[104] = 0xFFAFEEEE;  // PaleTurquoise
            ColorTable[105] = 0xFFDB7093;  // PaleVioletRed
            ColorTable[106] = 0xFFFFEFD5;  // PapayaWhip
            ColorTable[107] = 0xFFFFDAB9;  // PeachPuff
            ColorTable[108] = 0xFFCD853F;  // Peru
            ColorTable[109] = 0xFFFFC0CB;  // Pink
            ColorTable[110] = 0xFFDDA0DD;  // Plum
            ColorTable[111] = 0xFFB0E0E6;  // PowderBlue
            ColorTable[112] = 0xFF800080;  // Purple
            ColorTable[113] = 0xFFFF0000;  // Red
            ColorTable[114] = 0xFFBC8F8F;  // RosyBrown
            ColorTable[115] = 0xFF4169E1;  // RoyalBlue
            ColorTable[116] = 0xFF8B4513;  // SaddleBrown
            ColorTable[117] = 0xFFFA8072;  // Salmon
            ColorTable[118] = 0xFFF4A460;  // SandyBrown
            ColorTable[119] = 0xFF2E8B57;  // SeaGreen
            ColorTable[120] = 0xFFFFF5EE;  // SeaShell
            ColorTable[121] = 0xFFA0522D;  // Sienna
            ColorTable[122] = 0xFFC0C0C0;  // Silver
            ColorTable[123] = 0xFF87CEEB;  // SkyBlue
            ColorTable[124] = 0xFF6A5ACD;  // SlateBlue
            ColorTable[125] = 0xFF708090;  // SlateGray
            ColorTable[126] = 0xFFFFFAFA;  // Snow
            ColorTable[127] = 0xFF00FF7F;  // SpringGreen
            ColorTable[128] = 0xFF4682B4;  // SteelBlue
            ColorTable[129] = 0xFFD2B48C;  // Tan
            ColorTable[130] = 0xFF008080;  // Teal
            ColorTable[131] = 0xFFD8BFD8;  // Thistle
            ColorTable[132] = 0xFFFF6347;  // Tomato
            ColorTable[133] = 0x00FFFFFF;  // Transparent
            ColorTable[134] = 0xFF40E0D0;  // Turquoise
            ColorTable[135] = 0xFFEE82EE;  // Violet
            ColorTable[136] = 0xFFF5DEB3;  // Wheat
            ColorTable[137] = 0xFFFFFFFF;  // White
            ColorTable[138] = 0xFFF5F5F5;  // WhiteSmoke
            ColorTable[139] = 0xFFFFFF00;  // Yellow
            ColorTable[140] = 0xFF9ACD32;  // YellowGreen
        }

        internal static uint[] ColorTable = new uint[141];
    }
}
