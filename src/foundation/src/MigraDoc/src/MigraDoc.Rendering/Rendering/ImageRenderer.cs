// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
#if WPF
using System.IO;
#endif
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering.Resources;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders images.
    /// </summary>
    class ImageRenderer : ShapeRenderer
    {
        internal ImageRenderer(XGraphics gfx, Image image, FieldInfos? fieldInfos)
            : base(gfx, image, fieldInfos)
        {
            _image = image;
            ImageRenderInfo renderInfo = new() { DocumentObject = _shape };
            _renderInfo = renderInfo;
        }

        internal ImageRenderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos? fieldInfos)
            : base(gfx, renderInfo, fieldInfos)
        {
            _image = (Image)renderInfo.DocumentObject;
        }

        internal override void Format(Area area, FormatInfo? previousFormatInfo)
        {
            _imageFilePath = _image.GetFilePath(_documentRenderer.WorkingDirectory);
            // The Image is stored in the string if path starts with "base64:", otherwise we check whether the file exists.
            if (!_imageFilePath.StartsWith("base64:", StringComparison.Ordinal) &&
                !XImage.ExistsFile(_imageFilePath))
            {
                _failure = ImageFailure.FileNotFound;
                Debug.WriteLine(Messages2.ImageNotFound(_image.Name), "warning");
            }
            ImageFormatInfo formatInfo = (ImageFormatInfo)_renderInfo.FormatInfo;
            formatInfo.Failure = _failure;
            formatInfo.ImagePath = _imageFilePath;
            CalculateImageDimensions();
            base.Format(area, previousFormatInfo);
        }

        protected override XUnit ShapeHeight
        {
            get
            {
                ImageFormatInfo formatInfo = (ImageFormatInfo)_renderInfo.FormatInfo;
                return formatInfo.Height + _lineFormatRenderer.GetWidth();
            }
        }

        protected override XUnit ShapeWidth
        {
            get
            {
                ImageFormatInfo formatInfo = (ImageFormatInfo)_renderInfo.FormatInfo;
                return formatInfo.Width + _lineFormatRenderer.GetWidth();
            }
        }

        internal override void Render()
        {
            RenderFilling();

            ImageFormatInfo formatInfo = (ImageFormatInfo)_renderInfo.FormatInfo;
            Area contentArea = _renderInfo.LayoutInfo.ContentArea;
            XRect destRect = new XRect(contentArea.X, contentArea.Y, formatInfo.Width, formatInfo.Height);

            if (formatInfo.Failure == ImageFailure.None)
            {
                XImage? xImage = null;
                try
                {
                    XRect srcRect = new(formatInfo.CropX, formatInfo.CropY, formatInfo.CropWidth, formatInfo.CropHeight);
                    //xImage = XImage.FromFile(formatInfo.ImagePath);
                    xImage = CreateXImage(formatInfo.ImagePath);
                    _gfx.DrawImage(xImage, destRect, srcRect, XGraphicsUnit.Point); //Pixel.
                }
                catch (Exception)
                {
                    RenderFailureImage(destRect);
                }
                finally
                {
                    if (xImage != null)
                        xImage.Dispose();
                }
            }
            else
                RenderFailureImage(destRect);

            RenderLine();
        }

        void RenderFailureImage(XRect destRect)
        {
            _gfx.DrawRectangle(XBrushes.LightGray, destRect);
            string failureString;
            ImageFormatInfo formatInfo = (ImageFormatInfo)RenderInfo.FormatInfo;

            switch (formatInfo.Failure)
            {
                case ImageFailure.EmptySize:
                    failureString = Messages2.DisplayEmptyImageSize;
                    break;

                case ImageFailure.FileNotFound:
                    failureString = Messages2.DisplayImageFileNotFound;
                    break;

                case ImageFailure.InvalidType:
                    failureString = Messages2.DisplayInvalidImageType;
                    break;

                case ImageFailure.NotRead:
                default:
                    failureString = Messages2.DisplayImageNotRead;
                    break;
            }

            // Create stub font
            XFont font = new XFont("Courier New", 8);
            _gfx.DrawString(failureString, font, XBrushes.Red, destRect, XStringFormats.Center);
        }

        void CalculateImageDimensions()
        {
            ImageFormatInfo formatInfo = (ImageFormatInfo)_renderInfo.FormatInfo;

            if (formatInfo.Failure == ImageFailure.None)
            {
                XImage? xImage = null;
                try
                {
                    //xImage = XImage.FromFile(_imageFilePath);
                    xImage = CreateXImage(_imageFilePath);
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine(Messages2.InvalidImageType(ex.Message));
                    formatInfo.Failure = ImageFailure.InvalidType;
                }

                if (formatInfo.Failure == ImageFailure.None)
                {
                    try
                    {
                        XUnit usrWidth = _image.Width.Point;
                        XUnit usrHeight = _image.Height.Point;
                        //var usrWidthSet = _image.Values.Width is not null;
                        //var usrHeightSet = _image.Values.Height is not null;
                        var usrWidthSet = !_image.Values.Width.IsValueNullOrEmpty();
                        var usrHeightSet = !_image.Values.Height.IsValueNullOrEmpty();

                        XUnit resultWidth = usrWidth;
                        XUnit resultHeight = usrHeight;

                        Debug.Assert(xImage != null);
                        double xPixels = xImage.PixelWidth;
                        bool usrResolutionSet = _image.Values.Resolution is not null;

                        double horzRes = usrResolutionSet ? _image.Resolution : xImage.HorizontalResolution;
                        double vertRes = usrResolutionSet ? _image.Resolution : xImage.VerticalResolution;

                        // ReSharper disable CompareOfFloatsByEqualityOperator
                        if (horzRes == 0 && vertRes == 0)
                        {
                            horzRes = 72;
                            vertRes = 72;
                        }
                        else if (horzRes == 0)
                        {
                            Debug.Assert(false, "How can this be?");
                            horzRes = 72;
                        }
                        else if (vertRes == 0)
                        {
                            Debug.Assert(false, "How can this be?");
                            vertRes = 72;
                        }
                        // ReSharper restore CompareOfFloatsByEqualityOperator

                        XUnit inherentWidth = XUnit.FromInch(xPixels / horzRes);
                        double yPixels = xImage.PixelHeight;
                        XUnit inherentHeight = XUnit.FromInch(yPixels / vertRes);

                        //bool lockRatio = _image.IsNull("LockAspectRatio") ? true : _image.LockAspectRatio;
                        bool lockRatio = _image.Values.LockAspectRatio is null || _image.LockAspectRatio;

                        double scaleHeight = _image.ScaleHeight;
                        double scaleWidth = _image.ScaleWidth;
                        //bool scaleHeightSet = !_image.IsNull("ScaleHeight");
                        //bool scaleWidthSet = !_image.IsNull("ScaleWidth");
                        //bool scaleHeightSet = !_image._scaleHeight.IsNull;
                        //bool scaleWidthSet = !_image._scaleWidth.IsNull;
                        bool scaleHeightSet = _image.Values.ScaleHeight is not null;
                        bool scaleWidthSet = _image.Values.ScaleWidth is not null;

                        if (lockRatio && !(scaleHeightSet && scaleWidthSet))
                        {
                            if (usrWidthSet && !usrHeightSet)
                            {
                                resultHeight = inherentHeight / inherentWidth * usrWidth;
                            }
                            else if (usrHeightSet && !usrWidthSet)
                            {
                                resultWidth = inherentWidth / inherentHeight * usrHeight;
                            }
                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            else if (!usrHeightSet && !usrWidthSet)
                            {
                                resultHeight = inherentHeight;
                                resultWidth = inherentWidth;
                            }

                            if (scaleHeightSet)
                            {
                                resultHeight = resultHeight * scaleHeight;
                                resultWidth = resultWidth * scaleHeight;
                            }
                            if (scaleWidthSet)
                            {
                                resultHeight = resultHeight * scaleWidth;
                                resultWidth = resultWidth * scaleWidth;
                            }
                        }
                        else
                        {
                            if (!usrHeightSet)
                                resultHeight = inherentHeight;

                            if (!usrWidthSet)
                                resultWidth = inherentWidth;

                            if (scaleHeightSet)
                                resultHeight = resultHeight * scaleHeight;
                            if (scaleWidthSet)
                                resultWidth = resultWidth * scaleWidth;
                        }

                        formatInfo.CropWidth = (int)xPixels;
                        formatInfo.CropHeight = (int)yPixels;
                        if (_image.Values.PictureFormat != null)
                        {
                            PictureFormat picFormat = _image.PictureFormat;
                            //Cropping in pixels.
                            XUnit cropLeft = picFormat.CropLeft.Point;
                            XUnit cropRight = picFormat.CropRight.Point;
                            XUnit cropTop = picFormat.CropTop.Point;
                            XUnit cropBottom = picFormat.CropBottom.Point;
                            formatInfo.CropX = (int)(horzRes * cropLeft.Inch);
                            formatInfo.CropY = (int)(vertRes * cropTop.Inch);
                            formatInfo.CropWidth -= (int)(horzRes * ((XUnit)(cropLeft + cropRight)).Inch);
                            formatInfo.CropHeight -= (int)(vertRes * ((XUnit)(cropTop + cropBottom)).Inch);

                            //Scaled cropping of the height and width.
                            double xScale = resultWidth / inherentWidth;
                            double yScale = resultHeight / inherentHeight;

                            cropLeft = xScale * cropLeft;
                            cropRight = xScale * cropRight;
                            cropTop = yScale * cropTop;
                            cropBottom = yScale * cropBottom;

                            resultHeight = resultHeight - cropTop - cropBottom;
                            resultWidth = resultWidth - cropLeft - cropRight;
                        }
                        if (resultHeight <= 0 || resultWidth <= 0)
                        {
                            formatInfo.Width = XUnit.FromCentimeter(2.5);
                            formatInfo.Height = XUnit.FromCentimeter(2.5);
                            Debug.WriteLine(Messages2.EmptyImageSize);
                            _failure = ImageFailure.EmptySize;
                        }
                        else
                        {
                            formatInfo.Width = resultWidth;
                            formatInfo.Height = resultHeight;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(Messages2.ImageNotReadable(_image.Name, ex.Message));
                        formatInfo.Failure = ImageFailure.NotRead;
                    }
                    finally
                    {
                        if (xImage != null)
                            xImage.Dispose();
                    }
                }
            }
            if (formatInfo.Failure != ImageFailure.None)
            {
                //if (_image.Values.Width is not null)
                if (!_image.Values.Width.IsValueNullOrEmpty())
                    formatInfo.Width = _image.Width.Point;
                else
                    formatInfo.Width = XUnit.FromCentimeter(2.5);

                //if (_image.Values.Height is not null)
                if (!_image.Values.Height.IsValueNullOrEmpty())
                    formatInfo.Height = _image.Height.Point;
                else
                    formatInfo.Height = XUnit.FromCentimeter(2.5);
            }
        }

        XImage CreateXImage(string uri)
        {
            if (uri.StartsWith("base64:", StringComparison.Ordinal))
            {
                string base64 = uri.Substring("base64:".Length);
                byte[] bytes = Convert.FromBase64String(base64);
#if WPF || GDI
                // WPF stores a reference to the stream internally. We must not destroy the stream here, otherwise rendering the PDF will fail.
                // Same for GDI.
                // We have to rely on the garbage collector to properly dispose the MemoryStream.
                {
                    Stream stream = new MemoryStream(bytes);
                    XImage image = XImage.FromStream(stream);
                    return image;
                }
#else
                //using Stream stream = new MemoryStream(bytes);
                using Stream stream = new MemoryStream(bytes, 0, bytes.Length, true, true);
                XImage image = XImage.FromStream(stream);
                return image;
#endif
            }
            return XImage.FromFile(uri);
        }

        readonly Image _image;
        string _imageFilePath = null!;
        ImageFailure _failure;
    }
}
