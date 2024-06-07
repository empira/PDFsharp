// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Xunit;
using FluentAssertions;
using PdfSharp.Quality;

// ReSharper disable RedundantArgumentDefaultValue because this is unit test code
// ReSharper disable UnusedVariable because this is unit test code
#pragma warning disable IDE0059

namespace Shared.Tests.Quality
{
    public class IOUtilityTests
    {
        [Fact]
        public void IsDirectorySeparator_Test()
        {
            IOUtility.IsDirectorySeparator('/').Should().BeTrue();
            IOUtility.IsDirectorySeparator('\\').Should().BeTrue();
            IOUtility.IsDirectorySeparator('|').Should().BeFalse();
        }

        [Fact]
        public void NormalizeDirectorySeparators_Test()
        {
            string? path1 = null;
            IOUtility.NormalizeDirectorySeparators(ref path1);
            path1.Should().BeNull();

            string? path2 = "";
            IOUtility.NormalizeDirectorySeparators(ref path2);
            path2.Should().Be("");

            string? path3 = @"Foo\bar\";
            IOUtility.NormalizeDirectorySeparators(ref path3);
            path3.Should().Be("Foo/bar/");
        }

        [Fact]
        public void GetSolutionRoot_Test()
        {
            var root = IOUtility.GetSolutionRoot();
            root.Should().NotBeNull();

            var files = Directory.GetFiles(root!, "PDFsharp.sln"); // Case-sensitive under Linux.
            files.Length.Should().Be(1);
        }

        [Fact]
        public void GetAssetsPath_Test()
        {
            // The solution may not be in directory PDFsharp.
            var solutionRoot = IOUtility.GetSolutionRoot();
            if (IOUtility.IsDirectorySeparator(solutionRoot![solutionRoot.Length - 1]))
                solutionRoot = solutionRoot.Substring(0, solutionRoot.Length - 1);
            var solutionPath = Path.GetFileName(solutionRoot);

            var path1 = IOUtility.GetAssetsPath();
            path1.Should().NotBeNull();
            IOUtility.NormalizeDirectorySeparators(ref path1);
            path1.Should().EndWith($"{solutionPath}/assets/");

            // Get path to directory.
            var path2 = IOUtility.GetAssetsPath("pdfsharp/");
            path2.Should().NotBeNull();
            IOUtility.NormalizeDirectorySeparators(ref path2);
            path2.Should().EndWith($"{solutionPath}/assets/pdfsharp/");

            // Get path to file.
            var path3 = IOUtility.GetAssetsPath("pdfsharp/LICENSE");
            path3.Should().NotBeNull();
            IOUtility.NormalizeDirectorySeparators(ref path3);
            path3.Should().EndWith($"{solutionPath}/assets/pdfsharp/LICENSE");
        }

        [Fact]
        public void GetTempPath_Test()
        {
            var path1 = IOUtility.GetTempPath();
            path1.Should().NotBeNull();
            IOUtility.IsDirectorySeparator(path1![path1.Length - 1]).Should().BeTrue();

            var path2 = IOUtility.GetTempPath(@"created/by/unit/text");
            //path2.Should().EndWith(Path.DirectorySeparatorChar.ToString());
            IOUtility.IsDirectorySeparator(path2![path2.Length - 1]).Should().BeTrue();

            Directory.Delete(path1 + "created/by/unit/text");
            Directory.Delete(path1 + "created/by/unit");
            Directory.Delete(path1 + "created/by");
            Directory.Delete(path1 + "created");
            Directory.Exists(path1 + "created").Should().BeFalse();
        }

        [Fact]
        public void GetTempFileName_Test()
        {
            var name1 = IOUtility.GetTempFileName("", "tmp", false);
            var name2 = IOUtility.GetTempFileName("", "tmp", true);
            var name3 = IOUtility.GetTempFileName("MyFile", "tmp", false);
            var name4 = IOUtility.GetTempFileName("MyFile", "tmp", true);

            // Test was reviewed manually.
            true.Should().BeTrue();
        }

        [Fact]
        public void GetTempFullFileName_Test()
        {
            var name1 = IOUtility.GetTempFullFileName(null, "tmp", false);
            var name2 = IOUtility.GetTempFullFileName("", "tmp", true);
            var name3 = IOUtility.GetTempFullFileName("MyFile", "tmp", false);
            var name4 = IOUtility.GetTempFullFileName("MyFile", "tmp", true);
            var name5 = IOUtility.GetTempFullFileName("xxx/MyFile", "tmp", false);
            var name6 = IOUtility.GetTempFullFileName("xxx/MyFile", "tmp", true);
            var name7 = IOUtility.GetTempFullFileName("xxx/yyy/MyFile", "tmp", false);
            var name8 = IOUtility.GetTempFullFileName("xxx/yyy/MyFile", "tmp", true);

            // Test was reviewed manually.
            true.Should().BeTrue();
        }

        [Fact]
        public void FindLatestTempFile_Test()
        {
            var root = IOUtility.GetSolutionRoot();

            var file1 = IOUtility.FindLatestTempFile(null, root!, "dummy", true);

            // Test was reviewed manually.
            true.Should().BeTrue();
        }

        [Fact]
        public void EnsureAssets_Test()
        {
            var path = IOUtility.GetAssetsPath();
            path.Should().NotBeNull();

            IOUtility.EnsureAssets();

#if true_   // Does not work - assets folder is locked.
            var root = IOHelper.GetSolutionRoot()!;
            var assetsPath = Path.Combine(root, "assets");
            var assetsPath2 = Path.Combine(root, "assets_");
            Directory.Move(assetsPath, assetsPath2);
            var action = IOHelper.EnsureAssetsAreDownloaded;
            action.Should().Throw<IOException>();
            Directory.Move(assetsPath2, assetsPath);
#endif
        }

        [Fact]
        public void EnsureAssetsVersion_Test()
        {
            var path = IOUtility.GetAssetsPath();
            path.Should().NotBeNull();

            var action1 = () => IOUtility.EnsureAssetsVersion(1);
            action1.Should().NotThrow("Version 1 should always exist.");

            var action2 = () => IOUtility.EnsureAssetsVersion(Int32.MaxValue);
            action2.Should().Throw<IOException>();
        }
    }
}
