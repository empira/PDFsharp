// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class Surrogates : Snippet
    {
        public Surrogates()
        {
            Title = "Surrogates";
            PathName = "snippets/drawing/text/" + nameof(Surrogates);
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            //char rose = ';';
            //string rofl = "🤣";
            //string rofl2 = "\U0001f602";

            //string halloWorldChinese = "你好世界";

            //string xxx = "སྐྱེ་བོ་དག་གིས་འུ་ཐུག་ཐབས་ཟད་ཀྱིས་རྐྱེན་པས་བཙན་དབང་དང༌། གཉའ་གནོན་ལ་ངོ་རྒོལ་སྒེར་ལངས་བྱེད་པའི་མཐའ་མའི་བྱ་ཐབས་ལག་བསྟར་མི་དགོས་པ་ཞིག་བྱེད་ན།";


            if (!Capabilities.OperatingSystem.IsWindows)
                return;

            Cleanroom = true;
            IOUtility.EnsureAssets("fonts/Noto/Noto_Sans_TC/static/NotoSansTC-Regular.ttf");

#if !GDI && !WPF
            var fontSourceEmoji = XFontSource.CreateFromFile("c:/Windows/Fonts/seguiemj.ttf");
            var glyphTypefaceEmoji = new XGlyphTypeface(fontSourceEmoji);
            var fontEmoji = new XFont(glyphTypefaceEmoji, 10);
            
            //var fontSourceGothic = XFontSource.CreateFromFile("c:/Windows/Fonts/GOTHIC.TTF");
            //var glyphTypefaceGothic = new XGlyphTypeface(fontSourceGothic);
            //var fontGothic = new XFont(glyphTypefaceGothic, 10);

            var fontSourceSimSun = XFontSource.CreateFromFile("c:/Windows/Fonts/simsunb.ttf");
            //var fontSourceSimSun = XFontSource.CreateFromFile(IOUtility.GetAssetsPath("fonts/Noto/Noto_Sans_TC/static/NotoSansTC-Regular.ttf")!);
            var glyphTypefaceSimSun = new XGlyphTypeface(fontSourceSimSun);
            var fontSimSun = new XFont(glyphTypefaceSimSun, 10);
#else
            var fontEmoji = new XFont("Segoe UI Emoji", 10);
            //var fontGothic = new XFont("MS Gothic", 10);
            var fontSimSun = new XFont("SimSun", 10);
#endif

            //// Text Styles (resulting in own Type Faces).
            //BeginBox(gfx, 1, BoxOptions.Fill, "Segoe UI Emoji");
            //{
            //    //const string facename = "Segoe UI Emoji";
            //    //var options = new XPdfFontOptions(PdfFontEncoding.Unicode);

            //    //var fontRegular = new XFont(facename, 20, XFontStyleEx.Regular, options);
            //    ////var fontBold = new XFont(facename, 20, XFontStyleEx.Bold, options);

            //    gfx.DrawString("\udca9", fontEmoji, XBrushes.DarkSlateGray, 10, 30);
            //    gfx.DrawString("\U0001F339", fontEmoji, XBrushes.DarkSlateGray, 10, 50);
            //    gfx.DrawString("\U0001F339 " + rofl + " " + rofl2, fontEmoji, XBrushes.DarkSlateGray, 10, 70);
            //    //gfx.DrawString("Pile of Poo '\ud83d\udca9' \ud83d\ude48", fontRegular, XBrushes.DarkSlateGray, 10, 30);
            //    //gfx.DrawString("\ud83e\udd93\ud83e\udd93\ud83d\udc34\ud83e\udd84\ud83d\udc14\ud83d\udc3e\ud83d\udc12\ud83d\ude3b\ud83d\ude3c\ud83d\ude3d\ud83d\ude40", fontRegular, XBrushes.DarkSlateGray, 10, 70);
            //    //gfx.DrawString(String.Format("{0} (bold)", facename), fontBold, XBrushes.DarkSlateGray, 0, 112.5);
            //}
            //EndBox(gfx);


            // Text Styles (resulting in own Type Faces).
            BeginBox(gfx, 2, BoxOptions.Fill, "'Hello World' Chinese");
            {
                gfx.DrawString("世", fontSimSun, XBrushes.DarkSlateGray, 10, 30);
            }
            EndBox(gfx);
        }
    }
}
