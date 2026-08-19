// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Globalization;
#if WPF
using System.IO;
#endif
using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Forms;
using PdfSharp.Pdf.Signatures;
#if CORE
using PdfSharp.Fonts;
using PdfSharp.Quality;
#endif
using Xunit;

namespace PdfSharp.Tests.IO
{
    [Collection("PDFsharp")]
    public class IncrementalUpdateTests : IDisposable
    {
        public IncrementalUpdateTests()
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
        public void Save_incremental_writes_the_original_file_unchanged()
        {
            var originalBytes = CreateDocument(2);

            using var document = PdfReader.Open(new MemoryStream(originalBytes), PdfDocumentOpenMode.ModifyIncremental);
            document.AddPage();
            document.MarkAsModified(document.Pages);

            var updatedBytes = SaveIncremental(document);

            updatedBytes.Length.Should().BeGreaterThan(originalBytes.Length);
            updatedBytes.Take(originalBytes.Length).Should().Equal(originalBytes,
                "an incremental update must not touch a single byte of the original file");

            using var updatedDocument = PdfReader.Open(new MemoryStream(updatedBytes), PdfDocumentOpenMode.Import);
            updatedDocument.PageCount.Should().Be(3);
        }

        [Fact]
        public void Save_incremental_chains_the_cross_reference_sections()
        {
            var originalBytes = CreateDocument(1);
            var originalStartxref = LastStartxrefOf(originalBytes);

            using var document = PdfReader.Open(new MemoryStream(originalBytes), PdfDocumentOpenMode.ModifyIncremental);
            document.Info.Title = "Incremental update";
            document.MarkAsModified(document.Info);

            var updatedBytes = SaveIncremental(document);
            var updatedText = TextOf(updatedBytes);

            // The trailer of the update refers to the cross-reference section of the original file…
            updatedText.Should().Contain($"/Prev {originalStartxref}");
            // … and the last startxref refers to the cross-reference section of the update.
            var updatedStartxref = LastStartxrefOf(updatedBytes);
            updatedStartxref.Should().BeGreaterThan(originalBytes.Length);
            TextOf(updatedBytes).Substring((int)updatedStartxref, 4).Should().Be("xref");

            using var updatedDocument = PdfReader.Open(new MemoryStream(updatedBytes), PdfDocumentOpenMode.Import);
            updatedDocument.Info.Title.Should().Be("Incremental update");
        }

        [Fact]
        public void Open_mode_ModifyIncremental_preserves_the_object_numbers()
        {
            // Create a document with an object that Modify would remove and objects that Modify would renumber.
            var originalBytes = CreateDocument(3);
            var originalRoot = RootObjectNumberOf(originalBytes);

            using var incremental = PdfReader.Open(new MemoryStream(originalBytes), PdfDocumentOpenMode.ModifyIncremental);
            incremental.Catalog.RequiredReference.ObjectNumber.Should().Be(originalRoot,
                "ModifyIncremental must not renumber the objects of the original file");
            incremental.IsReadOnly.Should().BeFalse();

            // The objects written by an incremental update get object numbers behind the ones of the original file.
            var maxObjectNumber = incremental.Internals.GetAllObjects().Max(obj => obj.ObjectNumber);
            incremental.AddPage();
            incremental.Internals.GetAllObjects().Max(obj => obj.ObjectNumber).Should().BeGreaterThan(maxObjectNumber);
        }

        [Fact]
        public void Save_incremental_requires_open_mode_ModifyIncremental()
        {
            var originalBytes = CreateDocument(1);

            using var document = PdfReader.Open(new MemoryStream(originalBytes), PdfDocumentOpenMode.Modify);
            document.Info.Title = "No incremental update";

            Action save = () => document.SaveIncremental(new MemoryStream());
            save.Should().Throw<InvalidOperationException>().WithMessage("*ModifyIncremental*");
        }

        [Fact]
        public void Save_incremental_rejects_an_encrypted_document()
        {
            const string password = "Seecrit1243";
            using var stream = new MemoryStream();
            using (var newDocument = new PdfDocument())
            {
                newDocument.AddPage();
                newDocument.SecuritySettings.UserPassword = password;
                newDocument.Save(stream, false);
            }

            using var document = PdfReader.Open(new MemoryStream(stream.ToArray()), password,
                PdfDocumentOpenMode.ModifyIncremental);
            document.Info.Title = "Encrypted";
            document.MarkAsModified(document.Info);

            Action save = () => document.SaveIncremental(new MemoryStream());
            save.Should().Throw<NotSupportedException>().WithMessage("An encrypted document*");
        }

        [Fact]
        public void Save_incremental_rejects_the_removal_of_an_object()
        {
            var originalBytes = CreateDocument(2);

            using var document = PdfReader.Open(new MemoryStream(originalBytes), PdfDocumentOpenMode.ModifyIncremental);
            document.Internals.RemoveObject(document.Pages[1]);

            // Removing an object would require a free entry in the cross-reference section of the update.
            Action save = () => document.SaveIncremental(new MemoryStream());
            save.Should().Throw<NotSupportedException>().WithMessage("*Removing an object*");
        }

