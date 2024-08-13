// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Globalization;
#if WPF
using System.IO;
#endif
using System.Security.Cryptography.X509Certificates;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Signatures;
using PdfSharp.Quality;
using PdfSharp.Snippets.Pdf;
using Xunit;

namespace PdfSharp.Tests.Pdf
{
    [Collection("PDFsharp")]
    public class BouncyCastleSignerTests
    {
        /// <summary>
        /// The minimum assets version required.
        /// </summary>
        public const int RequiredAssets = 1014;

        [Fact]
        public void Sign_new_file_Bouncy()
        {
            IOUtility.EnsureAssetsVersion(RequiredAssets);

            var font = new XFont("Verdana", 10, XFontStyleEx.Regular);
            var fontHeader = new XFont("Verdana", 18, XFontStyleEx.Regular);
            using var document = new PdfDocument();
            var pdfPage = document.AddPage();
            var xGraphics = XGraphics.FromPdfPage(pdfPage);
            var layoutRectangle = new XRect(0, 72, pdfPage.Width.Point, pdfPage.Height.Point);
            xGraphics.DrawString("Document Signature Test", fontHeader, XBrushes.Black, layoutRectangle, XStringFormats.TopCenter);
            var textFormatter = new XTextFormatter(xGraphics);
            layoutRectangle = new XRect(72, 144, pdfPage.Width.Point - 144, pdfPage.Height.Point - 144);

            var text = "Lorem ipsum...";
            textFormatter.DrawString(text, font, new XSolidBrush(XColor.FromKnownColor(XKnownColor.Black)), layoutRectangle, XStringFormats.TopLeft);

            var pdfPosition = xGraphics.Transformer.WorldToDefaultPage(new XPoint(144, 216));
            var options = new DigitalSignatureOptions
            {
                ContactInfo = "John Doe",
                Location = "Seattle",
                Reason = "License Agreement",
                Rectangle = new XRect(pdfPosition.X, pdfPosition.Y, 200, 50),
                AppearanceHandler = new SignAppearanceHandler()
            };

            var pdfSignatureHandler = DigitalSignatureHandler.ForDocument(document, new BouncyCastleSigner(GetCertificate(), PdfMessageDigestType.SHA512), options);

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/signatures/BouncySignerTest");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void Sign_existing_file_Bouncy()
        {
            IOUtility.EnsureAssetsVersion(RequiredAssets);
            var pdfFolder = IOUtility.GetAssetsPath("archives/samples-1.5/PDFs");
            var pdfFile = Path.Combine(pdfFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), "SomeLayout.pdf");

            PdfDocument document = PdfReader.Open(File.OpenRead(pdfFile));
            var options = new DigitalSignatureOptions
            {
                ContactInfo = "John Doe",
                Location = "Seattle",
                Reason = "License Agreement",
                Rectangle = new(XUnit.FromCentimeter(1).Point, XUnit.FromCentimeter(0).Point,
                    XUnit.FromCentimeter(9.9).Point, XUnit.FromCentimeter(1.3).Point),
                AppearanceHandler = new SignAppearanceHandler()
            };
            var pdfSignatureHandler = DigitalSignatureHandler.ForDocument(document, new BouncyCastleSigner(GetCertificate(), PdfMessageDigestType.SHA512), options);

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/signatures/BouncySignExistingPdfTest");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        static (X509Certificate2, X509Certificate2Collection) GetCertificate()
        {
            var certFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/signatures");
            var pfxFile = Path.Combine(certFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), "test-cert_rsa_1024.pfx");
            var rawData = File.ReadAllBytes(pfxFile);

            // Do not use password literals for real certificates in source code.
            var certificatePassword = "Seecrit1243";

            var certificate = new X509Certificate2(rawData,
                certificatePassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            var collection = new X509Certificate2Collection();
            collection.Import(rawData, certificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            return (certificate, collection);
        }

        class SignAppearanceHandler : IAnnotationAppearanceHandler
        {
            public void DrawAppearance(XGraphics gfx, XRect rect)
            {
                var imageFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/signatures");
                var pngFile = Path.Combine(imageFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), "JohnDoe.png");
                var image = XImage.FromFile(pngFile);

                string text = "John Doe\nSeattle, " + DateTime.Now.ToString(CultureInfo.GetCultureInfo("EN-US"));
                var font = new XFont("Verdana", 7.0, XFontStyleEx.Regular);
                var textFormatter = new XTextFormatter(gfx);
                double num = (double)image.PixelWidth / image.PixelHeight;
                double signatureHeight = rect.Height * .4;
                var point = new XPoint(rect.Width / 10, rect.Height / 10);
                // Draw image.
                gfx.DrawImage(image, point.X, point.Y, signatureHeight * num, signatureHeight);
                // Adjust position for text. We draw it below image.
                point = new XPoint(point.X, rect.Height / 2d);
                textFormatter.DrawString(text, font, new XSolidBrush(XColor.FromKnownColor(XKnownColor.Black)), new XRect(point.X, point.Y, rect.Width, rect.Height - point.Y), XStringFormats.TopLeft);
            }
        }
    }
}
