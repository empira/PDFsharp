// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO;
//#if NET/FX_CORE || UWP || DNC10
//using System.Threading.Tasks;
//using Windows.Foundation;
//using Windows.Storage;
//#endif
#if CORE || GDI
#if !NETSTANDARD2_0_OR_GREATER
using System.Drawing.Imaging;
#endif
#endif
using PdfSharp.Drawing;
using PdfSharp.Events;
using PdfSharp.Pdf;

// ReSharper disable ConvertToAutoProperty

namespace PdfSharp.Quality
{
    /// <summary>
    /// Base class with common code for both features and snippets.
    /// </summary>
    public abstract class FeatureAndSnippetBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureAndSnippetBase"/> class.
        /// </summary>
        protected FeatureAndSnippetBase()
        { }

        /// <summary>
        /// Specifies how to draw a box on the PDF page.
        /// </summary>
        protected enum BoxOptions
        {
            /// <summary>
            /// Draw no box.
            /// </summary>
            None,

            /// <summary>
            /// Draw a box.
            /// </summary>
            Box,

            /// <summary>
            /// Draw 
            /// </summary>
            DrawX,

            /// <summary>
            /// Draw 
            /// </summary>
            Fill,

            /// <summary>
            /// Draw 
            /// </summary>
            Tile,
        }

        // The size of the test page was designed as 135mm times 180mm.
        // This is in point 382.677 times 510.236. To make it easier to check
        // PDF content the size is rounded to 380 times 510 point, that is
        // 134.055mm times 179.971mm
        //
        //public static readonly double WidthInMillimeter = 135;
        //public static readonly double HeightInMillimeter = 180;
        //public static readonly double WidthInPoint = 380; // 360
        //public static readonly double HeightInPoint = 510;  // 480

        /// <summary>
        /// The width of the PDF page in point.
        /// </summary>
        public const double WidthInPoint = 360;

        /// <summary>
        /// The height of the PDF page in point.
        /// </summary>
        public const double HeightInPoint = 480;

        /// <summary>
        /// The width of the PDF page in millimeter.
        /// </summary>
        public const double WidthInMillimeter = WidthInPoint * 25.4 / 72;  // 127cm

        /// <summary>
        /// The height of the PDF page in millimeter.
        /// </summary>
        public const double HeightInMillimeter = HeightInPoint * 25.4 / 72;  // 169.3‾cm

        // ReSharper disable InconsistentNaming
        /// <summary>
        /// The width of the PDF page in presentation units.
        /// </summary>
        public const double WidthInPU = 480;

        /// <summary>
        /// The width of the PDF page in presentation units.
        /// </summary>
        public const double HeightInPU = 640;
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// The width of the drawing box in presentation units.
        /// </summary>
        public const double BoxWidth = 224;  // = 168pt
        //public const double BoxWidth = 220;  // = 160pt

        /// <summary>
        /// The height of the drawing box in presentation units.
        /// </summary>
        public const double BoxHeight = 144;  // = 108pt
        //public const double BoxHeight = 150;  // = 200pt

        /// <summary>
        /// The center of the box.
        /// </summary>
        public XPoint BoxCenter { get; } = new(BoxWidth / 2, BoxHeight / 2);

        /// <summary>
        /// Gets the gray background brush for boxes.
        /// </summary>
        protected XSolidBrush Gray { get; } = new(XColor.FromArgb(204, 204, 204));

        /// <summary>
        /// Gets a tag that specifies the PDFsharp build technology.
        /// It is either 'core', 'gdi', or 'wpf'.
        /// </summary>
        public static string PdfSharpTechnology
        {
            get
            {
                if (Capabilities.Build.IsCoreBuild)
                    return "core";
                if (Capabilities.Build.IsGdiBuild)
                    return "gdi";
                if (Capabilities.Build.IsWpfBuild)
                    return "wpf";
                throw new NotImplementedException("Unknown build.");
            }
        }

