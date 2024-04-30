// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.Reflection;
#if true || GDI || WPF
using PdfSharp.Drawing;
#endif
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.RtfRendering.Resources;
using PdfSharp.Diagnostics;
using Image = MigraDoc.DocumentObjectModel.Shapes.Image;
//using System.Security.Policy;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Render an image to RTF.
    /// </summary>
    sealed class ImageRenderer : ShapeRenderer
    {
        internal ImageRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _image = (Image)domObj;

            _filePath = _image.GetFilePath(_docRenderer.WorkingDirectory);
            _isInline = DocumentRelations.HasParentOfType(_image, typeof(Paragraph)) ||
                       RenderInParagraph();

            CalculateImageDimensions();
        }

        /// <summary>
        /// Renders an image to RTF.
        /// </summary>
        internal override void Render()
        {
            bool renderInParagraph = RenderInParagraph();
            var elms = DocumentRelations.GetParent(_image) as DocumentElements;
            if (elms != null && !renderInParagraph &&
                !(DocumentRelations.GetParent(elms) is Section || DocumentRelations.GetParent(elms) is HeaderFooter))
            {
                Debug.WriteLine(Messages2.ImageFreelyPlacedInWrongContext(_image.Name), "warning");
                return;
            }
            if (renderInParagraph)
                StartDummyParagraph();

            if (!_isInline)
                StartShapeArea();

            RenderImage();
            if (!_isInline)
                EndShapeArea();

            if (renderInParagraph)
                EndDummyParagraph();
        }

        /// <summary>
        /// Renders image specific attributes and the image byte series to RTF.
        /// </summary>
        void RenderImage()
        {
            StartImageDescription();
            RenderImageAttributes();
            RenderByteSeries();
            EndImageDescription();
        }

        void StartImageDescription()
        {
            if (_isInline)
            {
                _rtfWriter.StartContent();
                _rtfWriter.WriteControlWithStar("shppict");
                _rtfWriter.StartContent();
                _rtfWriter.WriteControl("pict");
            }
            else
            {
                RenderNameValuePair("shapeType", "75"); // 75 is ImageFrame.
                StartNameValuePair("pib");
                _rtfWriter.StartContent();
                _rtfWriter.WriteControl("pict");
            }
        }

        void EndImageDescription()
        {
            if (_isInline)
            {
                _rtfWriter.EndContent();
                _rtfWriter.EndContent();
            }
            else
            {
                _rtfWriter.EndContent();
                EndNameValuePair();
            }
        }

        void RenderImageAttributes()
        {
            if (_isInline)
            {
                _rtfWriter.StartContent();
                _rtfWriter.WriteControlWithStar("picprop");
                RenderNameValuePair("shapeType", "75");
                RenderFillFormat();
                //REM: LineFormat is not completely supported in word.
                RenderLineFormat();
                _rtfWriter.EndContent();
            }
            RenderDimensionSettings();
            RenderCropping();
            RenderSourceType();
        }

        void RenderSourceType()
        {
            var extension = GetFileExtension(); // Path.GetExtension(_filePath);
            if (extension.IsValueNullOrEmpty())
            {
                _imageFile = null;
                Debug.WriteLine("No Image type given.", "warning");
                return;
            }

            switch (extension)
            {
                // Documentation: https://www.biblioscape.com/rtf15_spec.htm

                case ".jpeg":
                case ".jpg":
                    _rtfWriter.WriteControl("jpegblip");
                    break;

                case ".png":
                    _rtfWriter.WriteControl("pngblip");
                    break;

                case ".gif":
                    _rtfWriter.WriteControl("pngblip");
                    break;

                //// TODO BMP files. It is not that simple. Must extract the bytes we need.
                //case ".bmp":
                //    _rtfWriter.WriteControl("dibitmap0");
                //    break;
                //case ".bmp":
                //    _rtfWriter.WriteControl("wbitmap0");
                //    break;

                case ".pdf":
                    // Show a PDF logo in RTF document
                    _imageFile =
                      Assembly.GetExecutingAssembly().GetManifestResourceStream("MigraDoc.RtfRendering.Resources.PDF.png");
                    _rtfWriter.WriteControl("pngblip");
                    break;

                default:
                    Debug.WriteLine(Messages2.ImageTypeNotSupported(_image.Name), "warning");
                    _imageFile = null;
                    break;
            }
        }

        /// <summary>
        /// Renders scaling, width, and height for the image.
        /// </summary>
        void RenderDimensionSettings()
        {
            var shapeWidthPt = GetShapeWidth().Point;
            var shapeHeightPt = GetShapeHeight().Point;

            var scaleX = shapeWidthPt / _originalWidth.Point;
            var scaleY = shapeHeightPt / _originalHeight.Point;
            _rtfWriter.WriteControl("picscalex", (int)(scaleX * 100));
            _rtfWriter.WriteControl("picscaley", (int)(scaleY * 100));

            RenderUnit("pichgoal", shapeHeightPt / scaleY);
            RenderUnit("picwgoal", shapeWidthPt / scaleX);

            //A bit obscure, but necessary for Word 2000:
            _rtfWriter.WriteControl("pich", (int)(_originalHeight.Millimeter * 100));
            _rtfWriter.WriteControl("picw", (int)(_originalWidth.Millimeter * 100));
        }

        void CalculateImageDimensions()
        {
            // See also: void CalculateImageDimensions() in MigraDoc.Rendering.ImageRenderer.
            XImage? bip = null;
            try
            {
                if (_filePath.StartsWith("base64:", StringComparison.Ordinal))
                {
                    string base64 = _filePath.Substring("base64:".Length);
                    byte[] bytes = Convert.FromBase64String(base64);
                    _imageFile = new MemoryStream(bytes, 0, bytes.Length, true, true);
                    _xImage = bip = XImage.FromStream(_imageFile);
                    if (_imageFile.Position != 0)
                        _imageFile.Position = 0;
                }
                else
                {
                    _imageFile = File.OpenRead(_filePath);
                    //System.Drawing.Bitmap bip2 = new System.Drawing.Bitmap(imageFile);
                    _xImage = bip = XImage.FromFile(_filePath);
                }


                float horzResolution;
                float vertResolution;
                string ext = GetFileExtension(); // Path.GetExtension(_filePath).ToLower();
                float origHorzRes = (float)bip.HorizontalResolution;
                float origVertRes = (float)bip.VerticalResolution;

                _originalHeight = bip.PixelHeight * 72 / origVertRes;
                _originalWidth = bip.PixelWidth * 72 / origHorzRes;

                if (_image.Values.Resolution is null)
                {
                    horzResolution = ext == ".gif" ? 72 : (float)bip.HorizontalResolution;
                    vertResolution = ext == ".gif" ? 72 : (float)bip.VerticalResolution;
                }
                else
                {
                    horzResolution = (float)GetValueAsIntended("Resolution")!;
                    vertResolution = horzResolution;
                }

                double origHeight = bip.Size.Height * 72 / vertResolution;
                double origWidth = bip.Size.Width * 72 / horzResolution;

                _imageHeight = origHeight;
                _imageWidth = origWidth;

                bool scaleWidthIsNull = _image.Values.ScaleWidth is null;
                bool scaleHeightIsNull = _image.Values.ScaleHeight is null;
                float sclHeight = scaleHeightIsNull ? 1 : (float)GetValueAsIntended("ScaleHeight")!;
                _scaleHeight = sclHeight;
                float sclWidth = scaleWidthIsNull ? 1 : (float)GetValueAsIntended("ScaleWidth")!;
                _scaleWidth = sclWidth;

                bool doLockAspectRatio = _image.Values.LockAspectRatio is null || _image.LockAspectRatio;

                if (doLockAspectRatio && (scaleHeightIsNull || scaleWidthIsNull))
                {
                    if (_image.Values.Width is not null && _image.Values.Height is null)
                    {
                        _imageWidth = _image.Width;
                        _imageHeight = Unit.FromPoint(origHeight * _imageWidth.Point / origWidth);
                    }
                    else if (_image.Values.Height is not null && _image.Values.Width is null)
                    {
                        _imageHeight = _image.Height;
                        _imageWidth = Unit.FromPoint(origWidth * _imageHeight.Point / origHeight);
                    }
                    else if (_image.Values.Height is not null && _image.Values.Width is not null)
                    {
                        _imageWidth = _image.Width;
                        _imageHeight = _image.Height;
                    }
                    if (scaleWidthIsNull && !scaleHeightIsNull)
                        _scaleWidth = _scaleHeight;
                    else if (scaleHeightIsNull && !scaleWidthIsNull)
                        _scaleHeight = _scaleWidth;
                }
                else
                {
                    if (_image.Values.Width is not null)
                        _imageWidth = _image.Width;
                    if (_image.Values.Height is not null)
                        _imageHeight = _image.Height;
                }
                return;
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine(Messages2.ImageNotFound(_image.Name), "warning");
            }
            catch (Exception exc)
            {
                Debug.WriteLine(Messages2.ImageNotReadable(_image.Name, exc.Message), "warning");
            }
            finally
            {
                if (bip != null)
                {
                    _xImage = null;
                    bip.Dispose();
                }
            }

            //Setting defaults in case an error occurred.
            _imageFile = null;
            _imageHeight = (Unit)GetValueOrDefault("Height", Unit.FromInch(1));
            _imageWidth = (Unit)GetValueOrDefault("Width", Unit.FromInch(1));
            _scaleHeight = (double)GetValueOrDefault("ScaleHeight", 1.0);
            _scaleWidth = (double)GetValueOrDefault("ScaleWidth", 1.0);
        }

        string GetFileExtension()
        {
            if (!String.IsNullOrEmpty(_extension))
                return _extension;

            if (_filePath.StartsWith("base64:", StringComparison.Ordinal))
            {
                // Complicated case: We do not have a filename and must peek into the MemoryStream or the XImage.
                Debug.Assert(_imageFile != null);
                Debug.Assert(_xImage != null);

                if (Equals(_xImage.Format, XImageFormat.Png))
                    return _extension = ".png";
                if (Equals(_xImage.Format, XImageFormat.Gif))
                    return _extension = ".gif";
                //if (Equals(_xImage.Format, XImageFormat.Bmp)) // => Add BMP
                //    return _extension = ".bmp";
                if (Equals(_xImage.Format, XImageFormat.Jpeg))
                    return _extension = ".jpg";

                return _extension = "";
            }

            // Simple case: We have a filename.
            return _extension = Path.GetExtension(_filePath).ToLower();
        }

        /// <summary>
        /// Renders the image file as byte series.
        /// </summary>
        void RenderByteSeries()
        {
            if (_imageFile != null)
            {
                _imageFile.Seek(0, SeekOrigin.Begin);
                int byteVal;
                while ((byteVal = _imageFile.ReadByte()) != -1)
                {
                    string strVal = byteVal.ToString("x");
                    if (strVal.Length == 1)
                        _rtfWriter.WriteText("0");
                    _rtfWriter.WriteText(strVal);
                }
                _imageFile.Close();
            }
        }

        protected override Unit GetShapeHeight()
        {
            return _imageHeight * _scaleHeight;
        }

        protected override Unit GetShapeWidth()
        {
            return _imageWidth * _scaleWidth;
        }

        /// <summary>
        /// Renders the image cropping at all edges.
        /// </summary>
        void RenderCropping()
        {
            Translate("PictureFormat.CropLeft", "piccropl");
            Translate("PictureFormat.CropRight", "piccropr");
            Translate("PictureFormat.CropTop", "piccropt");
            Translate("PictureFormat.CropBottom", "piccropb");
        }

        readonly string _filePath;
        readonly Image _image;

        readonly bool _isInline;
        //FileStream imageFile;
        Stream? _imageFile;
        string? _extension;
        XImage? _xImage;
        Unit _imageWidth;
        Unit _imageHeight;

        Unit _originalHeight;
        Unit _originalWidth;

        //this is the user defined scaling, not the stuff to render as scalex, scaley
        double _scaleHeight;
        double _scaleWidth;
    }
}