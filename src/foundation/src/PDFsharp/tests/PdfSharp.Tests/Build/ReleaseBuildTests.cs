// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using System.Diagnostics;
using System.Reflection;
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
    public class ReleaseBuildTests // CHECK_BEFORE_RELEASE Run before release.
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

        //[Fact(Skip = "Do not run this test always.")]
        [Fact]
        public void Check_CS_files_for_non_ASCII_characters()
        {
            // This tests only runs in GDI build under .NET 4.6.2.
#if NET8_0_OR_GREATER || CORE || WPF
            return;
#else
            // Set dryRun to true to just check the files.
            // Set dryRun to false and files will be updated.
            const bool dryRun = true;
#if DEBUG
            _ = BuildInformation.BuildVersionNumber;
#endif
            var folder = IOUtility.GetAssetsPath();
            folder.Should().NotBeNull();
            Debug.Assert(folder != null);

            folder = Path.Combine(folder, "../src/");

            var list = new List<string>();
            GetFiles(folder, ref list);
            int updateNecessary = 0;
            foreach (var file in list)
            {
                var result = CheckFile(file, dryRun);
                updateNecessary += result;
            }
            updateNecessary.Should().Be(0);
#endif
        }

        static int CheckFile(string file, bool dryRun)
        {
            var bytes = File.ReadAllBytes(file);

            bool utf8Bom = bytes is [0xEF, 0xBB, 0xBF, ..];

            bool utf16Bom = bytes is [0xFF, 0xFE, ..] or [0xFE, 0xFF, ..];

            bool hasCommentAnsi = bytes is [0x2F, 0x2F, ..];
            bool hasCommentUtf8 = bytes is [0xEF, 0xBB, 0xBF, 0x2F, 0x2F, ..];
            bool hasCommentUtf16Be = bytes is [0xFE, 0xFF, 0x00, 0x2F, 0x00, 0x2F, ..];
            bool hasCommentUtf16Le = bytes is [0xFF, 0xFE, 0x2F, 0x00, 0x2F, 0x00, ..];

            bool hasComment = hasCommentAnsi || hasCommentUtf8 || hasCommentUtf16Be || hasCommentUtf16Le;

            if (!hasComment)
            {
                _ = typeof(int);
                throw new InvalidOperationException($"File '{file}' does not start with a comment.");
            }

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
                return Resave(bytes, true, file, true, dryRun);
            }

            if (utf8Bom && ascii)
            {
                // Resave as UTF-8 with no BOM.
                _ = typeof(int);
                // Assume it is UTF-8 and save without BOM.
                return Resave(bytes, false, file, false, dryRun);
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
                return Resave(bytes, true, file, true, dryRun);
            }

            return 0;

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

        static int Resave(byte[] bytes, bool isAnsi, string fileName, bool withBom, bool dryRun)
        {
            if (isAnsi)
            {
                // Interpret bytes as ANSI.
                var content = PdfEncoders.WinAnsiEncoding.GetString(bytes);
                var utf = Encoding.UTF8.GetBytes(content);

                if (withBom)
                {
                    // Save as UTF-8 with BOM.
                    return WriteFile(fileName, utf, true, dryRun);
                }
                else
                {
                    // Save as UTF-8 without BOM.
                    return WriteFile(fileName, utf, false, dryRun);
                }
            }
            else
            {
                // Interpret bytes as UTF-8.
                //var content = System.Globalization.

                if (withBom)
                {
                    // Save as UTF-8 with BOM.
                    return WriteFile(fileName, bytes, true, dryRun);
                }
                else
                {
                    // Save as UTF-8 without BOM.
                    return WriteFile(fileName, bytes, false, dryRun);
                }
            }
        }

        /// <summary>
        /// Writes the file.
        /// Must be called only if the file requires an update (add a non-existing BOM or remove an existing BOM).
        /// </summary>
        static int WriteFile(string fileName, byte[] bytes, bool addBom, bool dryRun)
        {
            bool utf8Bom = bytes is [0xEF, 0xBB, 0xBF, ..];

            (addBom == utf8Bom).Should().BeFalse("Should not come here.");

            if (dryRun)
            {
                if (addBom && !utf8Bom)
                {
                    // Write new BOM.
                    return 1;
                }
                else
                {
                    // Write bytes without existing BOM.
                    return 1;
                }
            }

            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                if (addBom && !utf8Bom)
                {
                    // Write new BOM.
                    fs.Write([0xEF, 0xBB, 0xBF], 0, 3);

                    // Write bytes as they are.
                    fs.Write(bytes, 0, bytes.Length);
                    return 1;
                }
                else
                {
                    // Write bytes without existing BOM.
                    fs.Write(bytes, 3, bytes.Length - 3);
                    return 1;
                }
            }
        }

        // ----------

        // TODO: Check for classes or functions ending with underscore.

        //[Fact]
        static void Check_renamed_classes_or_functions()
        {

        }

        //     [Fact]
        public static void Ensure_DEBUG_stuff_is_excluded()
        {
            const string aaa = "PdfSharp.Diagnostics.DebugBreakHelper";
#if DEBUG_
            true.Should().BeTrue();
#else
            var assembly = Assembly.GetAssembly(typeof(PdfDocument)) ?? throw new SystemException("What?");
            var type = assembly.GetType(aaa);
            type.Should().BeNull($"{aaa} must not be part of a release build.");
#endif
        }

        [Fact]
        public static void Ensure_developer_tags_are_removed()
        {
            string[] filesToExclude = ["blah.md", "blub.md"];
            string[] dirsToExclude = ["bin", "obj", "publish"];  // TODO: TestResults etc.

            var root = Directory.GetCurrentDirectory();
#if DEBUG_
            true.Should().BeTrue();
#else
            //var assembly = Assembly.GetAssembly(typeof(PdfDocument)) ?? throw new SystemException("What?");
            //var type = assembly.GetType(aaa);
            //type.Should().BeNull($"{aaa} should not be part of a release build.");
#endif
        }
    }
}
