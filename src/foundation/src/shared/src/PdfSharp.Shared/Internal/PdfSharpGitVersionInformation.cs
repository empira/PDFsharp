// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Internal
{
    /// <summary>
    /// Product version information from git version tool.
    /// </summary>
    public static class PdfSharpGitVersionInformation
    {
        /// <summary>
        /// The major version number of the product.
        /// </summary>
        public static string Major = global::GitVersionInformation.Major;

        /// <summary>
        /// The minor version number of the product.
        /// </summary>
        public static string Minor = global::GitVersionInformation.Minor;

        /// <summary>
        /// The patch number of the product.
        /// </summary>
        public static string Patch = global::GitVersionInformation.Patch;

        /// <summary>
        /// The Version pre-release string for NuGet.
        /// </summary>
        public static string PreReleaseLabel =  global::GitVersionInformation.PreReleaseLabel;

        /// <summary>
        /// The full version number.
        /// </summary>
        public static string MajorMinorPatch = global::GitVersionInformation.MajorMinorPatch;

        /// <summary>
        /// The full semantic version number created by GitVersion.
        /// </summary>
        public static string SemVer = global::GitVersionInformation.SemVer;

        /// <summary>
        /// The full informational version number created by GitVersion.
        /// </summary>
        public static string InformationalVersion =  global::GitVersionInformation.InformationalVersion;

        /// <summary>
        /// The branch name of the product.
        /// </summary>
        public static string BranchName = global::GitVersionInformation.BranchName;

        /// <summary>
        /// The commit date of the product.
        /// </summary>
        public static string CommitDate = global::GitVersionInformation.CommitDate;
    }
}
