// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// Show box.
#define BOX1
#define BOX2
#define BOX3
#define BOX4
#define BOX5
#define BOX6_
#define BOX7_
#define BOX8_

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
#if CORE || GDI
using System.Drawing;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Font
{
    public class PrivateFontCollection1Snippet : Snippet
    {
#if true
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.WinAnsi);
#else
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
#endif

        const string FamilyNameA = "Arial";
        const string FamilyNameT = "Tahoma";
        const double EmSize = 12;

        public PrivateFontCollection1Snippet()
        {
            //NoText = true;
            //Cleanroom = true;
            Title = "Private Font Selection (GDI only)";
            NoText = true;
        }

        public override void RenderSnippet(XGraphics gfx)
        {
#if GDI
            //XPoint pos;
            const string text = "We do a test de do do do de da da da.";

            // We add Regular and Bold fonts to a private font collection.
            // PDFsharp 1.50 beta 3 uses the regular font with simulated bold and ignores the bold fonts in the PFC.

#if true
            // There is one problem with Arial and one problem with Tahoma.
            // Maybe it’s the same bug, but showing two different effects.
            // Add Arial and Tahoma from the Fonts directory to the private font collection.
#if true
            // Register regular first.
            string[] fonts = {
                                "arial.ttf", "arialbd.ttf",
                                "TAHOMA.ttf", "TAHOMABD.ttf"
                             };
#else
            // Register bold first.
            // BOX3 causes an exception if bold fonts are loaded before regular fonts.
            string[] fonts = {
                                "arialbd.ttf", "arial.ttf", //"ariali.ttf", "arialbi.ttf", 
                                "TAHOMABD.ttf", "TAHOMA.ttf",
                             };
#endif

            var windir = Environment.GetFolderPath(Environment.SpecialFolder.System);

            foreach (var font in fonts)
            {
                // We could use "Environment.GetEnvironmentVariable("windir")" and avoid the "..".
                string path = Path.Combine(windir + @"\..\fonts\", font);
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    //XPrivateFontCollection.Add(stream);
                }
            }
            Debug.WriteLine(FontsDevHelper.GetFontCachesState());

#endif

#if BOX1
            BeginBox(gfx, 1, BoxOptions.Tile, "Native typefaces of " + FamilyNameA);
            {
                // We have to simulate a MigraDoc problem. First call MeasureString, then call DrawString.
                // Use Regular first, then use Bold.

                XSize sizeRegular, sizeBold;

                {
                    var fntRegular = new XFont(FamilyNameA, EmSize, XFontStyleEx.Regular, _fontOptions);
                    sizeRegular = gfx.MeasureString(text, fntRegular);
                }

                {
                    var fntBold = new XFont(FamilyNameA, EmSize, XFontStyleEx.Bold, _fontOptions);
                    sizeBold = gfx.MeasureString(text, fntBold);
                }

                {
                    var fntRegular = new XFont(FamilyNameA, EmSize, XFontStyleEx.Regular, _fontOptions);
                    gfx.DrawString(text, fntRegular, XBrushes.DarkBlue, new XPoint(16, 32));
                    gfx.DrawRectangle(XPens.Red, 16, 32, sizeRegular.Width, -sizeRegular.Height);
                }

                {
                    var fntBold = new XFont(FamilyNameA, EmSize, XFontStyleEx.Bold, _fontOptions);
                    gfx.DrawString(text, fntBold, XBrushes.DarkBlue, new XPoint(16, 64));
                    gfx.DrawRectangle(XPens.Red, 16, 64, sizeBold.Width, -sizeBold.Height);
                }

                //gfx.DrawString(Text, fntItalic, XBrushes.DarkBlue, new XPoint(16, 96));
                //gfx.DrawString(Text, fntBoldItalic, XBrushes.DarkBlue, new XPoint(16, 128));
            }
            EndBox(gfx);
#endif

#if BOX2
            BeginBox(gfx, 2, BoxOptions.Tile, "Native typefaces of " + FamilyNameA);
            {
                // We have to simulate a MigraDoc problem. First call MeasureString, then call DrawString.
                // Use Bold first, then use Regular.

                XSize sizeRegular, sizeBold;

                {
                    var fntBold = new XFont(FamilyNameA, EmSize, XFontStyleEx.Bold, _fontOptions);
                    sizeBold = gfx.MeasureString(text, fntBold);
                }

                {
                    var fntRegular = new XFont(FamilyNameA, EmSize, XFontStyleEx.Regular, _fontOptions);
                    sizeRegular = gfx.MeasureString(text, fntRegular);
                }

                {
                    var fntBold = new XFont(FamilyNameA, EmSize, XFontStyleEx.Bold, _fontOptions);
                    gfx.DrawString(text, fntBold, XBrushes.DarkBlue, new XPoint(16, 64));
                    gfx.DrawRectangle(XPens.Red, 16, 64, sizeBold.Width, -sizeBold.Height);
                }

                {
                    var fntRegular = new XFont(FamilyNameA, EmSize, XFontStyleEx.Regular, _fontOptions);
                    gfx.DrawString(text, fntRegular, XBrushes.DarkBlue, new XPoint(16, 32));
                    gfx.DrawRectangle(XPens.Red, 16, 32, sizeRegular.Width, -sizeRegular.Height);
                }

                //gfx.DrawString(Text, fntItalic, XBrushes.DarkBlue, new XPoint(16, 96));
                //gfx.DrawString(Text, fntBoldItalic, XBrushes.DarkBlue, new XPoint(16, 128));
            }
            EndBox(gfx);
#endif

#if BOX3
            BeginBox(gfx, 3, BoxOptions.Tile, "Native typefaces of " + FamilyNameT);
            {
                // We have to simulate a MigraDoc problem. First call MeasureString, then call DrawString.
                // Use Regular first, then use Bold.

                XSize sizeRegular, sizeBold;

                {
                    var fntRegular = new XFont(FamilyNameT, EmSize, XFontStyleEx.Regular, _fontOptions);
                    sizeRegular = gfx.MeasureString(text, fntRegular);
                }

                {
                    var fntBold = new XFont(FamilyNameT, EmSize, XFontStyleEx.Bold, _fontOptions);
                    sizeBold = gfx.MeasureString(text, fntBold);
                }

                {
                    var fntRegular = new XFont(FamilyNameT, EmSize, XFontStyleEx.Regular, _fontOptions);
                    gfx.DrawString(text, fntRegular, XBrushes.DarkBlue, new XPoint(16, 32));
                    gfx.DrawRectangle(XPens.Red, 16, 32, sizeRegular.Width, -sizeRegular.Height);
                }

                {
                    var fntBold = new XFont(FamilyNameT, EmSize, XFontStyleEx.Bold, _fontOptions);
                    gfx.DrawString(text, fntBold, XBrushes.DarkBlue, new XPoint(16, 64));
                    gfx.DrawRectangle(XPens.Red, 16, 64, sizeBold.Width, -sizeBold.Height);
                }

                //gfx.DrawString(Text, fntItalic, XBrushes.DarkBlue, new XPoint(16, 96));
                //gfx.DrawString(Text, fntBoldItalic, XBrushes.DarkBlue, new XPoint(16, 128));
            }
            EndBox(gfx);
#endif

#if BOX4
            BeginBox(gfx, 4, BoxOptions.Tile, "Native typefaces of " + FamilyNameT);
            {
                // We have to simulate a MigraDoc problem. First call MeasureString, then call DrawString.
                // Use Bold first, then use Regular.

                XSize sizeRegular, sizeBold;

                {
                    var fntBold = new XFont(FamilyNameT, EmSize, XFontStyleEx.Bold, _fontOptions);
                    double descent = GetDescent(fntBold);
                    double singleLineSpace = fntBold.GetHeight();
                    //sizeBold = gfx.MeasureString(text, fntBold);
                }

                {
                    var fntRegular = new XFont(FamilyNameT, EmSize, XFontStyleEx.Regular, _fontOptions);
                    double descent = GetDescent(fntRegular);
                    double singleLineSpace = fntRegular.GetHeight();
                    //sizeRegular = gfx.MeasureString(text, fntRegular);
                }

                {
                    var fntBold = new XFont(FamilyNameT, EmSize, XFontStyleEx.Bold, _fontOptions);
                    //double descent = GetDescent(fntBold);
                    //double singleLineSpace = fntBold.GetHeight();
                    gfx.MeasureString("De", fntBold);
                    gfx.MeasureString("do", fntBold);
                    gfx.MeasureString("do", fntBold);
                    sizeBold = gfx.MeasureString(text, fntBold);
                }

                {
                    var fntRegular = new XFont(FamilyNameT, EmSize, XFontStyleEx.Regular, _fontOptions);
                    //double descent = GetDescent(fntRegular);
                    //double singleLineSpace = fntRegular.GetHeight();
                    gfx.MeasureString("De", fntRegular);
                    gfx.MeasureString("do", fntRegular);
                    gfx.MeasureString("do", fntRegular);
                    sizeRegular = gfx.MeasureString(text, fntRegular);
                }

                {
                    var fntBold = new XFont(FamilyNameT, EmSize, XFontStyleEx.Bold, _fontOptions);
                    gfx.DrawString(text, fntBold, XBrushes.DarkBlue, new XPoint(16, 64));
                    gfx.DrawRectangle(XPens.Red, 16, 64, sizeBold.Width, -sizeBold.Height);
                }

                {
                    var fntRegular = new XFont(FamilyNameT, EmSize, XFontStyleEx.Regular, _fontOptions);
                    gfx.DrawString(text, fntRegular, XBrushes.DarkBlue, new XPoint(16, 32));
                    gfx.DrawRectangle(XPens.Red, 16, 32, sizeRegular.Width, -sizeRegular.Height);
                }

                //gfx.DrawString(Text, fntItalic, XBrushes.DarkBlue, new XPoint(16, 96));
                //gfx.DrawString(Text, fntBoldItalic, XBrushes.DarkBlue, new XPoint(16, 128));
            }
            EndBox(gfx);
#endif

#if BOX5
            BeginBox(gfx, 5, BoxOptions.Tile, "Native typefaces of " + FamilyNameA);
            {
                // We have to simulate a MigraDoc problem. First call MeasureString, then call DrawString.
                // Use Bold only.

                XSize sizeRegular, sizeBold;

                {
                    var fntRegular = new XFont(FamilyNameA, EmSize, XFontStyleEx.Bold, _fontOptions);
                    sizeRegular = gfx.MeasureString(text, fntRegular);
                }

                {
                    var fntBold = new XFont(FamilyNameA, EmSize, XFontStyleEx.Bold, _fontOptions);
                    sizeBold = gfx.MeasureString(text, fntBold);
                }

                {
                    var fntRegular = new XFont(FamilyNameA, EmSize, XFontStyleEx.Bold, _fontOptions);
                    gfx.DrawString(text, fntRegular, XBrushes.DarkBlue, new XPoint(16, 32));
                    gfx.DrawRectangle(XPens.Red, 16, 32, sizeRegular.Width, -sizeRegular.Height);
                }

                {
                    var fntBold = new XFont(FamilyNameA, EmSize, XFontStyleEx.Bold, _fontOptions);
                    gfx.DrawString(text, fntBold, XBrushes.DarkBlue, new XPoint(16, 64));
                    gfx.DrawRectangle(XPens.Red, 16, 64, sizeBold.Width, -sizeBold.Height);
                }

                //gfx.DrawString(Text, fntItalic, XBrushes.DarkBlue, new XPoint(16, 96));
                //gfx.DrawString(Text, fntBoldItalic, XBrushes.DarkBlue, new XPoint(16, 128));
            }
            EndBox(gfx);
#endif

#if BOX6
            BeginBox(gfx, 6, BoxOptions.Tile, FamilyName + " with simulated styles & measures");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, fntRegular, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text, fntRegular, pos);

                pos = new XPoint(16, 64);
                gfx.DrawString(Text, fntBoldSimulated, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text, fntBoldSimulated, pos);

                pos = new XPoint(16, 96);
                gfx.DrawString(Text, fntItalicSimulated, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text, fntItalicSimulated, pos);

                pos = new XPoint(16, 128);
                gfx.DrawString(Text, fntBoldItalicSimulated, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text, fntBoldItalicSimulated, pos);
            }
            EndBox(gfx);
