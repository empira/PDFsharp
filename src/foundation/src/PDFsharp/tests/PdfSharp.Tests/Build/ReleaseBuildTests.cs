// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using System.Diagnostics;
using System.Text;
using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Internal;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.Build
{
    [Collection("PDFsharp")]
    public class ReleaseBuildTests
    {
#if !DEBUG
        [Fact]
        public void Check_renamed_identifiers()
        {
            // Check to undo some temporary renames.
            const string automatic = nameof(PdfFontEmbedding.TryComputeSubset);
            (!automatic.EndsWith("_")).Should().BeTrue("some identifiers must be re-renamed before release.");
            _ = automatic;
        }
#endif

        [Fact(Skip = "Do not run this test always.")]
        //[Fact]
        public void Check_CS_files_for_non_ASCII_characters()
        {
#if NET6_0_OR_GREATER || CORE
            return;
#else
#if DEBUG
            _ = BuildInformation.BuildVersionNumber;
#endif
            var folder = IOUtility.GetAssetsPath();
            folder.Should().NotBeNull();
            Debug.Assert(folder != null);

            folder = Path.Combine(folder, "../src/");

#if true_
            folder = @"D:\repos\emp\PDFsharp-COPY\";
#endif

            var list = new List<string>();
            GetFiles(folder, ref list);
            foreach (var file in list)
            {
                CheckFile(file);
            }
#endif
        }

        static void CheckFile(string file)
        {
            var bytes = File.ReadAllBytes(file);

            bool utf8Bom = bytes is [0xEF, 0xBB, 0xBF, ..];

            bool utf16Bom = bytes is [0xFF, 0xFE, ..] or [0xFE, 0xFF, ..];

            int idx = 0;
            bool ascii = true;
            bool? nonAsciiBecauseOfApostrophe = null;
            //bool apostrophe = false;
            foreach (var b in bytes)
            {
                // Skip if apostrophe already set.
                if (b == 146) // ANSI right single quotation mark used as apostrophe.
                {
                    //apostrophe = true;
                    ascii = false;
                    if (nonAsciiBecauseOfApostrophe == null)
                    {
                        nonAsciiBecauseOfApostrophe = true;
                    }
                }

                // Skip if ascii already reset.
                if (b >= 127 && b != 146) // 128 is not assumed to be ASCII.
                {
                    ascii = false;
                    nonAsciiBecauseOfApostrophe = false;
                }

                idx++;
            }

            if (utf16Bom)
            {
                // Resave as UTF-8 with BOM to save space.
                // BOM may be removed in 2nd round.
                _ = typeof(int);
                throw new InvalidOperationException($"File '{file}' must be resaved as UTF-8.");
            }

            if (!utf8Bom && nonAsciiBecauseOfApostrophe == true)
            {
                // Resave ANSI as UTF-8 with BOM.
                _ = typeof(int);
                // Assume it is ANSI and save with BOM.
                Resave(bytes, true, file, true);
            }

            if (utf8Bom && ascii)
            {
                // Resave as UTF-8 with no BOM.
                _ = typeof(int);
                // Assume it is UTF-8 and save without BOM.
                Resave(bytes, false, file, false);
            }

            if (utf8Bom && !ascii)
            {
                // This is OK.
                _ = typeof(int);
            }

            if (!utf8Bom && ascii)
            {
                // This is OK.
                _ = typeof(int);
            }

            if (!utf8Bom && !ascii)
            {
                // This is either ANSI or UTF-8.
                // German Visual Studio checks context.
                // (German) Visual Studio Code assumes UTF-8.
                _ = typeof(int);
                // Assume it is ANSI and save with BOM.
                Resave(bytes, true, file, true);
            }

            //throw new InvalidOperationException(
            //    $"File '{file}' contains non-ASCII characters but has no byte-order mark");
        }

        void GetFiles(string path, ref List<string> files)
        {
            // Skip compiler generated code
            if (path.EndsWith("\\bin"))
                return;
            if (path.EndsWith("\\obj"))
                return;

            files.AddRange(Directory.GetFiles(path, "*.cs"));
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                GetFiles(directory, ref files);
            }
        }

        static void Resave(byte[] bytes, bool isAnsi, string fileName, bool withBom)
        {
            if (isAnsi)
            {
                // Interpret bytes as ANSI.
                var content = PdfEncoders.WinAnsiEncoding.GetString(bytes);
                var utf = Encoding.UTF8.GetBytes(content);

                if (withBom)
                {
                    // Save as UTF-8 with BOM.
                    WriteFile(fileName, utf, true);
                }
                else
                {
                    // Save as UTF-8 without BOM.
                    WriteFile(fileName, utf, false);
                }
            }
            else
            {
                // Interpret bytes as UTF-8.
                //var content = System.Globalization.

                if (withBom)
                {
                    // Save as UTF-8 with BOM.
                    WriteFile(fileName, bytes, true);
                }
                else
                {
                    // Save as UTF-8 without BOM.
                    WriteFile(fileName, bytes, false);
                }
            }
        }

        /// <summary>
        /// Writes the file.
        /// Must be called only if the file requires an update (add a non-existing BOM or remove an existing BOM).
        /// </summary>
        static void WriteFile(String fileName, Byte[] bytes, Boolean addBom)
        {
            bool utf8Bom = bytes is [0xEF, 0xBB, 0xBF, ..];

            (addBom == utf8Bom).Should().BeFalse("Should not come here.");

            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                if (addBom && !utf8Bom)
                {
                    // Write new BOM.
                    fs.Write([0xEF, 0xBB, 0xBF], 0, 3);

                    // Write bytes as they are.
                    fs.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    // Write bytes without existing BOM.
                    fs.Write(bytes, 3, bytes.Length - 3);
                }
            }
        }
    }
}
