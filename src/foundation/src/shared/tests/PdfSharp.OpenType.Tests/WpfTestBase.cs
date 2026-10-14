// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Quality.Testing;
using Xunit;

namespace PdfSharp.OpenType.Tests
{
    [Collection("PDFsharp")]
    public class OpenTypeTestBase : PdfSharpTestBase
    {
        public OpenTypeTestBase()
        { }

        //public void Dispose()
        //{ }

        //WpfBitmapSource render()
        //{

        //    var content = ContentReader.ReadContent(page);

        //    var pageSize = new Size((float)page.Width.Point, (float)page.Height.Point);

        //    //var device = CanvasDevice.GetSharedDevice();
        //    //var renderTarget = new CanvasRenderTarget(device, pageSize.Width, pageSize.Height, 4*72);

        //    int cx = (int)pageSize.Width;
        //    int cy = (int)pageSize.Height;
        //    int dpi = 600;
        //    double scaling = dpi / 72d;
        //    WpfRenderTargetBitmap renderBitmap =
        //        new WpfRenderTargetBitmap((int)(cx * scaling), (int)(cy * scaling), dpi, dpi, System.Windows.Media.PixelFormats.Pbgra32);

        //    WpfDrawingVisual drawingVisual = new WpfDrawingVisual();
        //    using (WpfDrawingContext context = drawingVisual.RenderOpen())
        //    {
        //        //var matrix = new WpfMatrix(1 / scaling, 0, 0, 1 / scaling, 0, 0);
        //        //context.PushTransform(new WpfMatrixTransform(matrix));
        //        //var group = new System.Windows.Media.GeometryGroup();
        //        //group.Children.Add(new System.Windows.Media.LineGeometry(new WpfPoint(0, 0), new WpfPoint(cx, cy)));
        //        //group.Children.Add(new System.Windows.Media.LineGeometry(new WpfPoint(0, cx), new WpfPoint(cx, 0)));
        //        //context.DrawGeometry(null, Colors.Red.CreateWpfPen(), group);
        //        ////VisualBrush visualBrush = new VisualBrush(target);
        //        //context.DrawRectangle(System.Windows.Media.Brushes.Red, null, new WpfRect(10, 10, 50, 50));

        //        XPdfProcessor processor = new XPdfProcessor();
        //        var target = new PdfSharp.Content.WPF.WpfRenderTarget(processor.PdfGraphicState, context, page);
        //        processor.SetRenderingTarget(target);
        //        processor.Apply(content, page);
        //    }

        //    renderBitmap.Render(drawingVisual);
        //    {
        //        var encoder = new PngBitmapEncoder();
        //        BitmapFrame frame = BitmapFrame.Create(renderBitmap);
        //        encoder.Frames.Add(frame);
        //        var fullName = IOUtility.GetTempFullFileName("unit-tests/" + typeof(PdfContentHelper).Namespace + "/" + nameof(PdfContentHelper) + "/" + "Test_TD" + Capabilities.Create.BuildTag, "png");
        //        using var stream = System.IO.File.Create(fullName);
        //        encoder.Save(stream);
        //    }
        //    return renderBitmap;
        //}
    }
}