#endif

#if BOX7
            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text2, fntRegular, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text2, fntRegular, pos);

                pos = new XPoint(16, 64);
                gfx.DrawString(Text2, fntBold, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text2, fntBold, pos);

                pos = new XPoint(16, 96);
                gfx.DrawString(Text2, fntItalic, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text2, fntItalicSimulated, pos);

                pos = new XPoint(16, 128);
                gfx.DrawString(Text2, fntBoldItalic, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text2, fntBoldItalic, pos);
            }
            EndBox(gfx);
#endif

#if BOX8
            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text2, fntRegular, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text2, fntRegular, pos);

                pos = new XPoint(16, 64);
                gfx.DrawString(Text2, fntBoldSimulated, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text2, fntBoldSimulated, pos);

                pos = new XPoint(16, 96);
                gfx.DrawString(Text2, fntItalicSimulated, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text2, fntItalicSimulated, pos);

                pos = new XPoint(16, 128);
                gfx.DrawString(Text2, fntBoldItalicSimulated, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text2, fntBoldItalicSimulated, pos);
            }
            EndBox(gfx);
#endif

#endif
        }
        internal static double GetDescent(XFont font)
        {
            double descent = font.Metrics.Descent;
            descent *= font.Size;
            descent /= font.FontFamily.GetEmHeight(font.Style);
            return descent;
        }
    }
}
