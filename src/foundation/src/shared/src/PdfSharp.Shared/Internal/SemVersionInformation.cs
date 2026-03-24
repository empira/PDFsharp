// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Internal
{
    /// <summary>
    /// Internal public fields with semantic versioning information.
    /// </summary>
    public static partial class SemVersionInformation
    {
        // Visual Studio is very smart while building a solution.
        // It can compile projects before its dependencies are compiled.
        // I assume this is possible because it has the source code of 
        // the projects. Therefore, we write code that depends on the
        // static fields below, while the fields are initialized when
        // this project is compiled.

        /// <summary>
        /// The major version number.
        /// </summary>
        //public static string Major = SemMajor;
        public const string Major = SemMajor;

        /// <summary>
        /// The minor version number.
        /// </summary>
        public const string Minor = SemMinor;

        /// <summary>
        /// The patch number.
        /// </summary>
        public static readonly string Patch = SemPatch;

        /// <summary>
        /// The full semantic version number.
        /// </summary>
        public static readonly string Version = SemVersion;

        /// <summary>
        /// The assembly file version number.
        /// </summary>
        public static readonly string FileVersion = SemFileVersion;

        /// <summary>
        /// The pre-release label.
        /// </summary>
        public static readonly string PreReleaseLabel = SemPreReleaseLabel;

        //public static string SemVer = SemSemVer;

        /// <summary>
        /// The assembly informational version.
        /// </summary>
        public static readonly string InformationalVersion = SemInformationalVersion;

        /// <summary>
        /// The name of the current Git branch.
        /// </summary>
        public static readonly string BranchName = SemBranchName;

        /// <summary>
        /// The date of the last commit.
        /// </summary>
        public static readonly string CommitDate = SemCommitDate;

        /// <summary>
        /// The SHA of the last commit.
        /// </summary>
        public static readonly string Sha = SemSha;

        /// <summary>
        /// The short SHA of the last commit.
        /// </summary>
        public static readonly string ShortSha = SemShortSha;
    }
}
