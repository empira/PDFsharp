// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using System.Drawing;
using Microsoft.Extensions.Logging;
#if !NETSTANDARD2_0_OR_GREATER
using System.Drawing.Imaging;
#endif
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.Logging;
#if GDI
using PdfSharp.Drawing;
using PdfSharp.Events;
using MigraDoc.Rendering;
#endif
#if WPF
using PdfSharp.Drawing;
using MigraDoc.Rendering;
#endif

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Summary description for ChartRenderer.
    /// </summary>
    sealed class ChartRenderer : ShapeRenderer
    {
        internal ChartRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _chart = (Chart)domObj;
            _isInline = DocumentRelations.HasParentOfType(_chart, typeof(Paragraph)) ||
              RenderInParagraph();
        }

        /// <summary>
        /// Renders an image to RTF.
        /// </summary>
        internal override void Render()
        {
            string fileName = Path.GetTempFileName();
            if (!StoreTempImage(fileName))
                return;

            bool renderInParagraph = RenderInParagraph();

            if (DocumentRelations.GetParent(_chart) is DocumentElements elms && !renderInParagraph && !(DocumentRelations.GetParent(elms) is Section || DocumentRelations.GetParent(elms) is HeaderFooter))
            {
                MigraDocLogHost.RtfRenderingLogger.LogWarning(MdRtfMsgs.ChartFreelyPlacedInWrongContext.Message);
                //Debug.WriteLine(Messages2.ChartFreelyPlacedInWrongContext, "warning");
                return;
            }

            if (renderInParagraph)
                StartDummyParagraph();

            if (!_isInline)
                StartShapeArea();

            RenderImage(fileName);

            if (!_isInline)
                EndShapeArea();

            if (renderInParagraph)
                EndDummyParagraph();

            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        /// <summary>
        /// Renders image specific attributes and the image byte series to RTF.
        /// </summary>
        void RenderImage(string fileName)
        {
            StartImageDescription();
            RenderImageAttributes();
            RenderByteSeries(fileName);
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
                RenderNameValuePair("shapeType", "75");//75 entspr. Bildrahmen.
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
                //RenderLineFormat();
                _rtfWriter.EndContent();
            }
            RenderDimensionSettings();
            RenderSourceType();
        }

        void RenderSourceType()
        {
            _rtfWriter.WriteControl("pngblip");
        }

        /// <summary>
        /// Renders scaling, width, and height for the image.
        /// </summary>
        void RenderDimensionSettings()
        {
            _rtfWriter.WriteControl("picscalex", 100);
            _rtfWriter.WriteControl("picscaley", 100);

            RenderUnit("pichgoal", GetShapeHeight());
            RenderUnit("picwgoal", GetShapeWidth());

            //A bit obscure, but necessary for Word 2000:
            _rtfWriter.WriteControl("pich", (int)(GetShapeHeight().Millimeter * 100));
            _rtfWriter.WriteControl("picw", (int)(GetShapeWidth().Millimeter * 100));
        }

        bool StoreTempImage(string fileName)
        {
#if !GDI
            // ReviewSTLA THHO4STLA
            // EXPERIMENTAL
            switch (Capabilities.Compatibility.ChartsCannotBeRendered)
            {
                case Capabilities.FeatureNotAvailableAction.DoNothing:
                    break;

                case Capabilities.FeatureNotAvailableAction.FailWithException:
                    //DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddString");
                    throw new NotSupportedException("Charts in RTF are not yet implemented for Core and GDI build.");
                    //break;

                case Capabilities.FeatureNotAvailableAction.LogWarning:
                    // TODO Logging. TODO Unified handling for PDFsharp and MigraDoc.
                    break;

                case Capabilities.FeatureNotAvailableAction.LogError:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            //// NET6 return false TODO
            //if (Capabilities.Compatibility.ChartsCannotBeRendered)
            //    throw new InvalidOperationException("This version of MigraDoc cannot render charts to RTF. Use the WPF build under Windows to create RTF files with charts.");

            return false;
#else
            try
            {
                const float resolution = 96;
                int horzPixels = (int)(GetShapeWidth().Inch * resolution);
                int vertPixels = (int)(GetShapeHeight().Inch * resolution);
#if false
                XGraphics gfx =
                    XGraphics.CreateMeasureContext(new XSize(horzPixels, vertPixels), XGraphicsUnit.Point, XPageDirection.Downwards, new RenderEvents());
#else
#if GDI
                Bitmap bmp = new Bitmap(horzPixels, vertPixels);
                XGraphics gfx = XGraphics.FromGraphics(Graphics.FromImage(bmp), new XSize(horzPixels, vertPixels), new RenderEvents());
#else
                // TODOWPF
                // TODOCORE
                return false;
#endif
#if WPF
                // TODOWPF
                XGraphics gfx = null; //XGraphics.FromGraphics(Graphics.FromImage(bmp), new XSize(horzPixels, vertPixels));
#endif
#endif
                //REM: Should not be necessary:
                gfx.ScaleTransform(resolution / 72);
                //gfx.PageUnit = XGraphicsUnit.Point;

                DocumentRenderer renderer = new MigraDoc.Rendering.DocumentRenderer(_chart.Document!);
                renderer.RenderObject(gfx, 0, 0, GetShapeWidth().Point, _chart);
#if GDI
                bmp.SetResolution(resolution, resolution);
                bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
#endif
            }
            catch (Exception)
            {
                return false;
            }
            return true;
#endif
            }
        //-/*private void CalculateImageDimensions()
        //{
        //  try
        //  {
        //    this.imageFile = File.OpenRead(this.filePath);
        //    System.Drawing.Bitmap bip = new System.Drawing.Bitmap(imageFile);
        //    float horzResolution;
        //    float vertResolution;
        //    string ext = Path.GetExtension(this.filePath).ToLower();
        //    float origHorzRes = bip.HorizontalResolution;
        //    float origVertRes = bip.VerticalResolution;

            //    this.originalHeight = bip.Height * 72 / origVertRes;
            //    this.originalWidth = bip.Width * 72 / origHorzRes;

            //      horzResolution = bip.HorizontalResolution;
            //      vertResolution = bip.VerticalResolution;
            //    }
            //    else
            //    {
            //      horzResolution= (float)GetValueAsIntended("Resolution");
            //      vertResolution= horzResolution;
            //    }

            //    Unit origHeight = bip.Size.Height * 72 / vertResolution;
            //    Unit origWidth = bip.Size.Width * 72 / horzResolution;

            //    this.imageHeight = origHeight;
            //    this.imageWidth = origWidth;

            //    bool scaleWidthIsNull = this.image.IsNull("ScaleWidth");
            //    bool scaleHeightIsNull = this.image.IsNull("ScaleHeight");
            //    float sclHeight = scaleHeightIsNull ? 1 : (float)GetValueAsIntended("ScaleHeight");
            //    this.scaleHeight= sclHeight;
            //    float sclWidth = scaleWidthIsNull ? 1 : (float)GetValueAsIntended("ScaleWidth");
            //    this.scaleWidth = sclWidth;

            //    bool doLockAspectRatio = this.image.IsNull("LockAspectRatio") || this.image.LockAspectRatio;

            //    if (doLockAspectRatio && (scaleHeightIsNull || scaleWidthIsNull))
            //    {
            //      if (!this.image.IsNull("Width") && this.image.IsNull("Height"))
            //      {
            //        imageWidth = this.image.Width;
            //        imageHeight = origHeight * imageWidth / origWidth;
            //      }
            //      else if (!this.image.IsNull("Height") && this.image.IsNull("Width"))
            //      {
            //        imageHeight = this.image.Height;
            //        imageWidth = origWidth * imageHeight / origHeight;
            //      }
            //      else if (!this.image.IsNull("Height") && !this.image.IsNull("Width"))
            //      {
            //        imageWidth = this.image.Width;
            //        imageHeight = this.image.Height;
            //      }
            //      if (scaleWidthIsNull && !scaleHeightIsNull)
            //        scaleWidth = scaleHeight;
            //      else if (scaleHeightIsNull && ! scaleWidthIsNull)
            //        scaleHeight =  scaleWidth;
            //    }
            //    else
            //    {
            //      if (!this.image.IsNull("Width"))
            //        imageWidth = this.image.Width;
            //      if (!this.image.IsNull("Height"))
            //        imageHeight = this.image.Height;
            //    }

            //    return;
            //  }
            //  catch(FileNotFoundException)
            //  {
            //    Debug.WriteLine(Messages.ImageNotFound(this.image.Name), "warning");
            //  }
            //  catch(Exception exc)
            //  {
            //    Debug.WriteLine(Messages.ImageNotReadable(this.image.Name, exc.Message), "warning");
            //  }

            //  //Setting defaults in case an error occurred.
            //  this.imageFile = null;
            //  this.imageHeight = (Unit)GetValueOrDefault("Height", Unit.FromInch(1));
            //  this.imageWidth = (Unit)GetValueOrDefault("Width", Unit.FromInch(1));
            //  this.scaleHeight = (double)GetValueOrDefault("ScaleHeight", 1.0);
            //  this.scaleWidth = (double)GetValueOrDefault("ScaleWidth", 1.0);
            //}*/

            /// <summary>
            /// Renders the image file as byte series.
            /// </summary>
        void RenderByteSeries(string fileName)
        {
            FileStream? imageFile = null;
            try
            {
                imageFile = new FileStream(fileName, FileMode.Open);

                imageFile.Seek(0, SeekOrigin.Begin);
                int byteVal;
                while ((byteVal = imageFile.ReadByte()) != -1)
                {
                    string strVal = byteVal.ToString("x");
                    if (strVal.Length == 1)
                        _rtfWriter.WriteText("0");
                    _rtfWriter.WriteText(strVal);
                }
            }
            catch
            {
                MigraDocLogHost.RtfRenderingLogger.LogError("Chart image file not read");
                //Debug.WriteLine("Chart image file not read", "warning");
            }
            finally
            {
                if (imageFile != null)
                    imageFile.Close();
            }
        }

        protected override Unit GetShapeHeight()
        {
            return base.GetShapeHeight() + base.GetLineWidth();
        }

        protected override Unit GetShapeWidth()
        {
            return base.GetShapeWidth() + base.GetLineWidth();
        }

        readonly Chart _chart;
        readonly bool _isInline;
    }
}
