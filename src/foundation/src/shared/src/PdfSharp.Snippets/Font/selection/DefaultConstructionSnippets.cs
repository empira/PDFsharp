// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#define VARIANT1  // Walrod, Battlestar, X-Files
#define VARIANT2_  // Walrod, Battlestar, X-Files

#define TIMES_NEW_ROMAN_  // Times New Roman
#define SEGOE_UI  // Segoe UI
#define SEGOE_UI_2_  // Segoe UI via platform API

// Show box.
#define BOX1
#define BOX2
#define BOX3
#define BOX4
#define BOX5
#define BOX6
#define BOX7
#define BOX8     

#define FRUTIGER_VIA_WPF_FONTFAMILY

using System;
using System.Diagnostics;
using System.Reflection;
//#if CORE || GDI
#if GDI
using System.Drawing;
using GdiFont = System.Drawing.Font;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
#if WUI
using Windows.UI.Xaml.Media;
#endif
using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Font
{
    public class DefaultWindowsFontsSnippet : Snippet
    {
        const string Text = "Sphinx";
        const string Text2 = "Sphinx abc test";
        const double EmSize = 24;
#if true
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.WinAnsi);
#else
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
#endif

#if TIMES_NEW_ROMAN
        const string FamilyName = "Times New Roman";
#elif SEGOE_UI
        const string FamilyName = "Segoe UI";
#endif

        public DefaultWindowsFontsSnippet()
        {
            //NoText = true;
            //Cleanroom = true;
            Title = "Installed Font Selection";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            XPoint pos;

#if true || CORE || GDI || WPF || WUI
            var fntRegular = new XFont(FamilyName, EmSize, XFontStyleEx.Regular, _fontOptions);
            var fntBold = new XFont(FamilyName, EmSize, XFontStyleEx.Bold, _fontOptions);
            var fntItalic = new XFont(FamilyName, EmSize, XFontStyleEx.Italic, _fontOptions);
            var fntBoldItalic = new XFont(FamilyName, EmSize, XFontStyleEx.BoldItalic, _fontOptions);

            var fntRegularUnderline = new XFont(FamilyName, EmSize, XFontStyleEx.Underline, _fontOptions);
            var fntBoldStrikeout = new XFont(FamilyName, EmSize, XFontStyleEx.Bold | XFontStyleEx.Strikeout, _fontOptions);
            var fntItalicStrikeoutUnderline = new XFont(FamilyName, EmSize,
                XFontStyleEx.Italic | XFontStyleEx.Strikeout | XFontStyleEx.Underline, _fontOptions);
            var fntBoldItalic2 = new XFont(FamilyName, EmSize, XFontStyleEx.BoldItalic, _fontOptions);

            var fntBoldSimulated = FontsDevHelper.CreateSpecialFont(FamilyName, EmSize, XFontStyleEx.Regular, _fontOptions,
                XStyleSimulations.BoldSimulation);
            var fntItalicSimulated = FontsDevHelper.CreateSpecialFont(FamilyName, EmSize, XFontStyleEx.Regular,
                _fontOptions, XStyleSimulations.ItalicSimulation);
            var fntBoldItalicSimulated = FontsDevHelper.CreateSpecialFont(FamilyName, EmSize, XFontStyleEx.Regular,
                _fontOptions, XStyleSimulations.BoldItalicSimulation);
#endif
#if CORE || GDI
            //var segoeRegular2 = new XFont(new Font("Segoe UI", 32, FontStyle.Regular, GraphicsUnit.World));
            //var segoeBold2 = new XFont(new Font("Segoe UI", 32, FontStyle.Bold, GraphicsUnit.World));
            //var segoeItalic2 = new XFont(new Font("Segoe UI", 32, FontStyle.Italic, GraphicsUnit.World));
            //var segoeBoldItalic2 = new XFont(new Font("Segoe UI", 32, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.World));
#endif
#if WPF
            //GlyphTypeface glyphTypeface = null;
            //var times = new XFont(glyphTypeface, new Font("Times New Roman", 10), null);
            var times = new XFont("Times New Roman", 10);
            var times2 = new XFont("Times New Roman", 10);
            var segoeFamily = new FontFamily("Segoe UI");
            var segoeRegular2 =
                new XFont(new Typeface(segoeFamily, FontStyles.Normal, FontWeights.Regular, FontStretches.Normal), 32);
            var segoeBold2 =
                new XFont(new Typeface(segoeFamily, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), 32);
            var segoeItalic2 =
                new XFont(new Typeface(segoeFamily, FontStyles.Italic, FontWeights.Regular, FontStretches.Normal), 32);
            var segoeOblique2 =
                new XFont(new Typeface(segoeFamily, FontStyles.Oblique, FontWeights.Regular, FontStretches.Normal), 32);
            var segoeBoldItalic2 =
                new XFont(new Typeface(segoeFamily, FontStyles.Italic, FontWeights.Bold, FontStretches.Normal), 32);
#endif

#if BOX1
            BeginBox(gfx, 1, BoxOptions.Tile, "Native typefaces of " + FamilyName);
            {
                gfx.DrawString(Text, fntRegular, XBrushes.DarkBlue, new XPoint(16, 32));
                gfx.DrawString(Text, fntBold, XBrushes.DarkBlue, new XPoint(16, 64));
                gfx.DrawString(Text, fntItalic, XBrushes.DarkBlue, new XPoint(16, 96));
                gfx.DrawString(Text, fntBoldItalic, XBrushes.DarkBlue, new XPoint(16, 128));
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

    public class FontFamilyConstructionSnippet : Snippet
    {
        const string Text = "Sphinx";

        public FontFamilyConstructionSnippet()
        {
            NoText = true;
            //Title = "Line Types";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
#if CORE || GDI || WUI
#if TIMES_NEW_ROMAN
#if !WUI
            var times = new FontFamily("Times New Roman");
#else
            var times = "Times New Roman";
#endif
            var timesRegular = new XFont(times, 32, XFontStyleEx.Bold);
            var timesBold = new XFont(times, 32, XFontStyleEx.Bold);
            var timesItalic = new XFont(times, 32, XFontStyleEx.Italic);
            var timesBoldItalic = new XFont(times, 32, XFontStyleEx.BoldItalic);

            var timesRegularUnderline = new XFont(times, 32, XFontStyleEx.Underline);
            var timesBoldStrikeout = new XFont(times, 32, XFontStyleEx.Bold | XFontStyleEx.Strikeout);
            var timesItalicStrikeoutUnderline = new XFont(times, 32, XFontStyleEx.Italic | XFontStyleEx.Strikeout | XFontStyleEx.Underline);
            var timesBoldItalic2 = new XFont(times, 42, XFontStyleEx.BoldItalic);
#endif
#if SEGOE_UI
#if !WUI && false
            var segoe = new FontFamily("Segoe UI");
#else
            var segoe = "Segoe UI";
#endif
            var segoeRegular = new XFont(segoe, 32, XFontStyleEx.Regular);
            var segoeBold = new XFont(segoe, 32, XFontStyleEx.Bold);
            var segoeItalic = new XFont(segoe, 32, XFontStyleEx.Italic);
            var segoeBoldItalic = new XFont(segoe, 32, XFontStyleEx.BoldItalic);
#endif
#if SEGOE_UI_2
            var segoeLight = new FontFamily("Segoe UI Light");
            var segoeRegularLight = new XFont(segoeLight, 32, XFontStyleEx.Regular);
            var segoeBoldLight = new XFont(segoeLight, 32, XFontStyleEx.Bold);
            var segoeItalicLight = new XFont(segoeLight, 32, XFontStyleEx.Italic);
            var segoeBoldItalicLight = new XFont(segoeLight, 32, XFontStyleEx.BoldItalic);
#endif
#endif
#if (CORE || GDI) && false
            var segoeRegular2 = new XFont(new GdiFont("Segoe UI", 32, FontStyle.Regular, GraphicsUnit.World));
            var segoeBold2 = new XFont(new GdiFont("Segoe UI", 32, FontStyle.Bold, GraphicsUnit.World));
            var segoeItalic2 = new XFont(new GdiFont("Segoe UI", 32, FontStyle.Italic, GraphicsUnit.World));
            var segoeBoldItalic2 = new XFont(new GdiFont("Segoe UI", 32, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.World));
#endif
#if WPF_
            GlyphTypeface glyphTypeface = null;
            //var times = new XFont(glyphTypeface, new Font("Times New Roman", 10), null);
            var times = new XFont("Times New Roman", 10);
            var times2 = new XFont("Times New Roman", 10);
            var segoeFamily=new FontFamily("Segoe UI");
            var segoeRegular2 = new XFont(new Typeface(segoeFamily,FontStyles.Normal, FontWeights.Regular,FontStretches.Normal), 32);
            var segoeBold2 = new XFont(new Typeface(segoeFamily, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), 32);
            var segoeItalic2 = new XFont(new Typeface(segoeFamily, FontStyles.Italic, FontWeights.Regular, FontStretches.Normal), 32);
            var segoeOblique2 = new XFont(new Typeface(segoeFamily, FontStyles.Oblique, FontWeights.Regular, FontStretches.Normal), 32);
            var segoeBoldItalic2 = new XFont(new Typeface(segoeFamily, FontStyles.Italic, FontWeights.Bold, FontStretches.Normal), 32);
#endif
#if WPF
#if TIMES_NEW_ROMAN
            FontFamily times = new FontFamily("Times New Roman");
            Typeface timesFace = new Typeface(times, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var timesRegular = new XFont(timesFace, 32);
            var timesBold = new XFont(times, 32, XFontStyleEx.Bold);
            var timesItalic = new XFont(times, 32, XFontStyleEx.Italic);
            var timesBoldItalic = new XFont(times, 32, XFontStyleEx.BoldItalic);

            var timesRegularUnderline = new XFont(times, 32, XFontStyleEx.Underline);
            var timesBoldStrikeout = new XFont(times, 32, XFontStyleEx.Bold | XFontStyleEx.Strikeout);
            var timesItalicStrikeoutUnderline = new XFont(times, 32,
                XFontStyleEx.Italic | XFontStyleEx.Strikeout | XFontStyleEx.Underline);
            var timesBoldItalic2 = new XFont(times, 42, XFontStyleEx.BoldItalic);

            //Typeface typeface = new Typeface("Segoe UI", FontStyles.);
            //var times = new XFont(typeface, 40);
            //var segoe = new XFontFamily("Segoe ui Semibold");
            //var sg =new XFont(segoe, 14, XFontStyleEx.Regular);
            //var times = new XFont("Times New Roman", 10);
            //  var times2 = new XFont("Times New Roman", 18);
#endif
#if SEGOE_UI_2
            var segoe = new FontFamily("Segoe UI");
            var segoeRegular = new XFont(segoe, 32, XFontStyleEx.Regular);
            //var segoeBold = new XFont(segoe, 32, XFontStyleEx.Bold);
            //var segoeItalic = new XFont(segoe, 32, XFontStyleEx.Italic);
            //var segoeBoldItalic = new XFont(segoe, 32, XFontStyleEx.BoldItalic);

            var segoeLight = new FontFamily("Segoe UI Light");
            var segoeRegularLight = new XFont(segoeLight, 32, XFontStyleEx.Regular);
            var segoeBoldLight = new XFont(segoeLight, 32, XFontStyleEx.Bold);
            //var segoeItalicLight = new XFont(segoeLight, 32, XFontStyleEx.Italic);
            //var segoeBoldItalicLight = new XFont(segoeLight, 32, XFontStyleEx.BoldItalic);
#endif

#endif

            BeginBox(gfx, 1, BoxOptions.Tile);
            {
#if TIMES_NEW_ROMAN
                gfx.DrawString(Text, timesRegular, XBrushes.DarkBlue, new XPoint(12, 32));
                gfx.DrawString(Text, timesBold, XBrushes.DarkBlue, new XPoint(12, 64));
                gfx.DrawString(Text, timesItalic, XBrushes.DarkBlue, new XPoint(12, 96));
                gfx.DrawString(Text, timesBoldItalic, XBrushes.DarkBlue, new XPoint(12, 128));
#endif
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
#if TIMES_NEW_ROMAN
                gfx.DrawString(Text, timesRegularUnderline, XBrushes.DarkBlue, new XPoint(12, 32));
                gfx.DrawString(Text, timesBoldStrikeout, XBrushes.DarkBlue, new XPoint(12, 64));
                gfx.DrawString(Text, timesItalicStrikeoutUnderline, XBrushes.DarkBlue, new XPoint(12, 96));
                gfx.DrawString(Text, timesBoldItalic2, XBrushes.DarkBlue, new XPoint(12, 132));
#endif
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
#if SEGOE_UI__
                gfx.DrawString(Text, segoeRegular, XBrushes.DarkOrange, new XPoint(12, 32));
                //gfx.DrawString(Text, segoeBold, XBrushes.DarkOrange, new XPoint(12, 64));
                //gfx.DrawString(Text, segoeItalic, XBrushes.DarkOrange, new XPoint(12, 96));
                //gfx.DrawString(Text, segoeBoldItalic, XBrushes.DarkOrange, new XPoint(12, 128));
#endif
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
#if SEGOE_UI_2
                gfx.DrawString(Text, segoeRegularLight, XBrushes.DarkOrange, new XPoint(12, 32));
                gfx.DrawString(Text, segoeBoldLight, XBrushes.DarkOrange, new XPoint(12, 64));
                //gfx.DrawString(Text, segoeItalicLight, XBrushes.DarkOrange, new XPoint(12, 96));
                //gfx.DrawString(Text, segoeBoldItalicLight, XBrushes.DarkOrange, new XPoint(12, 128));
#endif
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            {
            }
            EndBox(gfx);
        }
    }

    public class PlatformFontConstructionSnippet : Snippet
    {
        const string Text = "Sphinx";
        const string Text2 = "Sphinx abc test";
        const float EmSize = 24;
#if true
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.WinAnsi);
#else
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
#endif

#if TIMES_NEW_ROMAN
        const string FamilyName = "Times New Roman";
#elif SEGOE_UI
        const string FamilyName = "Segoe UI";
#endif

        public PlatformFontConstructionSnippet()
        {
            //NoText = true;
            //Cleanroom = true;
            Title = "Installed Font Selection";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            var brush = new XSolidBrush(XColor.FromArgb(0xA0, 0x00, 0x00, 0x80));
            XPoint pos;

#if CORE
            //var segoeUI = new FontFamily("Segoe UI");
            //var segoeUILight = new FontFamily("Segoe UI Light");
            //var segoeUISemilight = new FontFamily("Segoe UI Semilight");
            //var segoeUISemibold = new FontFamily("Segoe UI Semibold");
            //var segoeUIBlack = new FontFamily("Segoe UI Black");
            //var segoeCondensed = new FontFamily("Segoe Condensed");

            var fntRegular = new XFont("Segoe UI", EmSize, XFontStyleEx.Regular);
            var fntBold = new XFont("Segoe UI", EmSize, XFontStyleEx.Bold);
            var fntItalic = new XFont("Segoe UI", EmSize, XFontStyleEx.Italic);
            var fntBoldItalic = new XFont("Segoe UI", EmSize, XFontStyleEx.BoldItalic);

            var fntLight = new XFont("Segoe UI Light", EmSize, XFontStyleEx.Regular);
            var fntLightItalic = new XFont("Segoe UI Light", EmSize, XFontStyleEx.Italic);

            var fntSemiLight = new XFont("Segoe UI Semilight", EmSize, XFontStyleEx.Regular);
            var fntSemiLightItalic = new XFont("Segoe UI Semilight", EmSize, XFontStyleEx.Italic);

            var fntSemiBold = new XFont("Segoe UI Semibold", EmSize, XFontStyleEx.Regular);
            var fntSemiBoldItalic = new XFont("Segoe UI Semibold", EmSize, XFontStyleEx.Italic);

            var fntBlack = new XFont("Segoe UI Black", EmSize, XFontStyleEx.Regular);
            var fntBlackItalic = new XFont("Segoe UI Black", EmSize, XFontStyleEx.Italic);

            var fntCondenst = new XFont("Segoe Condensed", EmSize, XFontStyleEx.Regular);
            var fntCondenstBold = new XFont("Segoe Condensed", EmSize, XFontStyleEx.Bold);
            var fntCondenstItalic = new XFont("Segoe Condensed", EmSize, XFontStyleEx.Italic);
            var fntCondenstBoldItalic = new XFont("Segoe Condensed", EmSize, XFontStyleEx.BoldItalic);
#else
#if CORE || GDI
            var segoeUI = new FontFamily("Segoe UI");
            var segoeUILight = new FontFamily("Segoe UI Light");
            var segoeUISemilight = new FontFamily("Segoe UI Semilight");
            var segoeUISemibold = new FontFamily("Segoe UI Semibold");
            var segoeUIBlack = new FontFamily("Segoe UI Black");
            var segoeCondensed = new FontFamily("Segoe Condensed");

            var fntRegular = new XFont(new GdiFont(segoeUI, EmSize, FontStyle.Regular, GraphicsUnit.World));
            var fntBold = new XFont(new GdiFont(segoeUI, EmSize, FontStyle.Bold, GraphicsUnit.World));
            var fntItalic = new XFont(new GdiFont(segoeUI, EmSize, FontStyle.Italic, GraphicsUnit.World));
            var fntBoldItalic = new XFont(new GdiFont(segoeUI, EmSize, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.World));

            var fntLight = new XFont(new GdiFont(segoeUILight, EmSize, FontStyle.Regular, GraphicsUnit.World));
            var fntLightItalic = new XFont(new GdiFont(segoeUILight, EmSize, FontStyle.Italic, GraphicsUnit.World));

            var fntSemiLight = new XFont(new GdiFont(segoeUISemilight, EmSize, FontStyle.Regular, GraphicsUnit.World));
            var fntSemiLightItalic = new XFont(new GdiFont(segoeUISemilight, EmSize, FontStyle.Italic, GraphicsUnit.World));

            var fntSemiBold = new XFont(new GdiFont(segoeUISemibold, EmSize, FontStyle.Regular, GraphicsUnit.World));
            var fntSemiBoldItalic = new XFont(new GdiFont(segoeUISemibold, EmSize, FontStyle.Italic, GraphicsUnit.World));

            var fntBlack = new XFont(new GdiFont(segoeUIBlack, EmSize, FontStyle.Regular, GraphicsUnit.World));
            var fntBlackItalic = new XFont(new GdiFont(segoeUIBlack, EmSize, FontStyle.Italic, GraphicsUnit.World));

            var fntCondenst = new XFont(new GdiFont(segoeCondensed, EmSize, FontStyle.Regular, GraphicsUnit.World));
            var fntCondenstBold = new XFont(new GdiFont(segoeCondensed, EmSize, FontStyle.Bold, GraphicsUnit.World));
            var fntCondenstItalic = new XFont(new GdiFont(segoeCondensed, EmSize, FontStyle.Italic, GraphicsUnit.World));
            var fntCondenstBoldItalic = new XFont(new GdiFont(segoeCondensed, EmSize, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.World));
#endif
#if WPF
#if true
            var families = System.Windows.Media.Fonts.SystemFontFamilies;
            var typefaces = System.Windows.Media.Fonts.SystemTypefaces;
            //FontFamily; Typeface
            //_ = typeof(int);
            //#else
            var segoeUI = new FontFamily("Segoe UI");
            //var segoeUISemilight = new FontFamily("Segoe UI Semilight");
            var segoeCondensed = new FontFamily("Segoe");

            var segoeUISemilightFamilyTypefaces = segoeUI.FamilyTypefaces[16];

            var fntLight = new XFont(new Typeface(segoeUI, FontStyles.Normal, FontWeights.Light, FontStretches.Normal), EmSize);
            Debug.WriteLine(FontsDevHelper.GetFontCachesState());
            var fntLightItalic = new XFont(new Typeface(segoeUI, FontStyles.Italic, FontWeights.Light, FontStretches.Normal), EmSize);

            // What a bummer: The weight of SemiLight is 350, which is between Light (300) and Regular (400). And because there is 
            // no 'FontWeights.SemiLight' and no 'new FontWeight(350)' constructor, we must use the value directly from the FamilyTypeface object.
            var fntSemiLight = new XFont(new Typeface(segoeUI, FontStyles.Normal, segoeUI.FamilyTypefaces[16].Weight, FontStretches.Normal), EmSize);
            var fntSemiLightItalic = new XFont(new Typeface(segoeUI, FontStyles.Italic, segoeUI.FamilyTypefaces[16].Weight, segoeUISemilightFamilyTypefaces.Stretch), EmSize);

            var fntRegular = new XFont(new Typeface(segoeUI, FontStyles.Normal, FontWeights.Regular, FontStretches.Normal), EmSize);
            Debug.WriteLine(FontsDevHelper.GetFontCachesState());
            var fntItalic = new XFont(new Typeface(segoeUI, FontStyles.Italic, FontWeights.Regular, FontStretches.Normal), EmSize);

            var fntSemiBold = new XFont(new Typeface(segoeUI, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal), EmSize);
            var fntSemiBoldItalic = new XFont(new Typeface(segoeUI, FontStyles.Italic, FontWeights.SemiBold, FontStretches.Normal), EmSize);

            var fntBold = new XFont(new Typeface(segoeUI, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), EmSize);
            var fntBoldItalic = new XFont(new Typeface(segoeUI, FontStyles.Italic, FontWeights.Bold, FontStretches.Normal), EmSize);

            var fntBlack = new XFont(new Typeface(segoeUI, FontStyles.Normal, FontWeights.Black, FontStretches.Normal), EmSize);
            var fntBlackItalic = new XFont(new Typeface(segoeUI, FontStyles.Italic, FontWeights.Black, FontStretches.Normal), EmSize);

            var fntCondenst = new XFont(new Typeface(segoeCondensed, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), EmSize);
            var fntCondenstItalic = new XFont(new Typeface(segoeCondensed, FontStyles.Oblique, FontWeights.Normal, FontStretches.Condensed), EmSize);

            var fntCondenstBold = new XFont(new Typeface(segoeCondensed, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), EmSize);
            var fntCondenstBoldItalic = new XFont(new Typeface(segoeCondensed, FontStyles.Oblique, FontWeights.Bold, FontStretches.Condensed), EmSize);

            var fntOblique = new XFont(new Typeface(segoeUI, FontStyles.Oblique, FontWeights.Regular, FontStretches.Normal), EmSize);
            var fntBoldOblique = new XFont(new Typeface(segoeUI, FontStyles.Oblique, FontWeights.Bold, FontStretches.Condensed), EmSize);

            var fntThin = new XFont(new Typeface(segoeUI, FontStyles.Normal, FontWeights.Thin, FontStretches.Normal), EmSize);
            var fntExtraLight = new XFont(new Typeface(segoeUI, FontStyles.Normal, FontWeights.ExtraLight, FontStretches.Normal), EmSize);
            var fntUltraLight = new XFont(new Typeface(segoeUI, FontStyles.Normal, FontWeights.Light, FontStretches.Normal), EmSize);
#endif
#endif
#endif

#if BOX1
            BeginBox(gfx, 1, BoxOptions.Tile, "light - semilight - regular");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, fntLight, brush, pos);

                pos.Y += 32;
                gfx.DrawString(Text, fntSemiLight, brush, pos);

                pos.Y += 32;
                gfx.DrawString(Text, fntRegular, brush, pos);

                //pos.Y += 32;
                //gfx.DrawString(Text, fntBoldItalic, brush, pos);
            }
            EndBox(gfx);
#endif

#if BOX2
            BeginBox(gfx, 2, BoxOptions.Tile, "italic");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, fntLightItalic, brush, pos);

                pos.Y += 32;
                gfx.DrawString(Text, fntSemiLightItalic, brush, pos);

                pos.Y += 32;
                gfx.DrawString(Text, fntItalic, brush, pos);

                pos.Y += 32;
            }
            EndBox(gfx);
#endif

#if BOX3
            BeginBox(gfx, 3, BoxOptions.Tile, "semibold - bold - black");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, fntSemiBold, brush, pos);

                pos.Y += 32;
                gfx.DrawString(Text, fntBold, brush, pos);

                pos.Y += 32;
                gfx.DrawString(Text, fntBlack, brush, pos);
            }
            EndBox(gfx);
