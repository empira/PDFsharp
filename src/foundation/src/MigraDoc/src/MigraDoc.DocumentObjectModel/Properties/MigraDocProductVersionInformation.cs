// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#pragma warning disable 0436

namespace MigraDoc
{
    /// <summary>
    /// Version information for all MigraDoc related assemblies.
    /// </summary>
    // TODO: These literals are not completely in sync with the NuGet meta data. Should be reviewed and fixed.
    public static class MigraDocProductVersionInformation
    {
        // Cannot use const string anymore because GitVersionInformation used static string.
        // The fields are reordered to take initialization chronology into account.

        /// <summary>
        /// The title of the product.
        /// </summary>
        public const string Title = "empira MigraDoc";

        /// <summary>
        /// A characteristic description of the product.
        /// </summary>
        public const string Description = "Creating Documents on the Fly";

        /// <summary>
        /// The major version number of the product.
        /// </summary>
        public static readonly string VersionMajor = GitVersionInformation.Major;

        /// <summary>
        /// The minor version number of the product.
        /// </summary>
        public static readonly string VersionMinor = GitVersionInformation.Minor;

        /// <summary>
        /// The patch number of the product.
        /// </summary>
        public static readonly string VersionPatch = GitVersionInformation.Patch;

        /// <summary>
        /// The Version pre-release string for NuGet.
        /// </summary>
        public static readonly string VersionPreRelease = GitVersionInformation.NuGetPreReleaseTagV2;

        /// <summary>
        /// The PDF creator application information string.
        /// The PDF producer (created by) is PDFsharp anyway.
        /// </summary>
        public static readonly string Creator = $"{Title} {GitVersionInformation.NuGetVersion} ({Url})";

        /// <summary>
        /// The full version number.
        /// </summary>
        public static readonly string Version = GitVersionInformation.MajorMinorPatch;

        /// <summary>
        /// The full semantic version number created by GitVersion.
        /// </summary>
        public static readonly string SemanticVersion = GitVersionInformation.SemVer;

        /// <summary>
        /// The home page of this product.
        /// </summary>
        public const string Url = "www.pdfsharp.net";

        /// <summary>
        /// Unused.
        /// </summary>
        public const string Configuration = "";

        /// <summary>
        /// The company that created/owned the product.
        /// </summary>
        public const string Company = "empira Software GmbH, Troisdorf (Cologne Area), Germany";

        /// <summary>
        /// The name of the product.
        /// </summary>
        public const string Product = "empira MigraDoc";

        /// <summary>
        /// The copyright information.
        /// </summary>
        public const string Copyright = "Copyright © 2001-2024 empira Software GmbH."; // Also used as NuGet Copyright.

        /// <summary>
        /// The trademark of the product.
        /// </summary>
        public const string Trademark = "empira MigraDoc";

        /// <summary>
        /// Unused.
        /// </summary>
        public const string Culture = "";

        // Build = days since 2001-07-04 - change values ONLY here
#if DEBUG_
        // ReSharper disable RedundantNameQualifier
        //public static int BuildNumber = (System.DateTime.Now - new System.DateTime(2001, 7, 4)).Days;
        public static int BuildNumber = (System.DateTime.Now - new System.DateTime(2005, 1, 1)).Days; // Same BuildNumber like PDFsharp
                                                                                                      // ReSharper restore RedundantNameQualifier
#endif

#if Not_used_anymore

        /// <summary>
        /// E.g. "2005-01-01", for use in NuGet Script
        /// </summary>
        public const string VersionReferenceDate = "2005-01-01";

        /// <summary>
        /// Use _ instead of blanks and special characters. Can be complemented with a suffix in the NuGet Script.
        /// Nuspec Doc: The unique identifier for the package. This is the package name that is shown when packages are listed using the Package Manager Console.
        /// These are also used when installing a package using the Install-Package command within the Package Manager Console.
        /// Package IDs may not contain any spaces or characters that are invalid in an URL. In general, they follow the same rules as .NET namespaces do.
        /// So Foo.Bar is a valid ID, Foo! and Foo Bar are not. 
        /// </summary>
        public const string NuGetID = "PDFsharp-MigraDoc";

        /// <summary>
        /// Nuspec Doc: The human-friendly title of the package displayed in the Manage NuGet Packages dialog. If none is specified, the ID is used instead. 
        /// </summary>
        public const string NuGetTitle = "PDFsharp + MigraDoc";

        /// <summary>
        /// Nuspec Doc: A comma-separated list of authors of the package code.
        /// </summary>
        public const string NuGetAuthors = "empira Software GmbH";

        /// <summary>
        /// Nuspec Doc: A comma-separated list of the package creators. This is often the same list as in authors. This is ignored when uploading the package to the NuGet.org Gallery. 
        /// </summary>
        public const string NuGetOwners = "empira Software GmbH";

        /// <summary>
        /// Nuspec Doc: A long description of the package. This shows up in the right pane of the Add Package Dialog as well as in the Package Manager Console when listing packages using the Get-Package command. 
        /// </summary>
        public const string NuGetDescription = "MigraDoc Foundation - the Open Source .NET library that easily creates documents based on an object model with paragraphs, tables, styles, etc. and renders them into PDF or RTF.";

        /// <summary>
        /// Nuspec Doc: A description of the changes made in each release of the package. This field only shows up when the _Updates_ tab is selected and the package is an update to a previously installed package.
        /// It is displayed where the Description would normally be displayed. 
        /// </summary>                  
        public const string NuGetReleaseNotes = "";

        /// <summary>
        /// Nuspec Doc: A short description of the package. If specified, this shows up in the middle pane of the Add Package Dialog. If not specified, a truncated version of the description is used instead.
        /// </summary>                  
        public const string NuGetSummary = "Creating Documents on the Fly.";

        /// <summary>
        /// Nuspec Doc: The locale ID for the package, such as en-us.
        /// </summary>                  
        public const string NuGetLanguage = "";

        /// <summary>
        /// Nuspec Doc: A URL for the home page of the package.
        /// </summary>
        public const string NuGetProjectUrl = "http://www.pdfsharp.net/";

        /// <summary>
        /// Nuspec Doc: A URL for the image to use as the icon for the package in the Manage NuGet Packages dialog box. This should be a 32x32-pixel .png file that has a transparent background.
        /// </summary>
        public const string NuGetIconUrl = "http://www.pdfsharp.net/resources/MigraDoc-Logo-32x32.png";

        /// <summary>
        /// Nuspec Doc: A link to the license that the package is under.
        /// </summary>                  
        public const string NuGetLicenseUrl = "http://www.pdfsharp.net/MigraDoc_License.ashx";

        /// <summary>
        /// Nuspec Doc: A Boolean value that specifies whether the client needs to ensure that the package license (described by licenseUrl) is accepted before the package is installed.
        /// </summary>                  
        public const bool NuGetRequireLicenseAcceptance = false;

        /// <summary>
        /// Nuspec Doc: A space-delimited list of tags and keywords that describe the package. This information is used to help make sure users can find the package using
        /// searches in the Add Package Reference dialog box or filtering in the Package Manager Console window.
        /// </summary>                  
        public const string NuGetTags = "MigraDoc PdfSharp Pdf Document Generation";
#endif
    }
}
