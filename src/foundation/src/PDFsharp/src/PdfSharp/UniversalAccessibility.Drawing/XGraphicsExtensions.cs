// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;

// Re-Sharper disable InconsistentNaming

namespace PdfSharp.UniversalAccessibility.Drawing
{
    /// <summary>
    /// PDF/UA extensions.
    /// </summary>
    public static class XGraphicsExtensions
    {
        /// <summary>
        /// Extension for DrawString with a PDF Block Level Element tag.
        /// </summary>
        ///// <param name="gfx"></param>
        ///// <param name="text"></param>
        ///// <param name="font"></param>
        ///// <param name="brush"></param>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        ///// <param name="format"></param>
        ///// <param name="tag"></param>
        public static void DrawString(this XGraphics gfx, string text, XFont font, XBrush brush, double x, double y, XStringFormat format, PdfBlockLevelElementTag tag)
        {
            var sb = GetStructureBuilder(gfx);
            sb.BeginElement(tag);
            gfx.DrawString(text, font, brush, x, y, format);
            sb.End();
        }

        /// <summary>
        /// Extension for DrawString with a PDF Inline Level Element tag.
        /// </summary>
        ///// <param name="gfx"></param>
        ///// <param name="text"></param>
        ///// <param name="font"></param>
        ///// <param name="brush"></param>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        ///// <param name="format"></param>
        ///// <param name="tag"></param>
        public static void DrawString(this XGraphics gfx, string text, XFont font, XBrush brush, double x, double y, XStringFormat format, PdfInlineLevelElementTag tag)
        {
            var sb = GetStructureBuilder(gfx);
            sb.BeginElement(tag);
            gfx.DrawString(text, font, brush, x, y, format);
            sb.End();
        }

        #region DrawString overloads

        /// <summary>
        /// Extension for DrawString with a PDF Block Level Element tag.
        /// </summary>
        public static void DrawString(this XGraphics gfx, string text, XFont font, XBrush brush, XPoint point, XStringFormat format, PdfBlockLevelElementTag tag)
        {
            gfx.DrawString(text, font, brush, point.X, point.Y, format, tag);
        }

        /// <summary>
        /// Extension for DrawString with a PDF Inline Level Element tag.
        /// </summary>
        public static void DrawString(this XGraphics gfx, string text, XFont font, XBrush brush, XPoint point, XStringFormat format, PdfInlineLevelElementTag tag)
        {
            gfx.DrawString(text, font, brush, point.X, point.Y, format, tag);
        }

        /// <summary>
        /// Extension for DrawString with a PDF Block Level Element tag.
        /// </summary>
        public static void DrawString(this XGraphics gfx, string text, XFont font, XBrush brush, double x, double y, PdfBlockLevelElementTag tag)
        {
            gfx.DrawString(text, font, brush, x, y, XStringFormats.Default, tag);
        }

        /// <summary>
        /// Extension for DrawString with a PDF Inline Level Element tag.
        /// </summary>
        public static void DrawString(this XGraphics gfx, string text, XFont font, XBrush brush, double x, double y, PdfInlineLevelElementTag tag)
        {
            gfx.DrawString(text, font, brush, x, y, XStringFormats.Default, tag);
        }

        /// <summary>
        /// Extension for DrawString with a PDF Block Level Element tag.
        /// </summary>
        public static void DrawString(this XGraphics gfx, string text, XFont font, XBrush brush, XPoint point, PdfBlockLevelElementTag tag)
        {
            gfx.DrawString(text, font, brush, point.X, point.Y, tag);
        }

        /// <summary>
        /// Extension for DrawString with a PDF Inline Level Element tag.
        /// </summary>
        public static void DrawString(this XGraphics gfx, string text, XFont font, XBrush brush, XPoint point, PdfInlineLevelElementTag tag)
        {
            gfx.DrawString(text, font, brush, point.X, point.Y, tag);
        }

        #endregion

        /// <summary>
        /// Extension for DrawAbbreviation with a PDF Inline Level Element tag.
        /// </summary>
        public static void DrawAbbreviation(this XGraphics gfx, string abbreviation, string expandedText, XFont font, XBrush brush, double x, double y, XStringFormat format, PdfInlineLevelElementTag tag)
        {
            var sb = GetStructureBuilder(gfx);
            sb.BeginElement(tag);
            {
                gfx.DrawString(abbreviation, font, brush, x, y, format);
                sb.SetExpandedText(expandedText);
            }
            sb.End();
        }

        #region DrawAbbreviation overloads