        /// <summary>
        /// Creates a new PDF document.
        /// </summary>
        public void BeginPdfDocument()
        {
            var document = new PdfDocument();
            document.Info.Author = "PDFsharp team";
            document.PageLayout = PdfPageLayout.SinglePage;
            //document.PageMode = PdfPageMode.FullScreen;

            Document = document;
        }

        /// <summary>
        /// Ends access to the current PDF document and renders it to a memory stream.
        /// </summary>
        public void EndPdfDocument()
        {
            var stream = new MemoryStream();
            Document.Save(stream);

            Page = null!;
            XGraphics = null!;

            var bytes = stream.ToArray();
            PdfBytes = bytes;
        }

        /// <summary>
        /// Creates a new page in the current PDF document.
        /// </summary>
        public XGraphics BeginPdfPage()
        {
            return BeginPdfPage(XUnit.FromPoint(WidthInPoint), XUnit.FromPoint(HeightInPoint), XGraphicsUnit.Presentation, XPageDirection.Downwards);
#if true_
            //# DELETE
            //var page = Document.AddPage();

            //page.Width = WidthInPoint;  // = XUnit.FromPresentation(WidthInPU);
            //page.Height = HeightInPoint;  // = XUnit.FromPresentation(HeightInPU);

            //// All drawing is done in presentation units (1/96 inch).
            //var gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Presentation);
            //// Draw a bounding box.
            //var rect = new XRect(0.5, 0.5, WidthInPU - 1, HeightInPU - 1);
            //gfx.DrawRectangle(XBrushes.WhiteSmoke, rect);

            //Page = page;
            //XGraphics = gfx;
#endif
        }

        /// <summary>
        /// Creates a new page in the current PDF document.
        /// </summary>
        public XGraphics BeginPdfPage(XUnit width, XUnit height, XGraphicsUnit graphicsUnit, XPageDirection pageDirection = XPageDirection.Downwards)
        {
            var page = Document.AddPage();

            page.Width = width;  // = XUnit.FromPresentation(WidthInPU);
            page.Height = height;  // = XUnit.FromPresentation(HeightInPU);

            // All drawing is done in presentation units (1/96 inch).
            var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append, graphicsUnit, pageDirection);
            // Draw a bounding box. BUG if graphics unit is not Presentation.

            if (graphicsUnit != XGraphicsUnit.Point)
            {
                width.ConvertType(graphicsUnit);
                height.ConvertType(graphicsUnit);
            }

            var rect = new XRect(0.5, 0.5, width.Presentation - 1, height.Presentation - 1);
            gfx.DrawRectangle(XBrushes.WhiteSmoke, rect);

            Page = page;
            XGraphics = gfx;

