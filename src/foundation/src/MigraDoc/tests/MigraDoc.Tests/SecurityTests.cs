// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Linq;
using FluentAssertions;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using Xunit;
using static MigraDoc.Tests.SecurityTestHelper;

namespace MigraDoc.Tests
{
    [Collection("MGD")]
    public class SecurityTests
    {
        // Notice: Test opening files written by these tests in Adobe Acrobat Reader after changes that may affect encryption.
        // These tests are not able to test the correct encryption of these files, even if decryption with these tests works.
        //
        // PDFs written by these tests, Acrobat Reader must and must only be able to open the files WITH THE PASSWORD(S) assigned:
        // - "* w U*" meaning files written with user password, as the user password is required to open the file.
        //   - For "* w UO*" meaning files written with user and owner password, also the owner password must enable reading the document.
        // - "* Perm*" meaning files written by Permissions test, as the user password is required.
        //
        // PDFs written by these tests, Acrobat Reader must be able to open the files WITHOUT ANY PASSWORD(S) assigned:
        // - "* w O*" meaning files written with owner password only, as the owner password is not required to read the document.
        // - "* r *" meaning files written without encryption after reading encrypted files, as no password is needed.
        //
        // Default user/owner passwords are defined in constants in SecurityTestHelper.
        // For the following files other passwords are used:
        // - "*-long", see Test_Password_Long()
        // - "*-unic", see Test_Password_Unicode()
        //
        // PDF filename scheme:
        // All files start with "S_" followed by...
        // - Encryption info
        //   - _No = no encryption
        //   - Def = default encryption
        //   - V1 = encryption Version 1, RC4 with 40 bit key length
        //   - V2_40B = encryption Version 2, RC4 with 40 bit key length (PDF 1.4)
        //   - V2_128B = encryption Version 2, RC4 with 128 bit key length (PDF 1.4)
        //   - V4_RC4 = encryption Version 4, RC4 with 128 bit key length (PDF 1.5)
        //   - V4_AES = encryption Version 4, AES with 128 bit key length (PDF 1.6)
        //   - V5 = encryption Version 5, AES with 256 bit key length (PDF 2.0)
        // - Optionally additional encryption info
        //   - _XMeta = do not encrypt Metadata dictionary, by assigning Identity crypt filter to it. Valid for Version 4 and 5.
        // - Test info
        //   - w = file written by writing test
        //   - r = file written by reading test (this file is written without encryption after reading a file with the given encryption)
        //   - U = user password set
        //   - O = owner password set
        //   - -U = user password used for reading
        //   - -O = owner password used for reading
        //   - -X = no password used for reading
        //   - _Imp = PdfDocumentOpenMode.Import used for reading
        //   - -long = passwords with a length of 256 chars were used
        //   - -unic = passwords with special Unicode characters were used
        //   - _EmbEmb = not encrypted file containing an encrypted embedded file
        //   - _EmbWrap = encrypted file containing a not encrypted embedded file
        //   - _EmbBoth = encrypted file containing an encrypted embedded file
        //   - _EmbOnly = encrypted file with EncryptEmbeddedFilesOnly containing a not encrypted embedded file
        //   - Perm{X} = Permissions test

        // ReSharper disable once UnusedParameter.Local
        static void OpenPdf(string filename)
        {
#if true_ // Should be "true_" by default and in source control. Change to "true" to enable automatic PDF starting for testing purposes.
            Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
#endif
        }

        [Fact]
        public void Test_Write_NoPassword()
        {
            var filename = AddPrefixToFilename("w.pdf");
            WriteStandardTestDocument(filename);
            OpenPdf(filename);
        }

