// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Forms;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Signatures;
#if CORE
using PdfSharp.Fonts;
using PdfSharp.Quality;
#endif
using Xunit;

namespace PdfSharp.Tests.Pdf.Signatures
{
    [Collection("PDFsharp")]
    public class SignatureFieldNameTests : IDisposable
    {
        public SignatureFieldNameTests()
        {
            PdfSharpCore.ResetAll();
#if CORE
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

        [Fact]
        public void Signature_field_of_a_new_document_is_named_Signature1()
        {
            using var document = new PdfDocument();
            document.AddPage();

            var signedBytes = Sign(document);

            FieldNamesOf(signedBytes).Should().Equal("Signature1");
        }

        [Fact]
        public void Signature_field_of_an_already_signed_document_gets_an_unused_name()
        {
            using var document = new PdfDocument();
            document.AddPage();
            var signedBytes = Sign(document);

            // Sign the signed document again. The fully qualified names of interactive form fields must be
            // unique, so the second field must not be named "Signature1" again.
            using var signedDocument = PdfReader.Open(new MemoryStream(signedBytes), PdfDocumentOpenMode.Modify);
            var twiceSignedBytes = Sign(signedDocument);

            FieldNamesOf(twiceSignedBytes).Should().Equal("Signature1", "Signature2");
        }

        [Fact]
        public void Signature_field_does_not_take_the_name_of_an_existing_field()
        {
            using var document = new PdfDocument();
            document.AddPage();

            // A text field named "Signature1" already occupies the name the signature field would get.
            var acroForm = document.Catalog.GetOrCreateAcroForm();
            var textField = new PdfDictionary(document);
            textField.Elements.SetName(PdfFormField.Keys.FT, PdfFormFieldType.Text);
            textField.Elements.SetString(PdfFormField.Keys.T, "Signature1");
            document.Internals.AddObject(textField);
            acroForm.Fields.Elements.Add(textField);

            var signedBytes = Sign(document);

            FieldNamesOf(signedBytes).Should().Equal("Signature1", "Signature2");
        }

        static byte[] Sign(PdfDocument document)
        {
            var options = new DigitalSignatureOptions
            {
                ContactInfo = "John Doe",
                Location = "Seattle",
                Reason = "License Agreement",
                Rectangle = new XRect(36, 36, 200, 50),
                AppearanceHandler = new EmptyAppearanceHandler()
            };
            _ = DigitalSignatureHandler.ForDocument(document, new TestSigner(), options);

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }

        /// <summary>
        /// Gets the partial names of the fields at the root of the interactive form.
        /// </summary>
        internal static List<string> FieldNamesOf(byte[] pdfBytes)
        {
            using var document = PdfReader.Open(new MemoryStream(pdfBytes), PdfDocumentOpenMode.Import);
            var fields = document.Catalog.GetAcroForm()?.Elements.GetArray(PdfForm.Keys.Fields);
            fields.Should().NotBeNull();

            var names = new List<string>();
            for (int idx = 0; idx < fields!.Elements.Count; idx++)
                names.Add(fields.Elements.GetDictionary(idx)!.Elements.GetString(PdfFormField.Keys.T));
            return names;
        }
    }
}
