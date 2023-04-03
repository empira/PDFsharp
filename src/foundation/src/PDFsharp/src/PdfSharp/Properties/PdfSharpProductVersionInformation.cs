// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp
{
    /// <summary>
    /// Version information for all PDFsharp related assemblies.
    /// </summary>
    // TODO: These literals are not in sync with the NuGet meta data. Should be fixed.
    public static class PdfSharpProductVersionInformation
    {
        // Cannot use const string anymore because GitVersionInformation used static string.
        // The fields are reordered to take initialization chronology into account.

        /// <summary>
        /// The title of the product.
        /// </summary>
        public const string Title = "PDFsharp";

        /// <summary>
        /// A characteristic description of the product.
        /// </summary>
        public const string Description = "A .NET library for processing PDF.";

        /// <summary>
        /// The major version number of the product.
        /// </summary>
        public static readonly string VersionMajor = GitVersionInformation.Major;

        /// <summary>
        /// The minor version number of the product.
        /// </summary>
        public static readonly string VersionMinor = GitVersionInformation.Minor;

        ///// <summary>
        ///// The build number of the product.
        ///// </summary>
        //[Obsolete("Build version is not used anymore because we switched to Semantic Versioning. Use VersionPatch instead.")]
        //public const string VersionBuild = "xxxxx";  // DELETE

        /// <summary>
        /// The patch number of the product.
        /// </summary>
        public static readonly string VersionPatch = GitVersionInformation.Patch;

        /// <summary>
        /// The Version PreRelease String for NuGet.
        /// </summary>
        public static readonly string VersionPreRelease = GitVersionInformation.NuGetPreReleaseTagV2;

        /// <summary>
        /// The PDF creator application information string.
        /// </summary>
        public static readonly string Creator = $"{Title} {GitVersionInformation.NuGetVersion}{Technology}";

        /// <summary>
        /// The PDF producer (created by) information string.
        /// TODO: Called Creator in MigraDoc???
        /// </summary>
        public static readonly string Producer = $"{Title} {GitVersionInformation.NuGetVersion} ({Url})";

        /// <summary>
        /// The full version number.
        /// </summary>
        public static readonly string Version = GitVersionInformation.MajorMinorPatch;

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
        public const string Company = "empira Software GmbH, Cologne Area (Germany)";

        /// <summary>
        /// The name of the product.
        /// </summary>
        public const string Product = "PDFsharp";

        /// <summary>
        /// The copyright information.
        /// </summary>
        public const string Copyright = "Copyright © 2005-2023 empira Software GmbH.";

        /// <summary>
        /// The trademark of the product.
        /// </summary>
        public const string Trademark = "PDFsharp";

        /// <summary>
        /// Unused.
        /// </summary>
        public const string Culture = "";

#if DEBUG_
        /// <summary>
        /// The calculated build number.
        /// </summary>
        // ReSharper disable RedundantNameQualifier
        public static int BuildNumber = (DateTime.Now - new DateTime(2005, 1, 1)).Days;
        // ReSharper restore RedundantNameQualifier
#endif

#if Not_used_anymore
        /// <summary>
        /// E.g. "2005-01-01", for use in NuGet Script.
        /// </summary>
        public const string VersionReferenceDate = "2005-01-01";

        /// <summary>
        /// Use _ instead of blanks and special characters. Can be complemented with a suffix in the NuGet Script.
        /// Nuspec Doc: The unique identifier for the package. This is the package name that is shown when packages
        /// are listed using the Package Manager Console. These are also used when installing a package using the
        /// Install-Package command within the Package Manager Console. Package IDs may not contain any spaces
        /// or characters that are invalid in an URL. In general, they follow the same rules as .NET namespaces do.
        /// So Foo.Bar is a valid ID, Foo! and Foo Bar are not. 
        /// </summary>
        public const string NuGetID = "PDFsharp";

        /// <summary>
        /// Nuspec Doc: The human-friendly title of the package displayed in the Manage NuGet Packages dialog.
        /// If none is specified, the ID is used instead. 
        /// </summary>
        public const string NuGetTitle = "PDFsharp";

        /// <summary>
        /// Nuspec Doc: A comma-separated list of authors of the package code.
        /// </summary>
        public const string NuGetAuthors = "empira Software GmbH";

        /// <summary>
        /// Nuspec Doc: A comma-separated list of the package creators. This is often the same list as in authors.
        /// This is ignored when uploading the package to the NuGet.org Gallery. 
        /// </summary>
        public const string NuGetOwners = "empira Software GmbH";

        /// <summary>
        /// Nuspec Doc: A long description of the package. This shows up in the right pane of the Add Package Dialog
        /// as well as in the Package Manager Console when listing packages using the Get-Package command. 
        /// </summary>
        // This assignment must be written in one line because it will be parsed from a PS1 file.
        public const string NuGetDescription = "PDFsharp is the Open Source .NET library that easily creates and processes PDF documents on the fly from any .NET language. The same drawing routines can be used to create PDF documents, draw on the screen, or send output to any printer.";

        /// <summary>
        /// Nuspec Doc: A description of the changes made in each release of the package. This field only shows up
        /// when the _Updates_ tab is selected and the package is an update to a previously installed package.
        /// It is displayed where the Description would normally be displayed. 
        /// </summary>
        public const string NuGetReleaseNotes = "";

        /// <summary>
        /// Nuspec Doc: A short description of the package. If specified, this shows up in the middle pane of the
        /// Add Package Dialog. If not specified, a truncated version of the description is used instead.
        /// </summary>
        public const string NuGetSummary = "A .NET library for processing PDF.";

        /// <summary>
        /// Nuspec Doc: The locale ID for the package, such as en-us.
        /// </summary>
        public const string NuGetLanguage = "";

        /// <summary>
        /// Nuspec Doc: A URL for the home page of the package.
        /// </summary>
        /// <remarks>
        /// http://www.pdfsharp.net/NuGetPackage_PDFsharp-GDI.ashx
        /// http://www.pdfsharp.net/NuGetPackage_PDFsharp-WPF.ashx
        /// </remarks>
        public const string NuGetProjectUrl = "www.pdfsharp.net";

        /// <summary>
        /// Nuspec Doc: A URL for the image to use as the icon for the package in the Manage NuGet Packages
        /// dialog box. This should be a 32x32-pixel .png file that has a transparent background.
        /// </summary>
        public const string NuGetIconUrl = "http://www.pdfsharp.net/resources/PDFsharp-Logo-32x32.png";

        /// <summary>
        /// Nuspec Doc: A link to the license that the package is under.
        /// </summary>
        public const string NuGetLicenseUrl = "http://www.pdfsharp.net/PDFsharp_License.ashx";

        /// <summary>
        /// Nuspec Doc: A Boolean value that specifies whether the client needs to ensure that the package license (described by licenseUrl) is accepted before the package is installed.
        /// </summary>
        public const bool NuGetRequireLicenseAcceptance = false;

        /// <summary>
        /// Nuspec Doc: A space-delimited list of tags and keywords that describe the package. This information is used to help make sure users can find the package using
        /// searches in the Add Package Reference dialog box or filtering in the Package Manager Console window.
        /// </summary>
        public const string NuGetTags = "PDFsharp PDF creation";
#endif

        /// <summary>
        /// The technology tag of the product:
        /// (none) : Core -- .NET 6 or higher
        /// -gdi : GDI+ -- Windows only
        /// -wpf : WPF -- Windows only
        /// -hybrid : Both GDI+ and WPF (hybrid) -- for self-testing, not used anymore
        /// -sl : Silverlight -- deprecated
        /// -wp : Windows Phone -- deprecated
        /// -wrt : Windows RunTime -- deprecated
        /// -uwp : Universal Windows Platform -- not used anymore
        /// </summary>
#if CORE
        // .NET 6.0 without GDI+ and WPF
        public const string Technology = "";  // no tag
#elif GDI && !WPF
        // GDI+ - Windows Forms (System.Drawing)
        public const string Technology = "-gdi";
#elif WPF && !GDI
        // WPF - Windows Presentation Foundation
        public const string Technology = "-wpf";
#elif WPF && GDI
        // Hybrid - for testing only
        public const string Technology = "-h";
#error Should not come here anymore. May be revived in the future.
#elif UWP
        // UWP - Universal Windows Platform
        public const string Technology = "-uwp";
#endif
    }
}
