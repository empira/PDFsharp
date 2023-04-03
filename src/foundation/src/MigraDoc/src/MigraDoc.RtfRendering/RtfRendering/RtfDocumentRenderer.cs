// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Internals;
using MigraDoc.DocumentObjectModel.Visitors;
using PdfSharp.Diagnostics;
using Color = MigraDoc.DocumentObjectModel.Color;
using Font = MigraDoc.DocumentObjectModel.Font;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a MigraDoc document to RTF format.
    /// </summary>
    public class RtfDocumentRenderer : RendererBase
    {
        /// <summary>
        /// Initializes a new instance of the DocumentRenderer class.
        /// </summary>
        public RtfDocumentRenderer()
        { }

        /// <summary>
        /// This function is declared only for technical reasons!
        /// </summary>
        internal override void Render()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Renders a MigraDoc document to the specified file.
        /// </summary>
        public void Render(Document doc, string file, string workingDirectory)
        {
            StreamWriter? strmWrtr = null;
            try
            {
                _document = doc;
                _docObject = doc;
                _workingDirectory = workingDirectory;
                string path = file;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (workingDirectory != null)
                    path = Path.Combine(workingDirectory, file);

                strmWrtr = new StreamWriter(path, false, Encoding.GetEncoding(1252));
                _rtfWriter = new RtfWriter(strmWrtr);
                WriteDocument();
            }
            finally
            {
                if (strmWrtr != null)
                {
                    strmWrtr.Flush();
                    strmWrtr.Close();
                }
            }
        }

        /// <summary>
        /// Renders a MigraDoc document to the specified stream.
        /// </summary>
        public void Render(Document document, Stream stream, string workingDirectory)
        {
            Render(document, stream, true, workingDirectory);
        }

        /// <summary>
        /// Renders a MigraDoc document to the specified stream.
        /// </summary>
        public void Render(Document document, Stream stream, bool closeStream, string workingDirectory)
        {
            if (document == null)
                throw new ArgumentNullException("document");
            if (document.UseCmykColor)
                throw new InvalidOperationException("Cannot create RTF document with CMYK colors.");

            StreamWriter? strmWrtr = null;
            try
            {
                strmWrtr = new(stream, Encoding.GetEncoding(1252));
                _document = document;
                _docObject = document;
                _workingDirectory = workingDirectory;
                _rtfWriter = new(strmWrtr);
                WriteDocument();
            }
            finally
            {
                if (strmWrtr != null)
                {
                    strmWrtr.Flush();
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (stream != null)
                    {
                        if (closeStream)
                            strmWrtr.Close();
                        else
                            stream.Position = 0; // Reset the stream position if the stream is kept open.
                    }
                }
            }
        }

        /// <summary>
        /// Renders a MigraDoc to Rtf and returns the result as string.
        /// </summary>
        public string RenderToString(Document document, string workingDirectory)
        {
            if (document == null)
                throw new ArgumentNullException("document");
            if (document.UseCmykColor)
                throw new InvalidOperationException("Cannot create RTF document with CMYK colors.");

            _document = document;
            _docObject = document;
            _workingDirectory = workingDirectory;
            StringWriter? writer = null;
            try
            {
                writer = new();
                _rtfWriter = new(writer);
                WriteDocument();
                writer.Flush();
                return writer.GetStringBuilder().ToString();
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        /// <summary>
        /// Renders a MigraDoc document with help of the internal RtfWriter.
        /// </summary>
        void WriteDocument()
        {
            if (Document.EmbeddedFiles.Count > 0)
                throw new InvalidOperationException("Embedded files are not supported in RTF documents.");

            RtfFlattenVisitor flattener = new RtfFlattenVisitor();
            flattener.Visit(_document);
            Prepare();
            _rtfWriter.StartContent();
            RenderHeader();
            RenderDocumentArea();
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Prepares this renderer by collecting Information for font and color table.
        /// </summary>
        void Prepare()
        {
            _fontList.Clear();
            //Fonts 
            _fontList.Add("Symbol");
            _fontList.Add("Wingdings");
            _fontList.Add("Courier New");

            _colorList.Clear();
            _colorList.Add(Colors.Black);//!!necessary for borders!!
            _listList.Clear();
            ListInfoRenderer.Clear();
            ListInfoOverrideRenderer.Clear();
            CollectTables(_document);
        }

        /// <summary>
        /// Renders the RTF Header.
        /// </summary>
        void RenderHeader()
        {
            _rtfWriter.WriteControl("rtf", 1);
            _rtfWriter.WriteControl("ansi");
            _rtfWriter.WriteControl("ansicpg", 1252);
            _rtfWriter.WriteControl("deff", 0);//default font

            //Document properties can occur before and between the header tables.

            RenderFontTable();
            RenderColorTable();
            RenderStyles();
            //Lists are not yet implemented.
            RenderListTable();
        }

        /// <summary>
        /// Fills the font, color and (later!) list hashtables so they can be rendered and used by other renderers.
        /// </summary>
        void CollectTables(DocumentObject dom)
        {
            ValueDescriptorCollection vds = Meta.GetMeta(dom).ValueDescriptors;
            int count = vds.Count;
            for (int idx = 0; idx < count; idx++)
            {
                ValueDescriptor vd = vds[idx];
                if (!vd.IsRefOnly && !vd.IsNull(dom))
                {
                    if (vd.ValueType == typeof(Color))
                    {
                        Color clr = (Color)vd.GetValue(dom, GV.ReadWrite)!;
                        clr = clr.GetMixedTransparencyColor();
                        if (!_colorList.Contains(clr))
                            _colorList.Add(clr);
                    }
                    else if (vd.ValueType == typeof(Font))
                    {
                        Font fnt = (vd.GetValue(dom, GV.ReadWrite) as Font)!; //ReadOnly
                        if (!(fnt?.Values.Name).IsValueNullOrEmpty() && !_fontList.Contains(fnt!.Name))
                            _fontList.Add(fnt.Name);
                    }
                    else if (vd.ValueType == typeof(ListInfo))
                    {
                        ListInfo lst = (vd.GetValue(dom, GV.ReadWrite) as ListInfo)!; //ReadOnly
                        if (!_listList.Contains(lst))
                            _listList.Add(lst);
                    }
                    if (typeof(DocumentObject).IsAssignableFrom(vd.ValueType))
                    {
                        CollectTables((vd.GetValue(dom, GV.ReadWrite) as DocumentObject)!); //ReadOnly
                        if (typeof(DocumentObjectCollection).IsAssignableFrom(vd.ValueType))
                        {
                            DocumentObjectCollection coll = (vd.GetValue(dom, GV.ReadWrite) as DocumentObjectCollection)!; //ReadOnly
                            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                            if (coll != null)
                            {
                                foreach (var obj in coll)
                                {
                                    // SeriesCollection may contain null values.
                                    if (obj != null)
                                        CollectTables(obj);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renders the font hashtable within the RTF header.
        /// </summary>
        void RenderFontTable()
        {
            if (_fontList.Count == 0)
                return;

            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("fonttbl");
            for (int idx = 0; idx < _fontList.Count; ++idx)
            {
                _rtfWriter.StartContent();
                string name = (string)_fontList[idx];
                _rtfWriter.WriteControl("f", idx);
#if true
                //System.Drawing.Font font = new System.Drawing.Font(name, 12); //any size
                // See https://learn.microsoft.com/de-de/dotnet/api/system.drawing.font.gdicharset?view=dotnet-plat-ext-7.0
                // Use DEFAULT (1) anyway.
                _rtfWriter.WriteControl("fcharset", 1);
#else
                System.Drawing.Font font = new System.Drawing.Font(name, 12); //any size
                _rtfWriter.WriteControl("fcharset", (int)font.GdiCharSet);
#endif
                _rtfWriter.WriteText(name);
                _rtfWriter.WriteSeparator();
                _rtfWriter.EndContent();
            }
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Renders the color hashtable within the RTF header.
        /// </summary>
        void RenderColorTable()
        {
            if (_colorList.Count == 0)
                return;

            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("colortbl");
            //this would indicate index 0 as auto color:
            //this.rtfWriter.WriteSeparator();
            //left away cause there is no auto color in MigraDoc.
            foreach (var obj in _colorList)
            {
                Color color = (Color)obj;
                _rtfWriter.WriteControl("red", (int)color.R);
                _rtfWriter.WriteControl("green", (int)color.G);
                _rtfWriter.WriteControl("blue", (int)color.B);
                _rtfWriter.WriteSeparator();
            }
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Gets the font table index for the specified font name.
        /// </summary>
        internal int GetFontIndex(string fontName)
        {
            if (_fontList.Contains(fontName))
                return (int)_fontList.IndexOf(fontName);

            //development purpose exception
            throw new ArgumentException(@"Font does not exist in this document's font table.", "fontName");
        }

        /// <summary>
        /// Gets the color table index for the specified color.
        /// </summary>
        internal int GetColorIndex(Color color)
        {
            Color clr = color.GetMixedTransparencyColor();
            int idx = (int)_colorList.IndexOf(clr);
            // Development purpose exception.
            if (idx < 0)
                throw new ArgumentException(@"Color does not exist in this document's color table.", "color");
            return idx;
        }

        /// <summary>
        /// Gets the style index for the specified color.
        /// </summary>
        internal int GetStyleIndex(string styleName)
        {
            return _document.Styles.GetIndex(styleName);
        }

        /// <summary>
        /// Renders styles as part of the RTF header.
        /// </summary>
        void RenderStyles()
        {
            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("stylesheet");
            foreach (var style in _document.Styles)
            {
                Debug.Assert(style != null, nameof(style) + " != null");
                RendererFactory.CreateRenderer(style, this).Render();
            }
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Renders the list hashtable within the RTF header.
        /// </summary>
        void RenderListTable()
        {
            if (_listList.Count == 0)
                return;

            _rtfWriter.StartContent();
            _rtfWriter.WriteControlWithStar("listtable");
            foreach (var obj in _listList)
            {
                ListInfo lst = (ListInfo)obj;
                ListInfoRenderer lir = new ListInfoRenderer(lst, this);
                lir.Render();
            }
            _rtfWriter.EndContent();

            _rtfWriter.StartContent();
            _rtfWriter.WriteControlWithStar("listoverridetable");
            foreach (var obj in _listList)
            {
                ListInfo lst = (ListInfo)obj;
                ListInfoOverrideRenderer lir =
                    new ListInfoOverrideRenderer(lst, this);
                lir.Render();
            }
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Renders the RTF document area, which is all except the header.
        /// </summary>
        void RenderDocumentArea()
        {
            RenderInfo();
            RenderDocumentFormat();
            RenderGlobalPorperties();
            foreach (var sect in _document.Sections)
            {
                Debug.Assert(sect != null, nameof(sect) + " != null");
                RendererFactory.CreateRenderer(sect, this).Render();
            }
        }

        /// <summary>
        /// Renders global document properties, such as mirror margins and Unicode treatment.
        /// Note that a section specific margin mirroring does not work in Word.
        /// </summary>
        void RenderGlobalPorperties()
        {
            _rtfWriter.WriteControl("viewkind", 4);
            _rtfWriter.WriteControl("uc", 1);

            //Em4-Space doesn't work without this:
            _rtfWriter.WriteControl("lnbrkrule");

            //Footnotes only, no endnotes:
            _rtfWriter.WriteControl("fet", 0);

            //Enables title pages as (FirstpageHeader):
            _rtfWriter.WriteControl("facingp");

            // Space between paragraphs as maximum between space after and space before:
            _rtfWriter.WriteControl("htmautsp");

            // Word cannot realize the mirror margins property for single sections,
            // although rtf control words exist for this purpose.
            // Thus, the mirror margins property is set globally if it's true for the first section.
            var sec = _document.Sections.First as Section;
            if (sec != null)
            {
                if (sec.PageSetup.Values.MirrorMargins is not null && sec.PageSetup.MirrorMargins)
                    _rtfWriter.WriteControl("margmirror");
            }
        }

        /// <summary>
        /// Renders the document format such as standard tab stops and footnote settings.
        /// </summary>
        void RenderDocumentFormat()
        {
            Translate("DefaultTabStop", "deftab");
            Translate("FootnoteNumberingRule", "ftn");
            Translate("FootnoteLocation", "ftn", RtfUnit.Undefined, "bj", false);
            Translate("FootnoteNumberStyle", "ftnn");
            Translate("FootnoteStartingNumber", "ftnstart");
        }

        /// <summary>
        /// Renders footnote properties for a section. (Not part of the rtf specification, but necessary for Word)
        /// </summary>
        internal void RenderSectionProperties()
        {
            Translate("FootnoteNumberingRule", "sftn");
            Translate("FootnoteLocation", "sftn", RtfUnit.Undefined, "bj", false);
            Translate("FootnoteNumberStyle", "sftnn");
            Translate("FootnoteStartingNumber", "sftnstart");
        }

        /// <summary>
        /// Renders the document information of title, author, etc.
        /// </summary>
        void RenderInfo()
        {
            if (_document.Values.Info.IsValueNullOrEmpty())
                return;

            _rtfWriter.StartContent();
            _rtfWriter.WriteControl("f", 2); // Second font is Courier New. See Prepare().
            _rtfWriter.WriteControl("info");
            DocumentInfo info = _document.Info;
            if (!info.Values.Title.IsValueNullOrEmpty())
            {
                _rtfWriter.StartContent();
                _rtfWriter.WriteControl("title");
                _rtfWriter.WriteText(info.Title);
                _rtfWriter.EndContent();
            }
            if (!info.Values.Subject.IsValueNullOrEmpty())
            {
                _rtfWriter.StartContent();
                _rtfWriter.WriteControl("subject");
                _rtfWriter.WriteText(info.Subject);
                _rtfWriter.EndContent();
            }
            if (!info.Values.Author.IsValueNullOrEmpty())
            {
                _rtfWriter.StartContent();
                _rtfWriter.WriteControl("author");
                _rtfWriter.WriteText(info.Author);
                _rtfWriter.EndContent();
            }
            if (!info.Values.Keywords.IsValueNullOrEmpty())
            {
                _rtfWriter.StartContent();
                _rtfWriter.WriteControl("keywords");
                _rtfWriter.WriteText(info.Keywords);
                _rtfWriter.EndContent();
            }
            _rtfWriter.EndContent();
        }

        /// <summary>
        /// Gets the MigraDoc document that is currently rendered.
        /// </summary>
        internal Document Document
        {
            get { return _document; }
        }

        Document _document = null!;

        /// <summary>
        /// Gets the RtfWriter the document is rendered with.
        /// </summary>
        internal RtfWriter RtfWriter
        {
            get { return _rtfWriter; }
        }

        internal string WorkingDirectory
        {
            get { return _workingDirectory; }
        }
        string _workingDirectory = null!;

        readonly List<Color> _colorList = new List<Color>();
        readonly List<string> _fontList = new List<string>();
        readonly List<ListInfo> _listList = new List<ListInfo>();
    }
}