        [Fact]
        public void Save_incremental_rejects_a_document_with_a_cross_reference_stream()
        {
            var originalBytes = CreateDocumentWithCrossReferenceStream();

            using var document = PdfReader.Open(new MemoryStream(originalBytes), PdfDocumentOpenMode.ModifyIncremental);

            // PDFsharp writes a cross-reference table, which must not be chained to a cross-reference stream.
            Action save = () => document.SaveIncremental(new MemoryStream());
            save.Should().Throw<NotSupportedException>().WithMessage("*cross-reference stream*");
        }

        [Fact]
        public void Save_incremental_writes_the_modifications_declared_with_MarkAsModified()
        {
            var originalBytes = CreateDocument(1);
            var originalWidth = WidthOfFirstPage(originalBytes);

            // PDFsharp cannot detect the modification of an object, so a modification that is not declared
            // is not written by an incremental update.
            var notDeclaredBytes = ResizeFirstPage(originalBytes, declareAsModified: false);
            WidthOfFirstPage(notDeclaredBytes).Should().Be(originalWidth);

            var declaredBytes = ResizeFirstPage(originalBytes, declareAsModified: true);
            WidthOfFirstPage(declaredBytes).Should().Be(originalWidth + 100);

            static byte[] ResizeFirstPage(byte[] pdfBytes, bool declareAsModified)
            {
                using var document = PdfReader.Open(new MemoryStream(pdfBytes), PdfDocumentOpenMode.ModifyIncremental);
                var page = document.Pages[0];
                page.Width = XUnit.FromPoint(page.Width.Point + 100);
                if (declareAsModified)
                    document.MarkAsModified(page);
                return SaveIncremental(document);
            }

            static double WidthOfFirstPage(byte[] pdfBytes)
            {
                using var document = PdfReader.Open(new MemoryStream(pdfBytes), PdfDocumentOpenMode.Import);
                return document.Pages[0].Width.Point;
            }
        }

        [Fact]
        public void Save_incremental_adds_one_signature_after_the_other()
        {
            var originalBytes = CreateDocument(1);

            var onceSignedBytes = Sign(originalBytes);
            var twiceSignedBytes = Sign(onceSignedBytes);

            // Both updates keep all previously written bytes, so the first signature stays valid.
            onceSignedBytes.Take(originalBytes.Length).Should().Equal(originalBytes);
            twiceSignedBytes.Take(onceSignedBytes.Length).Should().Equal(onceSignedBytes);

            // Both signatures are in the document, the second one did not replace the first one.
            FieldNamesOf(twiceSignedBytes).Should().HaveCount(2);

            // The second signature covers the whole file except the hole of its /Contents entry.
            var byteRanges = ByteRangesOf(twiceSignedBytes);
            byteRanges.Count.Should().Be(2);
            var lastByteRange = byteRanges[byteRanges.Count - 1];
            lastByteRange[0].Should().Be(0);
            (lastByteRange[2] + lastByteRange[3]).Should().Be(twiceSignedBytes.Length);
        }

        /// <summary>
        /// Creates a PDF file with the specified number of pages.
        /// </summary>
        static byte[] CreateDocument(int pageCount)
        {
            using var document = new PdfDocument();
            for (int idx = 0; idx < pageCount; idx++)
                document.AddPage();
            document.Info.Author = "PDFsharp";

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }

        /// <summary>
        /// Creates a PDF file with one page that uses a cross-reference stream instead of a
        /// cross-reference table. PDFsharp can read such a file, but cannot write one.
        /// </summary>
        static byte[] CreateDocumentWithCrossReferenceStream()
        {
            var stream = new MemoryStream();
            var positions = new int[5];

            Write("%PDF-1.5\n%ÐÔÅØ\n");
            WriteObject(1, "<< /Type /Catalog /Pages 2 0 R >>");
            WriteObject(2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
            WriteObject(3, "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] >>");

            // The cross-reference stream itself is object 4. Its entries are 'type offset generation',
            // the field widths are defined by /W.
            positions[4] = (int)stream.Length;
            var entries = new byte[5 * 7];
            SetEntry(0, 0, 0, 65535);
            for (int objectNumber = 1; objectNumber <= 4; objectNumber++)
                SetEntry(objectNumber, 1, positions[objectNumber], 0);

            Write(Invariant($"4 0 obj\n<< /Type /XRef /Size 5 /W [1 4 2] /Root 1 0 R /Length {entries.Length} >>\nstream\n"));
            stream.Write(entries, 0, entries.Length);
            Write("\nendstream\nendobj\n");
            Write(Invariant($"startxref\n{positions[4]}\n%%EOF\n"));

            return stream.ToArray();

            void Write(string text)
            {
                foreach (var ch in text)
                    stream.WriteByte((byte)ch);
            }

            void WriteObject(int objectNumber, string value)
            {
                positions[objectNumber] = (int)stream.Length;
                Write(Invariant($"{objectNumber} 0 obj\n{value}\nendobj\n"));
            }

            void SetEntry(int index, byte type, int offset, int generation)
            {
                var start = index * 7;
                entries[start] = type;
                entries[start + 1] = (byte)(offset >> 24);
                entries[start + 2] = (byte)(offset >> 16);
                entries[start + 3] = (byte)(offset >> 8);
                entries[start + 4] = (byte)offset;
                entries[start + 5] = (byte)(generation >> 8);
                entries[start + 6] = (byte)generation;
            }
        }

        /// <summary>
        /// Adds a digital signature to the specified PDF file by an incremental update.
        /// </summary>
        static byte[] Sign(byte[] pdfBytes)
        {
            using var document = PdfReader.Open(new MemoryStream(pdfBytes), PdfDocumentOpenMode.ModifyIncremental);
            var options = new DigitalSignatureOptions
            {
                ContactInfo = "John Doe",
                Location = "Seattle",
                Reason = "License Agreement",
                Rectangle = new XRect(36, 36, 200, 50),
                AppearanceHandler = new EmptyAppearanceHandler()
            };
            _ = DigitalSignatureHandler.ForDocument(document, new TestSigner(), options);

            return SaveIncremental(document);
        }

        static byte[] SaveIncremental(PdfDocument document)
        {
            using var stream = new MemoryStream();
            document.SaveIncremental(stream);
            return stream.ToArray();
        }

        static string TextOf(byte[] pdfBytes)
        {
            var chars = new char[pdfBytes.Length];
            for (int idx = 0; idx < pdfBytes.Length; idx++)
                chars[idx] = (char)pdfBytes[idx];
            return new String(chars);
        }

        static long LastStartxrefOf(byte[] pdfBytes)
        {
            var text = TextOf(pdfBytes);
            var index = text.LastIndexOf("startxref", StringComparison.Ordinal);
            index.Should().BeGreaterThan(0);
            return Int64.Parse(text.Substring(index + "startxref".Length).Trim().Split('\n')[0].Trim(),
                CultureInfo.InvariantCulture);
        }

        static int RootObjectNumberOf(byte[] pdfBytes)
        {
            var text = TextOf(pdfBytes);
            var index = text.LastIndexOf("/Root", StringComparison.Ordinal);
            index.Should().BeGreaterThan(0);
            var value = text.Substring(index + "/Root".Length).TrimStart();
            return Int32.Parse(value.Substring(0, value.IndexOf(' ')), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the values of all /ByteRange entries of the specified PDF file in the order of their occurrence.
        /// </summary>
        static List<long[]> ByteRangesOf(byte[] pdfBytes)
        {
            var text = TextOf(pdfBytes);
            var byteRanges = new List<long[]>();
            var index = 0;
            while ((index = text.IndexOf("/ByteRange", index, StringComparison.Ordinal)) > 0)
            {
                var start = text.IndexOf('[', index);
                var end = text.IndexOf(']', start);
                byteRanges.Add(text.Substring(start + 1, end - start - 1)
                    .Split([' '], StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => Int64.Parse(value, CultureInfo.InvariantCulture))
                    .ToArray());
                index = end;
            }
            return byteRanges;
        }

        /// <summary>
        /// Gets the partial names of the fields at the root of the interactive form.
        /// </summary>
        static List<string> FieldNamesOf(byte[] pdfBytes)
        {
            using var document = PdfReader.Open(new MemoryStream(pdfBytes), PdfDocumentOpenMode.Import);
            var fields = document.Catalog.GetAcroForm()?.Elements.GetArray(PdfForm.Keys.Fields);
            fields.Should().NotBeNull();

            var names = new List<string>();
            for (int idx = 0; idx < fields!.Elements.Count; idx++)
                names.Add(fields.Elements.GetDictionary(idx)!.Elements.GetString(PdfFormField.Keys.T));
            return names;
        }

        /// <summary>
        /// A signer that creates a deterministic dummy signature, so that no certificate is needed.
        /// </summary>
        class TestSigner : IDigitalSigner
        {
            public string CertificateName => "PDFsharp unit test";

            public Task<int> GetSignatureSizeAsync() => Task.FromResult(SignatureSize);

            public Task<byte[]> GetSignatureAsync(Stream stream)
            {
                // Read the stream to ensure the ranges to be signed are readable.
                var buffer = new byte[4096];
                while (stream.Read(buffer, 0, buffer.Length) > 0)
                { }

                var signature = new byte[SignatureSize];
                for (int idx = 0; idx < signature.Length; idx++)
                    signature[idx] = (byte)idx;
                return Task.FromResult(signature);
            }

            const int SignatureSize = 512;
        }

        /// <summary>
        /// An appearance handler that draws nothing, so that no font is needed.
        /// </summary>
        class EmptyAppearanceHandler : IAnnotationAppearanceHandler
        {
            public void DrawAppearance(XGraphics gfx, XRect rect)
            { }
        }
    }
}