#endif

#if BOX4
            BeginBox(gfx, 4, BoxOptions.Tile, "italic");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, fntSemiBoldItalic, brush, pos);

                pos.Y += 32;
                gfx.DrawString(Text, fntBoldItalic, brush, pos);

                pos.Y += 32;
                gfx.DrawString(Text, fntBlackItalic, brush, pos);
            }
            EndBox(gfx);
#endif

#if BOX5
            BeginBox(gfx, 5, BoxOptions.Tile, "regular,condensed - bold,condensed");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, fntCondenst, brush, pos);

                pos.Y += 32;
                gfx.DrawString(Text, fntCondenstBold, brush, pos);

                //pos.Y += 32;
                //gfx.DrawString(Text, fntBold, brush, pos);

                //pos.Y += 32;
                //gfx.DrawString(Text, fntBlack, brush, pos);
            }
            EndBox(gfx);
#endif

#if BOX6
            BeginBox(gfx, 6, BoxOptions.Tile, "italic (simulated)");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, fntCondenstItalic, brush, pos);

                pos.Y += 32;
                gfx.DrawString(Text, fntCondenstBoldItalic, brush, pos);
            }
            EndBox(gfx);
#endif

#if BOX7
            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                //pos = new XPoint(16, 32);
                //gfx.DrawString(Text, fntCondenst, XBrushes.DarkBlue, pos);

                //pos.Y += 32;
                //gfx.DrawString(Text, fntCondenstBold, XBrushes.DarkBlue, pos);

                //pos.Y += 32;
                //gfx.DrawString(Text, fntCondenstItalic, XBrushes.DarkBlue, pos);

                //pos.Y += 32;
                //gfx.DrawString(Text, fntCondenstBoldItalic, XBrushes.DarkBlue, pos);
            }
            EndBox(gfx);
