// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Console = System.Console;

namespace PdfSharp.Internal
{
    internal class Program
    {
        // We completely eliminate GitVersion
        /// <summary>
        /// Why so cumbersome?
        /// We want to create the GitVersion information during a pre-build step only once,
        /// not more than 100 times for every single project and every target framework.
        /// Executing
        ///   dotnet gitversion > GitVersion.json
        /// in PowerShell would work fine, but requires GitVersion.Tool to be installed
        /// as a global dotnet tool. We don’t want to force every user to install this
        /// just to compile PDFsharp. So we create the GitVersion.json with this dummy
        /// project and execute it during the pre-build step.
        /// In this project GitVersion comes with a NuGet package.
        /// </summary>
        static void Main(string[] args)
        {
#if true
            Console.WriteLine("This program does nothing anymore.");
            return;
#else
            // 1st approach:
            // Compile and execute this project in a pre-build step of PdfSharp.Shared.
            // Comes to an endless recursion in Visual Studio build system and consumes
            // all processor power, all memory and freezes Windows.
            // Does not obviously not word.
            var gitVersionJson = $$"""
                {
                  "Major": "{{GitVersionInformation.Major}}",
                  "Minor": "{{GitVersionInformation.Minor}}",
                  "Patch": "{{GitVersionInformation.Patch}}",
                  "PreReleaseTag": "{{GitVersionInformation.PreReleaseTag}}",
                  "PreReleaseTagWithDash": "{{GitVersionInformation.PreReleaseTagWithDash}}",
                  "PreReleaseLabel": "{{GitVersionInformation.PreReleaseLabel}}",
                  "PreReleaseLabelWithDash": "{{GitVersionInformation.PreReleaseLabelWithDash}}",
                  "PreReleaseNumber":"{{GitVersionInformation.PreReleaseNumber}}",
                  "WeightedPreReleaseNumber": "{{GitVersionInformation.WeightedPreReleaseNumber}}",
                  "BuildMetaData":"{{GitVersionInformation.BuildMetaData}}",
                  "BuildMetaDataPadded":"{{GitVersionInformation.BuildMetaDataPadded}}",
                  "FullBuildMetaData": "{{GitVersionInformation.FullBuildMetaData}}",
                  "MajorMinorPatch": "{{GitVersionInformation.MajorMinorPatch}}",
                  "SemVer": "{{GitVersionInformation.SemVer}}",
                  "LegacySemVer": "{{GitVersionInformation.LegacySemVer}}",
                  "LegacySemVerPadded": "{{GitVersionInformation.LegacySemVerPadded}}",
                  "AssemblySemVer": "{{GitVersionInformation.AssemblySemVer}}",
                  "AssemblySemFileVer":"{{GitVersionInformation.AssemblySemFileVer}}",
                  "FullSemVer": "{{GitVersionInformation.FullSemVer}}",
                  "InformationalVersion":"{{GitVersionInformation.InformationalVersion}}",
                  "BranchName": "{{GitVersionInformation.BranchName}}",
                  "EscapedBranchName":"{{GitVersionInformation.EscapedBranchName}}",
                  "Sha": "{{GitVersionInformation.Sha}}",
                  "ShortSha": "{{GitVersionInformation.ShortSha}}",
                  "NuGetVersionV2": "{{GitVersionInformation.NuGetVersionV2}}",
                  "NuGetVersion": "{{GitVersionInformation.NuGetVersion}}",
                  "NuGetPreReleaseTagV2": "{{GitVersionInformation.NuGetPreReleaseTagV2}}",
                  "NuGetPreReleaseTag": "{{GitVersionInformation.NuGetPreReleaseTag}}",
                  "VersionSourceSha": "{{GitVersionInformation.VersionSourceSha}}",
                  "CommitsSinceVersionSource": "{{GitVersionInformation.CommitsSinceVersionSource}}",
                  "CommitsSinceVersionSourcePadded": "{{GitVersionInformation.CommitsSinceVersionSourcePadded}}",
                  "UncommittedChanges": "{{GitVersionInformation.UncommittedChanges}}",
                  "CommitDate": "{{GitVersionInformation.CommitDate}}"
                }
                """;

            var path = Directory.GetCurrentDirectory();
            if (Path.GetExtension(path) == ".GitVersion")
            {
                var file = Path.Combine(path, "../PdfSharp.Shared/GitVersion.json");
                //Console.WriteLine(file);
                // We are executed from during a pre-build step.
                File.WriteAllText(file, gitVersionJson);
            }
            else
            {
                Console.WriteLine("file");

                // We are executed from a developer in Visual Studio.
                File.WriteAllText("../../../../PdfSharp.Shared/GitVersion.json", gitVersionJson);
            }
#endif
        }
    }
}
