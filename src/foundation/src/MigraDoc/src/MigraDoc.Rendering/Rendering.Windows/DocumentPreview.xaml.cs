// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.IO;
using System.Windows.Markup;
using PdfSharp.Events;

namespace MigraDoc.Rendering.Windows
{
    /// <summary>
    /// Interaction logic for DocumentPreview.xaml
    /// </summary>
    public partial class DocumentPreview : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPreview"/> class.
        /// </summary>
        public DocumentPreview()
        {
            InitializeComponent();
            Width = Double.NaN;
            Height = Double.NaN;
            //this.preview.SetRenderEvent(new PdfSharp.Forms.PagePreview.RenderEvent(RenderPage));
        }

        /// <summary>
        /// Gets a DDL string or file.
        /// </summary>
        public string Ddl { get; private set; } = "";

        /// <summary>
        /// Sets a DDL string or file.
        /// Commit renderEvents to allow RenderTextEvent calls.
        /// </summary>
        public void SetDdl(string ddl, RenderEvents renderEvents)
        {
            Ddl = ddl;
            RenderEvents = renderEvents;

            if (Ddl != null)
            {
                Document = DdlReader.DocumentFromString(Ddl);
                Renderer = new DocumentRenderer(Document);

                //this.renderer.PrivateFonts = this.privateFonts;
                Renderer.PrepareDocument(RenderEvents);

                //IDocumentPaginatorSource source = this.documentViewer.Document;

                //IDocumentPaginatorSource source = this.documentViewer.Document;

                int pageCount = Renderer.FormattedDocument.PageCount;
                if (pageCount == 0)
                    return;

                // HA/CK: hardcoded A4 size
                //double pageWidth = XUnitPt.FromMillimeter(210).Presentation;
                //double pageHeight = XUnitPt.FromMillimeter(297).Presentation;
                //Size a4 = new Size(pageWidth, pageHeight);

                XUnitPt pageWidth, pageHeight;
                Size size96 = GetSizeOfPage(1, out pageWidth, out pageHeight);

                FixedDocument fixedDocument = new FixedDocument();
                fixedDocument.DocumentPaginator.PageSize = size96;

                for (int pageNumber = 1; pageNumber <= pageCount; pageNumber++)
                {
                    try
                    {
                        size96 = GetSizeOfPage(pageNumber, out pageWidth, out pageHeight);

                        DrawingVisual dv = new DrawingVisual();
                        DrawingContext dc = dv.RenderOpen();
                        //XGraphics gfx = XGraphics.FromDrawingContext(dc, new XSize(XUnitPt.FromMillimeter(210).Point, XUnitPt.FromMillimeter(297).Point), XGraphicsUnit.Point);
                        XGraphics gfx = XGraphics.FromDrawingContext(dc, new XSize(pageWidth.Point, pageHeight.Presentation), XGraphicsUnit.Point, RenderEvents);
                        Renderer.RenderPage(gfx, pageNumber, PageRenderOptions.All);
                        dc.Close();

                        // Create page content
                        PageContent pageContent = new PageContent();
                        pageContent.Width = size96.Width;
                        pageContent.Height = size96.Height;
                        FixedPage fixedPage = new FixedPage();
                        fixedPage.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFE, 0xFE, 0xFE));

                        UIElement visual = new DrawingVisualPresenter(dv);
                        visual.IsHitTestVisible = false; // Without this line scrolling by mouse wheel is not possible if the cursor is over page content.
                        FixedPage.SetLeft(visual, 0);
                        FixedPage.SetTop(visual, 0);

                        fixedPage.Width = size96.Width;
                        fixedPage.Height = size96.Height;

                        fixedPage.Children.Add(visual);

                        fixedPage.Measure(size96);
                        fixedPage.Arrange(new Rect(new Point(), size96));
                        fixedPage.UpdateLayout();

                        ((IAddChild)pageContent).AddChild(fixedPage);

                        fixedDocument.Pages.Add(pageContent);
                    }
                    catch (Exception)
                    {
                        // eat exception
                    }

                    viewer.Document = fixedDocument;
                }
            }
            else
                viewer.Document = null;
        }

        ///////// <summary>
        ///////// Sets a delegate that is invoked when the preview needs to be painted.
        ///////// </summary>
        //////public void SetRenderEvent(RenderEvent renderEvent)
        //////{
        //////  this.renderEvent = renderEvent;
        //////  Invalidate();
        //////}
        //////RenderEvent renderEvent;

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        public int Page
        {
            get => _page;
            set
            {
                try
                {
                    if (viewer != null)
                    {
                        if (_page != value)
                        {
                            _page = value;
                            //PageInfo pageInfo = this.renderer.formattedDocument.GetPageInfo(this.page);
                            //if (pageInfo.Orientation == PdfSharp.PageOrientation.Portrait)
                            //  this.viewer.PageSize = new Size((int)pageInfo.Width, (int)pageInfo.Height);
                            //else
                            //  this.viewer.PageSize = new Size((int)pageInfo.Height, (int)pageInfo.Width);

                            //this.viewer.Invalidate();
                            //OnPageChanged(new EventArgs());
                        }
                    }
                    else
                        _page = -1;
                }
                catch { }
            }
        }
        int _page;

        /// <summary>
        /// Gets the number of pages of the underlying formatted document.
        /// </summary>
        public int PageCount
        {
            get
            {
                if (Renderer != null)
                    return Renderer.FormattedDocument.PageCount;
                return 0;
            }
        }

        /// <summary>
        /// Goes to the first page.
        /// </summary>
        public void FirstPage()
        {
            if (Renderer != null)
            {
                Page = 1;
                viewer.GoToPage(1);
            }
        }

        /// <summary>
        /// Goes to the next page.
        /// </summary>
        public void NextPage()
        {
            if (Renderer != null && _page < PageCount)
            {
                Page++;
                //this.preview.Invalidate();
                //OnPageChanged(new EventArgs());
            }
        }

        /// <summary>
        /// Goes to the previous page.
        /// </summary>
        public void PrevPage()
        {
            if (Renderer != null && _page > 1)
            {
                Page--;
            }
        }

        /// <summary>
        /// Goes to the last page.
        /// </summary>
        public void LastPage()
        {
            if (Renderer != null)
            {
                Page = PageCount;
                //this.preview.Invalidate();
                //OnPageChanged(new EventArgs());
            }
        }

        Size GetSizeOfPage(int page, out XUnitPt width, out XUnitPt height)
        {
            if (Renderer == null)
            {
                width = height = 0;
                return Size.Empty;
            }

            var pageInfo = Renderer.FormattedDocument.GetPageInfo(page);
            if (pageInfo.Orientation == PdfSharp.PageOrientation.Portrait)
            {
                width = pageInfo.Width;
                height = pageInfo.Height;
            }
            else
            {
                width = pageInfo.Height;
                height = pageInfo.Width;
            }
            return new Size(width.Presentation, height.Presentation);
        }

        /// <summary>
        /// Gets the MigraDoc document that is previewed in this control.
        /// </summary>
        public Document? Document { get; private set; }

        /// <summary>
        /// Sets the MigraDoc document that is previewed in this control.
        /// Commit renderEvents to allow RenderTextEvent calls.
        /// </summary>
        public void SetDocument(Document document, RenderEvents renderEvents)
        {
            Document = document;
            RenderEvents = renderEvents;

            Renderer = new(Document);
            Renderer.PrepareDocument(RenderEvents);
            Page = 1;
            //this.preview.Invalidate();
        }

        /// <summary>
        /// Clears the MigraDoc document that is previewed in this control.
        /// </summary>
        public void ClearDocument()
        {
            Document = null;
            RenderEvents = null;

            Renderer = null;
            //this.preview.Invalidate();
        }

        /// <summary>
        /// Encapsulates the document’s render events.
        /// </summary>
        public RenderEvents? RenderEvents { get; private set; }

        /// <summary>
        /// Gets the underlying DocumentRenderer of the document currently in preview, or null if no renderer exists.
        /// You can use this renderer for printing or creating PDF file. This evades the necessity to format the
        /// document a second time when you want to print it and convert it into PDF.
        /// </summary>
        public DocumentRenderer? Renderer { get; private set; }

        /// <summary>
        /// Helper class to render a single visual.
        /// </summary>
        public class DrawingVisualPresenter : FrameworkElement
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DrawingVisualPresenter"/> class.
            /// </summary>
            public DrawingVisualPresenter(DrawingVisual visual)
            {
                _visual = visual;
            }

            /// <summary>
            /// Gets the number of visual child elements within this element, which is 1 in this class.
            /// </summary>
            protected override int VisualChildrenCount => 1;

            /// <summary>
            /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)"/>, and returns a child at the specified index from a collection of child elements.
            /// </summary>
            protected override Visual GetVisualChild(int index)
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _visual;
            }

            readonly DrawingVisual _visual;
        }
    }
}