#endif

#if BOX8
            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                //pos = new XPoint(16, 32);
                //gfx.DrawString(Text2, fntRegular, XBrushes.DarkBlue, pos);
                //gfx.DrawMeasureBox(Text2, fntRegular, pos);

                //pos = new XPoint(16, 64);
                //gfx.DrawString(Text2, fntBoldSimulated, XBrushes.DarkBlue, pos);
                //gfx.DrawMeasureBox(Text2, fntBoldSimulated, pos);

                //pos = new XPoint(16, 96);
                //gfx.DrawString(Text2, fntItalicSimulated, XBrushes.DarkBlue, pos);
                //gfx.DrawMeasureBox(Text2, fntItalicSimulated, pos);

                //pos = new XPoint(16, 128);
                //gfx.DrawString(Text2, fntBoldItalicSimulated, XBrushes.DarkBlue, pos);
                //gfx.DrawMeasureBox(Text2, fntBoldItalicSimulated, pos);
            }
            EndBox(gfx);
#endif
        }
    }

    public class PrivateFontCollectionSnippet : Snippet
    {
        const string Text = "Sphinx";
        const double EmSize = 24;

#if true
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.WinAnsi);
#else
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
#endif
        public PrivateFontCollectionSnippet()
        {
            //NoText = true;
            //Cleanroom = true;
            //Title = "Line Types";
        }

        static PrivateFontCollectionSnippet()
        {
#if CORE
            // No PrivateFontCollection - use font resolver.
#endif

#if GDI_
            var assembly = Assembly.GetExecutingAssembly();
            XPrivateFontCollection.Add(assembly.GetManifestResourceStream("PdfSharp.Features.Fonts.xfiles.ttf")!);
            XPrivateFontCollection.Add(assembly.GetManifestResourceStream("PdfSharp.Features.Fonts.oblivious.ttf")!);
#endif

#if WPF_
            var uri = new Uri("pack://application:,,,/");
            XPrivateFontCollection.Add(uri, "./Fonts/#X-Files");
            XPrivateFontCollection.Add(uri, "./Fonts/#Oblivious font");
#endif
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            var brush = new XSolidBrush(XColor.FromArgb(0x20, 0x20, 0x20));
            var brushSimulation = new XSolidBrush(XColors.DarkOrange);

            //Typeface typeface = new Typeface("segoe ui semibold");
            var xfiles = new XFont("x-files", EmSize, XFontStyleEx.Regular, _fontOptions);
            var oblivious = new XFont("Oblivious font", EmSize, XFontStyleEx.Regular, _fontOptions);
            Debug.WriteLine(FontsDevHelper.GetFontCachesState());

            var xfilesBold = new XFont("x-files", EmSize, XFontStyleEx.Bold, _fontOptions);
            var obliviousBold = new XFont("Oblivious font", EmSize, XFontStyleEx.Bold, _fontOptions);

            var xfilesItalic = new XFont("x-files", EmSize, XFontStyleEx.Italic, _fontOptions);
            var obliviousItalic = new XFont("Oblivious font", EmSize, XFontStyleEx.Italic, _fontOptions);

            var xfilesBoldItalic = new XFont("x-files", EmSize, XFontStyleEx.BoldItalic, _fontOptions);
            var obliviousBoldItalic = new XFont("Oblivious font", EmSize, XFontStyleEx.BoldItalic, _fontOptions);

            BeginBox(gfx, 1, BoxOptions.Tile, "X-files");
            {
                gfx.DrawString(Text, xfiles, brush, new XPoint(16, 32));
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile, "Simulated styles");
            {
                gfx.DrawString(Text, xfiles, brush, new XPoint(16, 32));
                gfx.DrawString(Text, xfilesBold, brush, new XPoint(16, 64));
                gfx.DrawString(Text, xfilesItalic, brush, new XPoint(16, 96));
                gfx.DrawString(Text, xfilesBoldItalic, brush, new XPoint(16, 128));
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile, "Oblivious");
            {
                gfx.DrawString(Text, oblivious, brush, new XPoint(16, 32));
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile, "Simulated styles");
            {
                gfx.DrawString(Text, oblivious, brush, new XPoint(16, 32));
                gfx.DrawString(Text, obliviousBold, brush, new XPoint(16, 64));
                gfx.DrawString(Text, obliviousItalic, brush, new XPoint(16, 96));
                gfx.DrawString(Text, obliviousBoldItalic, brush, new XPoint(16, 128));
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile, "X-Files");
            {
                gfx.DrawString(Text, xfiles, brush, new XPoint(16, 32));
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile, "Simulated styles");
            {
                gfx.DrawString(Text, xfiles, brush, new XPoint(16, 32));
                gfx.DrawString(Text, xfilesBold, brush, new XPoint(16, 64));
                gfx.DrawString(Text, xfilesItalic, brush, new XPoint(16, 96));
                gfx.DrawString(Text, xfilesBoldItalic, brush, new XPoint(16, 128));
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile, "Early Tickertape");
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile, "Simulated styles");
            {
            }
            EndBox(gfx);
        }
    }

    public class FrutigerFontsSnippet : Snippet
    {
        const string Text = "XÄß-Sphinx";
        const double EmSize = 24;

        public FrutigerFontsSnippet()
        {
            //NoText = true;
            //Cleanroom = true;
            //Title = "Line Types";
        }

        static FrutigerFontsSnippet()
        {
#if GDI_
            XPrivateFontCollection.Add(Assembly.GetExecutingAssembly().GetManifestResourceStream("PdfSharp.Features.Fonts.walrod.ttf")!);
            XPrivateFontCollection.Add(Assembly.GetExecutingAssembly().GetManifestResourceStream("PdfSharp.Features.Fonts.battlest.ttf")!);
            XPrivateFontCollection.Add(Assembly.GetExecutingAssembly().GetManifestResourceStream("PdfSharp.Features.Fonts.xfiles.ttf")!);
#endif

#if WPF
            Uri uriPackApplication = new Uri("pack://application:,,,/");
#if FRUTIGER_VIA_WPF_FONTFAMILY_
            // We have 'Frutiger' and 'Frutiger,bold'.
            XPrivateFontCollection.Add(uriPackApplication, "./Fonts/Frutiger/#Frutiger");
            XPrivateFontCollection.Add(uriPackApplication, "./Fonts/Frutiger/#FrutigerLight");
#endif
#endif
        }

#if VARIANT2___
    //XPrivateFontCollection.Add(uri, "./Fonts/HelveticaNeue/#Helvetica Neue");
    //XPrivateFontCollection.Add(uri, "./Fonts/HelveticaNeue/#Helvetica Neue Bold");
    //XPrivateFontCollection.Add(uri, "./Fonts/HelveticaNeue/#Helvetica Neue Light");
        static FontFamily fam1;
        static FontFamily fam2;
        static FontFamily fam3;

#endif

        public override void RenderSnippet(XGraphics gfx)
        {
            var brush = new XSolidBrush(XColor.FromArgb(0x20, 0x20, 0x20));
            var brushSimulation = new XSolidBrush(XColors.DarkOrange);

            var frStd = new XFont("Frutiger", EmSize);
            Debug.WriteLine(PdfSharp.Internal.FontsDevHelper.GetFontCachesState());
            var frBold = new XFont("Frutiger", EmSize, XFontStyleEx.Bold);
            var frItalic = new XFont("Frutiger", EmSize, XFontStyleEx.Italic);
            var frBoldItalic = new XFont("Frutiger", EmSize, XFontStyleEx.BoldItalic);

            var frLight = new XFont("FrutigerLight", EmSize);
            var frLightBold = new XFont("FrutigerLight", EmSize, XFontStyleEx.Bold);
            var frLightItalic = new XFont("FrutigerLight", EmSize, XFontStyleEx.Italic);
            var frLightBoldItalic = new XFont("FrutigerLight", EmSize, XFontStyleEx.BoldItalic);

            Debug.WriteLine(PdfSharp.Internal.FontsDevHelper.GetFontCachesState());

            XPoint pos;
#if BOX1
            BeginBox(gfx, 1, BoxOptions.Tile, "Native regular typefaces");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, frStd, brush, pos);
                gfx.DrawMeasureBox(Text, frStd, pos);

                pos = new XPoint(16, 64);
                gfx.DrawString(Text, frBold, brush, pos);
                gfx.DrawMeasureBox(Text, frBold, pos);

                pos = new XPoint(16, 96);
                gfx.DrawString(Text, frItalic, brushSimulation, pos);
                gfx.DrawMeasureBox(Text, frItalic, pos);

                pos = new XPoint(16, 128);
                gfx.DrawString(Text, frBoldItalic, brushSimulation, pos);
                gfx.DrawMeasureBox(Text, frBoldItalic, pos);
#if VARIANT2__
                gfx.DrawString(Text, hnStd, XBrushes.DarkGreen, new XPoint(16, 24));
                gfx.DrawString(Text, hnBold, XBrushes.DarkGreen, new XPoint(16, 48));
                gfx.DrawString(Text, hnLight, XBrushes.DarkGreen, new XPoint(16, 72));
#endif
            }
            EndBox(gfx);
