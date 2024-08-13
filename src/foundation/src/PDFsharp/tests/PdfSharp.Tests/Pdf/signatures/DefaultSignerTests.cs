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
using Xunit;
using SecurityTestHelper = PdfSharp.TestHelper.SecurityTestHelper;

namespace PdfSharp.Tests.Pdf
{
    [Collection("PDFsharp")]
    public class DefaultSignerTests
    {
        /// <summary>
        /// The minimum assets version required.
        /// </summary>
        public const int RequiredAssets = 1014;

        [Theory]
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, null)]
        public void Sign_new_file_with_DefaultAppearance(string certType, PdfMessageDigestType digestType, string timestampURL)
        {
#if DEBUG
            var version = PdfSharp.Internal.BuildInformation.BuildVersionNumber;
#endif
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
                // We do not set an appearance handler, so the default handler is used.
                // It is highly recommended to set an appearance handler to get a nicer representation of the signature.
                ContactInfo = "John Doe",
                Location = "Seattle",
                Reason = "License Agreement",
                Rectangle = new XRect(pdfPosition.X, pdfPosition.Y, 200, 50),
                AppName = "PDFsharp Library"
            };

            Uri? timestampURI = String.IsNullOrEmpty(timestampURL) ? null : new Uri(timestampURL, UriKind.Absolute);

            var pdfSignatureHandler = DigitalSignatureHandler.ForDocument(document, new PdfSharpDefaultSigner(GetCertificate(certType), digestType, timestampURI), options);

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/signatures/DefaultAppearanceHandler-" + certType);
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Theory]
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA1, null)]
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, null)]
#if NET6_0_OR_GREATER
        // Time stamping not implemented for .NET Standard or .NET Framework.
        // Some arbitrarily selected timestamp servers.
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, "http://zeitstempel.dfn.de")]
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, "http://timestamp.sectigo.com")]
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, "http://timestamp.apple.com/ts01")]
#if DEBUG
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, "http://timestamp.digicert.com")]
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, "http://ts.ssl.com")]
#endif
#endif
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA384, null)]
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA512, null)]
        [InlineData("test-cert_rsa_2048", PdfMessageDigestType.SHA1, null)]
        [InlineData("test-cert_rsa_2048", PdfMessageDigestType.SHA256, null)]
        [InlineData("test-cert_rsa_2048", PdfMessageDigestType.SHA384, null)]
        [InlineData("test-cert_rsa_2048", PdfMessageDigestType.SHA512, null)]
        [InlineData("test-cert_rsa_3072", PdfMessageDigestType.SHA1, null)]
        [InlineData("test-cert_rsa_3072", PdfMessageDigestType.SHA256, null)]
        [InlineData("test-cert_rsa_3072", PdfMessageDigestType.SHA384, null)]
        [InlineData("test-cert_rsa_3072", PdfMessageDigestType.SHA512, null)]
        [InlineData("test-cert_rsa_4096", PdfMessageDigestType.SHA1, null)]
        [InlineData("test-cert_rsa_4096", PdfMessageDigestType.SHA256, null)]
        [InlineData("test-cert_rsa_4096", PdfMessageDigestType.SHA384, null)]
        [InlineData("test-cert_rsa_4096", PdfMessageDigestType.SHA512, null)]
        public void Sign_new_file_Default(string certType, PdfMessageDigestType digestType, string timestampURL)
        {
#if DEBUG
            var version = PdfSharp.Internal.BuildInformation.BuildVersionNumber;
#endif
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
                AppearanceHandler = new SignatureAppearanceHandler()
            };

            Uri? timestampURI = String.IsNullOrEmpty(timestampURL) ? null : new Uri(timestampURL, UriKind.Absolute);

            var pdfSignatureHandler = DigitalSignatureHandler.ForDocument(document, new PdfSharpDefaultSigner(GetCertificate(certType), digestType, timestampURI), options);

            // pdfSignatureHandler What to do with it? Set SHA-level? Set TimeStamp?

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/signatures/DefaultSignerTest-" + certType);
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

#if NET6_0_OR_GREATER
        [SkippableTheory]
        //[InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA1, null)]
        //[InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, null)]
#if NET6_0_OR_GREATER
        // Time stamping not implemented for .NET Standard or .NET Framework.
        // Some arbitrarily selected timestamp servers.
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, "http://zeitstempel.dfn.de")]
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, "http://timestamp.sectigo.com")]
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, "http://timestamp.apple.com/ts01")]
#if DEBUG
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, "http://timestamp.digicert.com")]
        [InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA256, "http://ts.ssl.com")]