        [Fact]
        public void Test_Read_NoPassword()
        {
            const string tempFile = "temp.pdf";
            WriteStandardTestDocument(tempFile);

            var pdfDoc = PdfReader.Open(tempFile);
            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filename = AddPrefixToFilename("r.pdf");
            pdfRenderer.Save(filename);
            OpenPdf(filename);
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersionsAndDefault))]
        [ClassData(typeof(TestData.AllVersionsAndDefaultSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Write_UserPassword(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);
            var filename = AddPrefixToFilename("w U.pdf", options);
            WriteSecuredStandardTestDocument(filename, options);
            OpenPdf(filename);
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_UserPassword_Without_Fails(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            Exception? e = null;
            try
            {
                // ReSharper disable once UnusedVariable
                var pdfDoc = PdfReader.Open(tempFile);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            e.Should().BeOfType(typeof(PdfReaderException));
            e!.Message.Should().Contain("password is required");
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_UserPassword_Without_Import_Fails(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            Exception? e = null;
            try
            {
                // ReSharper disable once UnusedVariable
                // Without the user password also import should not work.
                var pdfDocImport = PdfReader.Open(tempFile, PdfDocumentOpenMode.Import);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            e.Should().BeOfType(typeof(PdfReaderException));
            e!.Message.Should().Contain("password is required");

        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_UserPassword_Wrong_Fails(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            Exception? e = null;
            try
            {
                // ReSharper disable once UnusedVariable
                var pdfDoc = PdfReader.Open(tempFile, PasswordWrong);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            e.Should().BeOfType(typeof(PdfReaderException));
            e!.Message.Should().Contain("password is invalid");
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_UserPassword(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            var pdfDoc = PdfReader.Open(tempFile, PasswordUserDefault);

            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filename = AddPrefixToFilename("r U.pdf", options);
            pdfRenderer.Save(filename);
            OpenPdf(filename);
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Write_OwnerPassword(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(false, true);
            var filename = AddPrefixToFilename("w O.pdf", options);
            WriteSecuredStandardTestDocument(filename, options);
            OpenPdf(filename);
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_OwnerPassword_Without_Fails(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(false, true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            Exception? e = null;
            try
            {
                // ReSharper disable once UnusedVariable
                // Default OpenMode in PdfReader.Open() is Modify, but without owner password modification is not allowed. So open should fail.
                var pdfDoc = PdfReader.Open(tempFile);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            e.Should().BeOfType(typeof(PdfReaderException));
            e!.Message.Should().Contain("password is required");
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_OwnerPassword_Without_Import(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(false, true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            // Default OpenMode in PdfReader.Open() is Modify, but with user password modification is not allowed. So we use OpenMode Import and import the pages instead.
            var pdfDocImport = PdfReader.Open(tempFile, PdfDocumentOpenMode.Import);
            var pdfDoc = new PdfDocument();
            foreach (var page in pdfDocImport.Pages)
                pdfDoc.AddPage(page);

            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filename = AddPrefixToFilename("r O-X_Imp.pdf", options);
            pdfRenderer.Save(filename);
            OpenPdf(filename);
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_OwnerPassword_Wrong_Fails(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(false, true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            Exception? e = null;
            try
            {
                // ReSharper disable once UnusedVariable
                var pdfDoc = PdfReader.Open(tempFile, PasswordWrong);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            e.Should().BeOfType(typeof(PdfReaderException));
            e!.Message.Should().Contain("password is invalid");
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_OwnerPassword(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(false, true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            var pdfDoc = PdfReader.Open(tempFile, PasswordOwnerDefault);

            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filename = AddPrefixToFilename("r O.pdf", options);
            pdfRenderer.Save(filename);
            OpenPdf(filename);
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Write_UserOwnerPassword(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            var filename = AddPrefixToFilename("w UO.pdf", options);
            WriteSecuredStandardTestDocument(filename, options);
            OpenPdf(filename);
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_UserAndOwnerPassword_Without_Fails(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            Exception? e = null;
            try
            {
                // ReSharper disable once UnusedVariable
                var pdfDoc = PdfReader.Open(tempFile);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            e.Should().BeOfType(typeof(PdfReaderException));
            e!.Message.Should().Contain("password is required");
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_UserAndOwnerPassword_Wrong_Fails(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            Exception? e = null;
            try
            {
                // ReSharper disable once UnusedVariable
                var pdfDoc = PdfReader.Open(tempFile, PasswordWrong);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            e.Should().BeOfType(typeof(PdfReaderException));
            e!.Message.Should().Contain("password is invalid");
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_UserAndOwnerPassword_User_Fails(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            Exception? e = null;
            try
            {
                // ReSharper disable once UnusedVariable
                // Default OpenMode in PdfReader.Open() is Modify, but with user password modification is not allowed. So open should fail.

                var pdfDoc = PdfReader.Open(tempFile, PasswordUserDefault);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            e.Should().BeOfType(typeof(PdfReaderException));
            e!.Message.Should().Contain("modify");
            e.Message.Should().Contain("owner password is required");
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_UserAndOwnerPassword_User_Import(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            // Default OpenMode in PdfReader.Open() is Modify, but with user password modification is not allowed. So we use OpenMode Import and import the pages instead.
            var pdfDocImport = PdfReader.Open(tempFile, PasswordUserDefault, PdfDocumentOpenMode.Import);
            var pdfDoc = new PdfDocument();
            foreach (var page in pdfDocImport.Pages)
                pdfDoc.AddPage(page);

            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filename = AddPrefixToFilename("r UO-U_Imp.pdf", options);
            pdfRenderer.Save(filename);
            OpenPdf(filename);
        }

        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Read_UserAndOwnerPassword_Owner(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            const string tempFile = "temp.pdf";
            WriteSecuredStandardTestDocument(tempFile, options);

            var pdfDoc = PdfReader.Open(tempFile, PasswordOwnerDefault);

            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filename = AddPrefixToFilename("r UO-O.pdf", options);
            pdfRenderer.Save(filename);
            OpenPdf(filename);
        }

        [Theory]
        [InlineData(TestOptionsEnum.V5)]
        // Older encryption versions don't support these password values. Passwords for revision 4 and earlier are up to 32 characters in length.
        public void Test_Password_Long(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            const string userPassword =
            "User56789a123456789b123456789c123456789d123456789e123456789f123456789g123456789h123456789i123456789j123456789k123456789l123456789m123456789n123456789o123456789p123456789q123456789r123456789s123456789t123456789u123456789v123456789w123456789x123456789y123456";
            const string ownerPassword =
                "Owner6789a123456789b123456789c123456789d123456789e123456789f123456789g123456789h123456789i123456789j123456789k123456789l123456789m123456789n123456789o123456789p123456789q123456789r123456789s123456789t123456789u123456789v123456789w123456789x123456789y123456";
            options.SetPasswords(userPassword, ownerPassword);

            // Write encrypted file.
            var filename = AddPrefixToFilename("w UO-long.pdf", options);
            WriteSecuredStandardTestDocument(filename, options);
            OpenPdf(filename);

            // Read encrypted file and write it without encryption.
            var pdfDoc = PdfReader.Open(filename, ownerPassword);

            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filenameRead = AddPrefixToFilename("r UO-long.pdf", options);
            pdfRenderer.Save(filenameRead);
            OpenPdf(filenameRead);
        }

        [Theory]
        [InlineData(TestOptionsEnum.V5)] // Explicitly test SASLprep, which is used in encryption version 5.
        // Older encryption versions don't support these password values. Passwords for revision 4 and earlier are limited to characters in the PDFDocEncoding character set.
        public void Test_Password_Unicode(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);

            // => "uḕﬃz"
            const string userPassword = "u" +
                                        "\u0113\u0300" + // Small E-macron and grave, normalized to small E-macron-grave.
                                        "\uFB03" + // Latin small ligature Ffi, mapped to "ffi"
                                        "z";

            // => "ガ"
            const string ownerPassword = "\u30AB\u3099"; // Japanese 'ka' and combining voiced sound mark, mapped to 'ga'.

            options.SetPasswords(userPassword, ownerPassword);

            // Write encrypted file.
            var filename = AddPrefixToFilename("w UO-unic.pdf", options);
            WriteSecuredStandardTestDocument(filename, options);
            OpenPdf(filename);

            // Read encrypted file and write it without encryption.
            var pdfDoc = PdfReader.Open(filename, ownerPassword);

            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filenameRead = AddPrefixToFilename("r UO-unic.pdf", options);
            pdfRenderer.Save(filenameRead);
            OpenPdf(filenameRead);
        }

        [Theory]
        [InlineData(TestOptionsEnum.Default)] // Testing with one encryption version should do it.
        public void Test_EmbeddedFile_WrapperEncrypted(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);

            // Write file to embed.
            var filenameEmbedded = "temp.pdf";
            var documentEmbedded = CreateEmptyTestDocument();

            var sectionEmbedded = documentEmbedded.AddSection();
            var paragraphEmbedded = sectionEmbedded.AddParagraph();
            paragraphEmbedded.AddText("Not encrypted embedded file.");
            var pdfRendererEmbedded = RenderDocument(documentEmbedded);

            pdfRendererEmbedded.Save(filenameEmbedded);

            // Write file containing embedded file with only this embedded file stream encrypted.
            var filename = AddPrefixToFilename("_EmbWrap w UO.pdf", options);
            var referenceNameEmbedded = "referenceEmbedded";
            var document = CreateEmptyTestDocument();
            document.AddEmbeddedFile(referenceNameEmbedded, filenameEmbedded);

            var section = document.AddSection();
            section.AddParagraph().AddText("Encrypted file containing not encrypted embedded file.");
            var paragraph = section.AddParagraph();
            var link = paragraph.AddHyperlinkToEmbeddedDocument(referenceNameEmbedded + '\\');
            link.AddText("Link to embedded file");

            var pdfRenderer = RenderSecuredDocument(document, options);
            pdfRenderer.Save(filename);

            // Read encrypted file and write it without encryption.
            var pdfDoc = PdfReader.Open(filename, PasswordOwnerDefault);
            var pdfRendererRead = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filenameRead = AddPrefixToFilename("_EmbWrap r UO.pdf", options);
            pdfRendererRead.Save(filenameRead);
            OpenPdf(filenameRead);
        }

        [Theory]
        [InlineData(TestOptionsEnum.Default)] // Testing with one encryption version should do it.
        public void Test_EmbeddedFile_EmbEncrypted(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);

            // Write file to embed.
            var filenameEmbedded = "temp.pdf";
            var documentEmbedded = CreateEmptyTestDocument();

            var sectionEmbedded = documentEmbedded.AddSection();
            var paragraphEmbedded = sectionEmbedded.AddParagraph();
            paragraphEmbedded.AddText("Encrypted embedded file.");
            var pdfRendererEmbedded = RenderSecuredDocument(documentEmbedded, options);

            pdfRendererEmbedded.Save(filenameEmbedded);

            // Write file containing embedded file with only this embedded file stream encrypted.
            var filename = AddPrefixToFilename("_EmbEmb w UO.pdf", options);
            var referenceNameEmbedded = "referenceEmbedded";
            var document = CreateEmptyTestDocument();
            document.AddEmbeddedFile(referenceNameEmbedded, filenameEmbedded);

            var section = document.AddSection();
            section.AddParagraph().AddText("Not encrypted file containing encrypted embedded file.");
            var paragraph = section.AddParagraph();
            var link = paragraph.AddHyperlinkToEmbeddedDocument(referenceNameEmbedded + '\\');
            link.AddText("Link to embedded file");

            var pdfRenderer = RenderDocument(document);
            pdfRenderer.Save(filename);

            // Read encrypted file and write it without encryption.
            var pdfDoc = PdfReader.Open(filename, PasswordOwnerDefault);
            var pdfRendererRead = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filenameRead = AddPrefixToFilename("_EmbEmb r UO.pdf", options);
            pdfRendererRead.Save(filenameRead);
            OpenPdf(filenameRead);
        }

        [Theory]
        [InlineData(TestOptionsEnum.Default)] // Testing with one encryption version should do it.
        public void Test_EmbeddedFile_BothEncrypted(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);

            // Write file to embed.
            var filenameEmbedded = "temp.pdf";
            var documentEmbedded = CreateEmptyTestDocument();

            var sectionEmbedded = documentEmbedded.AddSection();
            var paragraphEmbedded = sectionEmbedded.AddParagraph();
            paragraphEmbedded.AddText("Encrypted embedded file.");
            var pdfRendererEmbedded = RenderSecuredDocument(documentEmbedded, options);

            pdfRendererEmbedded.Save(filenameEmbedded);

            // Write file containing embedded file with only this embedded file stream encrypted.
            var filename = AddPrefixToFilename("_EmbBoth w UO.pdf", options);
            var referenceNameEmbedded = "referenceEmbedded";
            var document = CreateEmptyTestDocument();
            document.AddEmbeddedFile(referenceNameEmbedded, filenameEmbedded);

            var section = document.AddSection();
            section.AddParagraph().AddText("Encrypted file containing encrypted embedded file.");
            var paragraph = section.AddParagraph();
            var link = paragraph.AddHyperlinkToEmbeddedDocument(referenceNameEmbedded + '\\');
            link.AddText("Link to embedded file");

            var pdfRenderer = RenderSecuredDocument(document, options);
            pdfRenderer.Save(filename);

            // Read encrypted file and write it without encryption.
            var pdfDoc = PdfReader.Open(filename, PasswordOwnerDefault);
            var pdfRendererRead = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filenameRead = AddPrefixToFilename("_EmbBoth r UO.pdf", options);
            pdfRendererRead.Save(filenameRead);
            OpenPdf(filenameRead);
        }
#if true
        const string SkippedTestEmbeddedFilesMessage = "The feature to encrypt embedded files only is not yet correctly implemented. The resulting files may not be readyble with PDF readers.";

        [Theory]
        [ClassData(typeof(TestData.V4), Skip = SkippedTestEmbeddedFilesMessage)]
        [ClassData(typeof(TestData.V5), Skip = SkippedTestEmbeddedFilesMessage)]
#else
        [Theory]
        [ClassData(typeof(TestData.V4))]
        [ClassData(typeof(TestData.V5))]
        [ClassData(typeof(TestData.V4Skipped), Skip = SkippedTestOptionsMessage)]
        [ClassData(typeof(TestData.V5Skipped), Skip = SkippedTestOptionsMessage)]
#endif
        public void Test_EmbeddedFile_OnlyEFEncrypted(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);

            // Write file to embed.
            var filenameEmbedded = "temp.pdf";
            var documentEmbedded = CreateEmptyTestDocument();

            var sectionEmbedded = documentEmbedded.AddSection();
            var paragraphEmbedded = sectionEmbedded.AddParagraph();
            paragraphEmbedded.AddText("Not encrypted embedded file.");
            var pdfRendererEmbedded = RenderDocument(documentEmbedded);

            pdfRendererEmbedded.Save(filenameEmbedded);

            // Write file containing embedded file with only this embedded file stream encrypted.
            var filename = AddPrefixToFilename("_EmbOnly w UO.pdf", options);
            var referenceNameEmbedded = "referenceEmbedded";
            var document = CreateEmptyTestDocument();
            document.AddEmbeddedFile(referenceNameEmbedded, filenameEmbedded);

            var section = document.AddSection();
            section.AddParagraph().AddText("Encrypted file with only EF encrypted containing not encrypted embedded file.");
            var paragraph = section.AddParagraph();
            var link = paragraph.AddHyperlinkToEmbeddedDocument(referenceNameEmbedded + '\\');
            link.AddText("Link to embedded file");

            var pdfRenderer = RenderSecuredDocument(document, options);
            var pdfDocument = pdfRenderer.PdfDocument;
            pdfDocument.SecurityHandler.EncryptEmbeddedFilesOnly();
            pdfRenderer.Save(filename);

            // Read encrypted file and write it without encryption.
            var pdfDoc = PdfReader.Open(filename, PasswordOwnerDefault);
            var pdfRendererRead = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filenameRead = AddPrefixToFilename("_EmbOnly r UO.pdf", options);
            pdfRendererRead.Save(filenameRead);
            OpenPdf(filenameRead);
        }
        
        [Theory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Permissions(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);
            // Get all permission properties of PdfSecuritySettings by filtering with type bool and Name.StartsWith("Permit").
            var properties = typeof(PdfSecuritySettings).GetProperties();
            var permissionProperties = properties.Where(x => x.PropertyType == typeof(bool) && x.Name.StartsWith("Permit")).OrderBy(x => x.Name).ToList();
            var count = permissionProperties.Count;

            // Generates and checks test several test documents with each document containing one more permission property set to false (from none to all properties).
            for (var falseCount = 0; falseCount <= count; falseCount++)
            {
                var filename = AddPrefixToFilename($"Perm{falseCount}.pdf", options);

                var pdfRenderer = RenderSecuredStandardTestDocument(options);
                var pdfDoc = pdfRenderer.PdfDocument;

                // For this document set all properties from 0 to falseCount to false.
                for (var i = 0; i < falseCount; i++)
                {
                    var permissionProperty = permissionProperties[i];
                    permissionProperty.SetValue(pdfDoc.SecuritySettings, false);
                }

                pdfRenderer.Save(filename);

                var pdfDocRead = PdfReader.Open(filename, PasswordUserDefault);

                // All properties from 0 to falseCount should be false.
                for (var i = 0; i < falseCount; i++)
                {
                    var permissionProperty = permissionProperties[i];
                    permissionProperty.GetValue(pdfDocRead.SecuritySettings).Should().Be(false, $"{permissionProperty.Name} was set to 'false'.");
                }
                // All other properties (from falseCount to count) should be true.
                for (var i = falseCount; i < count; i++)
                {
                    var permissionProperty = permissionProperties[i];
                    permissionProperty.GetValue(pdfDocRead.SecuritySettings).Should().Be(true, $"{permissionProperty.Name} was not set to 'false'.");
                }
            }
        }
    }
}