#endif

#if BOX2
            BeginBox(gfx, 2, BoxOptions.Tile, "Simulated styles");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, frItalic, brush, pos);
                gfx.DrawMeasureBox(Text, frItalic, pos);

                pos = new XPoint(16, 64);
                gfx.DrawString(Text, frBoldItalic, brush, pos);
                gfx.DrawMeasureBox(Text, frBoldItalic, pos);

                //gfx.DrawString(Text, frBoldItalic, brush, new XPoint(16, 96));
                //gfx.DrawString(Text, frItalic2, brush, new XPoint(16, 132));
#if VARIANT1___
                gfx.DrawString(Text, walrodItalic, brush, new XPoint(16, 40));
#endif
            }
            EndBox(gfx);
#endif

#if BOX3
            BeginBox(gfx, 3, BoxOptions.Tile, "Native light typefaces");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, frLight, brush, pos);
                gfx.DrawMeasureBox(Text, frLight, pos);

                pos = new XPoint(16, 64);
                gfx.DrawString(Text, frLightBold, brush, pos);
                gfx.DrawMeasureBox(Text, frLightBold, pos);

                pos = new XPoint(16, 96);
                gfx.DrawString(Text, frLightItalic, brush, pos);
                gfx.DrawMeasureBox(Text, frLightItalic, pos);

                pos = new XPoint(16, 128);
                gfx.DrawString(Text, frLightBoldItalic, brushSimulation, pos);
                gfx.DrawMeasureBox(Text, frLightBoldItalic, pos);
