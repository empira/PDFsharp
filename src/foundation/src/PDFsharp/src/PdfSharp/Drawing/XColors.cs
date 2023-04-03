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
    ///<summary>
    /// Represents a set of 141 pre-defined RGB colors. Incidentally the values are the same
    /// as in System.Drawing.Color.
    /// </summary>
    public static class XColors
    {
        ///<summary>Gets a predefined color.</summary>
        public static XColor AliceBlue => new XColor(XKnownColor.AliceBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor AntiqueWhite => new XColor(XKnownColor.AntiqueWhite);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Aqua => new XColor(XKnownColor.Aqua);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Aquamarine => new XColor(XKnownColor.Aquamarine);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Azure => new XColor(XKnownColor.Azure);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Beige => new XColor(XKnownColor.Beige);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Bisque => new XColor(XKnownColor.Bisque);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Black => new XColor(XKnownColor.Black);

        ///<summary>Gets a predefined color.</summary>
        public static XColor BlanchedAlmond => new XColor(XKnownColor.BlanchedAlmond);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Blue => new XColor(XKnownColor.Blue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor BlueViolet => new XColor(XKnownColor.BlueViolet);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Brown => new XColor(XKnownColor.Brown);

        ///<summary>Gets a predefined color.</summary>
        public static XColor BurlyWood => new XColor(XKnownColor.BurlyWood);

        ///<summary>Gets a predefined color.</summary>
        public static XColor CadetBlue => new XColor(XKnownColor.CadetBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Chartreuse => new XColor(XKnownColor.Chartreuse);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Chocolate => new XColor(XKnownColor.Chocolate);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Coral => new XColor(XKnownColor.Coral);

        ///<summary>Gets a predefined color.</summary>
        public static XColor CornflowerBlue => new XColor(XKnownColor.CornflowerBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Cornsilk => new XColor(XKnownColor.Cornsilk);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Crimson => new XColor(XKnownColor.Crimson);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Cyan => new XColor(XKnownColor.Cyan);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkBlue => new XColor(XKnownColor.DarkBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkCyan => new XColor(XKnownColor.DarkCyan);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkGoldenrod => new XColor(XKnownColor.DarkGoldenrod);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkGray => new XColor(XKnownColor.DarkGray);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkGreen => new XColor(XKnownColor.DarkGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkKhaki => new XColor(XKnownColor.DarkKhaki);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkMagenta => new XColor(XKnownColor.DarkMagenta);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkOliveGreen => new XColor(XKnownColor.DarkOliveGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkOrange => new XColor(XKnownColor.DarkOrange);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkOrchid => new XColor(XKnownColor.DarkOrchid);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkRed => new XColor(XKnownColor.DarkRed);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkSalmon => new XColor(XKnownColor.DarkSalmon);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkSeaGreen => new XColor(XKnownColor.DarkSeaGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkSlateBlue => new XColor(XKnownColor.DarkSlateBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkSlateGray => new XColor(XKnownColor.DarkSlateGray);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkTurquoise => new XColor(XKnownColor.DarkTurquoise);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DarkViolet => new XColor(XKnownColor.DarkViolet);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DeepPink => new XColor(XKnownColor.DeepPink);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DeepSkyBlue => new XColor(XKnownColor.DeepSkyBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DimGray => new XColor(XKnownColor.DimGray);

        ///<summary>Gets a predefined color.</summary>
        public static XColor DodgerBlue => new XColor(XKnownColor.DodgerBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Firebrick => new XColor(XKnownColor.Firebrick);

        ///<summary>Gets a predefined color.</summary>
        public static XColor FloralWhite => new XColor(XKnownColor.FloralWhite);

        ///<summary>Gets a predefined color.</summary>
        public static XColor ForestGreen => new XColor(XKnownColor.ForestGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Fuchsia => new XColor(XKnownColor.Fuchsia);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Gainsboro => new XColor(XKnownColor.Gainsboro);

        ///<summary>Gets a predefined color.</summary>
        public static XColor GhostWhite => new XColor(XKnownColor.GhostWhite);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Gold => new XColor(XKnownColor.Gold);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Goldenrod => new XColor(XKnownColor.Goldenrod);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Gray => new XColor(XKnownColor.Gray);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Green => new XColor(XKnownColor.Green);

        ///<summary>Gets a predefined color.</summary>
        public static XColor GreenYellow => new XColor(XKnownColor.GreenYellow);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Honeydew => new XColor(XKnownColor.Honeydew);

        ///<summary>Gets a predefined color.</summary>
        public static XColor HotPink => new XColor(XKnownColor.HotPink);

        ///<summary>Gets a predefined color.</summary>
        public static XColor IndianRed => new XColor(XKnownColor.IndianRed);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Indigo => new XColor(XKnownColor.Indigo);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Ivory => new XColor(XKnownColor.Ivory);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Khaki => new XColor(XKnownColor.Khaki);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Lavender => new XColor(XKnownColor.Lavender);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LavenderBlush => new XColor(XKnownColor.LavenderBlush);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LawnGreen => new XColor(XKnownColor.LawnGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LemonChiffon => new XColor(XKnownColor.LemonChiffon);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightBlue => new XColor(XKnownColor.LightBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightCoral => new XColor(XKnownColor.LightCoral);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightCyan => new XColor(XKnownColor.LightCyan);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightGoldenrodYellow => new XColor(XKnownColor.LightGoldenrodYellow);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightGray => new XColor(XKnownColor.LightGray);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightGreen => new XColor(XKnownColor.LightGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightPink => new XColor(XKnownColor.LightPink);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightSalmon => new XColor(XKnownColor.LightSalmon);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightSeaGreen => new XColor(XKnownColor.LightSeaGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightSkyBlue => new XColor(XKnownColor.LightSkyBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightSlateGray => new XColor(XKnownColor.LightSlateGray);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightSteelBlue => new XColor(XKnownColor.LightSteelBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LightYellow => new XColor(XKnownColor.LightYellow);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Lime => new XColor(XKnownColor.Lime);

        ///<summary>Gets a predefined color.</summary>
        public static XColor LimeGreen => new XColor(XKnownColor.LimeGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Linen => new XColor(XKnownColor.Linen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Magenta => new XColor(XKnownColor.Magenta);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Maroon => new XColor(XKnownColor.Maroon);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MediumAquamarine => new XColor(XKnownColor.MediumAquamarine);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MediumBlue => new XColor(XKnownColor.MediumBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MediumOrchid => new XColor(XKnownColor.MediumOrchid);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MediumPurple => new XColor(XKnownColor.MediumPurple);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MediumSeaGreen => new XColor(XKnownColor.MediumSeaGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MediumSlateBlue => new XColor(XKnownColor.MediumSlateBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MediumSpringGreen => new XColor(XKnownColor.MediumSpringGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MediumTurquoise => new XColor(XKnownColor.MediumTurquoise);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MediumVioletRed => new XColor(XKnownColor.MediumVioletRed);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MidnightBlue => new XColor(XKnownColor.MidnightBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MintCream => new XColor(XKnownColor.MintCream);

        ///<summary>Gets a predefined color.</summary>
        public static XColor MistyRose => new XColor(XKnownColor.MistyRose);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Moccasin => new XColor(XKnownColor.Moccasin);

        ///<summary>Gets a predefined color.</summary>
        public static XColor NavajoWhite => new XColor(XKnownColor.NavajoWhite);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Navy => new XColor(XKnownColor.Navy);

        ///<summary>Gets a predefined color.</summary>
        public static XColor OldLace => new XColor(XKnownColor.OldLace);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Olive => new XColor(XKnownColor.Olive);

        ///<summary>Gets a predefined color.</summary>
        public static XColor OliveDrab => new XColor(XKnownColor.OliveDrab);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Orange => new XColor(XKnownColor.Orange);

        ///<summary>Gets a predefined color.</summary>
        public static XColor OrangeRed => new XColor(XKnownColor.OrangeRed);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Orchid => new XColor(XKnownColor.Orchid);

        ///<summary>Gets a predefined color.</summary>
        public static XColor PaleGoldenrod => new XColor(XKnownColor.PaleGoldenrod);

        ///<summary>Gets a predefined color.</summary>
        public static XColor PaleGreen => new XColor(XKnownColor.PaleGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor PaleTurquoise => new XColor(XKnownColor.PaleTurquoise);

        ///<summary>Gets a predefined color.</summary>
        public static XColor PaleVioletRed => new XColor(XKnownColor.PaleVioletRed);

        ///<summary>Gets a predefined color.</summary>
        public static XColor PapayaWhip => new XColor(XKnownColor.PapayaWhip);

        ///<summary>Gets a predefined color.</summary>
        public static XColor PeachPuff => new XColor(XKnownColor.PeachPuff);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Peru => new XColor(XKnownColor.Peru);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Pink => new XColor(XKnownColor.Pink);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Plum => new XColor(XKnownColor.Plum);

        ///<summary>Gets a predefined color.</summary>
        public static XColor PowderBlue => new XColor(XKnownColor.PowderBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Purple => new XColor(XKnownColor.Purple);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Red => new XColor(XKnownColor.Red);

        ///<summary>Gets a predefined color.</summary>
        public static XColor RosyBrown => new XColor(XKnownColor.RosyBrown);

        ///<summary>Gets a predefined color.</summary>
        public static XColor RoyalBlue => new XColor(XKnownColor.RoyalBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor SaddleBrown => new XColor(XKnownColor.SaddleBrown);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Salmon => new XColor(XKnownColor.Salmon);

        ///<summary>Gets a predefined color.</summary>
        public static XColor SandyBrown => new XColor(XKnownColor.SandyBrown);

        ///<summary>Gets a predefined color.</summary>
        public static XColor SeaGreen => new XColor(XKnownColor.SeaGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor SeaShell => new XColor(XKnownColor.SeaShell);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Sienna => new XColor(XKnownColor.Sienna);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Silver => new XColor(XKnownColor.Silver);

        ///<summary>Gets a predefined color.</summary>
        public static XColor SkyBlue => new XColor(XKnownColor.SkyBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor SlateBlue => new XColor(XKnownColor.SlateBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor SlateGray => new XColor(XKnownColor.SlateGray);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Snow => new XColor(XKnownColor.Snow);

        ///<summary>Gets a predefined color.</summary>
        public static XColor SpringGreen => new XColor(XKnownColor.SpringGreen);

        ///<summary>Gets a predefined color.</summary>
        public static XColor SteelBlue => new XColor(XKnownColor.SteelBlue);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Tan => new XColor(XKnownColor.Tan);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Teal => new XColor(XKnownColor.Teal);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Thistle => new XColor(XKnownColor.Thistle);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Tomato => new XColor(XKnownColor.Tomato);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Transparent => new XColor(XKnownColor.Transparent);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Turquoise => new XColor(XKnownColor.Turquoise);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Violet => new XColor(XKnownColor.Violet);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Wheat => new XColor(XKnownColor.Wheat);

        ///<summary>Gets a predefined color.</summary>
        public static XColor White => new XColor(XKnownColor.White);

        ///<summary>Gets a predefined color.</summary>
        public static XColor WhiteSmoke => new XColor(XKnownColor.WhiteSmoke);

        ///<summary>Gets a predefined color.</summary>
        public static XColor Yellow => new XColor(XKnownColor.Yellow);

        ///<summary>Gets a predefined color.</summary>
        public static XColor YellowGreen => new XColor(XKnownColor.YellowGreen);
    }
}
