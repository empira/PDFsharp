// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Internal
{
    /// <summary>
    /// This is a substitute included by the PdfSharp.Shared project in case the
    /// SemVersionInformation-generated.cs file was not yet generated when
    /// the parallel build process starts.
    /// </summary>
    // ReSharper disable once PartialTypeWithSinglePart because this file is linked into another project.
    public static partial class SemVersionInformation
    {
        const string SemMajor = "0";
        const string SemMinor = "0";
        const string SemPatch = "0";
        const string SemVersion = "0.0.1";
        const string SemFileVersion = "0.0.1.0";
        const string SemPreReleaseLabel = "null";
        const string SemInformationalVersion = "0.0.0-null-0";
        const string SemBranchName = "branch/null";
        const string SemCommitDate = "0000-00-00";
        const string SemSha = "0000000000000000000000000000000000000000";
        const string SemShortSha = "00000000";
    }

    // Note
    // Maybe we should generate SemVersionInformation-generated.cs in the debug/release folder.
    // When it is generated in the regular source folder the file is not used even if it
    // exists before the project is compiled. I assume regular files are collected in a snapshot
    // taken when the parallel build process starts.
    // But we will not try this, because SemVersion.props nevertheless does not exist when
    // build begins. Therefore, we must compile PdfSharp.Gitversion.proj at least once before
    // we build the solution to get correct version information.
    // But because of conditions in the build process the build will not fail even after a
    // git clone command after that the two generated files do not yet exist.
}