#if VARIANT1__
                gfx.DrawString(Text, battlestar, brush, new XPoint(16, 40));
#endif
#if VARIANT2
                gfx.DrawString(Text, hnLightBold, XBrushes.DarkGreen, new XPoint(16, 24));
                gfx.DrawString(Text, hnLightItalic, XBrushes.DarkGreen, new XPoint(16, 48));
                gfx.DrawString(Text, hnLightBoldItalic, XBrushes.DarkGreen, new XPoint(16, 72));
#endif
            }
            EndBox(gfx);
#endif

#if BOX4
            BeginBox(gfx, 4, BoxOptions.Tile, "Simulated styles");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, frLightBoldItalic, brush, pos);
                gfx.DrawMeasureBox(Text, frLightBoldItalic, pos);
            }
            EndBox(gfx);
#endif

#if BOX5
            BeginBox(gfx, 5, BoxOptions.Tile);
            { }
            EndBox(gfx);
#endif

#if BOX6
            BeginBox(gfx, 6, BoxOptions.Tile);
            { }
            EndBox(gfx);
#endif

#if BOX7
            BeginBox(gfx, 7, BoxOptions.Tile);
            { }
            EndBox(gfx);
#endif

#if BOX8
            BeginBox(gfx, 8, BoxOptions.Tile);
            { }
            EndBox(gfx);