        /// <summary>
        /// Extension for DrawAbbreviation with a Span PDF Inline Level Element tag.
        /// </summary>
        public static void DrawAbbreviation(this XGraphics gfx, string abbreviation, string expandedText, XFont font, XBrush brush, double x, double y, XStringFormat format)
        {
            gfx.DrawAbbreviation(abbreviation, expandedText, font, brush, x, y, format, PdfInlineLevelElementTag.Span);
        }

        /// <summary>
        /// Extension for DrawAbbreviation with a Span PDF Inline Level Element tag.
        /// </summary>
        public static void DrawAbbreviation(this XGraphics gfx, string abbreviation, string expandedText, XFont font, XBrush brush, XPoint point, XStringFormat format)
        {
            gfx.DrawAbbreviation(abbreviation, expandedText, font, brush, point.X, point.Y, format);
        }

        /// <summary>
        /// Extension for DrawAbbreviation with a Span PDF Inline Level Element tag.
        /// </summary>
        public static void DrawAbbreviation(this XGraphics gfx, string abbreviation, string expandedText, XFont font, XBrush brush, double x, double y)
        {
            gfx.DrawAbbreviation(abbreviation, expandedText, font, brush, x, y, XStringFormats.Default);
        }

        /// <summary>
        /// Extension for DrawAbbreviation with a Span PDF Inline Level Element tag.
        /// </summary>
        public static void DrawAbbreviation(this XGraphics gfx, string abbreviation, string expandedText, XFont font, XBrush brush, XPoint point)
        {
            gfx.DrawAbbreviation(abbreviation, expandedText, font, brush, point.X, point.Y, XStringFormats.Default);
        }

        #endregion

        /// <summary>
        /// Extension for DrawImage with an alternative text.
        /// </summary>
        public static void DrawImage(this XGraphics gfx, XImage image, double x, double y, string altText, XRect boundingBox)
        {
            var sb = GetStructureBuilder(gfx);
            sb.BeginElement(PdfIllustrationElementTag.Figure, altText, boundingBox);
            gfx.DrawImage(image, x, y);
            sb.End();
        }

        /// <summary>
        /// Extension for DrawImage with an alternative text.
        /// </summary>
        public static void DrawImage(this XGraphics gfx, XImage image, double x, double y, double width, double height, string altText, XRect boundingBox)
        {
            var sb = GetStructureBuilder(gfx);
            sb.BeginElement(PdfIllustrationElementTag.Figure, altText, boundingBox);
            gfx.DrawImage(image, x, y, width, height);
            sb.End();
        }

        #region DrawImage overloads

        /// <summary>
        /// Extension for DrawImage with an alternative text.
        /// </summary>
        public static void DrawImage(this XGraphics gfx, XImage image, XPoint point, string altText, XRect boundingBox)
        {
            gfx.DrawImage(image, point.X, point.Y, altText, boundingBox);
        }

        /// <summary>
        /// Extension for DrawImage with an alternative text.
        /// </summary>
        public static void DrawImage(this XGraphics gfx, XImage image, XPoint point, double width, double height, string altText, XRect boundingBox)
        {
            gfx.DrawImage(image, point.X, point.Y, width, height, altText, boundingBox);
        }

        /// <summary>
        /// Extension for DrawImage with an alternative text.
        /// </summary>
        public static void DrawImage(this XGraphics gfx, XImage image, XRect rect, string altText, XRect boundingBox)
        {
            gfx.DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height, altText, boundingBox);
        }

        // DrawImage functions without given BoundingBox.

        /// <summary>
        /// Extension for DrawImage with an alternative text.
        /// </summary>
        public static void DrawImage(this XGraphics gfx, XImage image, double x, double y, string altText)
        {
            var boundingBox = GetBoundingBox(image, x, y);
            gfx.DrawImage(image, x, y, altText, boundingBox);
        }

        /// <summary>
        /// Extension for DrawImage with an alternative text.
        /// </summary>
        public static void DrawImage(this XGraphics gfx, XImage image, double x, double y, double width, double height, string altText)
        {
            var boundingBox = GetBoundingBox(image, x, y, width, height);
            gfx.DrawImage(image, x, y, width, height, altText, boundingBox);
        }

        /// <summary>
        /// Extension for DrawImage with an alternative text.
        /// </summary>
        public static void DrawImage(this XGraphics gfx, XImage image, XPoint point, string altText)
        {
            gfx.DrawImage(image, point.X, point.Y, altText);
        }

        /// <summary>
        /// Extension for DrawImage with an alternative text.
        /// </summary>
        public static void DrawImage(this XGraphics gfx, XImage image, XPoint point, double width, double height, string altText)
        {
            gfx.DrawImage(image, point.X, point.Y, width, height, altText);
        }

