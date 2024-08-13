// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using System.Reflection;
using PdfSharp.Pdf;

namespace PdfSharp.Quality
{
    /// <summary>
    /// Static helper functions for file IO.
    /// </summary>
    [Obsolete("Use IOUtility.")]
    public static class IOHelper  // #DELETE
    // ReSharper restore InconsistentNaming
    {
#if true_
        protected async Task<string> SaveToStreamOrSaveToFileAsync(PdfDocument document, Stream stream, string filenameTag, bool show)
        {
            string filename;

            // Save and show the document.
            if (stream == null)
            {
                if (show)
                    filename = await SaveAndShowDocumentAsync(document, filenameTag).ConfigureAwait(false);
                else
                    filename = await SaveDocumentAsync(document, filenameTag).ConfigureAwait(false);
            }
            else
            {
                document.Save(stream, false);
                stream.Position = 0;
                filename = null;
            }
            return filename;
        }
#endif

#if true_ && (CORE || GDI || WPF)
        protected async Task<string> SaveAndShowDocumentAsync(PdfDocument document, string filenameTag)
        {
            // Save the PDF document...
            var filename = await SaveDocumentAsync(document, filenameTag).ConfigureAwait(false);

            // ... and start a viewer.
            Process.St7art(filename);

            return filename;
        }
#endif

        //#if NET/FX_CORE || WUI || DNC10
        //        static async Task<string> SaveAndShowDocumentAsync(PdfDocument document, string filenameTag)
        //        {
        //            // Save the PDF document...
        //            string filename = await SaveDocumentAsync(document, filenameTag).ConfigureAwait(false);

        //            // ... and start a viewer.
        //            //Process.St7art(filename);
        //            return filename;
        //        }
        //#endif

        //#if true && (CORE || GDI || WPF || WUI)
        //#if true && (NET/FX_CORE || WUI)
        //        static async Task<string> SaveDocumentAsync(PdfDocument document, string filenameTag)
        //        {
        //            var filename = String.Format("{0:N}_{1}_tempfile.pdf", Guid.NewGuid(), filenameTag);
        //            document.Save(filename);
        //            await Task.Factory.StartNew(() => { }).ConfigureAwait(false);
        //            return filename;
        //        }
        //#endif

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
}