#endif
        }
    }

    public class HelveticaNeueFontsSnippet : Snippet
    {
        const string Text = "Sphinx";
        const double EmSize = 24;

        const string FamilyName = "Helvetica Neue";
        //const string FamilyNameLight = "Helvetica Neue|light";
        const string FamilyNameLight = "Helvetica Neue Light";

#if true
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.WinAnsi);
#else
        readonly XPdfFontOptions _fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
#endif

        public HelveticaNeueFontsSnippet()
        {
            //NoText = true;
            //Cleanroom = true;
            //Title = "Line Types";
        }

        static HelveticaNeueFontsSnippet()
        {
#if GDI_
            XPrivateFontCollection.Add(Assembly.GetExecutingAssembly().GetManifestResourceStream("PdfSharp.Features.Fonts.walrod.ttf")!);
            XPrivateFontCollection.Add(Assembly.GetExecutingAssembly().GetManifestResourceStream("PdfSharp.Features.Fonts.battlest.ttf")!);
            XPrivateFontCollection.Add(Assembly.GetExecutingAssembly().GetManifestResourceStream("PdfSharp.Features.Fonts.xfiles.ttf")!);
#endif

#if WPF_
            Uri uriPackApplication = new Uri("pack://application:,,,/");
#if true_ //FRUTIGER_VIA_WPF_FONTFAMILY
            // We have 'Helvetica Neue', 'Helvetica Neue Light', and 'Helvetica Neue,bold'.
            XPrivateFontCollection.Add(uriPackApplication, "./Fonts/HelveticaNeue/#Helvetica Neue");
            XPrivateFontCollection.Add(uriPackApplication, "./Fonts/HelveticaNeue/#Helvetica Neue Bold");
            XPrivateFontCollection.Add(uriPackApplication, "./Fonts/HelveticaNeue/#Helvetica Neue Light");
#endif
#endif
        }

