// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering  // DELETE
{
    /// <summary>
    /// Use MigraDocProductVersionInformation instead.
    /// </summary>
    [Obsolete("Do not use this class anymore. It is not useful and will be removed.")]
    public static class MigraDocRenderingBuildInformation
    {
        /// <summary>
        /// Gets the git semantic version number.
        /// </summary>
        [Obsolete("Do not use this class anymore. Use 'PdfSharp.Internal.SemVersionInformation.Version' instead.")]
        public static string GitSemVer => "do not use this anymore";

        /// <summary>
        /// Gets the name of the branch.
        /// </summary>
        [Obsolete("Do not use this class anymore. Use 'PdfSharp.Internal.SemVersionInformation.BranchName' instead.")]
        public static string BranchName => "do not use this anymore";

        /// <summary>
        /// Gets the commit date.
        /// </summary>
        [Obsolete("Do not use this class anymore. Use 'PdfSharp.Internal.SemVersionInformation.CommitDate' instead.")]
        public static string CommitDate => "do not use this anymore";

        /// <summary>
        /// Gets the assembly title attribute value.
        /// </summary>
        public static string AssemblyTitle => "do not use this anymore";

        /// <summary>
        /// Gets the target platform attribute value.
        /// </summary>
        public static string TargetPlatform => "do not use this anymore";
    }
}
