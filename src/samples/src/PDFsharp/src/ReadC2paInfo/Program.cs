// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Numerics;
using System.Text;
using PdfSharp.Pdf.C2pa;
using PdfSharp.Pdf.IO;

namespace ReadC2paInfo
{
    /// <summary>
    /// Demonstrates the fix for issue #389: opens a PDF containing a C2PA content credentials
    /// manifest (a case that used to throw NotImplementedException) and reads the manifest via
    /// the new C2paManager API. Also decodes the JUMBF/CBOR/COSE structures inside the manifest
    /// so the claim, actions, and signature data can be cross-checked independently of PDFsharp.
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            var path = args.Length > 0 ? args[0] : FindDefaultDuckPdf();
            if (path is null || !File.Exists(path))
            {
                Console.Error.WriteLine("Could not find 'duck.pdf'. Pass the path to a PDF file as argument.");
                return 1;
            }

            Console.WriteLine($"Opening '{path}'...");
            using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            Console.WriteLine("Document opened successfully (this used to throw NotImplementedException before the issue #389 fix).");

            var c2pa = C2paManager.ForDocument(document);
            if (!c2pa.HasManifests)
            {
                Console.WriteLine("No C2PA manifests found in this document.");
                return 0;
            }

            Console.WriteLine();
            Console.WriteLine($"Found {c2pa.ManifestCount} C2PA manifest(s):");
            foreach (var info in c2pa.Manifests)
            {
                Console.WriteLine();
                Console.WriteLine($"  Names key      : {info.NamesKey}");
                Console.WriteLine($"  File name      : {info.FileName}");
                Console.WriteLine($"  Description    : {info.Description}");
                Console.WriteLine($"  MIME type      : {info.MimeType}");
                Console.WriteLine($"  AFRelationship : {info.AFRelationship}");
                Console.WriteLine($"  Manifest size  : {info.Data.Length} bytes");

                // A valid JUMBF manifest store starts with a "jumb" superbox: 4-byte length + 4-byte type.
                if (info.Data.Length >= 8)
                {
                    var boxLength = (info.Data[0] << 24) | (info.Data[1] << 16) | (info.Data[2] << 8) | info.Data[3];
                    var boxType = Encoding.ASCII.GetString(info.Data, 4, 4);
                    Console.WriteLine($"  JUMBF box      : type='{boxType}', length={boxLength} (matches manifest size: {boxLength == info.Data.Length})");
                }

                DumpManifestContents(info.Data);
            }

            return 0;
        }

        /// <summary>
        /// Parses the JUMBF box tree of a manifest and prints the claim, actions, and signature
        /// content it finds, decoded from CBOR/COSE, so the values can be verified independently.
        /// </summary>
        static void DumpManifestContents(byte[] manifestData)
        {
            var store = JumbfBox.Parse(manifestData).FirstOrDefault();
            if (store is null)
            {
                Console.WriteLine("  (could not parse JUMBF box tree)");
                return;
            }

            // The manifest store's first 'jumb' child is the manifest itself; its label is the manifest ID.
            var manifestBox = store.Children.FirstOrDefault(c => c.Type == "jumb");
            Console.WriteLine();
            Console.WriteLine($"  Manifest ID    : {manifestBox?.Label}");
            if (manifestBox is null)
                return;

            var claimBox = manifestBox.FindByLabel("c2pa.claim.v2") ?? manifestBox.FindByLabel("c2pa.claim");
            var claimCbor = claimBox?.FirstChildOfType("cbor");
            if (claimCbor is not null)
            {
                Console.WriteLine();
                Console.WriteLine($"  --- {claimBox!.Label} (raw CBOR dump) ---");
                Console.Write(Indent(Cbor.Dump(Cbor.Decode(claimCbor.Payload))));
            }

            var actionsBox = manifestBox.FindByLabel("c2pa.actions.v2") ?? manifestBox.FindByLabel("c2pa.actions");
            var actionsCbor = actionsBox?.FirstChildOfType("cbor");
            if (actionsCbor is not null)
            {
                Console.WriteLine();
                Console.WriteLine($"  --- {actionsBox!.Label} (raw CBOR dump) ---");
                Console.Write(Indent(Cbor.Dump(Cbor.Decode(actionsCbor.Payload))));
            }

            var signatureBox = manifestBox.FindByLabel("c2pa.signature");
            var signatureCbor = signatureBox?.FirstChildOfType("cbor");
            if (signatureCbor is not null)
            {
                var cose = CoseSignature.Parse(Cbor.Decode(signatureCbor.Payload));
                Console.WriteLine();
                Console.WriteLine("  --- c2pa.signature (COSE_Sign1) ---");
                Console.WriteLine($"    Algorithm     : {cose.Algorithm}");
                for (var i = 0; i < cose.Certificates.Count; i++)
                {
                    var cert = cose.Certificates[i];
                    var serialBigEndian = cert.GetSerialNumber().Reverse().ToArray();
                    var serialDecimal = new BigInteger(serialBigEndian, isUnsigned: true, isBigEndian: true);
                    Console.WriteLine($"    Certificate[{i}]:");
                    Console.WriteLine($"      Subject       : {cert.Subject}");
                    Console.WriteLine($"      Issuer        : {cert.Issuer}");
                    Console.WriteLine($"      Serial number : {serialDecimal}");
                }
            }
        }

        static string Indent(string text) =>
            string.Join(Environment.NewLine, text.TrimEnd('\r', '\n').Split('\n').Select(line => "    " + line.TrimEnd('\r'))) + Environment.NewLine;

        /// <summary>
        /// Walks up from the executable directory to find the duck.pdf test asset that ships with PdfSharp.Tests.
        /// </summary>
        static string? FindDefaultDuckPdf()
        {
            DirectoryInfo? dir = new(AppContext.BaseDirectory);
            for (var i = 0; i < 12 && dir is not null; i++)
            {
                var candidate = Path.Combine(dir.FullName, "src", "foundation", "src", "PDFsharp", "tests",
                    "PdfSharp.Tests", "Pdf.C2pa", "testdata", "duck.pdf");
                if (File.Exists(candidate))
                    return candidate;
                dir = dir.Parent;
            }
            return null;
        }
    }
}