#if VARIANT2___
    //XPrivateFontCollection.Add(uri, "./Fonts/HelveticaNeue/#Helvetica Neue");
    //XPrivateFontCollection.Add(uri, "./Fonts/HelveticaNeue/#Helvetica Neue Bold");
    //XPrivateFontCollection.Add(uri, "./Fonts/HelveticaNeue/#Helvetica Neue Light");
        static FontFamily fam1;
        static FontFamily fam2;
        static FontFamily fam3;

#endif

        public override void RenderSnippet(XGraphics gfx)
        {
            var brush = new XSolidBrush(XColor.FromArgb(0x20, 0x20, 0x20));

            var hvStd = new XFont(FamilyName, EmSize);
            Debug.WriteLine(PdfSharp.Internal.FontsDevHelper.GetFontCachesState());
            var hvBold = new XFont(FamilyName, EmSize, XFontStyleEx.Bold);
            var hvItalic = new XFont(FamilyName, EmSize, XFontStyleEx.Italic);
            var hvItalic2 = new XFont(FamilyName, EmSize, XFontStyleEx.Italic);
            var hvBoldItalic = new XFont(FamilyName, EmSize, XFontStyleEx.BoldItalic);

            var hvLight = new XFont(FamilyNameLight, EmSize);
            var hvLightBold = new XFont(FamilyNameLight, EmSize, XFontStyleEx.Bold);
            var hvLightItalic = new XFont(FamilyNameLight, EmSize, XFontStyleEx.Italic);
            var hvLightBoldItalic = new XFont(FamilyNameLight, EmSize, XFontStyleEx.BoldItalic);

            var hvLightSimulated = FontsDevHelper.CreateSpecialFont(FamilyNameLight, EmSize, XFontStyleEx.Regular,
                _fontOptions, XStyleSimulations.None);
            var hvLightBoldSimulated = FontsDevHelper.CreateSpecialFont(FamilyNameLight, EmSize, XFontStyleEx.Regular,
                _fontOptions, XStyleSimulations.BoldSimulation);
            var hvLightItalicSimulated = FontsDevHelper.CreateSpecialFont(FamilyNameLight, EmSize,
                XFontStyleEx.Regular, _fontOptions, XStyleSimulations.ItalicSimulation);
            var hvLightBoldItalicSimulated = FontsDevHelper.CreateSpecialFont(FamilyNameLight, EmSize,
                XFontStyleEx.Regular, _fontOptions, XStyleSimulations.BoldItalicSimulation);

            //hvLightSimulated = FontsDevHelper.CreateSpecialFont("Helvetica Neue Light", EmSize, XFontStyleEx.Regular, _fontOptions, XStyleSimulations.None);
            //hvLightBoldSimulated = FontsDevHelper.CreateSpecialFont("Helvetica Neue Light Bold", EmSize, XFontStyleEx.Regular, _fontOptions, XStyleSimulations.BoldSimulation);
            //hvLightItalicSimulated = FontsDevHelper.CreateSpecialFont("Helvetica Neue Light Italic", EmSize, XFontStyleEx.Regular, _fontOptions, XStyleSimulations.ItalicSimulation);
            //hvLightBoldItalicSimulated = FontsDevHelper.CreateSpecialFont("Helvetica Neue Light Bold Italic", EmSize, XFontStyleEx.Regular, _fontOptions, XStyleSimulations.BoldItalicSimulation);

            Debug.WriteLine(PdfSharp.Internal.FontsDevHelper.GetFontCachesState());

            XPoint pos;
#if BOX1
            BeginBox(gfx, 1, BoxOptions.Tile, "Native regular typefaces");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, hvStd, brush, pos);
                gfx.DrawMeasureBox(Text, hvStd, pos);

                pos = new XPoint(16, 64);
                gfx.DrawString(Text, hvBold, brush, pos);
                gfx.DrawMeasureBox(Text, hvBold, pos);

                pos = new XPoint(16, 96);
                gfx.DrawString(Text, hvItalic, brush, pos);
                gfx.DrawMeasureBox(Text, hvItalic, pos);

                pos = new XPoint(16, 128);
                gfx.DrawString(Text, hvBoldItalic, brush, pos);
                gfx.DrawMeasureBox(Text, hvBoldItalic, pos);
            }
            EndBox(gfx);
