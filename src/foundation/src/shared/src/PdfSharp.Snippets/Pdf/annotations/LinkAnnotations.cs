// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Pdf
{
    public class LinkAnnotations : Snippet
    {
        public LinkAnnotations()
        {
            Title = "Draw diverse link annotations";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
        }

        enum MyLinkTypes
        {
            DocumentLink,
            FileLink,
            WebLink
        }

        PdfLinkAnnotation AddHyperlink(PdfPage page, XGraphics gfx, XFont font, string text, MyLinkTypes linkType, object target)
        {
            // Hyperlinks are lost in the combined result file.

            var rect = new XRect(20, 20, 160, 50);

            var pen = new XPen(XColors.Blue);

            gfx.DrawRectangle(pen, rect);

            gfx.DrawString(text, font, XBrushes.Black,
                rect,
                XStringFormats.Center);

            // Convert the rect for the hyperlink.
            XRect rectLink = gfx.Transformer.WorldToDefaultPage(rect);

            PdfLinkAnnotation la = linkType switch
            {
                MyLinkTypes.DocumentLink => page.AddDocumentLink(new(rectLink), (int)target),
                MyLinkTypes.FileLink => page.AddFileLink(new(rectLink), (string)target),
                MyLinkTypes.WebLink => page.AddWebLink(new(rectLink), (string)target),
                _ => throw new ArgumentException($"'{linkType}' is not a valid link type.")
            };
            return la;
        }

        public void RenderSnippet(PdfPage page, XGraphics gfx)
        {
            // DocumentLink and FileLink don’t work here. Maybe they never worked. TODO_OLD: Repair AddHyperlink for DocumentLink and FileLink. $MaOs 

            var font = new XFont("Segoe UI", 20, XFontStyleEx.Regular);

            BeginBox(gfx, 1, BoxOptions.Box);
            {
                AddHyperlink(page, gfx, font, "2nd page", MyLinkTypes.DocumentLink, 2);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Box);
            {
                const string file = "LinkTarget.pdf";
                AddHyperlink(page, gfx, font, "File link", MyLinkTypes.FileLink, file);
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Box);
            {
                AddHyperlink(page, gfx, font, "www.empira.de", MyLinkTypes.WebLink, "http://www.empira.de/");
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Box);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Box);
            {
                var rsa = new PdfRubberStampAnnotation { Icon = PdfRubberStampAnnotationIcon.Approved, Flags = PdfAnnotationFlags.ReadOnly };

                var rect = gfx.Transformer.WorldToDefaultPage(new XRect(new XPoint(20, 20), new XSize(160, 50)));
                rsa.Rectangle = new PdfRectangle(rect);

                // Add the rubber stamp annotation to the page 
                page.Annotations.Add(rsa);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Box);
            {
                var rsa = new PdfRubberStampAnnotation
                          {
                              Icon = PdfRubberStampAnnotationIcon.TopSecret,
                              Flags = PdfAnnotationFlags.ReadOnly,
                              Title = "Annotation (title)",
                              Subject = "Annotation (subject)",
                              Contents = "This is the contents of the rubber stamp annotation.",
                              Color = XColors.Blue,
                              Opacity = 0.5
                          };

                var rect = gfx.Transformer.WorldToDefaultPage(new XRect(new XPoint(20, 20), new XSize(160, 50)));
                rsa.Rectangle = new PdfRectangle(rect);

                // Add the rubber stamp annotation to the page 
                page.Annotations.Add(rsa);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Fill);
            {
                // Create a PDF text annotation
                var textAnnot = new PdfTextAnnotation
                                {
                                    Title = "This is the title",
                                    Subject = "This is the subject",
                                    Contents = "This is the contents of the annotation.\rThis is the 2nd line.",
                                    Icon = PdfTextAnnotationIcon.Note
                                };

                gfx.DrawString("A text annotation.", font, XBrushes.Black, 20, 60, XStringFormats.Default);

                // Convert rectangle from world space to page space. This is necessary because the annotation is
                // placed relative to the bottom left corner of the page with units measured in point.
                XRect rect = gfx.Transformer.WorldToDefaultPage(new XRect(new XPoint(20, 20), new XSize(160, 50)));
                textAnnot.Rectangle = new PdfRectangle(rect);

                // Add the annotation to the page
                page.Annotations.Add(textAnnot);
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Fill);
            {
                // Create another PDF text annotation which is open and transparent
                var textAnnot = new PdfTextAnnotation
                                {
                                    Title = "Annotation 2 (title)",
                                    Subject = "Annotation 2 (subject)",
                                    Contents = "This is the contents of the 2nd annotation.",
                                    Icon = PdfTextAnnotationIcon.Help,
                                    Color = XColors.LimeGreen,
                                    Opacity = 0.5,
                                    Open = true
                                };

                gfx.DrawString("Text annotation 2.", font, XBrushes.Black, 20, 120, XStringFormats.Default);

                var rect = gfx.Transformer.WorldToDefaultPage(new XRect(new XPoint(20, 20), new XSize(160, 50)));
                textAnnot.Rectangle = new PdfRectangle(rect);

                // Add the 2nd annotation to the page
                page.Annotations.Add(textAnnot);
            }
            EndBox(gfx);
        }

        public void RenderTestPage2(PdfPage page, XGraphics gfx)
        {
            var font = new XFont("Segoe UI", 20, XFontStyleEx.Regular);

            BeginBox(gfx, 1, BoxOptions.Box);
            {
                AddHyperlink(page, gfx, font, "1st page", MyLinkTypes.DocumentLink, 1);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Box);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Box);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Box);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Box);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Box);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Box);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Box);
            {
            }
            EndBox(gfx);
        }
    }
}
