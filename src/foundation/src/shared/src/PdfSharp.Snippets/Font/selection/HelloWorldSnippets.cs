// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#define VARIANT1  // Walrod, Battlestar, X-Files
#define VARIANT2_  // Walrod, Battlestar, X-Files

#define TIMES_NEW_ROMAN_  // Times New Roman
#define SEGOE_UI  // Segoe UI
#define SEGOE_UI_2_  // Segoe UI via platform API

// Show box.
#define BOX1
#define BOX2_
#define BOX3_
#define BOX4_
#define BOX5_
#define BOX6_
#define BOX7_
#define BOX8_   

#define FRUTIGER_VIA_WPF_FONTFAMILY

using System;
using System.Diagnostics;
using System.Reflection;
//#if CORE || GDI
#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
#if UWP
using Windows.UI.Xaml.Media;
#endif
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Font
{
    public class HelloWorld1Snippet : Snippet
    {
        const string Text = "Sphinx";
        const string Text2 = "Sphinx abc test";
        const double EmSize = 24;
#if true
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.WinAnsi);
#else
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
#endif

        static IFontResolver fs = new SegoeWpFontResolver();

        //#if TIMES_NEW_ROMAN
        const string FamilyName = "Segoe WP";
        //#elif SEGOE_UI
        //        const string FamilyName = "Segoe UI";
        //#endif

        public HelloWorld1Snippet()
        {
            NoText = true;
            //Cleanroom = true;
            Title = "Installed Font Selection";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            GlobalFontSettings.FontResolver = fs;

            //  XPoint pos;

            var fntRegular = new XFont(FamilyName, EmSize, XFontStyleEx.Regular, _fontOptions);

#if BOX1
            BeginBox(gfx, 1, BoxOptions.Tile, "Native typefaces of " + FamilyName);
            {
                gfx.DrawString(Text, fntRegular, XBrushes.DarkBlue, new XPoint(16, 32));
                //gfx.DrawString(Text, fntBold, XBrushes.DarkBlue, new XPoint(16, 64));
                //gfx.DrawString(Text, fntItalic, XBrushes.DarkBlue, new XPoint(16, 96));
                //gfx.DrawString(Text, fntBoldItalic, XBrushes.DarkBlue, new XPoint(16, 128));
            }
            EndBox(gfx);
#endif

#if BOX2
            BeginBox(gfx, 2, BoxOptions.Tile, FamilyName + " with simulated styles");
            {
                gfx.DrawString(Text, fntRegular, XBrushes.DarkBlue, new XPoint(16, 32));
                gfx.DrawString(Text, fntBoldSimulated, XBrushes.DarkBlue, new XPoint(16, 64));
                gfx.DrawString(Text, fntItalicSimulated, XBrushes.DarkBlue, new XPoint(16, 96));
                gfx.DrawString(Text, fntBoldItalicSimulated, XBrushes.DarkBlue, new XPoint(16, 128));
            }
            EndBox(gfx);
#endif

#if BOX3
            BeginBox(gfx, 3, BoxOptions.Tile, "Typefaces of " + FamilyName);
            {
                gfx.DrawString(Text, fntRegular, XBrushes.DarkBlue, new XPoint(16, 32));
                gfx.DrawString(Text, fntBold, XBrushes.DarkBlue, new XPoint(16, 64));
                gfx.DrawString(Text, fntItalic, XBrushes.DarkBlue, new XPoint(16, 96));
                gfx.DrawString(Text, fntBoldItalic2, XBrushes.DarkBlue, new XPoint(16, 132));
#if SEGOE_UI___
                gfx.DrawString(Text + "1", segoeRegular, XBrushes.DarkOrange, new XPoint(12, 32));
                gfx.DrawString(Text + "2", segoeBold, XBrushes.DarkOrange, new XPoint(12, 64));
                gfx.DrawString(Text + "3", segoeItalic, XBrushes.DarkOrange, new XPoint(12, 96));
                gfx.DrawString(Text + "4", segoeBoldItalic, XBrushes.DarkOrange, new XPoint(12, 128));
#endif
            }
            EndBox(gfx);
#endif

#if BOX4
            BeginBox(gfx, 4, BoxOptions.Tile, "Decoration of " + FamilyName);
            {
                gfx.DrawString(Text, fntRegularUnderline, XBrushes.DarkBlue, new XPoint(16, 32));
                gfx.DrawString(Text, fntBoldStrikeout, XBrushes.DarkBlue, new XPoint(16, 64));
                gfx.WriteComment("fntItalicStrikeoutUnderline");
                gfx.DrawString(Text, fntItalicStrikeoutUnderline, XBrushes.DarkBlue, new XPoint(16, 96));
                gfx.DrawString(Text, fntBoldItalic2, XBrushes.DarkBlue, new XPoint(16, 132));
            }
            EndBox(gfx);
#endif

#if BOX5
            BeginBox(gfx, 5, BoxOptions.Tile, FamilyName + " with measures");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, fntRegular, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text, fntRegular, pos);

                pos = new XPoint(16, 64);
                gfx.DrawString(Text, fntBold, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text, fntBold, pos);

                pos = new XPoint(16, 96);
                gfx.DrawString(Text, fntItalic, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text, fntItalic, pos);

                pos = new XPoint(16, 128);
                gfx.DrawString(Text, fntBoldItalic, XBrushes.DarkBlue, pos);
                gfx.DrawMeasureBox(Text, fntBoldItalic, pos);

                //#if SEGOE_UI_2
                //                gfx.DrawString(Text, segoeRegular2, XBrushes.DarkOrange, new XPoint(12, 32));
                //                gfx.DrawString(Text, segoeBold2, XBrushes.DarkOrange, new XPoint(12, 64));
                //                gfx.DrawString(Text, segoeItalic2, XBrushes.DarkOrange, new XPoint(12, 96));
                //                gfx.DrawString(Text, segoeBoldItalic2, XBrushes.DarkOrange, new XPoint(12, 128));
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
        }
    }
}