#endif

#if BOX2
            BeginBox(gfx, 2, BoxOptions.Tile, "Simulated styles");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, hvItalic, brush, pos);
                gfx.DrawMeasureBox(Text, hvItalic, pos);

                pos = new XPoint(16, 64);
                gfx.DrawString(Text, hvBoldItalic, brush, pos);
                gfx.DrawMeasureBox(Text, hvBoldItalic, pos);
            }
            EndBox(gfx);
#endif

#if BOX3
            BeginBox(gfx, 3, BoxOptions.Tile, "Native light typefaces");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, hvLight, brush, pos);
                gfx.DrawMeasureBox(Text, hvLight, pos);

                pos = new XPoint(16, 64);
                gfx.DrawString(Text, hvLightBold, brush, pos);
                gfx.DrawMeasureBox(Text, hvLightBold, pos);

                pos = new XPoint(16, 96);
                gfx.DrawString(Text, hvLightItalic, brush, pos);
                gfx.DrawMeasureBox(Text, hvLightItalic, pos);

                pos = new XPoint(16, 128);
                gfx.DrawString(Text, hvLightItalic, brush, pos);
                gfx.DrawMeasureBox(Text, hvLightItalic, pos);
            }
            EndBox(gfx);
#endif

#if BOX4
            BeginBox(gfx, 4, BoxOptions.Tile, "Simulated styles");
            {
                pos = new XPoint(16, 32);
                gfx.DrawString(Text, hvLightSimulated, brush, pos);
                gfx.DrawMeasureBox(Text, hvLightSimulated, pos);

                pos = new XPoint(16, 64);
                gfx.DrawString(Text, hvLightBoldSimulated, brush, pos);
                gfx.DrawMeasureBox(Text, hvLightBoldSimulated, pos);

                pos = new XPoint(16, 96);
                gfx.DrawString(Text, hvLightItalicSimulated, brush, pos);
                gfx.DrawMeasureBox(Text, hvLightItalicSimulated, pos);

                pos = new XPoint(16, 128);
                gfx.DrawString(Text, hvLightBoldItalicSimulated, brush, pos);
                gfx.DrawMeasureBox(Text, hvLightBoldItalicSimulated, pos);
            }
            EndBox(gfx);
#endif

#if BOX5
            BeginBox(gfx, 5, BoxOptions.Tile);
            { }
            EndBox(gfx);
#endif

#if BOX6
            BeginBox(gfx, 6, BoxOptions.Tile);
            { }
            EndBox(gfx);
#endif

#if BOX7
            BeginBox(gfx, 7, BoxOptions.Tile);
            { }
            EndBox(gfx);
#endif

#if BOX8
            BeginBox(gfx, 8, BoxOptions.Tile);
            { }
            EndBox(gfx);
#endif
        }
    }
}
