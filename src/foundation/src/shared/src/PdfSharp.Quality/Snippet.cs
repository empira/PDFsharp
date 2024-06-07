// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System.IO.Pipes;

namespace PdfSharp.Quality
{
    /// <summary>
    /// Base class of code snippets for testing.
    /// </summary>
    public abstract class Snippet : FeatureAndSnippetBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Snippet"/> class.
        /// </summary>
        protected Snippet()
        { }

        /// <summary>
        /// Gets or sets the title of the snipped
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// Gets or sets the path or name where to store the PDF file.
        /// </summary>
        public string PathName { get; set; } = "snippets/path-not-specified-in-snippet/";

        /// <summary>
        /// Gets or sets the box option.
        /// </summary>
        protected BoxOptions BoxOption { get; set; } = BoxOptions.Tile;

        /// <summary>
        /// Gets or sets a value indicating how this <see cref="Snippet"/> is drawn.
        /// If it is true, all describing graphics like title and boxes are omitted.
        /// Use this option to produce a clean PDF for debugging purposes.
        /// If it is false, all describing graphics like title and boxes are drawn.
        /// This is the regular case.
        /// </summary>
        public bool Cleanroom { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all graphics output is omitted.
        /// </summary>
        public bool NoGraphic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all text output is omitted.
        /// </summary>
        public bool NoText { get; set; }

        /// <summary>
        /// The standard font name used in snippet.
        /// </summary>
        public static string FontNameStd { get; } = "Arial";

        /// <summary>
        /// Gets the standard font in snippets.
        /// </summary>
        public XFont FontStd =>
            // Create only on demand.
            _fontStd ??= new XFont(FontNameStd, 12, XFontStyleEx.Regular,
                new XPdfFontOptions(PdfFontEncoding.Unicode));
        XFont? _fontStd;

        /// <summary>
        /// Gets the header font in snippets.
        /// </summary>
        public XFont FontHeader =>
            // Create only on demand.
            _fontHeader ??= new XFont(FontNameStd, 9, XFontStyleEx.Regular,
                new XPdfFontOptions(PdfFontEncoding.Unicode));
        XFont? _fontHeader;

        /// <summary>
        /// Draws a text that states that a feature is not supported in a particular build.
        /// </summary>
        protected void DrawNotSupportedFeature(XGraphics gfx)
        {
            var message = $"This feature is not supported in {PdfSharpTechnology}.";

            gfx.DrawString(message, FontStd, XBrushes.DarkSlateGray, 10, 10);
        }

        /// <summary>
        /// Draws the header.
        /// </summary>
        public void DrawHeader(XGraphics gfx, string renderTarget)
        {
            if (Cleanroom || NoText)
                return;

            var header = $"{GetType().Name} - {PdfSharpTechnology} - {renderTarget}";
            gfx.DrawString(header, FontHeader, XBrushes.DarkSlateGray, WidthInPU / 2, 0, XStringFormats.TopCenter);
        }

        /// <summary>
        /// Draws the header.
        /// </summary>
        public void DrawHeader(string renderTarget)
        {
            DrawHeader(XGraphics, renderTarget);
        }

        /// <summary>
        /// Draws the header for a PDF document.
        /// </summary>
        public void DrawPdfHeader()
        {
            DrawHeader("PDF");
        }

        /// <summary>
        /// Draws the header for a PNG image.
        /// </summary>
        public void DrawPngHeader()
        {
            DrawHeader("PNG");
        }

        /// <summary>
        /// When implemented in a derived class renders the snippet in the specified XGraphic object.
        /// </summary>
        /// <param name="gfx">The XGraphics where to render the snippet.</param>
        public abstract void RenderSnippet(XGraphics gfx);

        /// <summary>
        /// When implemented in a derived class renders the snippet in an XGraphic object.
        /// </summary>
        public void RenderSnippet()
        {
            RenderSnippet(XGraphics);
        }

        /// <summary>
        /// Renders a snippet as PDF document.
        /// </summary>
        public void RenderSnippetAsPdf()
        {
            RenderSnippetAsPdf(XUnit.FromPoint(WidthInPoint), XUnit.FromPoint(HeightInPoint), XGraphicsUnit.Presentation, XPageDirection.Downwards);
        }

        /// <summary>
        /// Renders a snippet as PDF document.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="graphicsUnit"></param>
        /// <param name="pageDirection"></param>
        public void RenderSnippetAsPdf(XUnit width, XUnit height, XGraphicsUnit graphicsUnit, XPageDirection pageDirection)
        {
            BeginPdfDocument();
            BeginPdfPage(width, height, graphicsUnit, pageDirection);
            RenderSnippet();
            DrawPdfHeader();
            EndPdfPage();
            EndPdfDocument();
        }

        /// <summary>
        /// Renders a snippet as PNG image.
        /// </summary>
        public void RenderSnippetAsPng()
        {
#if !CORE && !WPF
            BeginImage();
            RenderSnippet();
            DrawPngHeader();
            EndPngImage();
#endif
        }

        /// <summary>
        /// Creates a PDF page for the specified document.
        /// </summary>
        /// <param name="doc">The document.</param>
        public PdfPage CreatePdfPage(PdfDocument doc)
        {
            var page = doc.AddPage();
            page.Width = XUnit.FromPresentation(WidthInPU);
            page.Height = XUnit.FromPresentation(HeightInPU);
            return page;
        }

        /// <summary>
        /// Translates origin of coordinate space to a box of size 220pp x 140pp.
        /// </summary>
        protected void BeginBox(XGraphics gfx, int n, BoxOptions options, string? description)
        {
            double dx = (WidthInPU - 2 * BoxWidth) / 3;
            double dy = (HeightInPU - 4 * BoxHeight) / 5;
            double x = (1 - n % 2) * BoxWidth + (2 - n % 2) * dx;
            double y = ((n + 1) / 2 - 1) * BoxHeight + ((n + 1) / 2 + 0) * dy;
            gfx.WriteComment($"========== Start Of Box {n} ==========");
            gfx.Save();
            gfx.TranslateTransform(x, y);
            if (Cleanroom)
            {
                gfx.WriteComment(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> -- begin of box content");

                return;
            }

            //Brush tileBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204));
            //if (options != BoxOptions.None)
            //{
            //  double strokeWidth = 0.75;
            //  double adjust = strokeWidth / 2;
            //  Pen pen = new Pen(tileBrush, 0.75);
            //  //pen.DashStyle = DashStyles.DashDot;
            //  dc.DrawRectangle(null, pen, new Rect(0 + adjust, 0 + adjust, BoxWidth - strokeWidth, BoxHeight - strokeWidth));
            //}
            switch (options)
            {
                case BoxOptions.None:
                    break;

                case BoxOptions.Box:
                    gfx.DrawRectangle(XPens.DarkGray, 0, 0, BoxWidth, BoxHeight);
                    break;

                case BoxOptions.DrawX:
                    {
                        var pen = new XPen(XColor.FromArgb(202, 202, 202), 3);
                        gfx.DrawLine(pen, 0, 0, BoxWidth, BoxHeight);
                        gfx.DrawLine(pen, 0, BoxHeight, BoxWidth, 0);
                    }
                    break;

                case BoxOptions.Fill:
                    {
                        gfx.DrawRectangle(Gray, 0, 0, BoxWidth, BoxHeight);
                    }
                    break;

                case BoxOptions.Tile:
                    {
#if true
                        DrawTiledBox(gfx, 0, 0, BoxWidth, BoxHeight, 8);
#else
                        var path = new XGraphicsPath();
                        var points = new List<XPoint>();
                        const double delta = 8;
                        points.Add(new XPoint(0, 0));
                        for (double xx = 0; xx < BoxWidth; xx += 2 * delta)
                        {
                            points.Add(new XPoint(xx, BoxHeight));
                            points.Add(new XPoint(xx + delta, BoxHeight));
                            points.Add(new XPoint(xx + delta, 0));
                            points.Add(new XPoint(xx + 2 * delta, 0));
                        }
                        points.Add(new XPoint(BoxWidth, BoxHeight));
                        for (double yy = BoxHeight; yy > 0; yy -= 2 * delta)
                        {
                            points.Add(new XPoint(0, yy));
                            points.Add(new XPoint(0, yy - delta));
                            points.Add(new XPoint(BoxWidth, yy - delta));
                            points.Add(new XPoint(BoxWidth, yy - 2 * delta));
                        }
                        path.AddPolygon(points.ToArray());
                        gfx.DrawPath(_gray, path);
#endif
                    }
                    break;
            }
            if (!NoText && !Cleanroom && !String.IsNullOrEmpty(description))
            {
                gfx.DrawString(description, new XFont("Arial", 8, XFontStyleEx.Italic), XBrushes.Black,
                    new XRect(0, BoxHeight + 8, BoxWidth, 0), XStringFormats.BaseLineCenter);
            }
            gfx.WriteComment(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> --  begin of box content");
        }

        /// <summary>
        /// Begins rendering the content of a box.
        /// </summary>
        /// <param name="gfx">The XGraphics object.</param>
        /// <param name="boxNumber">The box number.</param>
        protected void BeginBox(XGraphics gfx, int boxNumber)
        {
            BeginBox(gfx, boxNumber, BoxOption, null);
        }

        /// <summary>
        /// Begins rendering the content of a box.
        /// </summary>
        /// <param name="gfx">The XGraphics object.</param>
        /// <param name="boxNumber">The box number.</param>
        /// <param name="options">The box options.</param>
        protected void BeginBox(XGraphics gfx, int boxNumber, BoxOptions options)
        {
            BeginBox(gfx, boxNumber, options, null);
        }

        /// <summary>
        /// Ends rendering of the current box.
        /// </summary>
        /// <param name="gfx">The XGraphics object.</param>
        protected void EndBox(XGraphics gfx)
        {
            gfx.WriteComment("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< -- end of box content");
            gfx.Restore();
        }

        //[Obsolete]
        //protected void EndBox(XGraphics gfx, string subTitle)
        //{
        //    //if (!NoText)
        //    //{
        //    //    gfx.DrawString(subTitle, new XFont("Arial", 8, XFontStyleEx.Italic), XBrushes.Black,
        //    //        new XRect(0, BoxHeight + 8, BoxWidth, 0), XStringFormats.BaseLineCenter);
        //    //}
        //    EndBox(gfx);
        //}

        /// <summary>
        /// Draws a tiled box.
        /// </summary>
        /// <param name="gfx">The XGraphics object.</param>
        /// <param name="x">The left position of the box.</param>
        /// <param name="y">The top position of the box.</param>
        /// <param name="width">The width of the box.</param>
        /// <param name="height">The height of the box.</param>
        /// <param name="delta">The size of the grid square.</param>
        protected void DrawTiledBox(XGraphics gfx, double x, double y, double width, double height, double delta)
        {
            var path = new XGraphicsPath();
            var points = new List<XPoint> { new(x, y) };
            for (var xx = x; xx < x + width; xx += 2 * delta)
            {
                points.Add(new(xx, y + height));
                points.Add(new(xx + delta, y + height));
                points.Add(new(xx + delta, y));
                points.Add(new(xx + 2 * delta, y));
            }

            points.Add(new(x + width, y + height));
            for (var yy = y + height; yy > y; yy -= 2 * delta)
            {
                points.Add(new(x, yy));
                points.Add(new(x, yy - delta));
                points.Add(new(x + width, yy - delta));
                points.Add(new(x + width, yy - 2 * delta));
            }
            path.AddPolygon(points.ToArray());
             gfx.DrawPath(Gray, path);
        }

        /// <summary>
        /// Gets the rectangle of a box.
        /// </summary>
        protected static XRect RectBox { get; } = new(0, 0, BoxWidth, BoxHeight);

        /// <summary>
        /// Draws the center point of a box.
        /// </summary>
        /// <param name="gfx">The XGraphics object.</param>
        protected static void DrawCenterPoint(XGraphics gfx)
        {
            const double centerV = BoxHeight / 2;
            const double centerH = BoxWidth / 2;
            const double size = 2;

            gfx.DrawEllipse(XBrushes.Red, centerH - size / 2, centerV - size / 2, size, size);
        }

        /// <summary>
        /// Draws the alignment grid.
        /// </summary>
        /// <param name="gfx">The XGraphics object.</param>
        /// <param name="highlightHorizontal">if set to <c>true</c> [highlight horizontal].</param>
        /// <param name="highlightVertical">if set to <c>true</c> [highlight vertical].</param>
        protected static void DrawAlignmentGrid(XGraphics gfx, bool highlightHorizontal = false, bool highlightVertical = false)
        {
            var rectBox = new XRect(0, 0, BoxWidth, BoxHeight);

            const double centerV = BoxHeight / 2;
            const double centerH = BoxWidth / 2;

            gfx.DrawRectangle(XPens.YellowGreen, rectBox);
            gfx.DrawLine(XPens.YellowGreen, 0, centerV, BoxWidth, centerV);
            gfx.DrawLine(XPens.YellowGreen, centerH, 0, centerH, BoxHeight);

            if (highlightHorizontal)
                gfx.DrawLine(XPens.Red, 0, centerV, BoxWidth, centerV);

            if (highlightVertical)
                gfx.DrawLine(XPens.Red, centerH, 0, centerH, BoxHeight);
        }

        /// <summary>
        /// Draws a dotted line showing the art box.
        /// </summary>
        /// <param name="gfx">The XGraphics object.</param>
        protected void DrawArtBox(XGraphics gfx)
        {
            var pen1 = new XPen(XColors.DarkGreen, 1) { DashStyle = XDashStyle.Dot };

            gfx.DrawRectangle(pen1, 12, 12, 200, 120);
            gfx.DrawLine(pen1, 12, 12, 212, 132);
            gfx.DrawLine(pen1, 212, 12, 12, 132);
        }

        /// <summary>
        /// Gets the points of a pentagram in a unit circle.
        /// </summary>
        protected static XPoint[] Pentagram
        {
            get
            {
                var order = new[] { 0, 3, 1, 4, 2 };
                var pentagram = new XPoint[5];
                for (int idx = 0; idx < 5; idx++)
                {
                    double rad = order[idx] * 2 * Math.PI / 5 - Math.PI / 10;
                    pentagram[idx].X = Math.Cos(rad);
                    pentagram[idx].Y = Math.Sin(rad);
                }
                return pentagram;
            }
        }

        /// <summary>
        /// Gets the points of a pentagram relative to a center and scaled by a size factor.
        /// </summary>
        /// <param name="size">The scaling factor of the pentagram.</param>
        /// <param name="center">The center of the pentagram.</param>
        /// <returns></returns>
        protected static XPoint[] GetPentagram(double size, XPoint center)
        {
            var points = Pentagram;
            for (int idx = 0; idx < 5; idx++)
            {
                points[idx].X = points[idx].X * size + center.X;
                points[idx].Y = points[idx].Y * size + center.Y;
            }
            return points;
        }

        /// <summary>
        /// Creates a HelloWorld document, optionally with custom message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="unicode"></param>
        public static PdfDocument HelloWorldFactory(string? message = null, bool unicode = false)
        {
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

            // Create an empty page.
            var page = document.AddPage();

            // Get an XGraphics object for drawing.
            using var gfx = XGraphics.FromPdfPage(page);

            // Create a font.
            var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            var font = unicode
                ? new XFont(FontNameStd, 20, XFontStyleEx.Bold, options)
                : new XFont(FontNameStd, 20, XFontStyleEx.Bold);

            // Draw the text.
            gfx.DrawString(String.IsNullOrEmpty(message) ? "Hello, World!" : message, font, XBrushes.Black,
                new XRect(0, 0, page.Width.Point, page.Height.Point),
                XStringFormats.Center);

            return document;
        }

        /// <summary>
        /// Gets a XGraphics object for the last page of the specified document.
        /// </summary>
        /// <param name="doc">The PDF document.</param>
        public static XGraphics GfxForLastPage(PdfDocument doc)
        {
            XGraphics gfx = XGraphics.FromPdfPage(doc.Pages[^1]);
            return gfx;
        }

#if old
//        protected DrawingVisual PrepareDrawingVisual(out DrawingContext dc)
//        {
//            return PrepareDrawingVisual(out dc, true);
//        }

//        protected DrawingVisual PrepareDrawingVisual(out DrawingContext dc, bool drawBackground)
//        {
//            DrawingVisual dv = new DrawingVisual();
//            dc = dv.RenderOpen();
//            if (drawBackground)
//                DrawBackground(dc);
//            //dc.DrawLine(new Pen(Brushes.Red, 3), new Point(0, 0), new Point(WidthInPU, HeightInPU));
//            //dc.DrawLine(new Pen(Brushes.Red, 3), new Point(WidthInPU, 0), new Point(0, HeightInPU));
//            return dv;
//        }

//        void DrawBackground(DrawingContext dc)
//        {
//            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, WidthInPU, HeightInPU));
//        }

//        /// <summary>
//        /// Translates origin of coordinate space to a box of size 220pp x 140pp.
//        /// </summary>
//        protected void BeginBox(DrawingContext dc, int n, BoxOptions options, string description)
//        {
//            double dx = (WidthInPU - 2 * BoxWidth) / 3;
//            double dy = (HeightInPU - 4 * BoxHeight) / 5;
//            double x = (1 - n % 2) * BoxWidth + (2 - n % 2) * dx;
//            double y = ((n + 1) / 2 - 1) * BoxHeight + ((n + 1) / 2) * dy;
//            dc.PushTransform(new TranslateTransform(x, y));

//            Brush tileBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204));
//            //if (options != BoxOptions.None)
//            //{
//            //  double strokeWidth = 0.75;
//            //  double adjust = strokeWidth / 2;
//            //  Pen pen = new Pen(tileBrush, 0.75);
//            //  //pen.DashStyle = DashStyles.DashDot;
//            //  dc.DrawRectangle(null, pen, new Rect(0 + adjust, 0 + adjust, BoxWidth - strokeWidth, BoxHeight - strokeWidth));
//            //}
//            switch (options)
//            {
//                case BoxOptions.None:
//                    break;

//                case BoxOptions.Box:
//                    {
//                        Pen pen = new Pen(tileBrush, 3);
//                        dc.DrawLine(pen, new Point(0, 0), new Point(BoxWidth, 0));
//                        dc.DrawLine(pen, new Point(BoxWidth, 0), new Point(BoxWidth, BoxHeight));
//                        dc.DrawLine(pen, new Point(BoxWidth, BoxHeight), new Point(0, BoxHeight));
//                        dc.DrawLine(pen, new Point(0, BoxHeight), new Point(0, 0));
//                    }
//                    break;

//                case BoxOptions.DrawX:
//                    {
//                        Pen pen = new Pen(tileBrush, 3);
//                        dc.DrawLine(pen, new Point(0, 0), new Point(BoxWidth, BoxHeight));
//                        dc.DrawLine(pen, new Point(0, BoxHeight), new Point(BoxWidth, 0));
//                    }
//                    break;

//                case BoxOptions.Fill:
//                    {
//                        dc.DrawRectangle(tileBrush, null, new Rect(0, 0, BoxWidth, BoxHeight));
//                    }
//                    break;

//                case BoxOptions.Tile:
//                    {
//#if true
//                        double delta = 8;
//                        PathGeometry path = new PathGeometry();
//                        for (double xx = 0; xx < BoxWidth; xx += 2 * delta)
//                            path.AddGeometry(new RectangleGeometry(new Rect(xx, 0, delta, BoxHeight)));
//                        for (double yy = 0; yy < BoxHeight; yy += 2 * delta)
//                            path.AddGeometry(new RectangleGeometry(new Rect(0, yy, BoxWidth, delta)));
//                        dc.DrawGeometry(tileBrush, null, path);
//#else
//            double delta = 5;
//            bool draw1 = true;
//            for (double yy = 0; yy < BoxHeight; yy += delta, draw1 = !draw1)
//            {
//              bool draw2 = true;
//              for (double xx = 0; xx < BoxWidth; xx += delta, draw2 = !draw2)
//                if ((draw1 && draw2) || (!draw1 && !draw2))
//                  dc.DrawRectangle(tileBrush, null, new Rect(xx, yy, delta, delta));
//            }
//#endif
//                    }
//                    break;
//            }
//            if (options != BoxOptions.None && !String.IsNullOrEmpty(description))
//                dc.DrawText(new FormattedText(description, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 7.5, Brushes.Black), new Point(0, BoxHeight + 0));
//        }

//        protected void BeginBox(DrawingContext dc, int boxNumber)
//        {
//            BeginBox(dc, boxNumber, BoxOptions.None, null);
//        }

//        protected void BeginBox(DrawingContext dc, int boxNumber, BoxOptions options)
//        {
//            BeginBox(dc, boxNumber, options, null);
//        }

//        protected void EndBox(DrawingContext dc)
//        {
//            dc.Pop();
//        }

//        /// <summary>
//        /// Renders a visual as PNG, XPS, and PDF.
//        /// </summary>
//        public void RenderVisual(string name, RenderMethod renderMethod, bool copyAcroTests)
//        {
//            Name = name;

//            this.visual = renderMethod();
//            SaveImage();
//            SaveXps();
//            SavePdf();
//            AppendToResultPdf();

//            if (copyAcroTests && Directory.Exists("..\\..\\..\\!AcroTests"))
//            {
//                string from, to;
//                string s = Directory.GetCurrentDirectory(); // TO/DO: use GetDirectoryName(Assembly.GetExecutingAssembly().Location)
//                from = name + ".xml";
//                to = "..\\..\\..\\!AcroTests\\" + from;
//                if (File.Exists(to))
//                    File.Delete(to);
//                File.Copy(from, to);

//                try
//                {
//                    from = name + ".xps";
//                    to = "..\\..\\..\\AcroTests\\" + from;
//                    if (File.Exists(to))
//                        File.Delete(to);
//                    File.Copy(from, to);
//                }
//                catch { }
//            }
//        }

//        public void RenderVisual(string name, RenderMethod renderMethod)
//        {
//            RenderVisual(name, renderMethod, false);
//        }

//        /// <summary>
//        /// Renders the current visual to an image and saves it as a PNG file.
//        /// </summary>
//        public void SaveImage()
//        {
//            // Create an appropriate render bitmap
//            const int factor = 3;
//            int width = (int)(WidthInPoint * factor);
//            int height = (int)(HeightInPoint * factor);
//            this.image = new RenderTargetBitmap(width, height, 72 * factor, 72 * factor, PixelFormats.Default);
//            if (visual is UIElement)
//            {
//                // Perform layout on UIElement - otherwise nothing gets rendered
//                UIElement element = visual as UIElement;
//                Size size = new Size(WidthInPU, HeightInPU);
//                element.Measure(size);
//                element.Arrange(new Rect(new Point(), size));
//                element.UpdateLayout();
//            }
//            this.image.Render(visual);

//            // Save image as PNG
//            FileStream stream = new FileStream(Path.Combine(OutputDirectory, Name + ".png"), FileMode.Create);
//            PngBitmapEncoder encoder = new PngBitmapEncoder();
//            //string author = encoder.CodecInfo.Author.ToString();
//            encoder.Frames.Add(BitmapFrame.Create(this.image));
//            encoder.Save(stream);
//            stream.Close();
//        }

//        /// <summary>
//        /// Renders the current visual as a FixedPage and save it as XPS file.
//        /// </summary>
//        public void SaveXps()
//        {
//            XpsDocument xpsDocument = new XpsDocument(Path.Combine(OutputDirectory, Name + ".xps"), FileAccess.ReadWrite);
//            PageContent pageContent = new PageContent();

//            FixedPage fixedPage = new FixedPage();
//            fixedPage.Width = WidthInPU;
//            fixedPage.Height = HeightInPU;
//            fixedPage.Background = Brushes.Transparent;

//            // Visuals needs a UIElement as drawing container
//            VisualPresenter presenter = new VisualPresenter();
//            presenter.AddVisual(this.visual);

//            FixedPage.SetLeft(presenter, 0);
//            FixedPage.SetTop(presenter, 0);
//            fixedPage.Children.Add(presenter);

//            // Perform layout
//            Size size = new Size(WidthInPU, HeightInPU);
//            fixedPage.Measure(size);
//            fixedPage.Arrange(new Rect(new Point(), size));
//            fixedPage.UpdateLayout();

//            ((IAddChild)pageContent).AddChild(fixedPage);

//            FixedDocument fixedDocument = new FixedDocument();
//            fixedDocument.DocumentPaginator.PageSize = size;
//            fixedDocument.Pages.Add(pageContent);

//            // Save as XPS file
//            XpsDocumentWriter xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
//            xpsWriter.Write(fixedDocument);
//            xpsDocument.Close();

//            // Must call at least two times GC.Collect this to get access to the xps file even I write synchronously. This is a bug in WPF.
//            // Vista 64 .NET 3.5 SP1 installed
//            xpsDocument = null;
//            xpsWriter = null;
//            GC.Collect(10, GCCollectionMode.Forced);
//            GC.Collect(10, GCCollectionMode.Forced);
//            //GC.Collect(10, GCCollectionMode.Forced);
//            //GC.Collect(10, GCCollectionMode.Forced);
//        }

//        /// <summary>
//        /// Converts the XPS file to a PDF file.
//        /// </summary>
//        public void SavePdf()
//        {
//            PdfDocument document = new PdfDocument();
//            PdfPage page = document.AddPage();
//            page.Width = WidthInPoint;
//            page.Height = HeightInPoint;
//            PdfSharp.Xps.XpsRenderer.RenderPage_Test01(page, Path.Combine(OutputDirectory, Name + ".xps"));
//            document.Save(Path.Combine(OutputDirectory, Name + ".pdf"));
//        }

//        /// <summary>
//        /// Append PDF and bitmap image to result PDF file.
//        /// </summary>
//        public void AppendToResultPdf()
//        {
//            string resultFileName = Path.Combine(OutputDirectory, "~TestResult.pdf");
//            PdfDocument pdfResultDocument = null;
//            if (File.Exists(resultFileName))
//                pdfResultDocument = PdfReader.Open(resultFileName, PdfDocumentOpenMode.Modify);
//            else
//            {
//                pdfResultDocument = new PdfDocument();
//                pdfResultDocument.Info.Title = "XPS to PDF Unit Tests";
//                pdfResultDocument.Info.Author = "Stefan Lange";
//                pdfResultDocument.PageLayout = PdfPageLayout.SinglePage;
//                pdfResultDocument.PageMode = PdfPageMode.UseNone;
//                pdfResultDocument.ViewerPreferences.FitWindow = true;
//                pdfResultDocument.ViewerPreferences.CenterWindow = true;
//            }
//            PdfPage page = pdfResultDocument.AddPage();
//            page.Orientation = PageOrientation.Landscape;
//            XGraphics gfx = XGraphics.FromPdfPage(page);
//            gfx.DrawRectangle(XBrushes.GhostWhite, new XRect(0, 0, 1000, 1000));

//            double x1 = XUnit.FromMillimeter((297 - 2 * WidthInMillimeter) / 3);
//            double x2 = XUnit.FromMillimeter((297 - 2 * WidthInMillimeter) / 3 * 2 + WidthInMillimeter);
//            double y = XUnit.FromMillimeter((210 - HeightInMillimeter) / 2);
//            double yt = XUnit.FromMillimeter(HeightInMillimeter) + y + 20;
//            gfx.DrawString(String.Format("XPS to PDF Unit Test '{0}'", Name), new XFont("Arial", 9, XFontStyleEx.Bold),
//              XBrushes.DarkRed, new XPoint(x1, 30));

//            // Draw the PDF result
//            gfx.DrawString("This is a vector based PDF form created with PDFsharp from an XPS file", new XFont("Arial", 8),
//              XBrushes.DarkBlue, new XRect(x1, yt, WidthInPoint, 0), XStringFormats.Default);

//            XPdfForm form = XPdfForm.FromFile(Path.Combine(OutputDirectory, Name + ".pdf"));
//            gfx.DrawImage(form, new XPoint(x1, y));

//            // Draw the result bitmap
//            gfx.DrawString("As a reference, this is a bitmap image created with WPF from the Visual contained in the XPS file", new XFont("Arial", 8),
//              XBrushes.DarkBlue, new XRect(x2, yt, WidthInPoint, 0), XStringFormats.Default);

//            XImage image = XImage.FromFile(Path.Combine(OutputDirectory, Name + ".png"));
//            image.Interpolate = false;
//            gfx.DrawImage(image, new XPoint(x2, y));

//            pdfResultDocument.Save(resultFileName);
//        }

//        protected string GetDirectory(string path)
//        {
//            string dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // Directory.GetCurrentDirectory();
//            string dirName = path.Substring(0, path.IndexOf('/', StringComparison.Ordinal));

//            int slash;
//            while ((slash = dir.LastIndexOf("\\", StringComparison.Ordinal)) != -1)
//            {
//                if (dir.EndsWith(dirName, StringComparison.InvariantCultureIgnoreCase))
//                {
//                    dir = Path.Combine(dir, path.Substring(path.IndexOf('/', StringComparison.Ordinal) + 1));
//                    return dir;
//                }
//                dir = dir.Substring(0, slash);
//            }
//            return null;
//        }

//        public string Name;
//        Visual visual;
//        RenderTargetBitmap image;
//        public static string OutputDirectory = ".";
//    }

//    internal class XamlPresenter
//    {
//        public XamlPresenter(Type type, string name)
//        {
//            this.type = type;
//            this.name = name;
//        }
//        Type type;
//        string name;

//        public Visual CreateContent()
//        {
//            Canvas canvas = null;
//            using (Stream stream = Assembly.GetAssembly(this.type).GetManifestResourceStream(this.type, this.name))
//                if (stream != null)
//                    using (XmlReader xmlReader = XmlReader.Create(stream))
//                        canvas = (Canvas)XamlReader.Load(xmlReader);
//            Debug.Assert(canvas != null);
//            return canvas;
//        }
#endif

#if even_older
    ///// <summary>
    ///// Prepares new PDF page for drawing.
    ///// </summary>
    //public void BeginPdf()
    //{
    //  //document = new PdfDocument();
    //  //PdfPage page = document.AddPage();
    //  //page.Width = WidthInPoint;
    //  //page.Height = HeightInPoint;
    //  //this.pdfGfx = XGraphics.FromPdfPage(page);

    //  //// Draw a bounding box
    //  //XRect rect = new XRect(0.5, 0.5, WidthInPoint - 1, HeightInPoint - 1);
    //  //this.pdfGfx.DrawRectangle(XBrushes.WhiteSmoke, rect);
    //}

    ///// <summary>
    ///// Ends current PDF page.
    ///// </summary>
    //public void EndPdf()
    //{
    //  //this.pdfGfx = null;
    //}

    ///// <summary>
    ///// Prepares new bitmap image for drawing.
    ///// </summary>
    //public void BeginImage()
    //{
    //  //int factor = 4;
    //  //int width = (int)(WidthInPoint * factor);
    //  //int height = (int)(HeightInPoint * factor);
    //  //this.image = new RenderTargetBitmap(width, height, 72 * factor, 72 * factor, PixelFormats.Default);
    //  //this.visual = new DrawingVisual();
    //  //this.dc = this.visual.RenderOpen();

    //  //// Draw a bounding box
    //  ////Rect rect = new Rect(0, 0, width - 1, height - 1);
    //  //Rect rect = new Rect(0, 0, WidthInPoint * 4 / 3 - 1, HeightInPoint * 4 / 3 - 1);
    //  ////this.dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.LightGray, 1), rect);
    //  //this.dc.DrawRectangle(Brushes.WhiteSmoke, null, rect);

    //  ////double d = 10;
    //  ////Pen pen = new Pen(Brushes.Red, 5);
    //  ////this.dc.DrawLine(pen, rect.TopLeft, rect.TopLeft + new Vector(d, d));
    //  ////this.dc.DrawLine(pen, rect.TopRight, rect.TopRight + new Vector(-d, d));
    //  ////this.dc.DrawLine(pen, rect.BottomLeft, rect.BottomLeft + new Vector(d, -d));
    //  ////this.dc.DrawLine(pen, rect.BottomRight, rect.BottomRight + new Vector(-d, -d));

    //  ////this.dc.PushTransform(new ScaleTransform(factor, factor));
    //  //this.imgGfx = XGraphics.FromDrawingContext(this.dc, new XSize(WidthInPoint, HeightInPoint), XGraphicsUnit.Point);
    //}

    ///// <summary>
    ///// Ends current GDI+ image.
    ///// </summary>
    //public void EndImage()
    //{
    //  //this.gdiGfx.Dispose();
    //  //this.image.Dispose();
    //  //this.image.Dispose();

    //  //this.image = null;
    //  //this.gdiGfx = null;
    //  //this.imgGfx = null;
    //}
#endif
    }

    /// <summary>
    /// Extensions for the XGraphics class.
    /// </summary>
    public static class XGraphicsDevExtensions
    {
        /// <summary>
        /// Draws the measurement box for a specified text and a font.
        /// </summary>
        /// <param name="gfx">The XGraphics object</param>
        /// <param name="text">The text to be measured.</param>
        /// <param name="font">The font to be used for measuring.</param>
        /// <param name="pos">The start point of the box.</param>
        public static void DrawMeasureBox(this XGraphics gfx, string text, XFont font, XPoint pos)
        {
            var pen = new XPen(XColor.FromArgb(0x80, 0xFF, 0x00, 0x00), 1);
            var size = gfx.MeasureString(text, font);
            int space = font.CellSpace;
            //int units = font.UnitsPerEm;
            double lineSpace = font.GetHeight();
            int height = font.Height;
            int height2 = font.FontFamily.GetEmHeight(font.Style);
            int ascend = font.CellAscent;
            int descent = font.CellDescent;
            var point = new XPoint(pos.X, pos.Y - size.Height + descent * lineSpace / space);
            gfx.DrawRectangle(pen, new XRect(point, size));
            gfx.DrawLine(pen, pos.X + 0.5, pos.Y, pos.X + size.Width - 0.5, pos.Y);
        }
    }
}