            return gfx;
        }

        /// <summary>
        /// Ends the current PDF page.
        /// </summary>
        public void EndPdfPage()
        {
            if (_page is not null)
            {
                _page.Close();
                _page = null;
            }
        }

        /// <summary>
        /// Generates a PDF document for parallel comparison of two render results.
        /// </summary>
        public void GenerateParallelComparisonDocument()
        {
            var document = new PdfDocument();

            var page = document.AddPage();
            page.Width = XUnit.FromPoint(WidthInPoint * 2);
            page.Height = XUnit.FromPoint(HeightInPoint);

            var gfx = XGraphics.FromPdfPage(page);

            var width = page.Width.Point;
            var height = page.Height.Point;

            var leftBox = new XRect(0, 0, width / 2, height);
            DrawPdfToBox(gfx, leftBox);

            var rightBox = new XRect(width / 2, 0, width / 2, height);
            DrawImageToBox(gfx, rightBox);

            var stream = new MemoryStream();
            document.Save(stream);

            var bytes = stream.ToArray();
            ComparisonBytes = bytes;
        }

        /// <summary>
        /// Generates the serial comparison document. DOCTODO
        /// </summary>
        public void GenerateSerialComparisonDocument()
        {
            var document = new PdfDocument();

            var page = document.AddPage();
            page.Width = XUnit.FromPoint(WidthInPoint);
            page.Height = XUnit.FromPoint(HeightInPoint);

            var gfx = XGraphics.FromPdfPage(page);

            var pageBox = new XRect(0, 0, page.Width.Point, page.Height.Point);
            DrawPdfToBox(gfx, pageBox);

            page = document.AddPage();
            page.Width = XUnit.FromPoint(WidthInPoint);
            page.Height = XUnit.FromPoint(HeightInPoint);

            gfx = XGraphics.FromPdfPage(page);

            DrawImageToBox(gfx, pageBox);

            var stream = new MemoryStream();
            document.Save(stream);

            var bytes = stream.ToArray();
            ComparisonBytes = bytes;
        }

        void DrawPdfToBox(XGraphics gfx, XRect box)
        {
            if (PdfBytes == null)
            {
                DrawEmptyBox(gfx, box);
                return;
            }

            var stream = new MemoryStream(PdfBytes);
            var form = XPdfForm.FromStream(stream);
            gfx.DrawImage(form, box);
        }

        void DrawImageToBox(XGraphics gfx, XRect box)
        {
            if (PngBytes == null!)
            {
                DrawEmptyBox(gfx, box);
                return;
            }

            var stream = new MemoryStream(PngBytes);
            var image = XImage.FromStream(stream);
            image.Interpolate = false;
            gfx.DrawImage(image, box);
        }

        void DrawEmptyBox(XGraphics gfx, XRect box)
        {
            var pen = XPens.Red;
            gfx.DrawRectangle(pen, box);
            gfx.DrawLine(pen, box.TopLeft, box.BottomRight);
        }

        /// <summary>
        /// Saves the bytes to PDF file and show file.
        /// </summary>
        /// <param name="sourceBytes">The source bytes.</param>
        /// <param name="filepath">The filepath.</param>
        /// <param name="startViewer">if set to <c>true</c> [start viewer].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">sourceBytes</exception>
        public string SaveAndShowFile(byte[]? sourceBytes, string filepath = "", bool startViewer = false)
        {
            if (sourceBytes is null)
                throw new ArgumentNullException(nameof(sourceBytes));

            //filepath = CompleteFilePath(sourceBytes, filepath);
            filepath = IOUtility.GetTempFullFileName(filepath, GetFileExtension(sourceBytes), true);

            // Save the PDF document...
            using var stream = File.Create(filepath);
            {
                stream.Write(sourceBytes, 0, sourceBytes.Length);
                // Stream must be closed here, otherwise it cannot being copied under Linux.
                // To me, it is not clear what's the difference with windows.
                stream.Close();
            }

            if (startViewer)
                PdfFileUtility.ShowDocument(filepath);

            return filepath;
        }

        //string CompleteFilePath(byte[]? sourceBytes, string filepath)
        //{
        //    if (sourceBytes is null)
        //        throw new ArgumentNullException(nameof(sourceBytes));

        //    // Add generated filename, if filepath is only a directory (doesn't contain a file extension).
        //    return String.IsNullOrEmpty(Path.GetExtension(filepath))
        //        ? Path.Combine(filepath, GenerateFilename(sourceBytes))
        //        : filepath;
        //}

        //string GenerateFilename(byte[] sourceBytes)
        //{
        //    return $"{GetType().Name}-{PdfSharpTechnology}-tempfile.{GetFileExtension(sourceBytes)}";
        //}

        string GetFileExtension(byte[] sourceBytes)
        {
            if (sourceBytes == PdfBytes)
                return "pdf";
            if (sourceBytes == PngBytes)
                return "png";
            if (sourceBytes == ComparisonBytes)
                return "pdf";
            Debug.Assert(false);
            return "";
        }

        /// <summary>
        /// Saves and optionally shows a PDF document.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="startViewer">if set to <c>true</c> [start viewer].</param>
        [Obsolete("Use a XxxUtility class.")]
        public string SaveAndShowPdfDocument(string filepath, bool startViewer = false)
        {
            return SaveAndShowFile(PdfBytes, filepath, startViewer);
        }

        /// <summary>
        /// Saves and optionally shows a PNG image.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="startViewer">if set to <c>true</c> [start viewer].</param>
        [Obsolete("Use a XxxUtility class.")]
        public string SaveAndShowPngImage(string filepath, bool startViewer = false)
        {
            return SaveAndShowFile(PngBytes, filepath, startViewer);
        }

        /// <summary>
        /// Saves and shows a parallel comparison PDF document.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="startViewer">if set to <c>true</c> [start viewer].</param>
        [Obsolete("Use a XxxUtility class.")]
        public string SaveAndShowComparisonDocument(string filepath, bool startViewer = false)
        {
            return SaveAndShowFile(ComparisonBytes, filepath, startViewer);
        }

        /// <summary>
        /// Saves a stream to a file.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="path">The path.</param>
        [Obsolete("Use a XxxUtility class.")]
        public void SaveStreamToFile(Stream stream, string path)
        {
            int length = (int)stream.Length;
            var bytes = new byte[length];
            stream.Read(bytes, 0, length);

            using var fs = new FileStream(path, FileMode.Create);
            fs.Write(bytes, 0, length);
        }

        /// <summary>
        /// Prepares new bitmap image for drawing.
        /// </summary>
        public void BeginImage()
        {
#if CORE
#endif
#if GDI
            int factor = 5;
            int width = (int)(WidthInPoint * factor);
            int height = (int)(HeightInPoint * factor);

            var image = new Bitmap(width, height);

            var gfx = Graphics.FromImage(image);
            var xgfx = XGraphics.FromGraphics(gfx, new XSize(width, height), XGraphicsUnit.Presentation, new RenderEvents());
            //xgfx.DrawLine(XPens.Blue, 10, 10, 100, 100);
            xgfx.ScaleTransform(factor);
            XGraphics = xgfx;
            Image = image;

            //this.image = new RenderTargetBitmap(width, height, 72 * factor, 72 * factor, PixelFormats.Default);
            //this.visual = new DrawingVisual();
            //this.dc = this.visual.RenderOpen();

            //// Draw a bounding box
            ////Rect rect = new Rect(0, 0, width - 1, height - 1);
            //Rect rect = new Rect(0, 0, WidthInPoint * 4 / 3 - 1, HeightInPoint * 4 / 3 - 1);
            ////this.dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.LightGray, 1), rect);
            //this.dc.DrawRectangle(Brushes.WhiteSmoke, null, rect);

            ////double d = 10;
            ////Pen pen = new Pen(Brushes.Red, 5);
            ////this.dc.DrawLine(pen, rect.TopLeft, rect.TopLeft + new Vector(d, d));
            ////this.dc.DrawLine(pen, rect.TopRight, rect.TopRight + new Vector(-d, d));
            ////this.dc.DrawLine(pen, rect.BottomLeft, rect.BottomLeft + new Vector(d, -d));
            ////this.dc.DrawLine(pen, rect.BottomRight, rect.BottomRight + new Vector(-d, -d));

            ////this.dc.PushTransform(new ScaleTransform(factor, factor));
            //this.imgGfx = XGraphics.FromDrawingContext(this.dc, new XSize(WidthInPoint, HeightInPoint), XGraphicsUnit.Point);
#endif
            //int factor = 4;
            //int width = (int)(WidthInPoint * factor);
            //int height = (int)(HeightInPoint * factor);
            //this.image = new RenderTargetBitmap(width, height, 72 * factor, 72 * factor, PixelFormats.Default);
            //this.visual = new DrawingVisual();
            //this.dc = this.visual.RenderOpen();

            //// Draw a bounding box
            ////Rect rect = new Rect(0, 0, width - 1, height - 1);
            //Rect rect = new Rect(0, 0, WidthInPoint * 4 / 3 - 1, HeightInPoint * 4 / 3 - 1);
            ////this.dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.LightGray, 1), rect);
            //this.dc.DrawRectangle(Brushes.WhiteSmoke, null, rect);

            ////double d = 10;
            ////Pen pen = new Pen(Brushes.Red, 5);
            ////this.dc.DrawLine(pen, rect.TopLeft, rect.TopLeft + new Vector(d, d));
            ////this.dc.DrawLine(pen, rect.TopRight, rect.TopRight + new Vector(-d, d));
            ////this.dc.DrawLine(pen, rect.BottomLeft, rect.BottomLeft + new Vector(d, -d));
            ////this.dc.DrawLine(pen, rect.BottomRight, rect.BottomRight + new Vector(-d, -d));

            ////this.dc.PushTransform(new ScaleTransform(factor, factor));
            //this.imgGfx = XGraphics.FromDrawingContext(this.dc, new XSize(WidthInPoint, HeightInPoint), XGraphicsUnit.Point);
        }

        /// <summary>
        /// Ends the current GDI+ image.
        /// </summary>
        public void EndPngImage()
        {
#if CORE
#endif
#if GDI

            var stream = new MemoryStream();
            _image.Save(stream, ImageFormat.Png);

            var bytes = stream.ToArray();

            //_gfx.Dispose();
            //_image.Dispose();
            //_gfx = null;
            //_image = null;

            PngBytes = bytes;
#endif
        }

        /// <summary>
        /// Gets the current PDF document.
        /// </summary>
        public PdfDocument Document
        {
            get => _document ?? throw new InvalidOperationException("Document was not yet set.");
            private set
            {
                _document = value;
                //_page = null;
#if GDI
                _gfx = null!;
#endif
                _pdfBytes = Array.Empty<byte>();
            }
        }
        PdfDocument? _document;

        /// <summary>
        /// Gets the current PDF page.
        /// </summary>
        public PdfPage Page
        {
            get => _page ?? throw new InvalidOperationException("Page must not be null.");
            private set
            {
                _page = value;
#if GDI
                _gfx = null!;
#endif
            }
        }
        PdfPage? _page;

        /// <summary>
        /// Gets the current PDFsharp graphics object.
        /// </summary>
        public XGraphics XGraphics
        {
            get => _xgfx ?? throw new InvalidOperationException("XGraphics must not be null.");
            private set => _xgfx = value;
        }
        XGraphics? _xgfx;