        /// <summary>
        /// Extension for DrawImage with an alternative text.
        /// </summary>
        public static void DrawImage(this XGraphics gfx, XImage image, XRect rect, string altText)
        {
            gfx.DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height, altText);
        }

        static XRect GetBoundingBox(XImage image, double x, double y)
        {
            return GetBoundingBox(image, x, y, image.PointWidth, image.PointHeight);
        }

        static XRect GetBoundingBox(XImage image, double x, double y, double width, double height)
        {
            // TODO: Transformations are not yet considered.
            return new XRect(x, y, width, height);
        }

        #endregion

        /// <summary>
        /// Extension for DrawLink with an alternative text.
        /// </summary>
        public static void DrawLink(this XGraphics gfx, string s, XFont font, XBrush brush, double x, double y, XStringFormat format, PdfLinkAnnotation linkAnnotation, string altText)
        {
            var sb = GetStructureBuilder(gfx);
            sb.BeginElement(linkAnnotation, altText);
            gfx.DrawString(s, font, brush, x, y, format);
            sb.End();
        }

        #region DrawLink overloads

        /// <summary>
        /// Extension for DrawLink with an alternative text.
        /// </summary>
        public static void DrawLink(this XGraphics gfx, string s, XFont font, XBrush brush, XPoint point, XStringFormat format, PdfLinkAnnotation linkAnnotation, string altText)
        {
            gfx.DrawLink(s, font, brush, point.X, point.Y, format, linkAnnotation, altText);
        }

        /// <summary>
        /// Extension for DrawLink with an alternative text.
        /// </summary>
        public static void DrawLink(this XGraphics gfx, string s, XFont font, XBrush brush, double x, double y, PdfLinkAnnotation linkAnnotation, string altText)
        {
            gfx.DrawLink(s, font, brush, x, y, XStringFormats.Default, linkAnnotation, altText);
        }

        /// <summary>
        /// Extension for DrawLink with an alternative text.
        /// </summary>
        public static void DrawLink(this XGraphics gfx, string s, XFont font, XBrush brush, XPoint point, PdfLinkAnnotation linkAnnotation, string altText)
        {
            gfx.DrawLink(s, font, brush, point.X, point.Y, XStringFormats.Default, linkAnnotation, altText);
        }

        #endregion

        /// <summary>
        /// Extension draws a list item with PDF Block Level Element tags.
        /// </summary>
        public static void DrawListItem(this XGraphics gfx, string label, string text, XFont font, XBrush brush, double x, double y, double labelWidth, XStringFormat format)
        {
            var sb = GetStructureBuilder(gfx);
            sb.BeginElement(PdfBlockLevelElementTag.ListItem);
            sb.BeginElement(PdfBlockLevelElementTag.Label);
            gfx.DrawString(label, font, brush, x, y);
            sb.End();
            sb.BeginElement(PdfBlockLevelElementTag.ListBody);
            gfx.DrawString(text, font, brush, x + labelWidth, y);
            sb.End();
            sb.End();
        }

        #region DrawListItem overloads

        /// <summary>
        /// Extension draws a list item with PDF Block Level Element tags.
        /// </summary>
        public static void DrawListItem(this XGraphics gfx, string label, string text, XFont font, XBrush brush, XPoint point, double labelWidth, XStringFormat format)
        {
            gfx.DrawListItem(label, text, font, brush, point.X, point.Y, labelWidth, format);
        }

        /// <summary>
        /// Extension draws a list item with PDF Block Level Element tags.
        /// </summary>
        public static void DrawListItem(this XGraphics gfx, string label, string text, XFont font, XBrush brush, double x, double y, double labelWidth)
        {
            gfx.DrawListItem(label, text, font, brush, x, y, labelWidth, XStringFormats.Default);
        }

        /// <summary>
        /// Extension draws a list item with PDF Block Level Element tags.
        /// </summary>
        public static void DrawListItem(this XGraphics gfx, string label, string text, XFont font, XBrush brush, XPoint point, double labelWidth)
        {
            gfx.DrawListItem(label, text, font, brush, point.X, point.Y, labelWidth);
        }

        #endregion

        // ReSharper disable once InconsistentNaming
        static UAManager GetUAManager(XGraphics gfx)
        {
            var page = gfx.PdfPage;
            if (page == null)
                throw new InvalidOperationException("Graphics object must belong to a PDF document page.");

            var uaManager = page.Owner._uaManager;  // HACK: Should be a property 
            if (uaManager == null)
                throw new InvalidOperationException("Document is not a PDF/UA document.");
            return uaManager;
        }

        static StructureBuilder GetStructureBuilder(XGraphics gfx)
        {
            return GetUAManager(gfx).StructureBuilder;
        }
    }
}
