// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.IO;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Drawing;
using PdfSharp.Drawing.BarCodes;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering.Resources;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders barcodes.
    /// </summary>
    // Adapted from https://forum.pdfsharp.net/viewtopic.php?p=3332#p3332
    internal class BarcodeRenderer : ShapeRenderer
    {
        internal BarcodeRenderer(XGraphics gfx, Barcode barcode, FieldInfos fieldInfos)
            : base(gfx, barcode, fieldInfos)
        {
            this._barcode = barcode;
            BarcodeRenderInfo renderInfo = new BarcodeRenderInfo();
            renderInfo.DocumentObject = this._shape;
            this._renderInfo = renderInfo;
        }

        internal BarcodeRenderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos fieldInfos)
            : base(gfx, renderInfo, fieldInfos)
        {
            this._barcode = (Barcode)renderInfo.DocumentObject;
        }

        internal override void Format(Area area, FormatInfo previousFormatInfo)
        {
            BarcodeFormatInfo formatInfo = (BarcodeFormatInfo)this._renderInfo.FormatInfo;

            formatInfo.Height = this._barcode.Height.Point;
            formatInfo.Width = this._barcode.Width.Point;

            base.Format(area, previousFormatInfo);
        }

        protected override XUnit ShapeHeight
        {
            get
            {
                BarcodeFormatInfo formatInfo = (BarcodeFormatInfo)this._renderInfo.FormatInfo;
                return formatInfo.Height + this._lineFormatRenderer.GetWidth();
            }
        }

        protected override XUnit ShapeWidth
        {
            get
            {
                BarcodeFormatInfo formatInfo = (BarcodeFormatInfo)this._renderInfo.FormatInfo;
                return formatInfo.Width + this._lineFormatRenderer.GetWidth();
            }
        }

        internal override void Render()
        {
            RenderFilling();

            BarcodeFormatInfo formatInfo = (BarcodeFormatInfo)this._renderInfo.FormatInfo;
            Area contentArea = this._renderInfo.LayoutInfo.ContentArea;
            XRect destRect = new XRect(contentArea.X, contentArea.Y, formatInfo.Width, formatInfo.Height);

            BarCode gfxBarcode = null;

            if (this._barcode.Type == BarcodeType.Barcode39)
                gfxBarcode = new Code3of9Standard();
            else if (this._barcode.Type == BarcodeType.Barcode25i)
                gfxBarcode = new Code2of5Interleaved();
            else if (this._barcode.Type == BarcodeType.Barcode128)
                gfxBarcode = new Code128();

            // if gfxBarcode is null, the barcode type is not supported
            if (gfxBarcode != null)
            {
                gfxBarcode.Text = this._barcode.Code;
                gfxBarcode.Direction = CodeDirection.LeftToRight;
                gfxBarcode.Size = new XSize(ShapeWidth, ShapeHeight);

                this._gfx.DrawBarCode(gfxBarcode, XBrushes.Black, destRect.Location);
            }

            RenderLine();
        }

        Barcode _barcode;
    }
}
