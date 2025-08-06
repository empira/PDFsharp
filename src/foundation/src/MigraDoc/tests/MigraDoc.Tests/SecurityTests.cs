// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.IO;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using PdfSharp.Pdf.Signatures;
using PdfSharp.Quality;
using PdfSharp.TestHelper;
using PdfSharp.TestHelper.Analysis.ContentStream;
using Xunit;
using static MigraDoc.Tests.Helper.SecurityTestHelper;
using static PdfSharp.TestHelper.SecurityTestHelper;

namespace MigraDoc.Tests
{
    [Collection("PDFsharp")]
    public class SecurityTests : IDisposable
    {
        public SecurityTests()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
        }
        
        public void Dispose()
        {
            GlobalFontSettings.ResetFontManagement();
        }

        // Notice: Test opening files written by these tests in Adobe Acrobat Reader after changes that may affect encryption.
        // These tests are not able to test the correct encryption of these files, even if decryption with these tests works.
        //
        // PDFs written by these tests, Acrobat Reader must and must only be able to open the files WITH THE PASSWORD(S) assigned:
        // - "* w U*" meaning files written with user password, as the user password is required to open the file.
        //   Beware, that PDFsharp actually uses the user password as owner password too, if that is not given.
        //   Otherwise, a PDF application could get owner rights, when providing an empty password.
        //   This must be kept in mind, when providing test files written with other tools for SecurityTests tests.
        //   - For "* w UO*" meaning files written with user and owner password, also the owner password must allow reading the document.
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
        //   - V5R5ReadOnly = encryption Version5 Revision 6 (proprietary and deprecated, but supported for reading) (PDF 2.0)
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
            Process.Sta/rt(new ProcessStartInfo(filename) { UseShellExecute = true });
#endif
        }

        [SkippableFact]
        public void Test_Write_NoPassword()
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var filename = AddPrefixToFilename("w.pdf");
            WriteStandardTestDocument(filename);
            OpenPdf(filename);
        }

        [SkippableFact]
        public void Test_Read_NoPassword()
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var tempFile = GetStandardTestDocument();

            var pdfDoc = PdfReader.Open(tempFile);
            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filename = AddPrefixToFilename("r.pdf");
            pdfRenderer.Save(filename);
            OpenPdf(filename);
        }

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersionsAndDefault))]
        [ClassData(typeof(TestData.AllWriteVersionsAndDefaultSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Write_UserPassword(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);
            var filename = AddPrefixToFilename("w U.pdf", options);
            WriteSecuredStandardTestDocument(filename, options);
            OpenPdf(filename);
        }

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_UserPassword_Without_Fails(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);
            var tempFile = GetSecuredStandardTestDocument(options);

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

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_UserPassword_Without_Import_Fails(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);
            var tempFile = GetSecuredStandardTestDocument(options);

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

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_UserPassword_Wrong_Fails(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);
            var tempFile = GetSecuredStandardTestDocument(options);

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

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_UserPassword(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);
            var tempFile = GetSecuredStandardTestDocument(options);

            var pdfDoc = PdfReader.Open(tempFile, options.UserPassword!);

            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filename = AddPrefixToFilename("r U.pdf", options);
            pdfRenderer.Save(filename);
            OpenPdf(filename);
        }

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Write_OwnerPassword(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(false, true);
            var filename = AddPrefixToFilename("w O.pdf", options);
            WriteSecuredStandardTestDocument(filename, options);
            OpenPdf(filename);
        }

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_OwnerPassword_Without_Fails(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(false, true);
            var tempFile = GetSecuredStandardTestDocument(options);

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

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_OwnerPassword_Without_Import(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(false, true);
            var tempFile = GetSecuredStandardTestDocument(options);

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

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_OwnerPassword_Wrong_Fails(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(false, true);
            var tempFile = GetSecuredStandardTestDocument(options);

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

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_OwnerPassword(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(false, true);
            var tempFile = GetSecuredStandardTestDocument(options);

            var pdfDoc = PdfReader.Open(tempFile, options.OwnerPassword!);

            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filename = AddPrefixToFilename("r O.pdf", options);
            pdfRenderer.Save(filename);
            OpenPdf(filename);
        }

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Write_UserAndOwnerPassword(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            var filename = AddPrefixToFilename("w UO.pdf", options);
            WriteSecuredStandardTestDocument(filename, options);
            OpenPdf(filename);
        }

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_UserAndOwnerPassword_Without_Fails(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            var tempFile = GetSecuredStandardTestDocument(options);

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

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_UserAndOwnerPassword_Wrong_Fails(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            var tempFile = GetSecuredStandardTestDocument(options);

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

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_UserAndOwnerPassword_User_Fails(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            var tempFile = GetSecuredStandardTestDocument(options);

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

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_UserAndOwnerPassword_User_Import(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            var tempFile = GetSecuredStandardTestDocument(options);

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

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        [InlineData(TestOptionsEnum.V5R5ReadOnly)]
        public void Test_Read_UserAndOwnerPassword_Owner(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true, true);
            var tempFile = GetSecuredStandardTestDocument(options);

            var pdfDoc = PdfReader.Open(tempFile, options.OwnerPassword!);

            var pdfRenderer = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filename = AddPrefixToFilename("r UO-O.pdf", options);
            pdfRenderer.Save(filename);
            OpenPdf(filename);
        }

        [SkippableTheory]
        [InlineData(TestOptionsEnum.V5)]
        // Older encryption versions don’t support these password values. Passwords for revision 4 and earlier are up to 32 characters in length.
        public void Test_Password_Long(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

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

        [SkippableTheory]
        [InlineData(TestOptionsEnum.V5)] // Explicitly test SASLprep, which is used in encryption version 5.
        // Older encryption versions don’t support these password values. Passwords for revision 4 and earlier are limited to characters in the PDFDocEncoding character set.
        public void Test_Password_Unicode(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

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

        /// <summary>
        /// Tests to embed a not encrypted file in an encrypted file.
        /// </summary>
        [SkippableTheory]
        [InlineData(TestOptionsEnum.Default)] // Testing with one encryption version should do it.
        public void Test_EmbeddedFile_WrapperEncrypted(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

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

        /// <summary>
        /// Tests to embed an encrypted file in a not encrypted file.
        /// </summary>
        [SkippableTheory]
        [InlineData(TestOptionsEnum.Default)] // Testing with one encryption version should do it.
        public void Test_EmbeddedFile_EmbEncrypted(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

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

        /// <summary>
        /// Tests to embed an encrypted file in an encrypted file.
        /// </summary>
        [SkippableTheory]
        [InlineData(TestOptionsEnum.Default)] // Testing with one encryption version should do it.
        public void Test_EmbeddedFile_BothEncrypted(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTestsUnderDotNetFramework());

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
        const string SkippedTestEmbeddedFilesMessage = "The feature to encrypt embedded file streams only is not yet correctly implemented. The resulting files may not be readyble with PDF readers.";

        [SkippableTheory]
        [ClassData(typeof(TestData.V4), Skip = SkippedTestEmbeddedFilesMessage)]
        [ClassData(typeof(TestData.V5), Skip = SkippedTestEmbeddedFilesMessage)]
#else
        /// <summary>
        /// Tests to embed a not encrypted file in an encrypted file, in which only embedded file streams shall be encrypted.
        /// </summary>
        [SkippableTheory]
        [ClassData(typeof(TestData.V4))]
        [ClassData(typeof(TestData.V5))]
        [ClassData(typeof(TestData.V4Skipped), Skip = SkippedTestOptionsMessage)]
        [ClassData(typeof(TestData.V5Skipped), Skip = SkippedTestOptionsMessage)]
#endif
        public void Test_EmbeddedFile_OnlyEmbeddedFileStreamsEncrypted(TestOptionsEnum optionsEnum)
        {
            Skip.If(true || SkippableTests.SkipSlowTestsUnderDotNetFramework());

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
            pdfDocument.SecurityHandler.EncryptEmbeddedFileStreamsOnly();
            pdfRenderer.Save(filename);

            // Read encrypted file and write it without encryption.
            var pdfDoc = PdfReader.Open(filename, PasswordOwnerDefault);
            var pdfRendererRead = new PdfDocumentRenderer { PdfDocument = pdfDoc };

            var filenameRead = AddPrefixToFilename("_EmbOnly r UO.pdf", options);
            pdfRendererRead.Save(filenameRead);
            OpenPdf(filenameRead);
        }

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Permissions(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTests());

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

#if true
        [SkippableFact]
        public void Test_Strings()
        {
            Skip.If(SkippableTests.SkipSlowTests());

            var optionsEnum = TestOptionsEnum.V5; // Encryption V5 should be sufficient, as it even crashes on wrong encrypted string length.
#else
        [SkippableTheory]
        [ClassData(typeof(TestData.AllVersions))]
        [ClassData(typeof(TestData.AllVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_Strings(TestOptionsEnum optionsEnum)
        {
            Skip.If(SkippableTests.SkipSlowTests());
#endif
            // Cache old logger factory.
            var oldLoggerFactory = LogHost.Factory;

            try
            {
                // Add ListLoggerProvider to inspect log entries later.
                var listLoggerProvider = new MemoryLoggerProvider(LogLevel.Warning);
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .AddFilter("PDFsharp", LogLevel.Warning)
                        .AddFilter(level => true)
                        .AddProvider(listLoggerProvider)
                        .AddConsole();
                });
                LogHost.Factory = loggerFactory;

                // The randomizedTestStringCount should be big enough for a good chance to create encrypted strings that seem to begin with a Unicode BOM.
                // These strings are an important test case to ensure reread as unicode is done after decryption.
                const int randomizedTestStringCount = 100000;

                // Define test strings. Avoid spaces, hyphens or other characters that may split the strings in two objects in the page content stream.
                var testStrings = new List<string>
                {
                    "1234567890",
                    "!\"§$%&/()=",
                    "abcdefghijklmnopqrstuvwxyz",
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                    "^°³{[]}\\`´@€+*~#'<>|µ,;.:_",
                    "äöüÄÖÜ",
                    "áàâéèêíìîóòôúùûÁÀÂÉÈÊÍÌÎÓÒÔÚÙÛ",
                    "😢😞💪",
                    "\ud83d\udca9\ud83d\udca9\ud83d\udca9\u2713\u2714\u2705\ud83d\udc1b\ud83d\udc4c\ud83c\udd97\ud83d\udd95",
                    "D:20240312161031+01'00'",
                    "()\\(\\)\\" // Attention: This line has an additional check below in paragraphs.
                };

                // Add lots of randomized test strings to encourage the generation of encrypted strings, that seem to begin with a Unicode BOM.
                for (var i = 0; i < randomizedTestStringCount; i++)
                    testStrings.Add(Guid.NewGuid().ToString("N"));

                const int linesPerPage = 50;

                var date = DateTime.Now;

                var options = TestOptions.ByEnum(optionsEnum);
                options.SetDefaultPasswords(true);

                var doc = CreateEmptyTestDocument();

                var font = new XFont("Segoe UI Emoji", 10);

                var normalStyle = doc.Styles.Normal;
                normalStyle.ParagraphFormat.TabStops.AddTabStop(Unit.FromCentimeter(2));
                normalStyle.Font.Name = font.Name2;
                normalStyle.Font.Size = font.Size;

                var section = doc.AddSection();

                // Add all test strings as paragraphs.
                var pageLineCount = 0;
                for (var i = 0; i < testStrings.Count; i++)
                {
                    var testString = testStrings[i];

                    var p = section.AddParagraph(i + ".");
                    p.AddTab();
                    p.AddText(testString);

                    pageLineCount++;
                    if (pageLineCount > linesPerPage)
                    {
                        pageLineCount = 0;
                        section.AddPageBreak();
                    }
                }

                // Render the document.
                var pdfRenderer = new PdfDocumentRenderer { Document = doc };
                pdfRenderer.RenderDocument();

                var pdfDocument = pdfRenderer.PdfDocument;

                // Add all test strings to CustomValues dictionary. This ensures Lexer scanning is also tested with the test strings.
                var dict = pdfDocument.CustomValues;
                for (var i = 0; i < testStrings.Count; i++)
                {
                    var testString = testStrings[i];
                    var key = "/text" + i;
                    dict!.Elements.Add(key, new PdfString(testString));
                }

                // Secure and save the document.
                SecureDocument(pdfDocument, options);
                var encryptedFile = AddPrefixToFilename("Test_Strings w U.pdf", options);
                pdfRenderer.Save(encryptedFile);

                // Open the saved file
                pdfDocument = PdfReader.Open(encryptedFile, PasswordUserDefault);

                // Ensure entries in DocumentInformation have not changed.
                var documentInfo = pdfDocument.Info;
                // We don’t know the saved CreationDate exactly, but it should be less than 5 seconds ago.
                (documentInfo.CreationDate - date).TotalMilliseconds.Should().BeLessThan(5000, "PDF CreationDate should be less than 5 seconds ago.");
                documentInfo.Creator.Should().Be(MigraDocProductVersionInformation.Creator, "PDF Creator should match");
                //documentInfo.Producer.Should().Be($"{PdfSharpProductVersionInformation.Creator} under {RuntimeInformation.OSDescription}", "PDF Producer should match");
                documentInfo.Producer.Should().StartWith(PdfSharpProductVersionInformation.Creator, "PDF Producer should match");


                // Analyze the drawn text in the PDF’s content stream.
                pageLineCount = 0;
                var pageIdx = 0;
                ContentStreamEnumerator? streamEnumerator = null;

                // Check the test strings of the paragraphs.
                for (var i = 0; i < testStrings.Count; i++)
                {
                    var testString = testStrings[i];

                    if (pageLineCount == 0)
                        streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(pdfDocument, pageIdx);

                    streamEnumerator!.Text.MoveAndGetNext(true, out var textInfo).Should().BeTrue();
                    textInfo!.Text.Should().Be(i + ".");

                    streamEnumerator.Text.MoveAndGetNext(true, out textInfo).Should().BeTrue();
                    textInfo!.TextEquals(testString, font, out var encodedText).Should().BeTrue($"encoded test string {i} (\"{encodedText}\") should match in paragraphs");

                    if (i == 10)
                        textInfo.Text.Should().Be(@"\(\)\\\(\\\)\\", "test string {i} should match in paragraphs");

                    pageLineCount++;
                    if (pageLineCount > linesPerPage)
                    {
                        streamEnumerator.Text.MoveAndGetNext(true, out _).Should().BeFalse();
                        pageLineCount = 0;
                        pageIdx++;
                    }
                }
                streamEnumerator!.Text.MoveAndGetNext(true, out _).Should().BeFalse();

                // Check the test strings saved in CustomValues dictionary.
                dict = pdfDocument.CustomValues;
                for (var i = 0; i < testStrings.Count; i++)
                {
                    var testString = testStrings[i];

                    var value = dict!.Elements["/text" + i];
                    value.Should().NotBeNull();
                    var pdfString = value as PdfString;
                    pdfString.Should().NotBeNull();
                    pdfString!.Value.Should().Be(testString, $"test string {i} should match in CustomValues dictionary");
                }

                // Inspect log entries for not containing cryptographic exception entries.
                var pdfSharpLogger = listLoggerProvider.GetLogger("PDFsharp");
                pdfSharpLogger.Should().NotBeNull();

                foreach (var logEntry in pdfSharpLogger!.GetLogEntries())
                    logEntry.Message.Should().NotContain("A cryptographic exception occurred");

            }
            finally
            {
                // Restore old logger factory to not disturb other tests.
                LogHost.Factory = oldLoggerFactory;
            }
        }


        [Fact]
        public void Test_Hyperlink()
        {
            // Create a MigraDoc document.
            var document = CreateDocument();
            // Associate the MigraDoc document with a renderer.
            var pdfRenderer = new PdfDocumentRenderer
            {
                Document = document,
                PdfDocument = new PdfDocument
                {
                    PageLayout = PdfPageLayout.SinglePage
                }
            };
            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();
            // Set security settings directly on the PDF document
            var securitySettings = pdfRenderer.PdfDocument.SecuritySettings;
            securitySettings.OwnerPassword = "Secret";
            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFullFileName("HyperlinkWithEncryptionTest");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            // Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
            // Creates minimalistic document with hyperlink.
            static Document CreateDocument()
            {
                // Create a new MigraDoc document.
                var document = new Document();
                // Add a section to the document.
                var section = document.AddSection();
                // Add a paragraph to the section.
                var paragraph = section.AddParagraph();
                // Add a hyperlink to a web URL to the paragraph.
                var hyperlink = paragraph.AddHyperlink("https://docs.pdfsharp.net", HyperlinkType.Url);
                hyperlink.AddText("link");
                return document;
            }
        }

        [SkippableTheory]
        [ClassData(typeof(TestData.AllWriteVersions))]
        [ClassData(typeof(TestData.AllWriteVersionsSkipped), Skip = SkippedTestOptionsMessage)]
        public void Test_SignedDocument(TestOptionsEnum optionsEnum)
        {
            var options = TestOptions.ByEnum(optionsEnum);
            options.SetDefaultPasswords(true);

            var filename = AddPrefixToFilename("SigningWithEncryptionTest.pdf", options);
            
            var document = CreateDocument();
            SecureDocument(document, options);

            // Save the document.
            document.Save(filename);


            // Creates minimalistic document with hyperlink.
            static PdfDocument CreateDocument()
            {
                const int requiredAssets = 1014;
                string? timestampURL = null;

                var certType = "test-cert_rsa_1024";
                var digestType = PdfMessageDigestType.SHA256;

                IOUtility.EnsureAssetsVersion(requiredAssets);

                var font = new XFont("Verdana", 10, XFontStyleEx.Regular);
                var fontHeader = new XFont("Verdana", 18, XFontStyleEx.Regular);
                var document = new PdfDocument();
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

                return document;
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
        }
    }
}