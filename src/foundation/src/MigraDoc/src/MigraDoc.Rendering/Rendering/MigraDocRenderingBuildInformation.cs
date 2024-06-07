// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Reflection;
using System.Runtime.Versioning;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Information about the build of the renderer.
    /// </summary>
    public static class MigraDocRenderingBuildInformation
    {
        /// <summary>
        /// Gets the git semantic version number created by GitVersionTask.
        /// </summary>
        public static string GitSemVer => global::GitVersionInformation.SemVer;

        /// <summary>
        /// Gets the name of the branch created by GitVersionTask.
        /// </summary>
        public static string BranchName => global::GitVersionInformation.BranchName;

        /// <summary>
        /// Gets the commit date created by GitVersionTask.
        /// </summary>
        public static string CommitDate => global::GitVersionInformation.CommitDate;

        /// <summary>
        /// Gets the assembly title attribute value.
        /// </summary>
        public static string AssemblyTitle
            => ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

#if !NET6_0_OR_GREATER
        /// <summary>
        /// Gets the target platform attribute value.
        /// </summary>
        public static string TargetPlatform
        {
            get
            {
                // Hack since TargetPlatformAttribute is not available.
                return "Unknown Target Platform";
            }
        }
#else
        /// <summary>
        /// Gets the target platform attribute value.
        /// </summary>
        public static string TargetPlatform
        {
            get
            {
                var attribute = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(TargetPlatformAttribute), false);
                return attribute.Length == 1 ? ((TargetPlatformAttribute)attribute[0]).PlatformName : "";
            }
        }
#endif
    }
}
