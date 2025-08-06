// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering.Forms
{
    /// <summary>
    /// Event handler for the PagePreview event.
    /// </summary>
    public delegate void PagePreviewEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Represents a Windows control to display a MigraDoc document.
    /// </summary>
    public class DocumentPreview : UserControl
    {
        private PdfSharp.Forms.PagePreview _preview;
        private readonly Container _components = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPreview"/> class.
        /// </summary>
        public DocumentPreview()
        {
            InitializeComponent();
            _preview!.ZoomChanged += PreviewZoomChanged;
            _preview.SetRenderFunction(RenderPage);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_components != null!)
                    _components.Dispose();
            }
            base.Dispose(disposing);
        }

        Zoom GetNewZoomFactor(int currentZoom, bool larger)
        {
            int[] values = new int[]
      {
        10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 120, 140, 160, 180, 200, 
        250, 300, 350, 400, 450, 500, 600, 700, 800
      };

            if (currentZoom <= 10 && !larger)
                return Zoom.Percent10;
            if (currentZoom >= 800 && larger)
                return Zoom.Percent800;

            if (larger)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (currentZoom < values[i])
                        return (Zoom)values[i];
                }
            }
            else
            {
                for (int i = values.Length - 1; i >= 0; i--)
                {
                    if (currentZoom > values[i])
                        return (Zoom)values[i];
                }
            }
            return Zoom.Percent100;
        }

        /// <summary>
        /// Gets or sets the border style of the tree view control.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// One of the <see cref="T:System.Windows.Forms.BorderStyle"/> values. The default is <see cref="F:System.Windows.Forms.BorderStyle.Fixed3D"/>.
        /// </returns>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
        /// The assigned value is not one of the <see cref="T:System.Windows.Forms.BorderStyle"/> values.
        /// </exception>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        /// </PermissionSet>
        [DefaultValue((int)BorderStyle.Fixed3D), Description("Determines the style of the border."), Category("Preview Properties")]
        public new BorderStyle BorderStyle
        {
            get { return _preview.BorderStyle; }
            set { _preview.BorderStyle = value; }
        }

        // #PFC
        //w/// <summary>
        ///// Gets or sets the private fonts of the document. If used, must be set before Ddl is set!
        ///// </summary>
        //public XPrivateFontCollection PrivateFonts
        //{
        //    get { return _privateFonts; }
        //    set { _privateFonts = value; }
        //}
        //internal XPrivateFontCollection _privateFonts;

        /// <summary>
        /// Gets or sets a DDL string or file.
        /// </summary>
        public string Ddl
        {
            get { return _ddl; }
            set
            {
                _ddl = value;
                DdlUpdated();
            }
        }
        string _ddl = null!;

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        public int Page
        {
            get { return _page; }
            set
            {
                try
                {
                    if (_preview != null!)
                    {
                        if (_page != value)
                        {
                            _page = value;
                            PageInfo pageInfo = _renderer.FormattedDocument.GetPageInfo(_page);
                            if (pageInfo.Orientation == PdfSharp.PageOrientation.Portrait)
                                _preview.PageSize = new Size((int)pageInfo.Width, (int)pageInfo.Height);
                            else
                                _preview.PageSize = new Size((int)pageInfo.Height, (int)pageInfo.Width);

                            _preview.Invalidate();
                            OnPageChanged(EventArgs.Empty);
                        }
                    }
                    else
                        _page = -1;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch { }
            }
        }
        int _page;

        /// <summary>
        /// Gets the number of pages.
        /// </summary>
        public int PageCount
        {
            get
            {
                if (_renderer != null!)
                    return _renderer.FormattedDocument.PageCount;
                return 0;
            }
        }

        /// <summary>
        /// Goes to the first page.
        /// </summary>
        public void FirstPage()
        {
            if (_renderer != null!)
            {
                Page = 1;
                _preview.Invalidate();
                OnPageChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Goes to the next page.
        /// </summary>
        public void NextPage()
        {
            if (_renderer != null! && _page < PageCount)
            {
                Page++;
                _preview.Invalidate();
                OnPageChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Goes to the previous page.
        /// </summary>
        public void PrevPage()
        {
            if (_renderer != null! && _page > 1)
            {
                Page--;
            }
        }

        /// <summary>
        /// Goes to the last page.
        /// </summary>
        public void LastPage()
        {
            if (_renderer != null!)
            {
                Page = PageCount;
                _preview.Invalidate();
                OnPageChanged(EventArgs.Empty);
            }
        }

        ///// <summary>
        ///// Gets or sets the working directory.
        ///// </summary>
        //public string WorkingDirectory
        //{
        //  get
        //  {
        //    return this.workingDirectory;
        //  }
        //  set
        //  {
        //    this.workingDirectory = value;
        //  }
        //}
        //string workingDirectory = "";

        /// <summary>
        /// Called when the Ddl property has changed.
        /// </summary>
        void DdlUpdated()
        {
            if (_ddl != null!)
            {
                _document = DocumentObjectModel.IO.DdlReader.DocumentFromString(_ddl);
                _renderer = new DocumentRenderer(_document);
                //_renderer.PrivateFonts = _privateFonts;
                _renderer.PrepareDocument();
                Page = 1;
                _preview.Invalidate();
            }
            //      if (this.job != null)
            //        this.job.Dispose();
            //
            //      if (this.ddl == null || this.ddl == "")
            //        return;
            //
            //      this.job = new PrintJob();
            //      this.job.Type = JobType.Standard;
            //      this.job.Ddl = this.ddl;
            //      this.job.WorkingDirectory = this.workingDirectory;
            //      this.job.InitDocument();
            //      this.preview = this.job.GetPreview(this.Handle);
            //      this.previewHandle = this.preview.Hwnd;
            //
            //      if (this.preview != null)
            //        this.preview.Page = 1;
        }

        /// <summary>
        /// Gets or sets the MigraDoc document that is previewed in this control.
        /// </summary>
        public MigraDoc.DocumentObjectModel.Document Document
        {
            get { return _document; }
            set
            {
                if (value != null!)
                {
                    _document = value;
                    _renderer = new DocumentRenderer(value);
                    _renderer.PrepareDocument();
                    Page = 1;
                    _preview.Invalidate();
                }
                else
                {
                    _document = null!;
                    _renderer = null!;
                    _preview.Invalidate();
                }
            }
        }
        DocumentObjectModel.Document _document = null!;

        /// <summary>
        /// Gets the underlying DocumentRenderer of the document currently in preview, or null, if no renderer exists.
        /// You can use this renderer for printing or creating PDF file. This evades the necessity to format the
        /// document a second time when you want to print it or convert it into PDF.
        /// </summary>
        public DocumentRenderer Renderer
        {
            get { return _renderer; }
        }

        void RenderPage(XGraphics gfx)
        {
            if (_renderer == null!)
                return;

            if (_renderer != null!)
            {
                try
                {
                    _renderer.RenderPage(gfx, _page);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch { };
            }
        }
        DocumentRenderer _renderer = null!;

        /// <summary>
        /// Gets or sets a predefined zoom factor.
        /// </summary>
        [DefaultValue((int)Zoom.FullPage), Description("Determines the zoom of the page."), Category("Preview Properties")]
        public Zoom Zoom
        {
            get { return (Zoom)_preview.Zoom; }
            set
            {
                if (_preview.Zoom != (PdfSharp.Forms.Zoom)value)
                {
                    _preview.Zoom = (PdfSharp.Forms.Zoom)value;
                    OnZoomChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets an arbitrary zoom factor. The range is from 10 to 800.
        /// </summary>
        [DefaultValue((int)Zoom.FullPage), Description("Determines the zoom of the page."), Category("Preview Properties")]
        public int ZoomPercent
        {
            get { return _preview.ZoomPercent; }
            set
            {
                if (_preview.ZoomPercent != value)
                {
                    _preview.ZoomPercent = value;
                    OnZoomChanged(EventArgs.Empty);
                }
            }
        }
        //internal int _zoomPercent = 100;

        /// <summary>
        /// Makes zoom factor smaller.
        /// </summary>
        public void MakeSmaller()
        {
            ZoomPercent = (int)GetNewZoomFactor(ZoomPercent, false);
        }

        /// <summary>
        /// Makes zoom factor larger.
        /// </summary>
        public void MakeLarger()
        {
            ZoomPercent = (int)GetNewZoomFactor(ZoomPercent, true);
        }

        /// <summary>
        /// Gets or sets the color of the page.
        /// </summary>
        /// <value>The color of the page.</value>
        [Description("The background color of the page."), Category("Preview Properties")]
        public Color PageColor
        {
            get { return _preview.PageColor; }
            set { _preview.PageColor = value; }
        }
        Color _pageColor = Color.GhostWhite;

        /// <summary>
        /// Gets or sets the color of the desktop.
        /// </summary>
        /// <value>The color of the desktop.</value>
        [Description("The color of the desktop."), Category("Preview Properties")]
        public Color DesktopColor
        {
            get { return _preview.DesktopColor; }
            set { _preview.DesktopColor = value; }
        }
        internal Color desktopColor = SystemColors.ControlDark;

        /// <summary>
        /// Gets or sets a value indicating whether to show scrollbars.
        /// </summary>
        /// <value><c>true</c> if [show scrollbars]; otherwise, <c>false</c>.</value>
        [DefaultValue(true), Description("Determines whether the scrollbars are visible."), Category("Preview Properties")]
        public bool ShowScrollbars
        {
            get { return _preview.ShowScrollbars; }
            set { _preview.ShowScrollbars = value; }
        }
        internal bool showScrollbars = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the page.
        /// </summary>
        /// <value><c>true</c> if [show page]; otherwise, <c>false</c>.</value>
        [DefaultValue(true), Description("Determines whether the page visible."), Category("Preview Properties")]
        public bool ShowPage
        {
            get { return _preview.ShowPage; }
            set { _preview.ShowPage = value; }
        }
        internal bool showPage = true;

        /// <summary>
        /// Gets or sets the page size in point.
        /// </summary>
        [Description("Determines the size (in points) of the page."), Category("Preview Properties")]
        public Size PageSize
        {
            get { return new Size((int)_preview.PageSize.Width, (int)_preview.PageSize.Height); }
            set { _preview.PageSize = value; }
        }

        /// <summary>
        /// Raises the ZoomChanged event when the zoom factor changed.
        /// </summary>
        protected virtual void OnZoomChanged(EventArgs e)
        {
            if (ZoomChanged != null!)
                ZoomChanged(this, e);
        }

        /// <summary>
        /// Occurs when the zoom factor changed.
        /// </summary>
        [Description("Occurs when the zoom factor changed."), Category("Preview Properties")]
        public event PagePreviewEventHandler ZoomChanged = null!;

        /// <summary>
        /// Raises the ZoomChanged event when the current page changed.
        /// </summary>
        protected virtual void OnPageChanged(EventArgs e)
        {
            if (PageChanged != null!)
                PageChanged(this, e);
        }

        /// <summary>
        /// Occurs when the current page changed.
        /// </summary>
        [Description("Occurs when the current page changed."), Category("Preview Properties")]
        public event PagePreviewEventHandler PageChanged = null!;


        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _preview = new PdfSharp.Forms.PagePreview();
            SuspendLayout();
            // 
            // preview
            // 
            _preview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            _preview.DesktopColor = System.Drawing.SystemColors.ControlDark;
            _preview.Dock = System.Windows.Forms.DockStyle.Fill;
            _preview.Location = new System.Drawing.Point(0, 0);
            _preview.Name = "_preview";
            _preview.PageColor = System.Drawing.Color.GhostWhite;
            _preview.PageSize = new System.Drawing.Size(595, 842);
            _preview.Size = new System.Drawing.Size(200, 200);
            _preview.TabIndex = 0;
            _preview.Zoom = PdfSharp.Forms.Zoom.FullPage;
            _preview.ZoomPercent = 15;
            // 
            // PagePreview
            // 
            this.Controls.Add(_preview);
            Name = "PagePreview";
            Size = new System.Drawing.Size(200, 200);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int delta = e.Delta;
            if (delta > 0)
                PrevPage();
            else if (delta < 0)
                NextPage();
        }

        private void PreviewZoomChanged(object? sender, EventArgs e)
        {
            OnZoomChanged(e);
        }
    }
}
