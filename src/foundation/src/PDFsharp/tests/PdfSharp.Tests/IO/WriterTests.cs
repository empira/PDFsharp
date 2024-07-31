// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using System.IO;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Signatures;
using Xunit;
using System.Security.Cryptography.X509Certificates;

namespace PdfSharp.Tests.IO
{
    [Collection("PDFsharp")]
    public class WriterTests
    {
        [Fact]
        public void Write_import_file()
        {
            var testFile = IOUtility.GetAssetsPath("archives/samples-1.5/PDFs/SomeLayout.pdf")!;

            var filename = PdfFileUtility.GetTempPdfFileName("ImportTest");

            var doc = PdfReader.Open(testFile, PdfDocumentOpenMode.Import);

            Action save = () => doc.Save(filename);
            save.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Append_To_File()
        {
            var sourceFile = IOUtility.GetAssetsPath("archives/grammar-by-example/GBE/ReferencePDFs/WPF 1.31/Table-Layout.pdf")!;
            var targetFile = Path.Combine(Path.GetTempPath(), "AA-Append.pdf");
            File.Copy(sourceFile, targetFile, true);

            using var fs = File.Open(targetFile, FileMode.Open, FileAccess.ReadWrite);
            var doc = PdfReader.Open(fs, PdfDocumentOpenMode.Append);
            var numPages = doc.PageCount;
            var numContentsPerPage = new List<int>();
            foreach (var page in doc.Pages)
            {
                // remember count of existing contents
                numContentsPerPage.Add(page.Contents.Elements.Count);
                // add new content
                using var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawString("I was added", new XFont("Arial", 16), new XSolidBrush(XColors.Red), 40, 40);
            }

            doc.Save(fs, true);

            // verify that the new content is picked up
            var idx = 0;
            doc = PdfReader.Open(targetFile, PdfDocumentOpenMode.Import);
            doc.PageCount.Should().Be(numPages);
            foreach (var page in doc.Pages)
            {
                var c = page.Contents.Elements.Count;
                c.Should().Be(numContentsPerPage[idx] + 1);
                idx++;
            }
        }

        [Fact]
        public void Update_With_Deletion()
        {
            // create input file
            Append_To_File();

            var sourceFile = Path.Combine(Path.GetTempPath(), "AA-Append.pdf");
            var targetFile = Path.Combine(Path.GetTempPath(), "AA-Append-Delete.pdf");
            File.Copy(sourceFile, targetFile, true);

            using var fs = File.Open(targetFile, FileMode.Open, FileAccess.ReadWrite);
            var doc = PdfReader.Open(fs, PdfDocumentOpenMode.Append);
            var numPages = doc.Pages.Count;

            var firstPage = doc.Pages[0];
            // page will not be deleted, because it is referenced by other objects (Outlines)
            // delete contentes as well, so we have at least SOME "f"-entries in the new xref-table
            firstPage.Contents.Elements.Clear();
            doc.Pages.Remove(firstPage);

            doc.Save(fs, true);

            doc = PdfReader.Open(targetFile, PdfDocumentOpenMode.Import);
            doc.PageCount.Should().Be(numPages - 1);
            // new xref-table was checked manually (opened in notepad)
        }

        [Fact]
        public void Sign()
        {
            /**
             Easy way to create a self-signed certificate for testing.
             Put the following code in a file called "makecert.ps1" and execute it from PowerShell (tested with 7.4.2).
             (Adapt the variables to your liking)

             $date = Get-Date
             # mark valid for 10 years
             $date = $date.AddYears(10)
             # define some variables
             $issuedTo = "FooBar"
             $subject = "CN=" + $issuedTo
             $friendlyName = $issuedTo
             $exportFileName = $issuedTo + ".pfx"
             # create certificate and add to personal store
             $cert = New-SelfSignedCertificate -Type Custom -Subject $subject -KeyUsage DigitalSignature,NonRepudiation -KeyUsageProperty Sign -FriendlyName $friendlyName -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter $date
             # specify password for exported certificate
             $password = ConvertTo-SecureString -String "1234" -Force -AsPlainText 
             # export to current folder in pfx format
             Export-PfxCertificate -Cert $cert -FilePath $exportFileName -Password $password
             */
            var cert = new X509Certificate2(@"C:\Data\packdat.pfx", "1234");
            // sign 2 times
            for (var i = 1; i <= 2; i++)
            {
                var options = new PdfSignatureOptions
                {
                    Certificate = cert,
                    FieldName = "Signature-" + Guid.NewGuid().ToString("N"),
                    PageIndex = 0,
                    Rectangle = new XRect(120 * i, 40, 100, 60),
                    Location = "My PC",
                    Reason = "Approving Rev #" + i,
                    // Signature appearances can also consist of an image (Rectangle should be adapted to image's aspect ratio)
                    //Image = XImage.FromFile(@"C:\Data\stamp.png")
                };

                string sourceFile;
                string targetFile;
                // first signature
                if (i == 1)
                {
                    sourceFile = IOUtility.GetAssetsPath("archives/grammar-by-example/GBE/ReferencePDFs/WPF 1.31/Table-Layout.pdf")!;
                    targetFile = Path.Combine(Path.GetTempPath(), "AA-Signed.pdf");
                }
                // second signature
                else
                {
                    sourceFile = Path.Combine(Path.GetTempPath(), "AA-Signed.pdf");
                    targetFile = Path.Combine(Path.GetTempPath(), "AA-Signed-2.pdf");
                }
                File.Copy(sourceFile, targetFile, true);

                using var fs = File.Open(targetFile, FileMode.Open, FileAccess.ReadWrite);
                var signer = new PdfSigner(fs, options);
                var resultStream = signer.Sign();
                // overwrite input document
                fs.Seek(0, SeekOrigin.Begin);
                resultStream.CopyTo(fs);
            }

            using var finalDoc = PdfReader.Open(Path.Combine(Path.GetTempPath(), "AA-Signed-2.pdf"), PdfDocumentOpenMode.Modify);
            var acroForm = finalDoc.AcroForm;
            acroForm.Should().NotBeNull();
            var signatureFields = acroForm!.GetAllFields().OfType<PdfSignatureField>().ToList();
            signatureFields.Count.Should().Be(2);
        }
    }
}
