// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//using PdfSharp.Drawing;
//using PdfSharp.Pdf;
//using PdfSharp.Quality;

//namespace PdfSharp.Snippets.Security
//{
//    // TODO_OLD Remove code???
//    public class Passwords : SnippetBase
//    {
//        public Passwords()
//        {
//            Title = "PDF files with password security";
//        }

//        public override void RenderSnippet(XGraphics gfx)
//        {
//            BeginBox(gfx, 1, BoxOptions.Tile);
//            {
//            }
//            EndBox(gfx);

//            BeginBox(gfx, 2, BoxOptions.Box);
//            { }
//            EndBox(gfx);

//            BeginBox(gfx, 3, BoxOptions.Tile);
//            {
//            }
//            EndBox(gfx);

//            BeginBox(gfx, 4, BoxOptions.Box);
//            { }
//            EndBox(gfx);

//            BeginBox(gfx, 5, BoxOptions.Tile);
//            {
//            }
//            EndBox(gfx);

//            BeginBox(gfx, 6, BoxOptions.Box);
//            { }
//            EndBox(gfx);

//            BeginBox(gfx, 7, BoxOptions.Fill);
//            { }
//            EndBox(gfx);

//            BeginBox(gfx, 8, BoxOptions.Fill);
//            { }
//            EndBox(gfx);
//        }

//        public PdfDocument Ownerpassword()
//        {
//            var doc = HelloWorldFactory("Ownerpassword");
//            doc.SecuritySettings.OwnerPassword = TestClassBase.TestPassword;

//            AddHyperlink(doc);
//            return doc;
//        }

//        public PdfDocument Userpassword()
//        {
//            var doc = HelloWorldFactory("Userpassword");
//            doc.SecuritySettings.UserPassword = TestClassBase.TestPassword;

//            AddHyperlink(doc);
//            return doc;
//        }

//        public PdfDocument Bothpasswords()
//        {
//            var doc = HelloWorldFactory("Two passwords");
//            doc.SecuritySettings.OwnerPassword = TestClassBase.TestPassword + TestClassBase.TestPassword;
//            doc.SecuritySettings.UserPassword = TestClassBase.TestPassword;

//            AddHyperlink(doc);
//            return doc;
//        }

//        private void AddHyperlink(PdfDocument doc)
//        {
//            // Hyperlinks are lost in the combined result file.

//            using (var gfx = GfxForLastPage(doc))
//            {
//                var font = new XFont("Segoe UI", 20, XFontStyleEx.Regular);

//                var rect = new XRect(50, 20, 200, 50);

//                var pen = new XPen(XColors.Firebrick);

//                gfx.DrawRectangle(pen, rect);

//                gfx.DrawString("Click me!", font, XBrushes.Black,
//                    rect,
//                    XStringFormats.Center);

//                // Convert the rect for the hyperlink.
//                XRect rectLink = gfx.Transformer.WorldToDefaultPage(rect);

//                var page = doc.Pages[doc.Pages.Count - 1];
//                page.AddWebLink(new PdfRectangle(rectLink), "http://www.empira.de/");
//            }
//        }
//    }
//}