#if CORE
#endif
#if GDI
        /// <summary>
        /// Gets the current GDI+ graphics object.
        /// </summary>
        public Graphics Graphics
        {
            get { return _gfx; }
            private set
            {
                _gfx = value;
                _image = null!;
            }
        }
        Graphics _gfx = default!;

        /// <summary>
        /// Gets the current GDI+ image object.
        /// </summary>
        public Image Image
        {
            get { return _image; }
            private set { _image = value; }
        }
        Image _image = default!;
#endif

        /// <summary>
        /// Gets the bytes of a PDF document.
        /// </summary>
        public byte[]? PdfBytes
        {
            get => _pdfBytes;
            private set
            {
                _pdfBytes = value ?? [];
                _document = null;
#if GDI
                _gfx = null!;
#endif
                _page = null;
            }
        }
        byte[] _pdfBytes = [];

        /// <summary>
        /// Gets the bytes of a PNG image.
        /// </summary>
        public byte[] PngBytes
        {
            get => _pngBytes;
            private set
            {
                _pngBytes = [];
#if GDI
                _gfx = null!;
                _image = null!;
#endif
            }
        }
        byte[] _pngBytes = [];

        /// <summary>
        /// Gets the bytes of a comparision document.
        /// </summary>
        public byte[] ComparisonBytes
        {
            get => _comparisonBytes;
            private set => _comparisonBytes = value ?? [];
        }
        byte[] _comparisonBytes = [];

#if old  // #KEEP for reference
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

#if even_older  // #KEEP for reference
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