#endif
#endif
        //[InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA384, null)]
        //[InlineData("test-cert_rsa_1024", PdfMessageDigestType.SHA512, null)]
        //[InlineData("test-cert_rsa_2048", PdfMessageDigestType.SHA1, null)]
        //[InlineData("test-cert_rsa_2048", PdfMessageDigestType.SHA256, null)]
        //[InlineData("test-cert_rsa_2048", PdfMessageDigestType.SHA384, null)]
        //[InlineData("test-cert_rsa_2048", PdfMessageDigestType.SHA512, null)]
        //[InlineData("test-cert_rsa_3072", PdfMessageDigestType.SHA1, null)]
        //[InlineData("test-cert_rsa_3072", PdfMessageDigestType.SHA256, null)]
        //[InlineData("test-cert_rsa_3072", PdfMessageDigestType.SHA384, null)]
        //[InlineData("test-cert_rsa_3072", PdfMessageDigestType.SHA512, null)]
        //[InlineData("test-cert_rsa_4096", PdfMessageDigestType.SHA1, null)]
        //[InlineData("test-cert_rsa_4096", PdfMessageDigestType.SHA256, null)]
        //[InlineData("test-cert_rsa_4096", PdfMessageDigestType.SHA384, null)]
        //[InlineData("test-cert_rsa_4096", PdfMessageDigestType.SHA512, null)]
        public void Sign_new_file_Default_in_a_loop(string certType, PdfMessageDigestType digestType, string timestampURL)
        {
            Skip.If(SkippableTests.SkipSlowTests());

            int loops = 2;
#if DEBUG
            var version = PdfSharp.Internal.BuildInformation.BuildVersionNumber;
            loops = 100;
            // Reduce loops for slow timestamp servers.
            if (timestampURL.Contains("sect"))
                loops = 5;
#endif
            IOUtility.EnsureAssetsVersion(RequiredAssets);

            for (int loop = 1; loop <= loops; loop++)
            {

                var font = new XFont("Verdana", 10, XFontStyleEx.Regular);
                var fontHeader = new XFont("Verdana", 18, XFontStyleEx.Regular);
                using var document = new PdfDocument();
                var pdfPage = document.AddPage();
                var xGraphics = XGraphics.FromPdfPage(pdfPage);
                var layoutRectangle = new XRect(0, 72, pdfPage.Width.Point, pdfPage.Height.Point);
                xGraphics.DrawString("Document Signature Test", fontHeader, XBrushes.Black, layoutRectangle,
                    XStringFormats.TopCenter);
                var textFormatter = new XTextFormatter(xGraphics);
                layoutRectangle = new XRect(72, 144, pdfPage.Width.Point - 144, pdfPage.Height.Point - 144);

                var text = "Lorem ipsum...";
                textFormatter.DrawString(text, font, new XSolidBrush(XColor.FromKnownColor(XKnownColor.Black)),
                    layoutRectangle, XStringFormats.TopLeft);

                var pdfPosition = xGraphics.Transformer.WorldToDefaultPage(new XPoint(144, 216));
                var options = new DigitalSignatureOptions
                {
                    ContactInfo = "John Doe",
                    Location = "Seattle",
                    Reason = "License Agreement",
                    Rectangle = new XRect(pdfPosition.X, pdfPosition.Y, 200, 50),
                    AppearanceHandler = new SignatureAppearanceHandler()
                };

                Uri? timestampURI = String.IsNullOrEmpty(timestampURL) ? null : new Uri(timestampURL, UriKind.Absolute);

                var pdfSignatureHandler = DigitalSignatureHandler.ForDocument(document,
                    new PdfSharpDefaultSigner(GetCertificate(certType), digestType, timestampURI), options);

                // pdfSignatureHandler What to do with it? Set SHA-level? Set TimeStamp?

                // Save the document...
                string filename =
                    PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/signatures/DefaultSignerTest-" + certType);
                document.Save(filename);
                // ...and start a viewer.
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (loops <= 2)
                    PdfFileUtility.ShowDocumentIfDebugging(filename);
            }
        }
#endif

        [Fact]
        public void Sign_existing_file_Default()
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
                AppearanceHandler = new SignatureAppearanceHandler()
            };
            var pdfSignatureHandler = DigitalSignatureHandler.ForDocument(document, new PdfSharpDefaultSigner(GetCertificate("test-cert_rsa_1024"), PdfMessageDigestType.SHA512), options);

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/signatures/DefaultSignExistingPdfTest");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [SkippableFact]
        public void Sign_with_Certificate_from_Store()
        {
            const string certName = "empira Software GmbH";
            X509Certificate2 certificate = null!;
            var store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection cers = store.Certificates.Find(X509FindType.FindBySubjectName, certName, false);
            Skip.If(cers.Count == 0);

            if (cers.Count > 0)
            {
                certificate = cers[0];
            };

            for (int idx = 0; idx < cers.Count; idx++)
            {
                var cer = cers[idx];
                if (cer?.IssuerName.Name?.Contains("Sectigo") == true)
                {
                    certificate = cer;
                    break;
                }
            }
            store.Close();

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
                AppearanceHandler = new SignatureAppearanceHandler()
            };
            var pdfSignatureHandler = DigitalSignatureHandler.ForDocument(document, new PdfSharpDefaultSigner(certificate, PdfMessageDigestType.SHA256), options);

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/signatures/CertificateFromStore");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Theory(Skip = "errors on writing and maybe reading encrypted files with signature")]
        [ClassData(typeof(SecurityTestHelper.TestData.AllVersions))]
        [ClassData(typeof(SecurityTestHelper.TestData.AllVersionsSkipped), Skip = SecurityTestHelper.SkippedTestOptionsMessage)]
        public void Sign_and_encrypt_file_Default(SecurityTestHelper.TestOptionsEnum optionsEnum)
        {
            var testOptions = SecurityTestHelper.TestOptions.ByEnum(optionsEnum);
            testOptions.SetDefaultPasswords(true, true);
#if DEBUG
            var version = PdfSharp.Internal.BuildInformation.BuildVersionNumber;
#endif
            IOUtility.EnsureAssetsVersion(RequiredAssets);

            var font = new XFont("Verdana", 10.0, XFontStyleEx.Regular);
            using var document = new PdfDocument();
            var pdfPage = document.AddPage();
            var xGraphics = XGraphics.FromPdfPage(pdfPage);
            var layoutRectangle = new XRect(0.0, 0.0, pdfPage.Width.Point, pdfPage.Height.Point);
            xGraphics.DrawString("Signed encrypted sample document", font, XBrushes.Black, layoutRectangle, XStringFormats.TopCenter);
            var options = new DigitalSignatureOptions
            {
                ContactInfo = "John Doe",
                Location = "Seattle",
                Reason = "License Agreement",
                Rectangle = new XRect(36.0, 700.0, 400.0, 50.0),
                AppearanceHandler = new SignatureAppearanceHandler()
            };

            // Set encryption parameters.
            SecurityTestHelper.SecureDocument(document, testOptions);

            var pdfSignatureHandler = DigitalSignatureHandler.ForDocument(document, new PdfSharpDefaultSigner(GetCertificate("test-cert_rsa_1024"), PdfMessageDigestType.SHA512), options);

            // pdfSignatureHandler What to do with it? Set SHA-level? Set TimeStamp?

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/signatures/" + SecurityTestHelper.AddPrefixToFilename("DefaultSignAndEncryptPdfTest", testOptions));
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            // Read encrypted file and write it without encryption.
            var pdfDocRead = PdfReader.Open(filename, SecurityTestHelper.PasswordOwnerDefault);

            var filenameRead = PdfFileUtility.GetTempPdfFullFileName(SecurityTestHelper.AddPrefixToFilename("read DefaultSignAndEncryptedPdfTest", testOptions));
            pdfDocRead.Save(filenameRead);

            PdfFileUtility.ShowDocumentIfDebugging(filenameRead);
        }

        static X509Certificate2 GetCertificate(string certName)
        {
            var certFolder = IOUtility.GetAssetsPath("pdfsharp-6.x/signatures");
            var pfxFile = Path.Combine(certFolder ?? throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests."), $"{certName}.pfx");
            var rawData = File.ReadAllBytes(pfxFile);

            // Do not use password literals for real certificates in source code.
            var certificatePassword = "Seecrit1243";  //@@@???

            var certificate = new X509Certificate2(rawData,
                certificatePassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            return certificate;
        }

        class SignatureAppearanceHandler : IAnnotationAppearanceHandler
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
